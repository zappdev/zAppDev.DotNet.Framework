// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;

namespace zAppDev.DotNet.Framework.Collections
{
    public class IndexedCollection<T, K>
    {
        private readonly Dictionary<K, T> _internalDictionary = new Dictionary<K, T>();
        private Func<T, K> _getKeyFunc;

        public int Length => _internalDictionary.Count;

        public void SetKeyExpression(Func<T, K> generateKeyExpression)
        {
            _getKeyFunc = generateKeyExpression;
        }

        public void Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var key = _getKeyFunc(item);
            if (Contains(key))
            {
                //update value
                _internalDictionary[key] = item;
            }
            else
            {
                _internalDictionary.Add(key, item);
            }
        }

        public List<T> GetItems()
        {
            return _internalDictionary.Values.ToList();
        }

        public void Clear()
        {
            _internalDictionary.Clear();
        }

        public void AddRange(List<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public bool Contains(K key)
        {
            return _internalDictionary.ContainsKey(key);
        }

        public bool Contains(T item)
        {
            return _internalDictionary.ContainsValue(item);
        }

        public T Get(K key)
        {
            return Contains(key) ? _internalDictionary[key] : default(T);
        }

        public List<K> GetKeys()
        {
            return _internalDictionary.Keys.ToList();
        }

        public void Remove(T item)
        {
            var key = _getKeyFunc(item);
            if (Contains(key))
            {
                _internalDictionary.Remove(key);
            }
        }

        public void RemoveByKey(K key)
        {
            if (Contains(key))
            {
                _internalDictionary.Remove(key);
            }
        }
    }
}
