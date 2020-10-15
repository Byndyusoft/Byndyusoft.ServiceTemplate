namespace Byndyusoft.ServiceTemplate.Api
{
    using System.Linq;
    using System.Reflection;
    using System.Text.Json.Serialization;
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
    using OpenTracing.Contrib.NetCore.Internal;
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
                    .AddOpenTracing(builder => builder.ConfigureAspNetCore(options => options.Hosting.IgnorePatterns.Add(context => context.Items.Any())));

            services.Configure<GenericEventOptions>(options => options.IgnoreAll = true);

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

            services.AddOptions()
                    .Configure<DatabaseConnectionSettings>(Configuration.GetSection(nameof(DatabaseConnectionSettings)))
                    .Configure<S3Settings>(Configuration.GetSection(nameof(S3Settings)))
                    .Configure<RabbitSettings>(Configuration.GetSection(nameof(RabbitSettings)));

            services.AddHostedService<TemplateHostedService>();

            services.AddControllers()
                    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
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
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}