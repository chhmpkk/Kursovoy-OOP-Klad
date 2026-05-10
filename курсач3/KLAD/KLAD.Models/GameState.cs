namespace KLAD.Models
{
    public class GameState
    {
        public Maze Level { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public int TotalTreasures { get; set; }
        public bool IsGameOver { get; set; }

        /// <summary>
        /// Инициализирует состояние игры и начальные позиции игроков.
        /// </summary>
        public GameState()
        {
            Player1 = new Player { Id = 1, X = 1, Y = 1 };
            Player2 = new Player { Id = 2, X = 1, Y = 2 };
        }
    }
}

