using System.Collections.Generic;

namespace KLAD.Models
{
    public enum ElementType
    {
        Empty,
        Wall,
        Treasure,
        Prize
    }

    public enum PrizeType
    {
        SpeedUp,
        SpeedDown,
        WallAction 
    }

    public interface IMazeElement
    {
        ElementType Type { get; }
        bool IsPassable { get; }
    }

    public class Wall : IMazeElement
    {
        public ElementType Type => ElementType.Wall;
        public bool IsPassable => false;
    }

    public class EmptySpace : IMazeElement
    {
        public ElementType Type => ElementType.Empty;
        public bool IsPassable => true;
    }

    public class Treasure : IMazeElement
    {
        public ElementType Type => ElementType.Treasure;
        public bool IsPassable => true;
    }

    public class Prize : IMazeElement
    {
        public ElementType Type => ElementType.Prize;
        public bool IsPassable => true;
        public PrizeType PrizeType { get; set; }
    }

    public abstract class MazeElementDecorator : IMazeElement
    {
        protected IMazeElement _baseElement;

        public MazeElementDecorator(IMazeElement baseElement)
        {
            _baseElement = baseElement;
        }

        public virtual ElementType Type => _baseElement.Type;
        public virtual bool IsPassable => _baseElement.IsPassable;
        
        public IMazeElement GetBaseElement() => _baseElement;
    }

    public class TemporaryWallDecorator : MazeElementDecorator
    {
        public TemporaryWallDecorator(IMazeElement baseElement) : base(baseElement) { }

        public override bool IsPassable => false;
        public override ElementType Type => ElementType.Wall;
    }

    public class DestroyedWallDecorator : MazeElementDecorator
    {
        public DestroyedWallDecorator(IMazeElement baseElement) : base(baseElement) { }

        public override bool IsPassable => true;
        public override ElementType Type => ElementType.Empty;
    }

    public abstract class PrizeFactory
    {
        public abstract Prize CreatePrize();
    }

    public class SpeedUpPrizeFactory : PrizeFactory
    {
        public override Prize CreatePrize() => new Prize { PrizeType = PrizeType.SpeedUp };
    }

    public class SpeedDownPrizeFactory : PrizeFactory
    {
        public override Prize CreatePrize() => new Prize { PrizeType = PrizeType.SpeedDown };
    }

    public class WallActionPrizeFactory : PrizeFactory
    {
        public override Prize CreatePrize() => new Prize { PrizeType = PrizeType.WallAction };
    }

    public class Player
    {
        public int Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Speed { get; set; } = 4.0f;
        public int Score { get; set; }
        
        public int WallCharges { get; set; } 
        
        public float DirX { get; set; } = 0;
        public float DirY { get; set; } = 1;
    }

    public class Maze
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public IMazeElement[,] Grid { get; set; }

        public Maze(int width, int height)
        {
            Width = width;
            Height = height;
            Grid = new IMazeElement[width, height];
        }
    }

    public class GameState
    {
        public Maze Level { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public int TotalTreasures { get; set; }
        public bool IsGameOver { get; set; }

        public GameState()
        {
            Player1 = new Player { Id = 1, X = 1, Y = 1 };
            Player2 = new Player { Id = 2, X = 1, Y = 2 };
        }
    }
}