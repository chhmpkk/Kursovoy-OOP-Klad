namespace KLAD.Models
{
    public class SpeedDownPrizeFactory : PrizeFactory
    {
        public override Prize CreatePrize() => new Prize { PrizeType = PrizeType.SpeedDown };
    }
}
