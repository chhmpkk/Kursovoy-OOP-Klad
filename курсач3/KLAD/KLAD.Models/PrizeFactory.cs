namespace KLAD.Models
{
    public abstract class PrizeFactory
    {
        /// <summary>
        /// Создает и возвращает новый приз.
        /// </summary>
        public abstract Prize CreatePrize();
    }
}

