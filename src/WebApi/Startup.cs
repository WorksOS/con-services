using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.FileAccess.Service.WebAPI
{
  public class Startup
  {
    private const string LoggerRepoName = "WebApi";
    private readonly bool _isDevEnv;
    private IServiceCollection _serviceCollection;

    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      env.ConfigureLog4Net("log4net.xml", LoggerRepoName);

      _isDevEnv = env.IsEnvironment("Development");

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    public IConfigurationRoot Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddLogging();

      //Configure CORS
      services.AddCors(options =>
      {
        options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
                  .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization",
                      "X-VisionLink-CustomerUid", "X-VisionLink-UserUid", "Cache-Control")
                  .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
      });

      services.AddSwaggerGen();

      services.ConfigureSwaggerGen(options =>
      {
        options.SingleApiVersion(new Info
        {
          Version = "v1",
          Title = "File Access API",
          TermsOfService = "None"
        });

        var moduleName = typeof(Startup).GetTypeInfo().Assembly.ManifestModule.Name;
        var assemblyName = moduleName.Substring(0, moduleName.LastIndexOf('.'));
        var path = _isDevEnv
          ? "bin/Debug/netcoreapp1.1/"
          : string.Empty;

        options.IncludeXmlComments(path + assemblyName + ".xml");
        options.IgnoreObsoleteProperties();
        options.DescribeAllEnumsAsStrings();

      });
      //Swagger documentation can be viewed with http://localhost:5000/swagger/ui/index.html   

      //Configure application services
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddSingleton<IFileRepository, FileRepository>();
      services.AddMvc();


      _serviceCollection = services;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(LoggerRepoName);

      app.UseExceptionTrap();
      //Enable CORS before TID so OPTIONS works without authentication
      app.UseCors("VSS");

      //app.UseResponseCaching();//Disable for now
      app.UseMvc();

      app.UseSwagger();
      app.UseSwaggerUi();
    }
  }
}