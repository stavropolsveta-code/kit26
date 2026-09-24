namespace EchoOfAncients.Meta
{
    public class Building
    {
        public BuildingDefinition Definition;
        public int Level = 1;

        public Building(BuildingDefinition definition)
        {
            Definition = definition;
        }

        public bool IsMaxed => Level >= Definition.MaxLevel;

        public long UpgradeCost => LongRound(Definition.BaseCost * (1 + (Level - 1) * 0.5f));

        public ResourceType UpgradeResource => Definition.PrimaryResource;

        public float EffectiveMultiplier => Definition.ResourceMultiplier * Level;

        public float EffectiveTaxRate => Definition.TaxRate * Level;

        public bool HasFieldEffect => Definition.FieldEffect != FieldEffect.None;

        private static long LongRound(double value)
        {
            return (long)(value + 0.5);
        }
    }
}