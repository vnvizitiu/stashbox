﻿using System;
using System.Collections.Generic;
using Stashbox.Infrastructure;
using Stashbox.Infrastructure.Resolution;

namespace Stashbox
{
    /// <summary>
    /// Represents a resolution scope.
    /// </summary>
    public class ResolutionScope : ResolutionScopeBase, IDependencyResolver
    {
        private readonly IActivationContext activationContext;

        /// <summary>
        /// Constructs a resolution scope.
        /// </summary>
        /// <param name="activationContext">The dependency resolver.</param>
        public ResolutionScope(IActivationContext activationContext)
        {
            this.activationContext = activationContext;
        }

        /// <inheritdoc />
        public TKey Resolve<TKey>(string name = null, bool nullResultAllowed = false) =>
            (TKey)this.activationContext.Activate(typeof(TKey), this, name, nullResultAllowed);

        /// <inheritdoc />
        public object Resolve(Type typeFrom, string name = null, bool nullResultAllowed = false) =>
            this.activationContext.Activate(typeFrom, this, name, nullResultAllowed);

        /// <inheritdoc />
        public IEnumerable<TKey> ResolveAll<TKey>() =>
            (IEnumerable<TKey>)this.activationContext.Activate(typeof(IEnumerable<TKey>), this);

        /// <inheritdoc />
        public IEnumerable<object> ResolveAll(Type typeFrom)
        {
            var type = typeof(IEnumerable<>).MakeGenericType(typeFrom);
            return (IEnumerable<object>)this.activationContext.Activate(type, this);
        }

        /// <inheritdoc />
        public Delegate ResolveFactory(Type typeFrom, string name = null, bool nullResultAllowed = false, params Type[] parameterTypes) =>
            this.activationContext.ActivateFactory(typeFrom, parameterTypes, this, name, nullResultAllowed);

        /// <inheritdoc />
        public Func<TService> ResolveFactory<TService>(string name = null, bool nullResultAllowed = false) =>
            this.ResolveFactory(typeof(TService), name, nullResultAllowed) as Func<TService>;

        /// <inheritdoc />
        public Func<T1, TService> ResolveFactory<T1, TService>(string name = null, bool nullResultAllowed = false) =>
            this.ResolveFactory(typeof(TService), name, nullResultAllowed, typeof(T1)) as Func<T1, TService>;

        /// <inheritdoc />
        public Func<T1, T2, TService> ResolveFactory<T1, T2, TService>(string name = null, bool nullResultAllowed = false) =>
            this.ResolveFactory(typeof(TService), name, nullResultAllowed, typeof(T1), typeof(T2)) as Func<T1, T2, TService>;

        /// <inheritdoc />
        public Func<T1, T2, T3, TService> ResolveFactory<T1, T2, T3, TService>(string name = null, bool nullResultAllowed = false) =>
            this.ResolveFactory(typeof(TService), name, nullResultAllowed, typeof(T1), typeof(T2), typeof(T3)) as Func<T1, T2, T3, TService>;

        /// <inheritdoc />
        public Func<T1, T2, T3, T4, TService> ResolveFactory<T1, T2, T3, T4, TService>(string name = null, bool nullResultAllowed = false) =>
            this.ResolveFactory(typeof(TService), name, nullResultAllowed, typeof(T1), typeof(T2), typeof(T3)) as Func<T1, T2, T3, T4, TService>;

        /// <inheritdoc />
        public IDependencyResolver BeginScope() => new ResolutionScope(this.activationContext);
    }
}
