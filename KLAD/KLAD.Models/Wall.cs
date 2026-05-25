namespace KLAD.Models
{
    public class Wall : IMazeElement
    {
        public ElementType Type => ElementType.Wall;
        public bool IsPassable => false;
    }
}
