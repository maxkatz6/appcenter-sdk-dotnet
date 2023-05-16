// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;

namespace Microsoft.AppCenter.Utils
{
    public class DefaultApplicationSettings : IApplicationSettings
    {
        private readonly ConcurrentDictionary<string, object> _dictionary = new ConcurrentDictionary<string, object>();

        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            return (T)_dictionary.GetOrAdd(key, defaultValue);
        }

        public void SetValue(string key, object value)
        {
            _dictionary.TryAdd(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Remove(string key)
        {
            _dictionary.TryRemove(key, out _);
        }
    }
}
