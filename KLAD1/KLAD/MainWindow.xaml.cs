using System.Windows;
using System.Windows.Input;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using KLAD.Models;
using KLAD.Rendering;
using KLAD.Logic;

namespace KLAD
{
    public partial class MainWindow : Window
    {
        private GameState _gameState;
        private GameRenderer _renderer;
        private HashSet<Key> _pressedKeys = new HashSet<Key>();

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize basic game state
            _gameState = new GameState();
            
            try
            {
                var loader = new MazeLoader();
                string mapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Maps", "set.bmp");
                _gameState.Level = loader.LoadFromBmp(mapPath);
                
                // Спавним сокровища (если их нет на карте, добавим ровно 15 штук)
                SpawnTreasuresIfNeed(15);

                // Спавним 10 случайных призов после загрузки лабиринта
                var spawner = new PrizeSpawner();
                spawner.SpawnPrizes(_gameState.Level, 10);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load map: " + ex.Message);
                _gameState.Level = new Maze(20, 15); // Fallback mock maze
                for (int x = 0; x < 20; x++)
                    for (int y = 0; y < 15; y++)
                        _gameState.Level.Grid[x,y] = new EmptySpace();
                SpawnTreasuresIfNeed(15);
            }
            
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 3,
                RenderContinuously = true
            };
            OpenTkControl.Start(settings);

