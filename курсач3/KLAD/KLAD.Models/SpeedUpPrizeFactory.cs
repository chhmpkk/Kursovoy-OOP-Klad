namespace KLAD.Models
{
    /// <summary>
    /// Фабрика, создающая приз ускорения (SpeedUp).
    /// </summary>
    public class SpeedUpPrizeFactory : PrizeFactory
    {
        public override Prize CreatePrize() => new Prize { PrizeType = PrizeType.SpeedUp };
    }
}

