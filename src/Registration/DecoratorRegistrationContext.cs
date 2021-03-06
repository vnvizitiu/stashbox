﻿using Stashbox.Configuration;
using Stashbox.Entity;
using System;
using System.Collections.Generic;

namespace Stashbox.Registration
{
    internal class DecoratorRegistrationContext : IDecoratorRegistrationContext
    {
        public IRegistrationContext RegistrationContext { get; }

        public Type ServiceType => this.RegistrationContext.ServiceType;

        public Type ImplementationType => this.RegistrationContext.ImplementationType;

        public DecoratorRegistrationContext(IRegistrationContext registrationContext)
        {
            this.RegistrationContext = registrationContext;
        }

        public IFluentDecoratorRegistrator WithInjectionParameters(params InjectionParameter[] injectionParameters)
        {
            this.RegistrationContext.WithInjectionParameters(injectionParameters);
            return this;
        }

        public IFluentDecoratorRegistrator WithAutoMemberInjection(
            Rules.AutoMemberInjectionRules rule = Rules.AutoMemberInjectionRules.PropertiesWithPublicSetter)
        {
            this.RegistrationContext.WithAutoMemberInjection(rule);
            return this;
        }

        public IFluentDecoratorRegistrator WithConstructorSelectionRule(Func<IEnumerable<ConstructorInformation>, IEnumerable<ConstructorInformation>> rule)
        {
            this.RegistrationContext.WithConstructorSelectionRule(rule);
            return this;
        }

        public IFluentDecoratorRegistrator WithoutDisposalTracking()
        {
            this.RegistrationContext.WithoutDisposalTracking();
            return this;
        }

        public IFluentDecoratorRegistrator ReplaceExisting()
        {
            this.RegistrationContext.ReplaceExisting();
            return this;
        }

        public IFluentDecoratorRegistrator WithConstructorByArgumentTypes(params Type[] argumentTypes)
        {
            this.RegistrationContext.WithConstructorByArgumentTypes(argumentTypes);
            return this;
        }

        public IFluentDecoratorRegistrator WithConstructorByArguments(params object[] arguments)
        {
            this.RegistrationContext.WithConstructorByArguments(arguments);
            return this;
        }

        public IFluentDecoratorRegistrator InjectMember(string memberName, object dependencyName = null)
        {
            this.RegistrationContext.InjectMember(memberName, dependencyName);
            return this;
        }
    }
}