            this.KeyDown += Window_KeyDown;
            this.KeyUp += Window_KeyUp;
        }

        private void SpawnTreasuresIfNeed(int targetCount)
        {
            int currentTreasures = 0;
            // Считаем сколько уже есть
            for (int x = 0; x < _gameState.Level.Width; x++)
            {
                for (int y = 0; y < _gameState.Level.Height; y++)
                {
                    if (_gameState.Level.Grid[x, y] is Treasure)
                        currentTreasures++;
                }
            }

            int toSpawn = targetCount - currentTreasures;
            if (toSpawn > 0)
            {
                Random rnd = new Random();
                int spawned = 0;
                int maxAttempts = toSpawn * 10;
                int attempts = 0;

                while (spawned < toSpawn && attempts < maxAttempts)
                {
                    attempts++;
                    int x = rnd.Next(_gameState.Level.Width);
                    int y = rnd.Next(_gameState.Level.Height);

                    if (_gameState.Level.Grid[x, y].Type == ElementType.Empty)
                    {
                        _gameState.Level.Grid[x, y] = new Treasure();
                        spawned++;
                    }
                }
                _gameState.TotalTreasures = currentTreasures + spawned;
            }
            else
            {
                _gameState.TotalTreasures = currentTreasures;
            }
        }

        private void OpenTkControl_OnReady()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            
            _renderer = new GameRenderer();
        }

        private void OpenTkControl_OnRender(TimeSpan delta)
        {
            UpdateGameLogic((float)delta.TotalSeconds);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            if (_renderer != null && _gameState != null)
            {
                _renderer.RenderState(_gameState);
            }
        }

        private void UpdateGameLogic(float deltaTime)
        {
            if (_gameState.IsGameOver) return;

            MovePlayer(_gameState.Player1, Key.W, Key.S, Key.A, Key.D, deltaTime);
            MovePlayer(_gameState.Player2, Key.Up, Key.Down, Key.Left, Key.Right, deltaTime);
        }

        private void MovePlayer(Player p, Key up, Key down, Key left, Key right, float dt)
        {
            float dx = 0;
            float dy = 0;

            if (_pressedKeys.Contains(up)) dy -= 1;
            if (_pressedKeys.Contains(down)) dy += 1;
            if (_pressedKeys.Contains(left)) dx -= 1;
            if (_pressedKeys.Contains(right)) dx += 1;

            if (dx != 0 || dy != 0)
            {
                // Запоминаем направление взгляда
                p.DirX = dx;
                p.DirY = dy;

                float newX = p.X + dx * p.Speed * dt;
                float newY = p.Y + dy * p.Speed * dt;

                // Проверка столкновений со стенами (упрощенная AABB)
                if (CanMoveTo(newX, newY))
                {
                    p.X = newX;
                    p.Y = newY;
                    CheckCellInteractions(p);
                }
            }
        }

        private bool CanMoveTo(float x, float y)
        {
            // Упрощенная проверка центра игрока
            int gridX = (int)Math.Round(x);
            int gridY = (int)Math.Round(y);

            if (gridX < 0 || gridX >= _gameState.Level.Width || gridY < 0 || gridY >= _gameState.Level.Height)
                return false;

            var element = _gameState.Level.Grid[gridX, gridY];
            if (element == null) return true;

            return element.IsPassable;
        }

        private void CheckCellInteractions(Player p)
        {
            int gridX = (int)Math.Round(p.X);
            int gridY = (int)Math.Round(p.Y);

            if (gridX >= 0 && gridX < _gameState.Level.Width && gridY >= 0 && gridY < _gameState.Level.Height)
            {
                var element = _gameState.Level.Grid[gridX, gridY];
                if (element is Treasure)
                {
                    p.Score++;
                    _gameState.Level.Grid[gridX, gridY] = new EmptySpace(); // Съедаем
                    CheckWinCondition();
                }
                else if (element is Prize prize)
                {
                    ApplyPrize(p, prize);
                    _gameState.Level.Grid[gridX, gridY] = new EmptySpace(); // Съедаем
                }
            }
        }

        private void CheckWinCondition()
        {
            int winScore = (_gameState.TotalTreasures / 2) + 1; // Большинство

            if (_gameState.Player1.Score >= winScore)
            {
                _gameState.IsGameOver = true;
                MessageBox.Show("Игрок 1 (WASD) победил!", "Игра окончена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (_gameState.Player2.Score >= winScore)
            {
                _gameState.IsGameOver = true;
                MessageBox.Show("Игрок 2 (Стрелочки) победил!", "Игра окончена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ApplyPrize(Player p, Prize prize)
        {
            switch (prize.PrizeType)
            {
                case PrizeType.SpeedUp: p.Speed *= 1.5f; break;
                case PrizeType.SpeedDown: p.Speed *= 0.5f; break;
                case PrizeType.WallAction: p.WallCharges++; break;
            }
        }

        private void TryWallAction(Player p)
        {
            if (p.WallCharges <= 0 || _gameState.IsGameOver) return;

            // Определяем клетку перед игроком на основе его направления
            int targetX = (int)Math.Round(p.X + (p.DirX == 0 ? 0 : Math.Sign(p.DirX)));
            int targetY = (int)Math.Round(p.Y + (p.DirY == 0 ? 0 : Math.Sign(p.DirY)));

            if (targetX >= 0 && targetX < _gameState.Level.Width && targetY >= 0 && targetY < _gameState.Level.Height)
            {
                var targetElement = _gameState.Level.Grid[targetX, targetY];

                if (targetElement.Type == ElementType.Empty)
                {
                    // Строим временную стену (Декоратор над пустым пространством)
                    _gameState.Level.Grid[targetX, targetY] = new TemporaryWallDecorator(targetElement);
                    p.WallCharges--;
                }
                else if (targetElement is TemporaryWallDecorator || targetElement.Type == ElementType.Wall)
                {
                    // Ломаем стену (Декоратор над стеной)
                    _gameState.Level.Grid[targetX, targetY] = new DestroyedWallDecorator(targetElement);
                    p.WallCharges--;
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            _pressedKeys.Add(e.Key);

            if (e.Key == Key.Space)
                TryWallAction(_gameState.Player1);
            else if (e.Key == Key.Enter)
                TryWallAction(_gameState.Player2);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            _pressedKeys.Remove(e.Key);
        }
        
        protected override void OnClosed(EventArgs e)
        {
            _renderer?.Dispose();
            base.OnClosed(e);
        }
    }
}