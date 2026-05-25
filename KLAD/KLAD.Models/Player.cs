namespace KLAD.Models
{
    public class Player
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Speed { get; set; } = 4.0f;
        public int Score { get; set; }
        
        public int WallCharges { get; set; }
        //вектор взгляда
        public float DirX { get; set; } = -1;
        public float DirY { get; set; } = 1;
    }
}
