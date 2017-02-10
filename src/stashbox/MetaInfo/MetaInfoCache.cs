﻿using Stashbox.Attributes;
using Stashbox.Entity;
using Stashbox.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Stashbox.Configuration;

namespace Stashbox.MetaInfo
{
    /// <summary>
    /// Represents a store which contains metadata about the services.
    /// </summary>
    public class MetaInfoCache
    {
        private readonly IContainerConfigurator containerConfigurator;

        /// <summary>
        /// Contains the generic type contraints of the type if it has any.
        /// </summary>
        public readonly IDictionary<int, Type[]> GenericTypeConstraints;

        /// <summary>
        /// The type of the actual service implementation.
        /// </summary>
        public Type TypeTo { get; }

        /// <summary>
        /// Stores the reflected constructor informations.
        /// </summary>
        public ConstructorInformation[] Constructors { get; private set; }

        /// <summary>
        /// Stores the reflected injection method informations.
        /// </summary>
        public MethodInformation[] InjectionMethods { get; private set; }

        /// <summary>
        /// Stores the reflected injection memeber informations.
        /// </summary>
        public MemberInformation[] InjectionMembers { get; private set; }

        /// <summary>
        /// Constructs the <see cref="MetaInfoCache"/>
        /// </summary>
        /// <param name="containerConfigurator">The container configurator.</param>
        /// <param name="typeTo">The type of the actual service implementation.</param>
        public MetaInfoCache(IContainerConfigurator containerConfigurator, Type typeTo)
        {
            this.TypeTo = typeTo;
            this.containerConfigurator = containerConfigurator;
            this.GenericTypeConstraints = new Dictionary<int, Type[]>();

            var typeInfo = typeTo.GetTypeInfo();
            this.AddConstructors(typeInfo.DeclaredConstructors);
            this.AddMethods(typeInfo.DeclaredMethods);
            this.InjectionMembers = this.FillMembers(typeInfo).ToArray();
            this.CollectGenericConstraints(typeInfo);
        }

        private void CollectGenericConstraints(TypeInfo typeInfo)
        {
            if (!typeInfo.IsGenericType && !typeInfo.IsGenericTypeDefinition)
                return;

            foreach (var typeInfoGenericTypeParameter in typeInfo.GenericTypeParameters)
            {
                var paramTypeInfo = typeInfoGenericTypeParameter.GetTypeInfo();
                var pos = paramTypeInfo.GenericParameterPosition;
                var cons = paramTypeInfo.GetGenericParameterConstraints();

                if (cons.Length > 0)
                    this.GenericTypeConstraints.Add(pos, cons);
            }
        }

        private void AddConstructors(IEnumerable<ConstructorInfo> infos)
        {
            this.Constructors = infos.Where(info => !info.IsStatic).Select(info => new ConstructorInformation
            {
                Constructor = info,
                Parameters = this.FillParameters(info.GetParameters()).ToArray()
            }).ToArray();
        }

        private void AddMethods(IEnumerable<MethodInfo> infos)
        {
            this.InjectionMethods = infos.Where(methodInfo => methodInfo.GetCustomAttribute<InjectionMethodAttribute>() != null).Select(info => new MethodInformation
            {
                Method = info,
                Parameters = this.FillParameters(info.GetParameters()).ToArray()
            }).ToArray();
        }

        private IEnumerable<TypeInformation> FillParameters(IEnumerable<ParameterInfo> parameters)
        {
            return parameters.Select(parameterInfo => new TypeInformation
            {
                Type = parameterInfo.ParameterType,
                DependencyName = parameterInfo.GetCustomAttribute<DependencyAttribute>() != null ?
                                 parameterInfo.GetCustomAttribute<DependencyAttribute>().Name : null,
                ParentType = this.TypeTo,
                CustomAttributes = parameterInfo.GetCustomAttributes().ToArray(),
                ParameterName = parameterInfo.Name,
                HasDefaultValue = parameterInfo.HasDefaultValue,
                DefaultValue = parameterInfo.DefaultValue
            });
        }

        private IEnumerable<MemberInformation> FillMembers(TypeInfo typeInfo)
        {
            return this.SelectProperties(typeInfo.DeclaredProperties.Where(property => property.CanWrite))
                   .Select(propertyInfo => new MemberInformation
                   {
                       TypeInformation = new TypeInformation
                       {
                           Type = propertyInfo.PropertyType,
                           DependencyName = propertyInfo.GetCustomAttribute<DependencyAttribute>() != null ?
                                        propertyInfo.GetCustomAttribute<DependencyAttribute>().Name : null,
                           ParentType = this.TypeTo,
                           CustomAttributes = propertyInfo.GetCustomAttributes().ToArray(),
                           ParameterName = propertyInfo.Name,
                           IsMember = true
                       },
                       MemberInfo = propertyInfo
                   })
                   .Concat(this.SelectFields(typeInfo.DeclaredFields.Where(field => !field.IsInitOnly))
                           .Where(fieldInfo => fieldInfo.GetCustomAttribute<DependencyAttribute>() != null)
                           .Select(fieldInfo => new MemberInformation
                           {
                               TypeInformation = new TypeInformation
                               {
                                   Type = fieldInfo.FieldType,
                                   DependencyName = fieldInfo.GetCustomAttribute<DependencyAttribute>() != null
                                       ? fieldInfo.GetCustomAttribute<DependencyAttribute>().Name
                                       : null,
                                   ParentType = this.TypeTo,
                                   CustomAttributes = fieldInfo.GetCustomAttributes().ToArray(),
                                   ParameterName = fieldInfo.Name,
                                   IsMember = true
                               },
                               MemberInfo = fieldInfo
                           }));
        }

        private IEnumerable<FieldInfo> SelectFields(IEnumerable<FieldInfo> fields)
        {
            if (this.containerConfigurator.ContainerConfiguration.MemberInjectionWithoutAnnotationEnabled)
                return fields.Where(fieldInfo => fieldInfo.GetCustomAttribute<DependencyAttribute>() != null ||
                    this.containerConfigurator.ContainerConfiguration.MemberInjectionWithoutAnnotationRule.HasFlag(Rules.AutoMemberInjection.PrivateFields));
            else
                return fields.Where(fieldInfo => fieldInfo.GetCustomAttribute<DependencyAttribute>() != null);
        }

        private IEnumerable<PropertyInfo> SelectProperties(IEnumerable<PropertyInfo> properties)
        {
            if (this.containerConfigurator.ContainerConfiguration.MemberInjectionWithoutAnnotationEnabled)
                return properties.Where(property => property.GetCustomAttribute<DependencyAttribute>() != null ||
                    (this.containerConfigurator.ContainerConfiguration.MemberInjectionWithoutAnnotationRule.HasFlag(Rules.AutoMemberInjection.PropertiesWithPublicSetter) && property.SetMethod.IsPublic) ||
                     this.containerConfigurator.ContainerConfiguration.MemberInjectionWithoutAnnotationRule.HasFlag(Rules.AutoMemberInjection.PropertiesWithLimitedAccess));
            else
                return properties.Where(property => property.GetCustomAttribute<DependencyAttribute>() != null);
        }
    }
}
