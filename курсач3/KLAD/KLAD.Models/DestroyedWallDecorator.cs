namespace KLAD.Models
{
    /// <summary>
    /// Декоратор, делающий стену проходимой (разрушенной).
    /// </summary>
    public class DestroyedWallDecorator : MazeElementDecorator
    {
        public DestroyedWallDecorator(IMazeElement baseElement) : base(baseElement) { }

        public override bool IsPassable => true;
        public override ElementType Type => ElementType.Empty;
    }
}

