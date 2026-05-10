namespace KLAD.Models
{
    /// <summary>
    /// Модель игрока. Хранит позицию, счет, скорость и заряды стен.
    /// </summary>
    public class Player
    {
        public int Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Speed { get; set; } = 4.0f;
        public int Score { get; set; }
        
        public int WallCharges { get; set; }
        
        public float DirX { get; set; } = 0;
        public float DirY { get; set; } = 1;
    }
}

