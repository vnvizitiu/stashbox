﻿using Ronin.Common;
using Stashbox.Entity;
using Stashbox.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stashbox.Registration
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private ImmutableTree<ImmutableTree<IServiceRegistration>> serviceRepository;
        private ImmutableTree<ImmutableTree<IServiceRegistration>> genericDefinitionRepository;
        private readonly object syncObject = new object();

        public RegistrationRepository()
        {
            this.serviceRepository = ImmutableTree<ImmutableTree<IServiceRegistration>>.Empty;
            this.genericDefinitionRepository = ImmutableTree<ImmutableTree<IServiceRegistration>>.Empty;
        }

        public IEnumerable<IServiceRegistration> GetAllRegistrations()
        {
            return this.serviceRepository.Enumerate().SelectMany(tree => tree.Value.Enumerate().Select(reg => reg.Value))
                .Concat(this.genericDefinitionRepository.Enumerate().SelectMany(tree => tree.Value.Enumerate().Select(reg => reg.Value)));
        }

        public bool TryGetRegistrationWithConditions(TypeInformation typeInfo, out IServiceRegistration registration)
        {
            return typeInfo.DependencyName == null ? this.TryGetByTypeKeyWithConditions(typeInfo, out registration) : this.TryGetByNamedKey(typeInfo, out registration);
        }

        public bool TryGetRegistrationWithConditionsWithoutGenericDefinitionExtraction(TypeInformation typeInfo, out IServiceRegistration registration)
        {
            return typeInfo.DependencyName == null ? this.TryGetByTypeKeyWithConditionsWithoutGenericDefinitionExtraction(typeInfo, out registration) :
                this.TryGetByNamedKeyWithoutGenericDefinitionExtraction(typeInfo, out registration);
        }

        public bool TryGetRegistration(TypeInformation typeInfo, out IServiceRegistration registration)
        {
            return typeInfo.DependencyName == null ? this.TryGetByTypeKey(typeInfo, out registration) : this.TryGetByNamedKey(typeInfo, out registration);
        }

        public void AddRegistration(Type typeKey, IServiceRegistration registration, string nameKey)
        {
            var immutableTree = ImmutableTree<IServiceRegistration>.Empty;
            var newTree = immutableTree.AddOrUpdate(nameKey.GetHashCode(), registration);

            lock (this.syncObject)
            {
                this.serviceRepository = this.serviceRepository.AddOrUpdate(typeKey.GetHashCode(), newTree, (oldValue, newValue) =>
                {
                    return oldValue.AddOrUpdate(nameKey.GetHashCode(), registration, (oldRegistration, newReg) => oldRegistration);
                });
            }
        }

        public void AddOrUpdateRegistration(Type typeKey, IServiceRegistration registration, string nameKey)
        {
            var immutableTree = ImmutableTree<IServiceRegistration>.Empty;
            var newTree = immutableTree.AddOrUpdate(nameKey.GetHashCode(), registration);

            lock (this.syncObject)
            {
                this.serviceRepository = this.serviceRepository.AddOrUpdate(typeKey.GetHashCode(), newTree, (oldValue, newValue) => newValue);
            }
        }

        public void AddGenericDefinition(Type typeKey, IServiceRegistration registration, string nameKey)
        {
            var immutableTree = ImmutableTree<IServiceRegistration>.Empty;
            var newTree = immutableTree.AddOrUpdate(nameKey.GetHashCode(), registration);

            lock (this.syncObject)
            {
                this.genericDefinitionRepository = this.genericDefinitionRepository.AddOrUpdate(typeKey.GetHashCode(), newTree, (oldValue, newValue) =>
                {
                    return oldValue.AddOrUpdate(nameKey.GetHashCode(), registration, (oldRegistration, newReg) => oldRegistration);
                });
            }
        }

        public void AddOrUpdateGenericDefinition(Type typeKey, IServiceRegistration registration, string nameKey)
        {
            var immutableTree = ImmutableTree<IServiceRegistration>.Empty;
            var newTree = immutableTree.AddOrUpdate(nameKey.GetHashCode(), registration);

            lock (this.syncObject)
            {
                this.genericDefinitionRepository = this.genericDefinitionRepository.AddOrUpdate(typeKey.GetHashCode(), newTree, (oldValue, newValue) => newValue);
            }
        }

        public bool TryGetTypedRepositoryRegistrations(TypeInformation typeInfo, out IServiceRegistration[] registrations)
        {
            var serviceRegistrations = this.serviceRepository.GetValueOrDefault(typeInfo.Type.GetHashCode());
            if (serviceRegistrations == null)
            {
                Type genericTypeDefinition;
                if (this.TryHandleOpenGenericType(typeInfo.Type, out genericTypeDefinition))
                {
                    serviceRegistrations = this.genericDefinitionRepository.GetValueOrDefault(genericTypeDefinition.GetHashCode());
                }
                else
                {
                    registrations = null;
                    return false;
                }
            }

            registrations = serviceRegistrations?.Enumerate().Select(reg => reg.Value).ToArray();
            return registrations != null;
        }

        public bool Constains(Type type, string name = null)
        {
            var regs = this.serviceRepository.GetValueOrDefault(type.GetHashCode());
            if (name != null)
                return regs != null && regs.GetValueOrDefault(name.GetHashCode()) != null;
            else
                return regs != null;
        }

        public bool ConstainsTypeKeyWithConditions(TypeInformation typeInfo)
        {
            var registrations = this.serviceRepository.GetValueOrDefault(typeInfo.Type.GetHashCode());
            if (registrations != null)
                return registrations.Value != null &&
                       registrations.Enumerate()
                           .Any(registration => registration.Value.IsUsableForCurrentContext(typeInfo));
            {
                Type genericTypeDefinition;
                if (!this.TryHandleOpenGenericType(typeInfo.Type, out genericTypeDefinition)) return false;
                registrations = this.genericDefinitionRepository.GetValueOrDefault(genericTypeDefinition.GetHashCode());
                return registrations != null && registrations.Enumerate().Any(registration => registration.Value.IsUsableForCurrentContext(new TypeInformation
                {
                    Type = genericTypeDefinition,
                    ParentType = typeInfo.ParentType,
                    DependencyName = typeInfo.DependencyName,
                    CustomAttributes = typeInfo.CustomAttributes
                }));
            }
        }

        public bool ConstainsTypeKeyWithConditionsWithoutGenericDefinitionExtraction(TypeInformation typeInfo)
        {
            var registrations = this.serviceRepository.GetValueOrDefault(typeInfo.Type.GetHashCode());
            if (registrations == null) return false;
            return registrations.Value != null && registrations.Enumerate().Any(registration => registration.Value.IsUsableForCurrentContext(typeInfo));
        }

        public void CleanUp()
        {
            foreach (var registration in this.serviceRepository.Enumerate().Select(reg => reg.Value).SelectMany(registrations => registrations.Enumerate()))
            {
                registration.Value.CleanUp();
            }

            this.serviceRepository = null;
        }

        private bool TryGetByTypeKey(TypeInformation typeInfo, out IServiceRegistration registration)
        {
            ImmutableTree<IServiceRegistration> registrations;
            if (!this.TryGetRegistrationsByType(typeInfo.Type, out registrations))
            {
                registration = null;
                return false;
            }

            registration = registrations.Value;
            return true;
        }

        private bool TryGetByTypeKeyWithConditions(TypeInformation typeInfo, out IServiceRegistration registration)
        {
            ImmutableTree<IServiceRegistration> registrations;
            if (!this.TryGetRegistrationsByType(typeInfo.Type, out registrations))
            {
                registration = null;
                return false;
            }

            var serviceRegistrations = registrations.Enumerate().Select(reg => reg.Value).ToArray();
            if (serviceRegistrations.Any(reg => reg.HasCondition))
                registration = serviceRegistrations.Where(reg => reg.HasCondition)
                                                   .FirstOrDefault(reg => reg.IsUsableForCurrentContext(typeInfo));
            else
                registration = serviceRegistrations.FirstOrDefault(reg => reg.IsUsableForCurrentContext(typeInfo));

            return registration != null;
        }

        private bool TryGetByTypeKeyWithConditionsWithoutGenericDefinitionExtraction(TypeInformation typeInfo, out IServiceRegistration registration)
        {
            ImmutableTree<IServiceRegistration> registrations;
            if (!this.TryGetRegistrationsByTypeWithoutGenericDefinitionExtraction(typeInfo.Type, out registrations))
            {
                registration = null;
                return false;
            }

            var serviceRegistrations = registrations.Enumerate().Select(reg => reg.Value).ToArray();
            if (serviceRegistrations.Any(reg => reg.HasCondition))
                registration = serviceRegistrations.Where(reg => reg.HasCondition)
                                                   .FirstOrDefault(reg => reg.IsUsableForCurrentContext(typeInfo));
            else
                registration = serviceRegistrations.FirstOrDefault(reg => reg.IsUsableForCurrentContext(typeInfo));

            return registration != null;
        }

        private bool TryGetRegistrationsByType(Type type, out ImmutableTree<IServiceRegistration> registrations)
        {
            registrations = this.serviceRepository.GetValueOrDefault(type.GetHashCode());
            if (registrations != null) return true;
            Type genericTypeDefinition;
            if (this.TryHandleOpenGenericType(type, out genericTypeDefinition))
            {
                registrations = this.genericDefinitionRepository.GetValueOrDefault(genericTypeDefinition.GetHashCode());
            }
            else
            {
                return false;
            }

            return registrations != null;
        }

        private bool TryGetRegistrationsByTypeWithoutGenericDefinitionExtraction(Type type, out ImmutableTree<IServiceRegistration> registrations)
        {
            registrations = this.serviceRepository.GetValueOrDefault(type.GetHashCode());
            return registrations != null;
        }

        private bool TryGetByNamedKey(TypeInformation typeInfo, out IServiceRegistration registration)
        {
            ImmutableTree<IServiceRegistration> registrations;
            if (this.TryGetRegistrationsByType(typeInfo.Type, out registrations))
            {
                registration = registrations.GetValueOrDefault(typeInfo.DependencyName.GetHashCode());
                return registration != null;
            }

            registration = null;
            return false;
        }

        private bool TryGetByNamedKeyWithoutGenericDefinitionExtraction(TypeInformation typeInfo, out IServiceRegistration registration)
        {
            ImmutableTree<IServiceRegistration> registrations;
            if (this.TryGetRegistrationsByTypeWithoutGenericDefinitionExtraction(typeInfo.Type, out registrations))
            {
                registration = registrations.GetValueOrDefault(typeInfo.DependencyName.GetHashCode());
                return registration != null;
            }

            registration = null;
            return false;
        }

        private bool TryHandleOpenGenericType(Type type, out Type genericTypeDefinition)
        {
            if (type.IsConstructedGenericType)
            {
                genericTypeDefinition = type.GetGenericTypeDefinition();
                return true;
            }

            genericTypeDefinition = null;
            return false;
        }
    }
}
