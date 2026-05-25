using System;
using System.Drawing;
using System.IO;
using KLAD.Models;

namespace KLAD.Logic
{
    public class MazeLoader
    {
        public Maze LoadFromBmp(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Map file not found: {filePath}");

            
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
        private IMazeElement GetElementFromColor(Color color)
        {
            
            if (color.R == 0 && color.G == 0 && color.B == 0) 
                return new Wall();
            else if (color.R == 255 && color.G == 255 && color.B == 0) 
                return new Treasure();
            else 
                return new EmptySpace();
        }
    }
}
