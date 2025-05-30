using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Helpers
{
    public static class DictionaryExtensions
    {
        public static bool IsEqualTo<TKey, TValue>(
            this IDictionary<TKey, TValue> dict1,
            IDictionary<TKey, TValue> dict2,
            IEqualityComparer<TValue>? valueComparer = null
        )
        {
            if (ReferenceEquals(dict1, dict2))
                return true;

            if (dict1 == null || dict2 == null)
                return false;

            if (dict1.Count != dict2.Count)
                return false;

            valueComparer ??= EqualityComparer<TValue>.Default;

            foreach (var kvp in dict1)
            {
                if (!dict2.TryGetValue(kvp.Key, out TValue? value2))
                    return false;

                if (!valueComparer.Equals(kvp.Value, value2))
                    return false;
            }

            return true;
        }
    }
}
