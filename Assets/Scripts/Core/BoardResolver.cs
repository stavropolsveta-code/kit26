using System;
using System.Collections.Generic;

namespace EchoOfAncients.Core
{
    public class TurnReport
    {
        public bool ValidMove;
        public int Cascades;
        public int ChipsCleared;
        public int IceBroken;
        public int SpecialsCreated;
        public int EmbersCreated;
        public int SeedsCreated;
        public int SproutsSpawned;
        public List<SpecialType> CreatedSpecialTypes = new List<SpecialType>();
    }

    public static class BoardResolver
    {
        private static readonly Random Rng = new Random();

        public static bool TryMove(Board board, GridPosition from, GridPosition to, out TurnReport report, FieldRules rules = null)
        {
            report = new TurnReport();

            if (!IsAdjacent(from, to) || !CanMoveChip(board, from) || !CanMoveChip(board, to))
            {
                return false;
            }

            SwapChips(board, from, to);

            var initial = MatchFinder.FindAll(board);
            if (initial.Matches.Count == 0)
            {
                SwapChips(board, from, to);
                return false;
            }

            report.ValidMove = true;
            ResolveCascades(board, report, rules ?? FieldRules.None());
            return true;
        }

        public static void ResolveCascades(Board board, TurnReport report, FieldRules rules = null)
        {
            rules = rules ?? FieldRules.None();
            var matches = MatchFinder.FindAll(board);
            while (matches.Matches.Count > 0)
            {
                report.Cascades++;
                ResolvePass(board, matches, report, rules);
                ApplyGravity(board);
                SpawnFillers(board);
                matches = MatchFinder.FindAll(board);
            }
        }

        private struct SpecialPlacement
        {
            public TileType Type;
            public SpecialType Special;
        }

        private static void ResolvePass(Board board, MatchResult matches, TurnReport report, FieldRules rules)
        {
            var specialsToCreate = DetermineMatchSpecials(board, matches, rules);

            var clearSet = new HashSet<GridPosition>(matches.AllAffected);
            var heavyIce = new HashSet<GridPosition>();

            var queue = new Queue<GridPosition>();
            foreach (var p in clearSet)
            {
                var chip = board.Cells[p.X, p.Y].Chip;
                if (chip != null && chip.IsSpecial) queue.Enqueue(p);
            }

            var detonated = new HashSet<GridPosition>();
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                if (!detonated.Add(p)) continue;

                var chip = board.Cells[p.X, p.Y].Chip;
                if (chip == null || !chip.IsSpecial) continue;

                var blast = new HashSet<GridPosition>();
                AddBlast(board, p, chip.Special, blast);

                if (chip.Special == SpecialType.Ember)
                {
                    foreach (var q in blast) heavyIce.Add(q);
                }
                if (chip.Special == SpecialType.Seed)
                {
                    report.SproutsSpawned += 2;
                    SpawnSprouts(board, chip.Type, 2);
                }

                foreach (var q in blast)
                {
                    if (!clearSet.Add(q)) continue;
                    var qchip = board.Cells[q.X, q.Y].Chip;
                    if (qchip != null && qchip.IsSpecial) queue.Enqueue(q);
                }
            }

            foreach (var p in clearSet)
            {
                if (!board.IsInside(p)) continue;
                var cell = board.Cells[p.X, p.Y];

                if (cell.Obstacle.Type == ObstacleType.Ice)
                {
                    int damage = heavyIce.Contains(p) ? 2 : 1;
                    cell.Obstacle.Damage(damage);
                    if (cell.Obstacle.IsEmpty)
                    {
                        cell.Chip = new Chip(TileType.None);
                        report.IceBroken++;
                    }
                }
                else if (cell.IsBlocked)
                {
                    continue;
                }
                else if (cell.Chip != null && !cell.Chip.IsEmpty)
                {
                    cell.Chip = new Chip(TileType.None);
                    report.ChipsCleared++;
                }
            }

            foreach (var pair in specialsToCreate)
            {
                var p = pair.Key;
                var placement = pair.Value;
                if (placement.Type == TileType.None) continue;
                board.Cells[p.X, p.Y].Chip = new Chip(placement.Type, placement.Special);
                report.SpecialsCreated++;
                report.CreatedSpecialTypes.Add(placement.Special);
                if (placement.Special == SpecialType.Ember) report.EmbersCreated++;
                else if (placement.Special == SpecialType.Seed) report.SeedsCreated++;
            }
        }

