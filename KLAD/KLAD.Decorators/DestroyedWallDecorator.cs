namespace KLAD.Models
{    public class DestroyedWallDecorator : MazeElementDecorator
    {
        public DestroyedWallDecorator(IMazeElement baseElement) : base(baseElement) { }

        public override bool IsPassable => true;
        public override ElementType Type => ElementType.Empty;
    }
}
