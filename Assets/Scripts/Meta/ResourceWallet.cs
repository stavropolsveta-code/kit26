using System;
using System.Collections.Generic;
using EchoOfAncients.Core;

namespace EchoOfAncients.Meta
{
    public class ResourceWallet
    {
        private readonly Dictionary<ResourceType, long> _amounts = new Dictionary<ResourceType, long>();
        private readonly HashSet<ResourceType> _unlocked = new HashSet<ResourceType>();

        public ResourceWallet()
        {
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                _amounts[type] = 0;
            }
            _unlocked.Add(ResourceType.Wood);
        }

        public long Get(ResourceType type)
        {
            return _amounts.TryGetValue(type, out var value) ? value : 0;
        }

        public bool IsUnlocked(ResourceType type)
        {
            return _unlocked.Contains(type);
        }

        public void Unlock(ResourceType type)
        {
            _unlocked.Add(type);
        }

        public void Add(ResourceType type, long amount)
        {
            if (amount <= 0) return;
            _amounts[type] = Get(type) + amount;
        }

        public void SetAmount(ResourceType type, long amount)
        {
            _amounts[type] = Math.Max(0, amount);
        }

        public bool TrySpend(ResourceType type, long amount)
        {
            if (amount < 0) return false;
            if (Get(type) < amount) return false;
            _amounts[type] = Get(type) - amount;
            return true;
        }

        public bool HasAll(IReadOnlyDictionary<ResourceType, long> costs)
        {
            foreach (var kvp in costs)
            {
                if (Get(kvp.Key) < kvp.Value) return false;
            }
            return true;
        }

        public bool TrySpendAll(IReadOnlyDictionary<ResourceType, long> costs)
        {
            if (!HasAll(costs)) return false;
            foreach (var kvp in costs)
            {
                _amounts[kvp.Key] = Get(kvp.Key) - kvp.Value;
            }
            return true;
        }
    }
}