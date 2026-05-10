namespace KLAD.Models
{
    /// <summary>
    /// Пустое пространство, по которому могут перемещаться игроки.
    /// </summary>
    public class EmptySpace : IMazeElement
    {
        public ElementType Type => ElementType.Empty;
        public bool IsPassable => true;
    }
}

