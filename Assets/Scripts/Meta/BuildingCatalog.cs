using System.Collections.Generic;

namespace EchoOfAncients.Meta
{
    public static class BuildingCatalog
    {
        public static Dictionary<string, BuildingDefinition> Create()
        {
            return new Dictionary<string, BuildingDefinition>
            {
                ["town_hall"] = new BuildingDefinition
                {
                    Id = "town_hall",
                    DisplayName = "Ратуша",
                    PrimaryResource = ResourceType.Wood,
                    BaseCost = 100,
                    MaxLevel = 10,
                    TaxRate = 0.02f
                },
                ["warehouse"] = new BuildingDefinition
                {
                    Id = "warehouse",
                    DisplayName = "Склад",
                    PrimaryResource = ResourceType.Wood,
                    BaseCost = 250,
                    ResourceMultiplier = 0.1f
                },
                ["forge"] = new BuildingDefinition
                {
                    Id = "forge",
                    DisplayName = "Кузница",
                    PrimaryResource = ResourceType.Stone,
                    BaseCost = 600,
                    MaxLevel = 3,
                    FieldEffect = FieldEffect.TMatchDropsEmber
                },
                ["tree_of_life"] = new BuildingDefinition
                {
                    Id = "tree_of_life",
                    DisplayName = "Древо Жизни",
                    PrimaryResource = ResourceType.Essence,
                    BaseCost = 2000,
                    MaxLevel = 3,
                    FieldEffect = FieldEffect.LMatchDropsSeed
                },
                ["library"] = new BuildingDefinition
                {
                    Id = "library",
                    DisplayName = "Библиотека",
                    PrimaryResource = ResourceType.Stone,
                    BaseCost = 1500,
                    MaxLevel = 5,
                    FieldEffect = FieldEffect.HighlightMoves
                },
                ["temple"] = new BuildingDefinition
                {
                    Id = "temple",
                    DisplayName = "Храм",
                    PrimaryResource = ResourceType.Crystal,
                    BaseCost = 4000,
                    TaxRate = 0.04f
                }
            };
        }
    }
}