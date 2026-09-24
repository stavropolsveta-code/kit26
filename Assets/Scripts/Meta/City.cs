using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoOfAncients.Meta
{
    public class City
    {
        private readonly ResourceWallet _wallet;
        private readonly Dictionary<string, Building> _buildings = new Dictionary<string, Building>();
        private readonly Dictionary<string, BuildingDefinition> _catalog;

        public event Action<string, int> BuildingChanged;

        public City(ResourceWallet wallet, Dictionary<string, BuildingDefinition> catalog)
        {
            _wallet = wallet;
            _catalog = catalog;
        }

        public IReadOnlyDictionary<string, Building> Buildings => _buildings;

        public int Population { get; set; }

        public bool Has(string id)
        {
            return _buildings.ContainsKey(id);
        }

        public Building Get(string id)
        {
            return _buildings.TryGetValue(id, out var b) ? b : null;
        }

        public bool TryBuild(string id)
        {
            if (!_catalog.TryGetValue(id, out var definition)) return false;
            if (_buildings.ContainsKey(id)) return false;

            return TryApplyCost(definition.PrimaryResource, definition.BaseCost, () =>
            {
                _buildings[id] = new Building(definition);
                BuildingChanged?.Invoke(id, 1);
            });
        }

        public bool TryUpgrade(string id)
        {
            if (!_buildings.TryGetValue(id, out var building)) return false;
            if (building.IsMaxed) return false;

            var cost = building.UpgradeCost;
            return TryApplyCost(building.UpgradeResource, cost, () =>
            {
                building.Level++;
                BuildingChanged?.Invoke(id, building.Level);
            });
        }

        public bool Restore(string id, int level)
        {
            if (!_catalog.TryGetValue(id, out var definition)) return false;
            if (level < 1) level = 1;
            if (level > definition.MaxLevel) level = definition.MaxLevel;
            _buildings[id] = new Building(definition) { Level = level };
            return true;
        }

        public void Clear()
        {
            _buildings.Clear();
        }

        public long CollectTaxes()
        {
            long total = 0;
            foreach (var building in _buildings.Values)
            {
                if (building.Definition.TaxRate <= 0f) continue;
                var income = (long)(Population * building.EffectiveTaxRate);
                if (income <= 0) continue;
                _wallet.Add(building.Definition.PrimaryResource, income);
                total += income;
            }
            return total;
        }

        public List<BuildingDefinition> UnbuiltCatalog()
        {
            return _catalog.Values.Where(d => !_buildings.ContainsKey(d.Id)).OrderBy(d => d.BaseCost).ToList();
        }

        private bool TryApplyCost(ResourceType resource, long cost, Action apply)
        {
            if (!_wallet.TrySpend(resource, cost)) return false;
            apply();
            return true;
        }
    }
}