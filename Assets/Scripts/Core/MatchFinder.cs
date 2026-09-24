using System.Collections.Generic;

namespace EchoOfAncients.Core
{
    public class MatchResult
    {
        public List<List<GridPosition>> Matches = new List<List<GridPosition>>();
        public HashSet<GridPosition> AllAffected = new HashSet<GridPosition>();

        public void Add(List<GridPosition> match)
        {
            Matches.Add(match);
            foreach (var p in match)
            {
                AllAffected.Add(p);
            }
        }
    }

    public static class MatchFinder
    {
        public static MatchResult FindAll(Board board)
        {
            var result = new MatchResult();
            if (board == null) return result;

            FindHorizontal(board, result);
            FindVertical(board, result);
            return result;
        }

        private static void FindHorizontal(Board board, MatchResult result)
        {
            for (int y = 0; y < board.Height; y++)
            {
                int runStart = 0;
                TileType type = TileType.None;
                for (int x = 0; x <= board.Width; x++)
                {
                    TileType current = TileType.None;
                    if (x < board.Width)
                    {
                        var chip = board.GetChip(x, y);
                        if (chip != null && !chip.IsEmpty && !board.Cells[x, y].IsBlocked)
                        {
                            current = chip.Type;
                        }
                    }

                    if (x < board.Width && current == type && type != TileType.None)
                    {
                        continue;
                    }

                    if (type != TileType.None && x - runStart >= 3)
                    {
                        var match = new List<GridPosition>();
                        for (int i = runStart; i < x; i++)
                        {
                            match.Add(new GridPosition(i, y));
                        }
                        result.Add(match);
                    }

                    runStart = x;
                    type = current;
                }
            }
        }

        private static void FindVertical(Board board, MatchResult result)
        {
            for (int x = 0; x < board.Width; x++)
            {
                int runStart = 0;
                TileType type = TileType.None;
                for (int y = 0; y <= board.Height; y++)
                {
                    TileType current = TileType.None;
                    if (y < board.Height)
                    {
                        var chip = board.GetChip(x, y);
                        if (chip != null && !chip.IsEmpty && !board.Cells[x, y].IsBlocked)
                        {
                            current = chip.Type;
                        }
                    }

                    if (y < board.Height && current == type && type != TileType.None)
                    {
                        continue;
                    }

                    if (type != TileType.None && y - runStart >= 3)
                    {
                        var match = new List<GridPosition>();
                        for (int i = runStart; i < y; i++)
                        {
                            match.Add(new GridPosition(x, i));
                        }
                        result.Add(match);
                    }

                    runStart = y;
                    type = current;
                }
            }
        }

        public static bool HasAnyMatch(Board board)
        {
            return FindAll(board).Matches.Count > 0;
        }
    }
}