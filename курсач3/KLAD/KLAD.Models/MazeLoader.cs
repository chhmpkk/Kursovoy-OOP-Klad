using System;
using System.Drawing;
using System.IO;
using KLAD.Models;

namespace KLAD.Logic
{
    public class MazeLoader
    {
        /// <summary>
        /// Загружает лабиринт из изображения формата BMP.
        /// Каждый пиксель изображения конвертируется в соответствующий элемент лабиринта.
        /// </summary>
        /// <param name="filePath">Путь к файлу изображения.</param>
        /// <returns>Загруженный лабиринт (объект Maze).</returns>
        public Maze LoadFromBmp(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Map file not found: {filePath}");

            // Using System.Drawing.Common for simple BMP pixel parsing
            using (var bitmap = new Bitmap(filePath))
            {
                var maze = new Maze(bitmap.Width, bitmap.Height);

                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        var pixel = bitmap.GetPixel(x, y);
                        maze.Grid[x, y] = GetElementFromColor(pixel);
                    }
                }

                return maze;
            }
        }

        /// <summary>
        /// Преобразует цвет пикселя в элемент лабиринта.
        /// </summary>
        /// <param name="color">Цвет пикселя из изображения.</param>
        /// <returns>Соответствующий интерфейс IMazeElement (Стена, Сокровище или Пустота).</returns>
        private IMazeElement GetElementFromColor(Color color)
        {
            // Пример маппинга цветов. Black = Wall, White = Empty, Yellow = Treasure
            if (color.R == 0 && color.G == 0 && color.B == 0) // Black
                return new Wall();
            else if (color.R == 255 && color.G == 255 && color.B == 0) // Yellow
                return new Treasure();
            else // Default to Empty
                return new EmptySpace();
        }
    }
}