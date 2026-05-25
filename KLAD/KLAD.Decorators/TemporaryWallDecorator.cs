namespace KLAD.Models
{
    public class TemporaryWallDecorator : MazeElementDecorator
    {
        public TemporaryWallDecorator(IMazeElement baseElement) : base(baseElement) { }

        public override bool IsPassable => false;
        public override ElementType Type => ElementType.Wall;
    }
}
