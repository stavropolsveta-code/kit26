namespace EchoOfAncients.Core
{
    public class Obstacle
    {
        public ObstacleType Type;
        public int Layers;

        public Obstacle(ObstacleType type, int layers)
        {
            Type = type;
            Layers = layers;
        }

        public bool IsEmpty => Type == ObstacleType.None;

        public bool CanContainChip => Type != ObstacleType.Rock;

        public void Damage(int amount = 1)
        {
            if (Type == ObstacleType.Ice)
            {
                Layers -= amount;
                if (Layers <= 0)
                {
                    Layers = 0;
                    Type = ObstacleType.None;
                }
            }
        }
    }
}