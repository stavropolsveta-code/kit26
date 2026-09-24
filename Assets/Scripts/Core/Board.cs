namespace EchoOfAncients.Core
{
    public class Board
    {
        public int Width { get; }
        public int Height { get; }
        public Cell[,] Cells { get; }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new Cell[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Cells[x, y] = new Cell();
                }
            }
        }

        public bool IsInside(GridPosition p)
        {
            return p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;
        }

        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public void Place(GridPosition p, Chip chip, Obstacle obstacle = null)
        {
            if (!IsInside(p)) return;
            Cells[p.X, p.Y].Chip = chip;
            if (obstacle != null)
            {
                Cells[p.X, p.Y].Obstacle = obstacle;
            }
        }

        public Chip GetChip(int x, int y)
        {
            return Cells[x, y].Chip;
        }

        public void Clear()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Cells[x, y].Chip = new Chip(TileType.None);
                    Cells[x, y].Obstacle = new Obstacle(ObstacleType.None, 0);
                }
            }
        }
    }
}