namespace KLAD.Models
{
    /// <summary>
    /// ‘абрика, создающа€ приз зар€да дл€ работы со стенами.
    /// </summary>
    public class WallActionPrizeFactory : PrizeFactory
    {
        public override Prize CreatePrize() => new Prize { PrizeType = PrizeType.WallAction };
    }
}

