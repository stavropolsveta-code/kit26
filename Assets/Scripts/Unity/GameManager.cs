using System;
using System.Collections.Generic;
using UnityEngine;
using EchoOfAncients.Core;
using EchoOfAncients.Meta;

namespace EchoOfAncients.Unity
{
    public class GameManager : MonoBehaviour
    {
        private const string LocalSaveKey = "echo_save";

        [Header("Поле")]
        public BoardView BoardView;
        public int Width = 8;
        public int Height = 8;

        [Header("Уровень")]
        public int MoveLimit = 25;

        public ResourceWallet Wallet { get; private set; }
        public City City { get; private set; }
        public LevelSession Session { get; private set; }

        public event Action SaveRequested;

        private void Awake()
        {
            Wallet = new ResourceWallet();
            City = new City(Wallet, BuildingCatalog.Create());
            City.BuildingChanged += OnCityBuildingChanged;

            var board = BoardGenerator.Generate(Width, Height);
            SeedDefaultObstacles(board);

            if (BoardView != null)
            {
                BoardView.Build(board);
                BoardView.SwapRequested += OnSwapRequested;
                BoardView.TurnResolved += OnTurnResolved;
                SyncFieldRules();
            }

            var config = new LevelConfig
            {
                Width = Width,
                Height = Height,
                MoveLimit = MoveLimit,
                Goals = new List<LevelGoal>
                {
                    new LevelGoal { Type = GoalType.Score, Target = 1000 },
                    new LevelGoal { Type = GoalType.BreakIce, Target = 3 },
                    new LevelGoal { Type = GoalType.Collect, CollectTile = TileType.Wood, Target = 2 }
                }
            };
            Session = new LevelSession(config);
            Session.StateChanged += OnSessionStateChanged;

#if !UNITY_WEBGL
            TryLoadLocal();
#endif
        }

        public GameSave CaptureSave()
        {
            var save = GameSave.Create();
            save.population = City != null ? City.Population : 0;
            save.resources = new long[System.Enum.GetValues(typeof(ResourceType)).Length];
            if (Wallet != null)
            {
                foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
                {
                    save.resources[(int)type] = Wallet.Get(type);
                }
            }
            if (City != null)
            {
                foreach (var kvp in City.Buildings)
                {
                    save.buildings.Add(new GameSave.BuildingState { id = kvp.Key, level = kvp.Value.Level });
                }
            }
            return save;
        }

        public void ApplySave(GameSave save)
        {
            if (save == null) return;

            if (Wallet != null && save.resources != null)
            {
                foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
                {
                    int idx = (int)type;
                    if (idx < save.resources.Length) Wallet.SetAmount(type, save.resources[idx]);
                }
            }

            if (City != null)
            {
                City.Clear();
                if (save.buildings != null)
                {
                    foreach (var b in save.buildings) City.Restore(b.id, b.level);
                }
                City.Population = save.population;
            }

            SyncFieldRules();
        }

        public string SerializeSave()
        {
            return JsonUtility.ToJson(CaptureSave());
        }

        public void ApplySerializedSave(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var save = JsonUtility.FromJson<GameSave>(json);
                ApplySave(save);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Эхо Древних] Не удалось применить сохранение: {e.Message}");
            }
        }

        public void SaveLocal()
        {
            PlayerPrefs.SetString(LocalSaveKey, SerializeSave());
            PlayerPrefs.Save();
        }

        public void TryLoadLocal()
        {
            if (PlayerPrefs.HasKey(LocalSaveKey))
            {
                ApplySerializedSave(PlayerPrefs.GetString(LocalSaveKey));
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && Session != null) SaveLocal();
        }

        private void OnApplicationQuit()
        {
            SaveLocal();
        }

        private static void SeedDefaultObstacles(Board board)
        {
            int midX = board.Width / 2;
            int midY = board.Height / 2;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int x = midX + dx;
                    int y = midY + dy;
                    if (!board.IsInside(x, y)) continue;
                    board.Cells[x, y].Obstacle = new Obstacle(ObstacleType.Ice, 2);
                }
            }
            board.Cells[1, 1].Obstacle = new Obstacle(ObstacleType.Ice, 1);
            board.Cells[WidthX(board), board.Height - 2].Obstacle = new Obstacle(ObstacleType.Rock, 0);
        }

        private static int WidthX(Board board)
        {
            return board.Width - 2;
        }

        private void OnSwapRequested(GridPosition from, GridPosition to)
        {
            if (BoardView != null)
            {
                BoardView.ApplyMove(from, to);
            }
        }

        private void OnCityBuildingChanged(string id, int level)
        {
            SyncFieldRules();
            SaveRequested?.Invoke();
            SaveLocal();
        }

        public void SyncFieldRules()
        {
            if (BoardView == null) return;
            var rules = new FieldRules();
            if (City != null)
            {
                if (City.Has("forge")) rules.ForgeEmber = true;
                if (City.Has("tree_of_life")) rules.TreeSeeds = true;
            }
            BoardView.Rules = rules;
        }

        private void OnTurnResolved(TurnReport report)
        {
            Session.RegisterTurn(report);

            if (Session.IsWon)
            {
                GrantLevelReward();
                SaveRequested?.Invoke();
                SaveLocal();
            }
        }

        private void GrantLevelReward()
        {
            Wallet.Add(ResourceType.Wood, 50);
            Wallet.Add(ResourceType.Stone, 25);
            Wallet.Add(ResourceType.Essence, 5);
        }

        private void OnSessionStateChanged()
        {
            if (Session.IsWon)
            {
                Debug.Log("[Эхо Древних] Уровень пройден! Ресурсы зачислены.");
            }
            else if (Session.IsLost)
            {
                Debug.Log("[Эхо Древних] Ходы закончились.");
            }
        }

        private void Update()
        {
            if (Session != null && Session.Config.Timed)
            {
                Session.TickTime(Time.deltaTime);
            }
        }

        private void OnGUI()
        {
            if (Session == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 260, 320), GUI.skin.box);
            GUILayout.Label($"Ходы: {Session.MovesLeft}   Счёт: {Session.Score}  ({Session.Config.MoveLimit})");
            foreach (var goal in Session.Config.Goals)
            {
                GUILayout.Label($"{goal.Type} {goal.CollectTile}: {goal.Current}/{goal.Target}");
            }

            GUILayout.Space(6);
            GUILayout.Label($"Дерево: {Wallet.Get(ResourceType.Wood)}  Камень: {Wallet.Get(ResourceType.Stone)}  Эссенция: {Wallet.Get(ResourceType.Essence)}");
            GUILayout.Space(6);

            foreach (var definition in City.UnbuiltCatalog())
            {
                if (GUILayout.Button($"Построить {definition.DisplayName} ({definition.BaseCost} {definition.PrimaryResource})"))
                {
                    City.TryBuild(definition.Id);
                }
            }

            foreach (var kvp in City.Buildings)
            {
                var b = kvp.Value;
                if (b.IsMaxed) continue;
                GUILayout.Label($"{b.Definition.DisplayName} ур.{b.Level}");
                if (GUILayout.Button($"  Улучшить ({b.UpgradeCost} {b.UpgradeResource})"))
                {
                    City.TryUpgrade(b.Definition.Id);
                }
            }

            if (City.Buildings.Count > 0)
            {
                GUILayout.Space(4);
                if (GUILayout.Button("Собрать налоги (+эссенция)"))
                {
                    City.Population = 50;
                    City.CollectTaxes();
                }
            }
            GUILayout.EndArea();
        }
    }
}