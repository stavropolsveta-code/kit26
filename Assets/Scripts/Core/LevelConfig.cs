using System.Collections.Generic;

namespace EchoOfAncients.Core
{
    public enum GoalType
    {
        Score = 0,
        Collect = 1,
        BreakIce = 2
    }

    public class LevelGoal
    {
        public GoalType Type;
        public TileType CollectTile;
        public long Target;
        public long Current;

        public bool IsComplete => Current >= Target;
    }

    public class LevelConfig
    {
        public int Width = 8;
        public int Height = 8;
        public int MoveLimit = 25;
        public bool Timed;
        public float TimeSeconds = 90f;
        public List<LevelGoal> Goals = new List<LevelGoal>();
    }
}