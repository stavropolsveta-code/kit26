namespace EchoOfAncients.Core
{
    public class Cell
    {
        public Chip Chip;
        public Obstacle Obstacle;

        public Cell()
        {
            Chip = new Chip(TileType.None);
            Obstacle = new Obstacle(ObstacleType.None, 0);
        }

        public bool IsBlocked => Obstacle.Type == ObstacleType.Rock;

        public bool IsLockedByIce => Obstacle.Type == ObstacleType.Ice;
    }
}