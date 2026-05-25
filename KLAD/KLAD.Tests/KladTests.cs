using Xunit;
using KLAD.Models;
using KLAD.Logic;
using System.Linq;
using System.Collections.Generic;

namespace KLAD.Tests
{
    public class KladTests
    {
        [Fact]
        public void Maze_Initialization_SetsCorrectDimensions()
        {
            var maze = new Maze(10, 20);
            Assert.Equal(10, maze.Width);
            Assert.Equal(20, maze.Height);
            Assert.NotNull(maze.Grid);
            Assert.Equal(10, maze.Grid.GetLength(0));
            Assert.Equal(20, maze.Grid.GetLength(1));
        }

        [Fact]
        public void Player_InitialValues_AreCorrect()
        {
            var player = new Player();
            Assert.Equal(4.0f, player.Speed);
            Assert.Equal(0, player.Score);
            Assert.Equal(0, player.DirX);
            Assert.Equal(1, player.DirY);
        }

        [Fact]
        public void Player_SetProperties_ValuesUpdated()
        {
            var player = new Player { X = 5, Y = 10, Score = 100, Speed = 6.0f };
            Assert.Equal(5, player.X);
            Assert.Equal(10, player.Y);
            Assert.Equal(100, player.Score);
            Assert.Equal(6.0f, player.Speed);
        }

        [Fact]
        public void GameState_Constructor_InitializesPlayers()
        {
            var gameState = new GameState();
            Assert.NotNull(gameState.Player1);
            Assert.NotNull(gameState.Player2);
            Assert.Equal(1, gameState.Player1.X);
            Assert.Equal(1, gameState.Player1.Y);
            Assert.Equal(1, gameState.Player2.X);
            Assert.Equal(2, gameState.Player2.Y);
        }

        [Fact]
        public void Wall_Properties_AreCorrect()
        {
            var wall = new Wall();
            Assert.Equal(ElementType.Wall, wall.Type);
            Assert.False(wall.IsPassable);
        }

        [Fact]
        public void Treasure_Properties_AreCorrect()
        {
            var treasure = new Treasure();
            Assert.Equal(ElementType.Treasure, treasure.Type);
            Assert.True(treasure.IsPassable);
        }

        [Fact]
        public void Prize_Properties_AreCorrect()
        {
            var prize = new Prize { PrizeType = PrizeType.SpeedUp };
            Assert.Equal(ElementType.Prize, prize.Type);
            Assert.True(prize.IsPassable);
            Assert.Equal(PrizeType.SpeedUp, prize.PrizeType);
        }

        [Fact]
        public void SpeedUpPrizeFactory_CreatesSpeedUpPrize()
        {
            var factory = new SpeedUpPrizeFactory();
            var prize = factory.CreatePrize();
            Assert.Equal(PrizeType.SpeedUp, prize.PrizeType);
        }

        [Fact]
        public void SpeedDownPrizeFactory_CreatesSpeedDownPrize()
        {
            var factory = new SpeedDownPrizeFactory();
            var prize = factory.CreatePrize();
            Assert.Equal(PrizeType.SpeedDown, prize.PrizeType);
        }

        [Fact]
        public void WallActionPrizeFactory_CreatesWallActionPrize()
        {
            var factory = new WallActionPrizeFactory();
            var prize = factory.CreatePrize();
            Assert.Equal(PrizeType.WallAction, prize.PrizeType);
        }

        [Fact]
        public void PrizeSpawner_SpawnsCorrectNumberOfPrizes()
        {
            var maze = new Maze(10, 10);
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                    maze.Grid[x, y] = new EmptySpace();

            var spawner = new PrizeSpawner();
            spawner.SpawnPrizes(maze, 5);

            int prizeCount = 0;
            foreach (var element in maze.Grid)
            {
                if (element is Prize) prizeCount++;
            }

            Assert.Equal(5, prizeCount);
        }

        [Fact]
        public void PrizeSpawner_DoesNotOverwriteWalls()
        {
            var maze = new Maze(3, 3);
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    maze.Grid[x, y] = new Wall();
            
            maze.Grid[1, 1] = new EmptySpace();

            var spawner = new PrizeSpawner();
            spawner.SpawnPrizes(maze, 5); 

            int prizeCount = 0;
            int wallCount = 0;
            foreach (var element in maze.Grid)
            {
                if (element is Prize) prizeCount++;
                if (element is Wall) wallCount++;
            }

            Assert.Equal(1, prizeCount);
            Assert.Equal(8, wallCount);
        }

        [Fact]
        public void TemporaryWallDecorator_Properties_AreCorrect()
        {
            var wall = new Wall();
            var decorator = new TemporaryWallDecorator(wall);
            Assert.Equal(ElementType.Wall, decorator.Type);
            Assert.False(decorator.IsPassable);
        }

        [Fact]
        public void DestroyedWallDecorator_Properties_AreCorrect()
        {
            var wall = new Wall();
            var decorator = new DestroyedWallDecorator(wall);
            Assert.Equal(ElementType.Empty, decorator.Type);
            Assert.True(decorator.IsPassable);
        }

        [Fact]
        public void Maze_SmallestPossible_Initializes()
        {
            var maze = new Maze(1, 1);
            Assert.Single(maze.Grid);
        }

        [Fact]
        public void Player_Movement_UpdatesPosition()
        {
            var player = new Player { X = 0, Y = 0 };
            player.X += 1.5f;
            player.Y -= 0.5f;
            Assert.Equal(1.5f, player.X);
            Assert.Equal(-0.5f, player.Y);
        }

        [Fact]
        public void GameState_TotalTreasures_CanBeSet()
        {
            var gameState = new GameState { TotalTreasures = 10 };
            Assert.Equal(10, gameState.TotalTreasures);
        }

        [Fact]
        public void PrizeType_Enum_HasExpectedValues()
        {
            Assert.Contains(PrizeType.SpeedUp, System.Enum.GetValues<PrizeType>());
            Assert.Contains(PrizeType.SpeedDown, System.Enum.GetValues<PrizeType>());
            Assert.Contains(PrizeType.WallAction, System.Enum.GetValues<PrizeType>());
        }

        [Fact]
        public void ElementType_Enum_HasExpectedValues()
        {
            Assert.Contains(ElementType.Empty, System.Enum.GetValues<ElementType>());
            Assert.Contains(ElementType.Wall, System.Enum.GetValues<ElementType>());
            Assert.Contains(ElementType.Treasure, System.Enum.GetValues<ElementType>());
            Assert.Contains(ElementType.Prize, System.Enum.GetValues<ElementType>());
        }

        [Fact]
        public void Player_WallCharges_CanBeModified()
        {
            var player = new Player { WallCharges = 3 };
            player.WallCharges--;
            Assert.Equal(2, player.WallCharges);
        }
    }
}
