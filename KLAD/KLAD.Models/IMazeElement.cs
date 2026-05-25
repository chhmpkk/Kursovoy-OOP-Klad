namespace KLAD.Models
{
    public interface IMazeElement
    {
        ElementType Type { get; }
        bool IsPassable { get; }
    }
}
