namespace Byndyusoft.ServiceTemplate.Domain.EventArgs
{
    /// <summary>
    ///     Аргументы события возврата сообщения
    /// </summary>
    public class MessageReturnedEventArgs
    {
        public MessageReturnedEventArgs(string messageKey)
        {
            MessageKey = messageKey;
        }

        /// <summary>
        ///     Ключ возвращенного сообщения
        /// </summary>
        public string MessageKey { get; }
    }
}