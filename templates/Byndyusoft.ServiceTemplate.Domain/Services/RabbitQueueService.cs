namespace Byndyusoft.ServiceTemplate.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;
    using EasyNetQ;
    using EasyNetQ.SystemMessages;
    using EasyNetQ.Topology;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using OpenTracing;
    using OpenTracing.Propagation;
    using OpenTracing.Tag;
    using Settings;
    using MessageReturnedEventArgs = EventArgs.MessageReturnedEventArgs;

    public class RabbitQueueService : IQueueService
    {
        private const string MessageKeyHeader = "MessageKey";
        private readonly ILogger<RabbitQueueService> _logger;
        private readonly RabbitSettings _settings;
        private readonly ITracer _tracer;
        private IBus? _bus;
        private IExchange? _exchange;
        private bool _isInitialized;

        public RabbitQueueService(ILogger<RabbitQueueService> logger,
                                  IOptions<RabbitSettings> options,
                                  ITracer tracer)
        {
            _logger = logger;
            _tracer = tracer;
            _settings = options.Value;
        }

        public event Func<MessageReturnedEventArgs, Task>? MessageReturned;

        public async Task Initialize()
        {
            if (_isInitialized)
            {
                _logger.LogError("Already initialized");
                return;
            }

            _bus = RabbitHutch.CreateBus(_settings.ConnectionString, x => x.Register<IClusterHostSelectionStrategy<ConnectionFactoryInfo>, RandomClusterHostSelectionStrategy<ConnectionFactoryInfo>>());

            _bus.Advanced.MessageReturned += OnMessageReturned;

            _bus.Advanced.Conventions.ErrorQueueNamingConvention = info => $"{info.Exchange}.{info.RoutingKey}.error";
            _bus.Advanced.Conventions.ErrorExchangeNamingConvention = info => $"{info.Exchange}.error";

            _exchange = await _bus.Advanced.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Direct).ConfigureAwait(false);

            if (string.IsNullOrEmpty(_settings.RoutingKey) == false)
                await InitializeQueue(_bus, _exchange, _settings.RoutingKey).ConfigureAwait(false);

            if (string.IsNullOrEmpty(_settings.ErrorRoutingKey) == false)
                await InitializeQueue(_bus, _exchange, _settings.ErrorRoutingKey).ConfigureAwait(false);

            if (string.IsNullOrEmpty(_settings.IncomingQueue) == false)
                await _bus.Advanced.QueueDeclareAsync(_settings.IncomingQueue).ConfigureAwait(false);

            _isInitialized = true;
        }

        public async Task ResendErrorMessages()
        {
            if (_isInitialized == false)
                throw new InvalidOperationException("Initialize bus before use");

            var queue = await _bus.Advanced
                                  .QueueDeclareAsync($"{_settings.IncomingQueue}.error")
                                  .ConfigureAwait(false);

            var messageCount = _bus.Advanced.MessageCount(queue);

            while (messageCount > 0)
            {
                var getResult = _bus.Advanced.Get<Error>(queue);

                if (getResult.MessageAvailable)
                {
                    var error = getResult.Message.Body;
                    var headers = error.BasicProperties.Headers.ToDictionary(x => x.Key, x => x.Value.ToString());

                    var properties = new MessageProperties
                                         {
                                             ContentType = MediaTypeNames.Application.Json,
                                             DeliveryMode = 2
                                         };

                    foreach (var header in headers)
                        properties.Headers.Add(header.Key, header.Value);

                    //TODO: избавиться от рефлексии
                    var messageBodyType = Type.GetType(error.BasicProperties.Type);
                    var messageGenericType = typeof(Message<>).MakeGenericType(messageBodyType);
                    var package = new JsonSerializer().BytesToMessage(messageBodyType, Encoding.ASCII.GetBytes(error.Message));

                    var newMessage = Activator.CreateInstance(messageGenericType, package, properties) as IMessage;

                    //TODO включить паблиш конфермс
                    await _bus.Advanced
                              .PublishAsync(_exchange, error.RoutingKey, true, newMessage)
                              .ConfigureAwait(false);
                }
                else
                {
                    _logger.LogInformation("Message unavailable");
                }

                messageCount--;
            }
        }

        public async Task PushDocument<TMessage>(TMessage message, string key, Dictionary<string, string>? headers = null)
        {
            if (_isInitialized == false)
                throw new InvalidOperationException("Initialize bus before use");

            var span = _tracer.BuildSpan(nameof(PushDocument)).Start();

            var properties = new MessageProperties
                                 {
                                     ContentType = MediaTypeNames.Application.Json,
                                     DeliveryMode = 2
                                 };

            var carrier = new RabbitInjectAdapter(properties.Headers);

            _tracer.Inject(_tracer.ActiveSpan.Context, BuiltinFormats.HttpHeaders, carrier);

            properties.Headers.Add(MessageKeyHeader, key);

            if (headers != null)
                foreach (var header in headers)
                    properties.Headers.Add(header.Key, header.Value);

            //TODO включить паблиш конфермс
            await _bus.Advanced
                      .PublishAsync(_exchange, _settings.RoutingKey, true, new Message<TMessage>(message, properties))
                      .ConfigureAwait(false);

            span.Finish();
        }

        //TODO: Возможно, должно быть не здесь
        public async Task PushError<TError>(TError errorMessage, string key, Dictionary<string, string>? headers = null)
        {
            if (_isInitialized == false)
                throw new InvalidOperationException("Initialize bus before use");

            var span = _tracer.BuildSpan(nameof(PushError)).Start();

            var properties = new MessageProperties
                                 {
                                     ContentType = MediaTypeNames.Application.Json,
                                     DeliveryMode = 2
                                 };

            var carrier = new RabbitInjectAdapter(properties.Headers);

            _tracer.Inject(_tracer.ActiveSpan.Context, BuiltinFormats.HttpHeaders, carrier);

            properties.Headers.Add(MessageKeyHeader, key);

            if (headers != null)
                foreach (var header in headers)
                    properties.Headers.Add(header.Key, header.Value);

            //TODO включить паблиш конфермс
            await _bus.Advanced
                      .PublishAsync(_exchange, _settings.ErrorRoutingKey, true, new Message<TError>(errorMessage, properties))
                      .ConfigureAwait(false);

            span.Finish();
        }

        public void SubscribeAsync<TMessage>(Func<TMessage, Task> processMessage) where TMessage : class
        {
            if (string.IsNullOrEmpty(_settings.IncomingQueue))
                throw new NullReferenceException(nameof(_settings.IncomingQueue));

            _bus.Advanced.Consume<TMessage>(new Queue(_settings.IncomingQueue, false), (message, messageInfo) => OnMessage(message, processMessage));
        }

        /// <summary>
        ///     Обработка вернувшихся сообщений
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnMessageReturned(object sender, EasyNetQ.MessageReturnedEventArgs args)
        {
            var stringDictionary = args.MessageProperties.Headers.ToDictionary(x => x.Key, x => Encoding.UTF8.GetString((byte[]) x.Value));
            var textMapExtractAdapter = new TextMapExtractAdapter(stringDictionary);
            var spanContext = _tracer.Extract(BuiltinFormats.HttpHeaders, textMapExtractAdapter);

            using (_tracer.BuildSpan(nameof(OnMessageReturned)).AddReference(References.ChildOf, spanContext).StartActive(true))
            using (_logger.BeginScope(new[] {new KeyValuePair<string, object>(nameof(_tracer.ActiveSpan.Context.TraceId), _tracer.ActiveSpan.Context.TraceId)}))
            {
                _tracer.ActiveSpan.SetTag(Tags.Error, true);

                if (args.MessageProperties.Headers.TryGetValue(MessageKeyHeader, out var bytes) && bytes is byte[] value)
                {
                    var key = Encoding.UTF8.GetString(value);

                    _logger.LogError("Message returned {Exchange} {RoutingKey} reason {ReturnReason} {Key}",
                                     args.MessageReturnedInfo.Exchange,
                                     args.MessageReturnedInfo.RoutingKey,
                                     args.MessageReturnedInfo.ReturnReason,
                                     key);

                    MessageReturned?.Invoke(new MessageReturnedEventArgs(key));
                }
                else
                {
                    _logger.LogError("Can not get error message filename");
                }
            }
        }

        public void Dispose()
        {
            if (_bus != null)
            {
                _bus.Advanced.MessageReturned -= OnMessageReturned;
                _bus.Dispose();
            }

            _isInitialized = false;
        }

        private async Task OnMessage<TMessage>(IMessage<TMessage> message, Func<TMessage, Task> processMessage)
        {
            var stringDictionary = message.Properties.Headers.ToDictionary(x => x.Key, x => Encoding.UTF8.GetString((byte[]) x.Value));
            var textMapExtractAdapter = new TextMapExtractAdapter(stringDictionary);
            var spanContext = _tracer.Extract(BuiltinFormats.HttpHeaders, textMapExtractAdapter);

            using (_tracer.BuildSpan(nameof(OnMessage)).AddReference(References.FollowsFrom, spanContext).StartActive(true))
            using (_logger.BeginScope(new[] {new KeyValuePair<string, object>(nameof(_tracer.ActiveSpan.Context.TraceId), _tracer.ActiveSpan.Context.TraceId)}))
            {
                var tryCount = 0;

                //TODO надо фильтровать ошибки базы, с3 и реббита, но тогда цикл должен быть выше по абстракции
                while (true)
                    try
                    {
                        await processMessage(message.Body).ConfigureAwait(false);
                        break;
                    }
                    catch (Exception e)
                    {
                        tryCount++;
                        _logger.LogError(e, $"Process error, try count {tryCount}");

                        if (tryCount >= 5)
                            throw;
                    }
            }
        }

        private static async Task InitializeQueue(IBus bus, IExchange exchange, string routingKey)
        {
            var queue = await bus.Advanced.QueueDeclareAsync($"{exchange.Name}.{routingKey}").ConfigureAwait(false);

            await bus.Advanced.BindAsync(exchange, queue, routingKey).ConfigureAwait(false);
        }
    }
}