using System.Collections.Generic;

namespace EchoOfAncients.Meta
{
    public enum FieldEffect
    {
        None = 0,
        TMatchDropsEmber = 1,
        LMatchDropsSeed = 2,
        HighlightMoves = 3,
        TaxIncome = 4
    }

    public class BuildingDefinition
    {
        public string Id;
        public string DisplayName;
        public ResourceType PrimaryResource;
        public long BaseCost;
        public int MaxLevel = 5;
        public FieldEffect FieldEffect;
        public float ResourceMultiplier = 0f;
        public float TaxRate = 0f;
    }
}