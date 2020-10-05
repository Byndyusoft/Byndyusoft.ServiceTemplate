namespace Byndyusoft.ServiceTemplate.Domain.Settings
{
    public class RabbitSettings
    {
        /// <summary>
        ///     Строка подключения к реббиту
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Иксчендж для отправки сообщений
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        ///     Ключ маршрутизации, по которому надо отправлять сообщения
        /// </summary>
        public string RoutingKey { get; set; }

        /// <summary>
        ///     Название очереди, которая используется для обработки входящих сообщений
        /// </summary>
        public string? IncomingQueue { get; set; }

        /// <summary>
        ///     Ключ маршрутизации, по которому надо отправлять ошибки для генерации уведомлений о прочтении с отказом
        /// </summary>
        public string ErrorRoutingKey { get; set; }
    }
}