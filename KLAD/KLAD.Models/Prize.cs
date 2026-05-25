namespace KLAD.Models
{
    public class Prize : IMazeElement
    {
        public ElementType Type => ElementType.Prize;
        public bool IsPassable => true;
        public PrizeType PrizeType { get; set; }
    }
}
