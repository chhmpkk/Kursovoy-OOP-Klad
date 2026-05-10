namespace KLAD.Models
{
    /// <summary>
    /// Декоратор, превращающий пустую клетку во временную стену.
    /// </summary>
    public class TemporaryWallDecorator : MazeElementDecorator
    {
        public TemporaryWallDecorator(IMazeElement baseElement) : base(baseElement) { }

        public override bool IsPassable => false;
        public override ElementType Type => ElementType.Wall;
    }
}

