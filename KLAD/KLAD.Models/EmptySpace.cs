namespace KLAD.Models
{
    public class EmptySpace : IMazeElement
    {
        public ElementType Type => ElementType.Empty;
        public bool IsPassable => true;
    }
}
