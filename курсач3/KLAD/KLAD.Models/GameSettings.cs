namespace KLAD.Models
{
    /// <summary>
    /// Настройки игровой сессии.
    /// </summary>
    public class GameSettings
    {
        public int TargetTreasures { get; set; } = 15;
        public int TargetPrizes { get; set; } = 10;
    }
}

