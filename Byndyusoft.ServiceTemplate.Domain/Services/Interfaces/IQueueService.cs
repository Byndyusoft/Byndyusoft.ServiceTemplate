namespace Byndyusoft.ServiceTemplate.Domain.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EventArgs;

    public interface IQueueService
    {
        /// <summary>
        ///     Обработка вернувшегося сообщения. В параметрах передается ключ сообщения
        /// </summary>
        event Func<MessageReturnedEventArgs, Task> MessageReturned;

        /// <summary>
        ///     Инициализация работы очереди. Необходимо вызывать при старте приложения
        /// </summary>
        /// <returns></returns>
        Task Initialize();

        /// <summary>
        ///     Отправка сообщения.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        /// <param name="key">уникальный ключ сообщения, по которому его можно будет идентифицировать, если оно вернется</param>
        /// <param name="headers"></param>
        /// <returns></returns>
        Task PushDocument<TMessage>(TMessage message, string key, Dictionary<string, string>? headers = null);

        /// <summary>
        ///     Отправка сообщения об ошибке при обработке пакета
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="key">уникальный ключ сообщения, по которому его можно будет идентифицировать, если оно вернется</param>
        /// <param name="headers"></param>
        /// <returns></returns>
        Task PushError<TError>(TError errorMessage, string key, Dictionary<string, string>? headers = null);

        /// <summary>
        ///     Отправить сообщения из очереди с ошибками в обычную очередь
        /// </summary>
        Task ResendErrorMessages();

        /// <summary>
        ///     Подписаться на очередь асинхронно
        /// </summary>
        /// <param name="processMessage">Метод обработки документа</param>
        void SubscribeAsync<TMessage>(Func<TMessage, Task> processMessage) where TMessage : class;
    }
}