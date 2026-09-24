using System;
using System.Collections.Generic;

namespace EchoOfAncients.Core
{
    public static class BoardGenerator
    {
        private static readonly Random Rng = new Random();

        public static Board Generate(int width, int height)
        {
            var board = new Board(width, height);

            var available = new List<TileType>
            {
                TileType.Leaf, TileType.Wood, TileType.Stone,
                TileType.Water, TileType.Fire, TileType.Crystal
            };

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileType type;
                    do
                    {
                        type = available[Rng.Next(available.Count)];
                    }
                    while (CreatesMatch(board, x, y, type));

                    board.Cells[x, y].Chip = new Chip(type);
                }
            }

            return board;
        }

        private static bool CreatesMatch(Board board, int x, int y, TileType type)
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

        public static void ApplyLayout(Board board, TileType[,] layout)
        {
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    board.Cells[x, y].Chip = new Chip(layout[x, y]);
                }
            }
        }
    }
}