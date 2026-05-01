using System;
using System.Collections.Generic;
using KLAD.Models;

namespace KLAD.Logic
{
    public class PrizeSpawner
    {
        private readonly Random _random = new Random();
        private readonly List<PrizeFactory> _factories;

        public PrizeSpawner()
        {
            // Регистрируем фабрики для каждого типа призов
            _factories = new List<PrizeFactory>
            {
                new SpeedUpPrizeFactory(),
                new SpeedDownPrizeFactory(),
                new WallActionPrizeFactory()
            };
        }

        public void SpawnPrizes(Maze maze, int numberOfPrizes)
        {
            int spawned = 0;
            int maxAttempts = numberOfPrizes * 10; // Защита от зацикливания
            int attempts = 0;

            while (spawned < numberOfPrizes && attempts < maxAttempts)
            {
                attempts++;
                
                int x = _random.Next(maze.Width);
                int y = _random.Next(maze.Height);

                // Спавним приз только в пустом пространстве
                if (maze.Grid[x, y] != null && maze.Grid[x, y].Type == ElementType.Empty)
                {
                    // Выбираем случайную фабрику
                    int factoryIndex = _random.Next(_factories.Count);
                    PrizeFactory factory = _factories[factoryIndex];
                    
                    // Создаем приз через фабричный метод и помещаем в лабиринт
                    maze.Grid[x, y] = factory.CreatePrize();
                    spawned++;
                }
            }
        }
    }
}