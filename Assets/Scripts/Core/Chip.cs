namespace EchoOfAncients.Core
{
    public class Chip
    {
        public TileType Type;
        public SpecialType Special;

        public Chip(TileType type, SpecialType special = SpecialType.None)
        {
            Type = type;
            Special = special;
        }

        public Chip Clone()
        {
            return new Chip(Type, Special);
        }

        public bool IsEmpty => Type == TileType.None;

        public bool IsSpecial => Special != SpecialType.None;
    }
}