namespace Byndyusoft.ServiceTemplate.Api.Installers
{
    using Autofac;
    using DataAccess;
    using DataAccess.ConnectionFactories;
    using Domain.Services;
    using Domain.Services.Interfaces;

    public class ServicesInstaller : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NpgConnectionFactory>().As<IConnectionFactory>().SingleInstance();
            builder.RegisterType<DatabaseService>().As<IDatabaseService>().SingleInstance();
            builder.RegisterType<S3DocumentStorageService>().As<IDocumentStorageService>().SingleInstance();
            builder.RegisterType<RabbitQueueService>().As<IQueueService>().SingleInstance();
        }
    }
}