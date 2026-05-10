namespace KLAD.Models
{
    /// <summary>
    /// Базовый интерфейс для любого элемента на сетке лабиринта.
    /// </summary>
    public interface IMazeElement
    {
        ElementType Type { get; }
        bool IsPassable { get; }
    }
}

