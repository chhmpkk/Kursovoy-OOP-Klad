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
        private GameSettings _settings = new GameSettings();
        private bool _isGameRunning = false;

        /// <summary>
        /// Конструктор основного окна. Настраивает OpenGL Control.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            var openTkSettings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 3,
                RenderContinuously = true
            };
            OpenTkControl.Start(openTkSettings);

            this.KeyDown += Window_KeyDown;
            this.KeyUp += Window_KeyUp;
        }

        /// <summary>
        /// Запускает новую игру, генерирует лабиринт и ставит игроков в начальные позиции.
        /// </summary>
        private void StartNewGame()
        {
            _gameState = new GameState();
            _pressedKeys.Clear();
            
            _gameState.Level = new Maze(20, 15);
            Random rnd = new Random();
            
            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 15; y++)
                {
                    if (x == 0 || x == 19 || y == 0 || y == 14)
                    {
                        _gameState.Level.Grid[x, y] = new Wall();
                    }
                    else if (rnd.Next(100) < 20)
                    {
                        _gameState.Level.Grid[x, y] = new Wall();
                    }
                    else
                    {
                        _gameState.Level.Grid[x, y] = new EmptySpace();
                    }
                }
            }

            _gameState.Level.Grid[1, 1] = new EmptySpace();
            _gameState.Level.Grid[1, 2] = new EmptySpace();
            _gameState.Player1.X = 1; _gameState.Player1.Y = 1;
            _gameState.Player2.X = 1; _gameState.Player2.Y = 2;

            SpawnTreasures(_settings.TargetTreasures);
            
            var spawner = new PrizeSpawner();
            spawner.SpawnPrizes(_gameState.Level, _settings.TargetPrizes);

            _isGameRunning = true;
            MainMenuGrid.Visibility = Visibility.Collapsed;
            SettingsMenuGrid.Visibility = Visibility.Collapsed;
            HelpMenuGrid.Visibility = Visibility.Collapsed;
            GameOverGrid.Visibility = Visibility.Collapsed;
            
            UpdateUIStatus();
        }

        private void UpdateUIStatus()
        {
            if (_gameState == null) return;

            // Расчет скорости в процентах (базовая скорость 4.0)
            int p1SpeedPct = (int)(_gameState.Player1.Speed / 4.0f * 100);
            int p2SpeedPct = (int)(_gameState.Player2.Speed / 4.0f * 100);

            TxtTotalTreasures.Text = $"Сокровищ на карте: {_gameState.TotalTreasures}";
            TxtP1Status.Text = $"Скор: {p1SpeedPct}%, Заряды: {_gameState.Player1.WallCharges}, Счет: {_gameState.Player1.Score}";
            TxtP2Status.Text = $"Скор: {p2SpeedPct}%, Заряды: {_gameState.Player2.WallCharges}, Счет: {_gameState.Player2.Score}";
        }

        private void SpawnTreasures(int targetCount)
        {
            Random rnd = new Random();
            int spawned = 0;
            int maxAttempts = targetCount * 10;
            int attempts = 0;

            while (spawned < targetCount && attempts < maxAttempts)
            {
                attempts++;
                int x = rnd.Next(1, _gameState.Level.Width - 1);
                int y = rnd.Next(1, _gameState.Level.Height - 1);

                if (_gameState.Level.Grid[x, y].Type == ElementType.Empty)
                {
                    if ((x == 1 && y == 1) || (x == 1 && y == 2)) continue;

                    _gameState.Level.Grid[x, y] = new Treasure();
                    spawned++;
                }
            }
            _gameState.TotalTreasures = spawned;
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
            if (_isGameRunning)
            {
                UpdateGameLogic((float)delta.TotalSeconds);
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            if (_renderer != null && _gameState != null && _gameState.Level != null)
            {
                _renderer.RenderState(_gameState);
            }
        }

        private void UpdateGameLogic(float deltaTime)
        {
            if (_gameState == null || _gameState.IsGameOver) return;

            MovePlayer(_gameState.Player1, Key.W, Key.S, Key.A, Key.D, deltaTime);
            MovePlayer(_gameState.Player2, Key.Up, Key.Down, Key.Left, Key.Right, deltaTime);
            
            // Обновляем UI статуса на каждом кадре, чтобы изменения были мгновенными
            UpdateUIStatus();
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
                p.DirX = dx;
                p.DirY = dy;

                float newX = p.X + dx * p.Speed * dt;
                float newY = p.Y + dy * p.Speed * dt;

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
                    _gameState.Level.Grid[gridX, gridY] = new EmptySpace();
                    CheckWinCondition();
                }
                else if (element is Prize prize)
                {
                    ApplyPrize(p, prize);
                    _gameState.Level.Grid[gridX, gridY] = new EmptySpace();
                }
            }
        }

        private void CheckWinCondition()
        {
            int winScore = (_gameState.TotalTreasures / 2) + 1;

            if (_gameState.Player1.Score >= winScore)
            {
                ShowGameOver("Победил Игрок 1 (WASD)!", true);
            }
            else if (_gameState.Player2.Score >= winScore)
            {
                ShowGameOver("Победил Игрок 2 (Стрелочки)!", true);
            }
            else if (_gameState.Player1.Score + _gameState.Player2.Score == _gameState.TotalTreasures)
            {
                if (_gameState.Player1.Score == _gameState.Player2.Score)
                {
                    ShowGameOver("Ничья! Сокровища кончились.", false);
                }
            }
        }

        private void ShowGameOver(string message, bool hasWinner)
        {
            _gameState.IsGameOver = true;
            _isGameRunning = false;
            
            TbWinnerText.Text = message;
            TbWinnerText.Foreground = hasWinner ? System.Windows.Media.Brushes.Lime : System.Windows.Media.Brushes.Yellow;
            
            GameOverGrid.Visibility = Visibility.Visible;
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
            if (p.WallCharges <= 0 || !_isGameRunning || _gameState.IsGameOver) return;

            int targetX = (int)Math.Round(p.X + (p.DirX == 0 ? 0 : Math.Sign(p.DirX)));
            int targetY = (int)Math.Round(p.Y + (p.DirY == 0 ? 0 : Math.Sign(p.DirY)));

            if (targetX >= 0 && targetX < _gameState.Level.Width && targetY >= 0 && targetY < _gameState.Level.Height)
            {
                var targetElement = _gameState.Level.Grid[targetX, targetY];

                if (targetElement.Type == ElementType.Empty)
                {
                    _gameState.Level.Grid[targetX, targetY] = new TemporaryWallDecorator(targetElement);
                    p.WallCharges--;
                }
                else if (targetElement is TemporaryWallDecorator || targetElement.Type == ElementType.Wall)
                {
                    _gameState.Level.Grid[targetX, targetY] = new DestroyedWallDecorator(targetElement);
                    p.WallCharges--;
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isGameRunning) return;
            
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

        private void BtnStartGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            MainMenuGrid.Visibility = Visibility.Collapsed;
            SettingsMenuGrid.Visibility = Visibility.Visible;
            
            TbTreasures.Text = _settings.TargetTreasures.ToString();
            TbPrizes.Text = _settings.TargetPrizes.ToString();
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            MainMenuGrid.Visibility = Visibility.Collapsed;
            HelpMenuGrid.Visibility = Visibility.Visible;
        }

        private void BtnBackFromHelp_Click(object sender, RoutedEventArgs e)
        {
            HelpMenuGrid.Visibility = Visibility.Collapsed;
            MainMenuGrid.Visibility = Visibility.Visible;
        }

        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TbTreasures.Text, out int t) && t > 0) _settings.TargetTreasures = t;
            if (int.TryParse(TbPrizes.Text, out int p) && p >= 0) _settings.TargetPrizes = p;
            
            SettingsMenuGrid.Visibility = Visibility.Collapsed;
            MainMenuGrid.Visibility = Visibility.Visible;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnBackToMenu_Click(object sender, RoutedEventArgs e)
        {
            GameOverGrid.Visibility = Visibility.Collapsed;
            MainMenuGrid.Visibility = Visibility.Visible;
        }
        
        protected override void OnClosed(EventArgs e)
        {
            _renderer?.Dispose();
            base.OnClosed(e);
        }
    }
}
