namespace Byndyusoft.ServiceTemplate.Api.Extensions
{
    using System;
    using System.Reflection;
    using Jaeger;
    using Jaeger.Reporters;
    using Jaeger.Samplers;
    using Jaeger.Senders;
    using Jaeger.Senders.Thrift;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using OpenTracing;
    using OpenTracing.Contrib.NetCore.Configuration;
    using OpenTracing.Contrib.NetCore.CoreFx;
    using OpenTracing.Util;

    public static class JaegerServiceCollectionExtensions
    {
        private static readonly string JaegerTraceUri = "/api/traces";
        private static readonly string ServiceStatusUri = "/api/status";
        private static readonly string MetricsUri = "/metrics";
        private static readonly string SwaggerUri = "/swagger";

        public static IServiceCollection AddJaeger(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<ITracer>(serviceProvider =>
                                               {
                                                   var serviceName = Assembly.GetEntryAssembly().GetName().Name;

                                                   var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                                                   Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory).RegisterSenderFactory<ThriftSenderFactory>();

                                                   var senderConfig = Configuration.SenderConfiguration.FromIConfiguration(loggerFactory, configuration.GetSection("Jaeger"));

                                                   var reporter = new RemoteReporter.Builder()
                                                                  .WithLoggerFactory(loggerFactory)
                                                                  .WithSender(senderConfig.GetSender())
                                                                  .Build();

                                                   var sampler = new ConstSampler(sample: true);

                                                   var tracer = new Tracer.Builder(serviceName)
                                                                .WithLoggerFactory(loggerFactory)
                                                                .WithExpandExceptionLogs()
                                                                .WithReporter(reporter)
                                                                .WithSampler(sampler)
                                                                .Build();

                                                   GlobalTracer.RegisterIfAbsent(tracer);

                                                   return tracer;
                                               });

            // Prevent endless loops when OpenTracing is tracking HTTP requests to Jaeger.
            services.Configure<HttpHandlerDiagnosticOptions>(options => options.IgnorePatterns.Add(request => request.RequestUri.AbsolutePath.EndsWith(JaegerTraceUri)));
            services.Configure<AspNetCoreDiagnosticOptions>(options =>
                                                                {
                                                                    options.Hosting.IgnorePatterns.Add(context => context.Request.Path.Value.EndsWith(ServiceStatusUri, StringComparison.InvariantCultureIgnoreCase));
                                                                    options.Hosting.IgnorePatterns.Add(context => context.Request.Path.Value.EndsWith(MetricsUri, StringComparison.InvariantCultureIgnoreCase));
                                                                    options.Hosting.IgnorePatterns.Add(context => context.Request.Path.Value.StartsWith(SwaggerUri, StringComparison.InvariantCultureIgnoreCase));
                                                                });

            return services;
        }
    }
}