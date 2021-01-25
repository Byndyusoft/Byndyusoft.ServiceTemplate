namespace Byndyusoft.ServiceTemplate.Api
{
    using System.Reflection;
    using System.Text.Json.Serialization;
    using Amazon.S3;
    using Autofac;
    using Domain.Settings;
    using Extensions;
    using HostedServices;
    using Installers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using Swashbuckle.AspNetCore.SwaggerUI;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddJaeger(Configuration)
                    .AddOpenTracing();

            services.AddApiVersioning(options =>
                                          {
                                              options.DefaultApiVersion = ApiVersion.Default;
                                              options.AssumeDefaultVersionWhenUnspecified = true;
                                              options.ReportApiVersions = true;
                                          });

            services.AddVersionedApiExplorer(options =>
                                                 {
                                                     options.GroupNameFormat = "'v'VVV";
                                                     options.SubstituteApiVersionInUrl = true;
                                                 });
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen();

            var rabbitSettingsConfigurationSection = Configuration.GetSection(nameof(RabbitSettings));
            var s3SettingsConfigurationSection = Configuration.GetSection(nameof(S3Settings));
            var databaseConnectionSettingsConfigurationSection = Configuration.GetSection(nameof(DatabaseConnectionSettings));

            services.AddOptions()
                    .Configure<DatabaseConnectionSettings>(databaseConnectionSettingsConfigurationSection)
                    .Configure<S3Settings>(s3SettingsConfigurationSection)
                    .Configure<RabbitSettings>(rabbitSettingsConfigurationSection);

            services.AddHostedService<TemplateHostedService>();

            services.AddControllers()
                    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            var rabbitSettings = rabbitSettingsConfigurationSection.Get<RabbitSettings>();
            var s3Settings = s3SettingsConfigurationSection.Get<S3Settings>();
            var databaseConnectionSettings = databaseConnectionSettingsConfigurationSection.Get<DatabaseConnectionSettings>();

            services.AddHealthChecks()
                    .AddNpgSql(databaseConnectionSettings.ConnectionString)
                    .AddRabbitMQ(rabbitConnectionString: rabbitSettings.ConnectionString)
                    .AddS3(options =>
                               {
                                   options.AccessKey = s3Settings.AccessKey;
                                   options.BucketName = s3Settings.BucketName;
                                   options.SecretKey = s3Settings.SecretKey;
                                   options.S3Config = new AmazonS3Config
                                                          {
                                                              ServiceURL = s3Settings.ServiceUrl,
                                                              ForcePathStyle = true
                                                          };
                               });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterAssemblyModules(typeof(ServicesInstaller).GetTypeInfo().Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage()
                   .UseSwagger()
                   .UseSwaggerUI(options =>
                                     {
                                         foreach (var apiVersionDescription in apiVersionDescriptionProvider.ApiVersionDescriptions)
                                             options.SwaggerEndpoint($"/swagger/{apiVersionDescription.GroupName}/swagger.json", apiVersionDescription.GroupName.ToUpperInvariant());

                                         options.DisplayRequestDuration();
                                         options.DefaultModelRendering(ModelRendering.Model);
                                         options.DefaultModelExpandDepth(3);
                                     });
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
                                 {
                                     endpoints.MapControllers();
                                     endpoints.MapHealthChecks("/healthz");
                                 });
        }
    }
}