        private static void SpawnSprouts(Board board, TileType type, int count)
        {
            var empty = new List<GridPosition>();
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var cell = board.Cells[x, y];
                    if ((cell.Chip == null || cell.Chip.IsEmpty) && !cell.IsBlocked && cell.Obstacle.Type != ObstacleType.Ice)
                    {
                        empty.Add(new GridPosition(x, y));
                    }
                }
            }

            for (int i = 0; i < count && empty.Count > 0; i++)
            {
                int idx = Rng.Next(empty.Count);
                var p = empty[idx];
                empty.RemoveAt(idx);
                board.Cells[p.X, p.Y].Chip = new Chip(type);
            }
        }

        private static Dictionary<GridPosition, SpecialPlacement> DetermineMatchSpecials(Board board, MatchResult matches, FieldRules rules)
        {
            var crossed = DetermineCrossCells(board, matches);

            var result = new Dictionary<GridPosition, SpecialPlacement>();

            foreach (var match in matches.Matches)
            {
                var length = match.Count;
                var type = board.Cells[match[0].X, match[0].Y].Chip?.Type ?? TileType.None;
                if (type == TileType.None) continue;

                SpecialType special = SpecialType.None;
                if (length >= 5)
                {
                    special = SpecialType.Bomb;
                }
                else if (length == 4)
                {
                    special = IsHorizontalMatch(match)
                        ? SpecialType.LineHorizontal
                        : SpecialType.LineVertical;
                }

                if (special != SpecialType.None && !crossed.Contains(match[match.Count - 1]))
                {
                    result[match[match.Count - 1]] = new SpecialPlacement { Type = type, Special = special };
                }
            }

            foreach (var c in crossed)
            {
                var type = board.Cells[c.X, c.Y].Chip?.Type ?? TileType.None;
                SpecialType special = SpecialType.Bomb;
                if (rules.ForgeEmber) special = SpecialType.Ember;
                else if (rules.TreeSeeds) special = SpecialType.Seed;
                result[c] = new SpecialPlacement { Type = type, Special = special };
            }

            return result;
        }

        private static HashSet<GridPosition> DetermineCrossCells(Board board, MatchResult matches)
        {
            var horizontal = new List<List<GridPosition>>();
            var vertical = new List<List<GridPosition>>();
            foreach (var match in matches.Matches)
            {
                if (IsHorizontalMatch(match)) horizontal.Add(match);
                else vertical.Add(match);
            }

            var crossed = new HashSet<GridPosition>();
            foreach (var hMatch in horizontal)
            {
                foreach (var hPos in hMatch)
                {
                    foreach (var vMatch in vertical)
                    {
                        if (vMatch.Contains(hPos))
                        {
                            crossed.Add(hPos);
                        }
                    }
                }
            }
            return crossed;
        }

        private static void AddBlast(Board board, GridPosition origin, SpecialType special, HashSet<GridPosition> target)
        {
            if (special == SpecialType.Bomb || special == SpecialType.Ember)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        var p = new GridPosition(origin.X + dx, origin.Y + dy);
                        if (board.IsInside(p)) target.Add(p);
                    }
                }
            }
            else if (special == SpecialType.LineHorizontal)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    var p = new GridPosition(x, origin.Y);
                    if (board.IsInside(p)) target.Add(p);
                }
            }
            else if (special == SpecialType.LineVertical)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var p = new GridPosition(origin.X, y);
                    if (board.IsInside(p)) target.Add(p);
                }
            }
        }

        private static bool IsAdjacent(GridPosition a, GridPosition b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) == 1;
        }

        private static bool CanMoveChip(Board board, GridPosition p)
        {
            if (!board.IsInside(p)) return false;
            var cell = board.Cells[p.X, p.Y];
            return cell.Chip != null && !cell.Chip.IsEmpty && cell.Obstacle.Type != ObstacleType.Ice && !cell.IsBlocked;
        }

        private static void SwapChips(Board board, GridPosition a, GridPosition b)
        {
            var tmp = board.Cells[a.X, a.Y].Chip;
            board.Cells[a.X, a.Y].Chip = board.Cells[b.X, b.Y].Chip;
            board.Cells[b.X, b.Y].Chip = tmp;
        }

        private static bool IsHorizontalMatch(List<GridPosition> match)
        {
            int y = match[0].Y;
            foreach (var p in match)
            {
                if (p.Y != y) return false;
            }
            return true;
        }

        private static void ApplyGravity(Board board)
        {
            for (int x = 0; x < board.Width; x++)
            {
                bool changed = true;
                while (changed)
                {
                    changed = false;
                    for (int y = 0; y < board.Height - 1; y++)
                    {
                        var here = board.Cells[x, y];
                        var below = board.Cells[x, y + 1];
                        if (IsMovableChip(here) && IsEmptyCell(below))
                        {
                            below.Chip = here.Chip;
                            here.Chip = new Chip(TileType.None);
                            changed = true;
                        }
                    }
                }
            }
        }

        private static bool IsMovableChip(Cell cell)
        {
            return cell.Chip != null && !cell.Chip.IsEmpty && cell.Obstacle.Type != ObstacleType.Ice && !cell.IsBlocked;
        }

        private static bool IsEmptyCell(Cell cell)
        {
            bool hasChip = cell.Chip != null && !cell.Chip.IsEmpty;
            bool hasIce = cell.Obstacle.Type == ObstacleType.Ice;
            return !hasChip && !hasIce && !cell.IsBlocked;
        }

        private static void SpawnFillers(Board board)
        {
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = board.Height - 1; y >= 0; y--)
                {
                    var cell = board.Cells[x, y];
                    if (cell.Chip != null && !cell.Chip.IsEmpty) continue;
                    if (cell.IsBlocked || cell.Obstacle.Type == ObstacleType.Ice) continue;

                    TileType type;
                    do
                    {
                        type = (TileType)Rng.Next((int)TileType.Leaf, (int)TileType.Crystal + 1);
                    }
                    while (WouldCreateImmediateMatch(board, x, y, type));

                    cell.Chip = new Chip(type);
                }
            }
        }

        private static bool WouldCreateImmediateMatch(Board board, int x, int y, TileType type)
        {
            if (x >= 2 &&
                board.GetChip(x - 1, y)?.Type == type &&
                board.GetChip(x - 2, y)?.Type == type)
            {
                return true;
            }
            if (y >= 2 &&
                board.GetChip(x, y - 1)?.Type == type &&
                board.GetChip(x, y - 2)?.Type == type)
            {
                return true;
            }
            return false;
        }
    }
}