﻿using System;

namespace Stashbox.Infrastructure
{
    /// <summary>
    /// Represents a resolution scope.
    /// </summary>
    public interface IResolutionScope : IDisposable
    {
        /// <summary>
        /// The root scope.
        /// </summary>
        IResolutionScope RootScope { get; }

        /// <summary>
        /// True if the scope contains scoped instances, otherwise false.
        /// </summary>
        bool HasScopedInstances { get; }
        
        /// <summary>
        /// The name of the scope, if it's null then it's a regular nameless scope.
        /// </summary>
        object Name { get; }

        /// <summary>
        /// Adds or updates an instance in the scope.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void AddScopedInstance(Type key, object value);

        /// <summary>
        /// Gets an instance from the scope.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The item or null if it doesn't exists.</returns>
        object GetScopedInstanceOrDefault(Type key);

        /// <summary>
        /// Adds a service for further disposable tracking.
        /// </summary>
        /// <typeparam name="TDisposable">The type parameter.</typeparam>
        /// <param name="disposable">The <see cref="IDisposable"/> object.</param>
        /// <returns>The <see cref="IDisposable"/> object.</returns>
        TDisposable AddDisposableTracking<TDisposable>(TDisposable disposable)
            where TDisposable : IDisposable;

        /// <summary>
        /// Adds a service with a cleanup delegate.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="finalizable">The object to cleanup.</param>
        /// <param name="finalizer">The cleanup delegate.</param>
        /// <returns>The object to cleanup.</returns>
        TService AddWithFinalizer<TService>(TService finalizable, Action<TService> finalizer);

        /// <summary>
        /// Gets or adds an item to the scope.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="factory">The value factory used if the item doesn't exist yet.</param>
        /// <returns>The scoped item.</returns>
        object GetOrAddScopedItem(object key, Func<IResolutionScope, object> factory);
    }
}
