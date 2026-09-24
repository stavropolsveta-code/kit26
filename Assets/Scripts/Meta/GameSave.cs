using System;
using System.Collections.Generic;

namespace EchoOfAncients.Meta
{
    [Serializable]
    public class GameSave
    {
        public int currentLevel = 1;
        public int population;

        [Serializable]
        public class BuildingState
        {
            public string id;
            public int level;
        }

        public long[] resources = new long[5];

        public List<BuildingState> buildings = new List<BuildingState>();

        public static GameSave Create()
        {
            return new GameSave();
        }
    }
}