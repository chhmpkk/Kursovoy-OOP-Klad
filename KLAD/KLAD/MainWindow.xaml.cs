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
        private GameState _gameState = null!;
        private GameRenderer _renderer = null!;
        private HashSet<Key> _pressedKeys = new HashSet<Key>();//для сбора нажатий( когда отжим-удл, когда нажм+доб)
        private GameSettings _settings = new GameSettings();
        private bool _isGameRunning = false;

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

        private void UpdateUIStatus()
        {
            if (_gameState == null) return;//запущена ли игра(оооооо как всё запущеноооо)

            int p1SpeedPct = (int)(_gameState.Player1.Speed / 4.0f * 100);//отображ
            int p2SpeedPct = (int)(_gameState.Player2.Speed / 4.0f * 100);//отображ
            //инф
            TxtTotalTreasures.Text = $"Сокровищ на карте: {_gameState.TotalTreasures}";
            TxtP1Status.Text = $"Скор: {p1SpeedPct}%, Заряды: {_gameState.Player1.WallCharges}, Счет: {_gameState.Player1.Score}";
            TxtP2Status.Text = $"Скор: {p2SpeedPct}%, Заряды: {_gameState.Player2.WallCharges}, Счет: {_gameState.Player2.Score}";
        }

        private void SpawnTreasures(int targetCount)
        {
            Random rnd = new Random();
            int spawned = 0;
            int maxAttempts = targetCount * 10;//защита от вечного цикла
            int attempts = 0;

            while (spawned < targetCount && attempts < maxAttempts)
            {
                attempts++;
                int x = rnd.Next(1, _gameState.Level.Width - 1);//не появ на стенках
                int y = rnd.Next(1, _gameState.Level.Height - 1);//не появ на стенках

                if (_gameState.Level.Grid[x, y].Type == ElementType.Empty)//пусто ли
                {
                    if ((x == 1 && y == 1) || (x == _gameState.Level.Width - 2 && y == _gameState.Level.Height - 2)) continue;//защита от спавна 

                    _gameState.Level.Grid[x, y] = new Treasure();//спавн
                    spawned++;//счётчик
                }
            }
            _gameState.TotalTreasures = spawned;//кол-во для победы
        }

        private void OpenTkControl_OnReady()//1 раз когда opengl готов к работе
        {
            GL.Enable(EnableCap.Blend);//прозрачность
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);//смешивание цветов для прозрачности
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);//серый цвет фона
            
            _renderer = new GameRenderer();//объект который всё отрисовывает на экране
        }

        private void OpenTkControl_OnRender(TimeSpan delta)//delta -скок времени прошло с пред. кадра
        {
            if (_isGameRunning)
            {
                UpdateGameLogic((float)delta.TotalSeconds);//рачёт логики перемещ
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);//старый кадр на новый, иначе мазня
            
            if (_renderer != null && _gameState != null && _gameState.Level != null)
            {
                _renderer.RenderState(_gameState);//команда для отрисовки текущего состояния
            }
        }
        
        private void UpdateGameLogic(float deltaTime)
        {
            if (_gameState == null || _gameState.IsGameOver) return;

            MovePlayer(_gameState.Player1, Key.W, Key.S, Key.A, Key.D, deltaTime);
            MovePlayer(_gameState.Player2, Key.Up, Key.Down, Key.Left, Key.Right, deltaTime);
            
            UpdateUIStatus();
        }

        private void StartNewGame()
        {
            _gameState = new GameState();
            _pressedKeys.Clear();

            string mapsDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Maps");
            string[] mapFiles = System.IO.Directory.Exists(mapsDir) ? System.IO.Directory.GetFiles(mapsDir, "*.bmp") : new string[0];
            
            if (mapFiles.Length > 0)
            {
                Random rnd = new Random();
                string selectedMap = mapFiles[rnd.Next(mapFiles.Length)];
                var loader = new MazeLoader();
                _gameState.Level = loader.LoadFromBmp(selectedMap);
            }
            else
            {
                MessageBox.Show("Файлы карт (*.bmp) не найдены в папке Maps!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _gameState = null!;
                return;
            }

            int w = _gameState.Level.Width;
            int h = _gameState.Level.Height;

            _gameState.Player1.X = 1; _gameState.Player1.Y = 1;
            _gameState.Player2.X = w - 2; _gameState.Player2.Y = h - 2;
            _gameState.Level.Grid[1, 1] = new EmptySpace();
            _gameState.Level.Grid[w - 2, h - 2] = new EmptySpace();

            SpawnTreasures(_settings.TargetTreasures);
            
            var spawner = new PrizeSpawner();
            spawner.SpawnPrizes(_gameState.Level, _settings.TargetPrizes);

            _isGameRunning = true;
            HudBorder.Visibility = Visibility.Visible;
            MainMenuGrid.Visibility = Visibility.Collapsed;
            SettingsMenuGrid.Visibility = Visibility.Collapsed;
            HelpMenuGrid.Visibility = Visibility.Collapsed;
            GameOverGrid.Visibility = Visibility.Collapsed;
            
            UpdateUIStatus();
        }


        private void MovePlayer(Player p, Key up, Key down, Key left, Key right, float dt)//перемещ
        {
            float dx = 0;
            float dy = 0;

            if (_pressedKeys.Contains(up)) dy -= 1;
            if (_pressedKeys.Contains(down)) dy += 1;
            if (_pressedKeys.Contains(left)) dx -= 1;
            if (_pressedKeys.Contains(right)) dx += 1;

            if (dx != 0 || dy != 0)
            {
                p.DirX = dx;//сох направ
                p.DirY = dy;

                float newX = p.X + dx * p.Speed * dt;//формула движ(с deltatime для оптим)
                float newY = p.Y + dy * p.Speed * dt;//формула движ(с deltatime для оптим)

                if (CanMoveTo(newX, newY))
                {
                    p.X = newX;//новая коорда
                    p.Y = newY;//новая коорда
                    CheckCellInteractions(p); 
                }
            }
        }

        private bool CanMoveTo(float x, float y)
        {
            float cx = x + 0.5f;//вырав по центру клетки лабиринта 
            float cy = y + 0.5f;//вырав по центру клетки лабиринта 

            float radius = 0.2f;//радиус зазора спрайта и стены
            //выч коорд 4ёх углов игрока 
            float[] checkX = { cx - radius, cx + radius, cx - radius, cx + radius };
            float[] checkY = { cy - radius, cy - radius, cy + radius, cy + radius };

            for (int i = 0; i < 4; i++)
            {
                //дробную часть отсекаем 
                int gridX = (int)Math.Floor(checkX[i]);
                int gridY = (int)Math.Floor(checkY[i]);
                //проверка границ мира
                if (gridX < 0 || gridX >= _gameState.Level.Width || gridY < 0 || gridY >= _gameState.Level.Height)
                    return false;
                //обращение к объекту в массиве через интерфейс
                var element = _gameState.Level.Grid[gridX, gridY];
                if (element != null && !element.IsPassable)//хотя бы 1 угл упёрся - стоим
                    return false;
            }

            return true;//если всё ок всё ок
        }

        private void CheckCellInteractions(Player p)
        {
            //находим клетку в которой стоит игрок 
            int gridX = (int)Math.Floor(p.X + 0.5f);
            int gridY = (int)Math.Floor(p.Y + 0.5f);

            if (gridX >= 0 && gridX < _gameState.Level.Width && gridY >= 0 && gridY < _gameState.Level.Height)
            {
                //берём объект из массива
                var element = _gameState.Level.Grid[gridX, gridY];
                if (element is Treasure)//сокр
                {
                    p.Score++; 
                    _gameState.Level.Grid[gridX, gridY] = new EmptySpace(); 
                    CheckWinCondition();
                }
                else if (element is Prize prize)//приз
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
            
            HudBorder.Visibility = Visibility.Collapsed;
            TbWinnerText.Text = message;
            TbWinnerText.Foreground = hasWinner ? System.Windows.Media.Brushes.Lime : System.Windows.Media.Brushes.Yellow;
            
            GameOverGrid.Visibility = Visibility.Visible;
        }

        private void ApplyPrize(Player p, Prize prize)
        {
            switch (prize.PrizeType)
            {
                case PrizeType.SpeedUp: 
                    p.Speed *= 1.5f; 
                    break;
                case PrizeType.SpeedDown: 
                    Player opponent = (p == _gameState.Player1) ? _gameState.Player2 : _gameState.Player1;
                    opponent.Speed *= 0.5f; 
                    break;
                case PrizeType.WallAction: 
                    p.WallCharges++; 
                    break;
            }
        }

        private void TryWallAction(Player p)
        {
            if (p.WallCharges <= 0 || !_isGameRunning || _gameState.IsGameOver) return;//услвовия 

            int targetX = (int)Math.Floor(p.X + 0.5f + (p.DirX > 0 ? 1 : p.DirX < 0 ? -1 : 0));//нахождение на какую клетку 
            int targetY = (int)Math.Floor(p.Y + 0.5f + (p.DirY > 0 ? 1 : p.DirY < 0 ? -1 : 0));//нахождение на какую клетку 

            if (targetX >= 0 && targetX < _gameState.Level.Width && targetY >= 0 && targetY < _gameState.Level.Height)//проверка границ
            {
                var targetElement = _gameState.Level.Grid[targetX, targetY];//получ клетки

                if (targetElement.Type == ElementType.Empty)//замены
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
            
            _pressedKeys.Add(e.Key);//добавляем в коллекцию

            if (e.Key == Key.Space)
                TryWallAction(_gameState.Player1);
            else if (e.Key == Key.Enter)
                TryWallAction(_gameState.Player2);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            _pressedKeys.Remove(e.Key);//удаляем из таблицы
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
            HudBorder.Visibility = Visibility.Collapsed;
            MainMenuGrid.Visibility = Visibility.Visible;
        }
        //очистка видеопамять при окончании игры, избежания утечки памяти 
        protected override void OnClosed(EventArgs e)
        {
            _renderer?.Dispose();
            base.OnClosed(e);
        }
    }
}
