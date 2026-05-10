namespace KLAD.Models
{
    /// <summary>
    /// Приз (бонус), дающий временное или постоянное преимущество игроку.
    /// </summary>
    public class Prize : IMazeElement
    {
        public ElementType Type => ElementType.Prize;
        public bool IsPassable => true;
        public PrizeType PrizeType { get; set; }
    }
}

