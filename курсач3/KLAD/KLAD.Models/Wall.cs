namespace KLAD.Models
{
    /// <summary>
    /// Непроходимая стена лабиринта.
    /// </summary>
    public class Wall : IMazeElement
    {
        public ElementType Type => ElementType.Wall;
        public bool IsPassable => false;
    }
}

