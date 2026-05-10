namespace KLAD.Models
{
    /// <summary>
    /// Базовый класс декоратора элементов лабиринта (Паттерн Decorator).
    /// </summary>
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
}

