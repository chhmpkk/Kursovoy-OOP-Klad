namespace KLAD.Models
{
    public class Maze
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public IMazeElement[,] Grid { get; set; }

        /// <summary>
        /// Конструктор лабиринта.
        /// </summary>
        public Maze(int width, int height)
        {
            Width = width;
            Height = height;
            Grid = new IMazeElement[width, height];
        }
    }
}

