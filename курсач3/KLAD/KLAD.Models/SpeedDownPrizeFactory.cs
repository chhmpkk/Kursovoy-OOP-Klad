namespace KLAD.Models
{
    /// <summary>
    /// ‘абрика, создающа€ приз замедлени€ (SpeedDown) дл€ противника.
    /// </summary>
    public class SpeedDownPrizeFactory : PrizeFactory
    {
        public override Prize CreatePrize() => new Prize { PrizeType = PrizeType.SpeedDown };
    }
}

