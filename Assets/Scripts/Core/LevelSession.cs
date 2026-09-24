using System;

namespace EchoOfAncients.Core
{
    public class LevelSession
    {
        public LevelConfig Config { get; }
        public int MovesLeft { get; private set; }
        public long Score { get; private set; }
        public bool IsWon { get; private set; }
        public bool IsLost { get; private set; }

        public event Action StateChanged;

        public LevelSession(LevelConfig config)
        {
            Config = config;
            MovesLeft = config.MoveLimit;
        }

        public bool AllGoalsComplete()
        {
            foreach (var goal in Config.Goals)
            {
                if (!goal.IsComplete) return false;
            }
            return true;
        }

        public void RegisterTurn(TurnReport report)
        {
            if (IsWon || IsLost) return;

            Score += report.ChipsCleared * 10L;
            var iceGoal = FindGoal(GoalType.BreakIce);
            if (iceGoal != null && report.IceBroken > 0)
            {
                iceGoal.Current = Math.Min(iceGoal.Target, iceGoal.Current + report.IceBroken);
            }

            if (Config.MoveLimit > 0)
            {
                MovesLeft--;
            }

            Evaluate();
        }

        public void RegisterCollect(TileType tile, long amount)
        {
            if (IsWon || IsLost) return;
            var goal = FindCollectionGoal(tile);
            if (goal != null)
            {
                goal.Current = Math.Min(goal.Target, goal.Current + amount);
            }
            Evaluate();
        }

        public void TickTime(float delta)
        {
            if (!Config.Timed || IsWon || IsLost) return;
            Config.TimeSeconds -= delta;
            if (Config.TimeSeconds <= 0f)
            {
                Config.TimeSeconds = 0f;
                IsLost = true;
                StateChanged?.Invoke();
            }
        }

        private void Evaluate()
        {
            if (AllGoalsComplete())
            {
                IsWon = true;
            }
            else if (Config.MoveLimit > 0 && MovesLeft <= 0)
            {
                IsLost = true;
            }
            StateChanged?.Invoke();
        }

        private LevelGoal FindGoal(GoalType type)
        {
            foreach (var goal in Config.Goals)
            {
                if (goal.Type == type) return goal;
            }
            return null;
        }

        private LevelGoal FindCollectionGoal(TileType tile)
        {
            foreach (var goal in Config.Goals)
            {
                if (goal.Type == GoalType.Collect && goal.CollectTile == tile) return goal;
            }
            return null;
        }
    }
}