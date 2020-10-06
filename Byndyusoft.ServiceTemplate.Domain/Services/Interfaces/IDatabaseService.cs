namespace Byndyusoft.ServiceTemplate.Domain.Services.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Сервис работы с базой данных
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        ///     Пример запроса
        /// </summary>
        /// <returns>Результат запроса</returns>
        Task<int[]> SampleSelect();
    }
}