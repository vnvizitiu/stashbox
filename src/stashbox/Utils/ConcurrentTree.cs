﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Stashbox.Utils
{
    /// <summary>
    /// Represents an immutable AVL tree
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class ConcurrentTree<TValue> : ConcurrentTree<int, TValue>
    {
        /// <summary>
        /// Inserts an item into the tree if it doesn't exist.
        /// </summary>
        /// <param name="value">The value which will be insertedt.</param>
        public void Add(TValue value)
        {
            base.AddOrUpdate(value.GetHashCode(), value);
        }
    }

    /// <summary>
    /// Represents an immutable AVL tree
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class ConcurrentTree<TKey, TValue> : IEnumerable<TValue>
    {
        /// <summary>
        /// Static factory method.
        /// </summary>
        /// <returns>A new tree instance</returns>
        public static ConcurrentTree<TKey, TValue> Create() => new ConcurrentTree<TKey, TValue>();

        private AvlTree<TKey, TValue> repository;

        /// <summary>
        /// The current root value of the tree,
        /// </summary>
        public TValue Value => this.repository.Value;

        /// <summary>
        /// Inidicates that the tree has more nodes than the root one.
        /// </summary>
        public bool HasMultipleItems => this.repository.HasMultipleItems;

        /// <summary>
        /// Constructs the <see cref="ConcurrentTree{TKey, TValue}"/>
        /// </summary>
        public ConcurrentTree()
        {
            this.repository = new AvlTree<TKey, TValue>();
        }

        /// <summary>
        /// Returns with the value specified by the given key if it's exist, otherwise it's default value will be returned.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <returns>The found or the default value.</returns>
        public TValue GetOrDefault(TKey key) => this.repository.GetOrDefault(key);

        /// <summary>
        /// Inserts an item into the tree if it doesn't exist, otherwise the existing item will be replaced if the update delegate is set.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="value">The value which will be inserted if it doesn't exist yet.</param>
        /// <param name="updateDelegate">The update delegate which will be invoked when the value is already stored on the tree.</param>
        /// <returns>The modified tree.</returns>
        public ConcurrentTree<TKey, TValue> AddOrUpdate(TKey key, TValue value, Func<TValue, TValue, TValue> updateDelegate = null)
        {
            var currentRepo = this.repository;
            var newRepo = this.repository.AddOrUpdate(key, value, updateDelegate);

            if (!this.TrySwapCurrentRepository(currentRepo, newRepo))
                this.SwapCurrentRepository(repo => repo.AddOrUpdate(key, value, updateDelegate));

            return this;
        }

        private bool TrySwapCurrentRepository(AvlTree<TKey, TValue> currentRepo, AvlTree<TKey, TValue> newRepo) =>
            Interlocked.CompareExchange(ref repository, newRepo, currentRepo) == currentRepo;

        private void SwapCurrentRepository(Func<AvlTree<TKey, TValue>, AvlTree<TKey, TValue>> repoFactory)
        {
            AvlTree<TKey, TValue> currentRepo;
            AvlTree<TKey, TValue> newRepo;
            int counter = 0;

            do
            {
                if (++counter > 20)
                    throw new InvalidOperationException("Swap quota exceeded.");

                currentRepo = this.repository;
                newRepo = repoFactory(currentRepo);
            } while (Interlocked.CompareExchange(ref repository, newRepo, currentRepo) != currentRepo);
        }

        /// <inheritdoc />
        public IEnumerator<TValue> GetEnumerator() => this.repository.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.repository.GetEnumerator();
    }
}
