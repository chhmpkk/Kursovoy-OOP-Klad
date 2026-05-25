namespace KLAD.Models
{
    public class Treasure : IMazeElement
    {
        public ElementType Type => ElementType.Treasure;
        public bool IsPassable => true;
    }
}
