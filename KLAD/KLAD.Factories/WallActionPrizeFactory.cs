namespace KLAD.Models
{
    public class WallActionPrizeFactory : PrizeFactory
    {
        public override Prize CreatePrize() => new Prize { PrizeType = PrizeType.WallAction };
    }
}
