namespace Byndyusoft.ServiceTemplate.Domain.Services.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDocumentStorageService
    {
        /// <summary>
        ///     Сохранение документа в хранилище
        /// </summary>
        /// <param name="documentIdentifier"></param>
        /// <param name="bytes"></param>
        /// <param name="cancellationToken"></param>
        Task Upload(string documentIdentifier, byte[] bytes, CancellationToken cancellationToken);

        /// <summary>
        ///     Скачивание документа из хранилища
        /// </summary>
        /// <param name="documentIdentifier"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<byte[]> Download(string documentIdentifier, CancellationToken cancellationToken);

        /// <summary>
        ///     Генерация уникального идентификатор файла и оригинальнозо названия файла
        /// </summary>
        /// <param name="originalFileName"></param>
        /// <returns></returns>
        string GenerateIdentifier(string? originalFileName);

        /// <summary>
        ///     Проверяем, существует ли объект в хранилище
        /// </summary>
        /// <param name="documentIdentifier">дентификатор файла</param>
        Task<bool> Exists(string documentIdentifier);

        /// <summary>
        ///     Получение общедоступной ссылки на документ
        /// </summary>
        /// <param name="documentIdentifier"></param>
        /// <returns></returns>
        string GetLink(string documentIdentifier);

        /// <summary>
        ///     Удаление документа из хранилища
        /// </summary>
        /// <param name="documentIdentifier">идентификатор документа</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Delete(string documentIdentifier, CancellationToken cancellationToken);
    }
}