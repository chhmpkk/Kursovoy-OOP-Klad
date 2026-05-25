namespace KLAD.Models
{
    public class SpeedUpPrizeFactory : PrizeFactory
    {
        public override Prize CreatePrize() => new Prize { PrizeType = PrizeType.SpeedUp };
    }
}
