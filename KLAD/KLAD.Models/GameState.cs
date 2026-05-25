namespace KLAD.Models
{
    //класс хранилища всей инфы 
    public class GameState
    {
        public Maze Level { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public int TotalTreasures { get; set; }
        public bool IsGameOver { get; set; }
        public GameState()
        {
            Player1 = new Player { X = 1, Y = 1 };
            Player2 = new Player { X = 1, Y = 2 };
        }
    }
}
