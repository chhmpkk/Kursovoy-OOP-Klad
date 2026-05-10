namespace KLAD.Models
{
    /// <summary>
    /// Сокровище. Цель игры - собрать как можно больше сокровищ.
    /// </summary>
    public class Treasure : IMazeElement
    {
        public ElementType Type => ElementType.Treasure;
        public bool IsPassable => true;
    }
}

