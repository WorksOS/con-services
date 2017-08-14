using System;
using Dapper;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.Productivity3D.Scheduler.WebAPI;
using VSS.Productivity3D.Scheduler.WebAPI.Utilities;

namespace VSS.Productivity3D.Scheduler.WebApi
{
  /// <summary>
  /// VSS.Productivity3D.Scheduler startup
  /// </summary>
  public class Startup
  {
    private const string loggerRepoName = "WebApi";
    IServiceCollection serviceCollection;
    private MySqlStorage storage = null;

    /// <summary>
    /// VSS.Productivity3D.Scheduler startup
    /// </summary>
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();
     
      env.ConfigureLog4Net("log4net.xml", loggerRepoName);

      Configuration = builder.Build();
    }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    /// <value>
    /// The configuration.
    /// </value>
    public IConfigurationRoot Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddLogging();

      //Configure CORS
      services.AddCors(options =>
      {
        options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
          .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization",
            "X-VisionLink-CustomerUID", "X-VisionLink-UserUid", "X-Jwt-Assertion", "X-VisionLink-ClearCache")
          .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
      });

      services.AddSingleton<IConfigurationStore, GenericConfiguration>();

      var hangfireConnectionString = ConnectionUtils.GetConnectionString("Scheduler");
      storage = new MySqlStorage(hangfireConnectionString,
        new MySqlStorageOptions
        {
          QueuePollInterval = TimeSpan.FromSeconds(15),
          JobExpirationCheckInterval = TimeSpan.FromHours(1),
          CountersAggregateInterval = TimeSpan.FromMinutes(5),
          PrepareSchemaIfNecessary = true,
          DashboardJobListLimit = 50000,
          TransactionTimeout = TimeSpan.FromMinutes(1),
        });

      services.AddHangfire(x => x.UseStorage(storage));
      serviceCollection = services;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    /// <summary>
    /// Configures the specified application.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="env">The env.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection.BuildServiceProvider();

      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      app.UseHangfireDashboard();
      app.UseHangfireServer();

      RecurringJob.AddOrUpdate(() => SomeJob(), Cron.Minutely);
      RecurringJob.AddOrUpdate(() => DatabaseCleanupJob(), Cron.Minutely);


      using (var server = new BackgroundJobServer(storage))
      {
        Console.WriteLine("Hangfire Server started. Press any key to exit...");
      }

      //app.UseExceptionTrap();
      app.UseCors("VSS");
      app.UseMvc();

    }


    public void SomeJob()
    {
      Console.WriteLine($"{DateTime.Now} {this.ToString()}: Recurring SomeJob completed successfully!");
    }

    public void DatabaseCleanupJob()
    {

      var dbConnection = ConnectionUtils.CreateFilterConnection();
      var filter = new Filter()
      {
        CustomerUid = Guid.NewGuid().ToString(),
        UserUid = Guid.NewGuid().ToString(),
        ProjectUid = Guid.NewGuid().ToString(),
        FilterUid = Guid.NewGuid().ToString(),
        Name = "shouldBeEmpty",
        FilterJson = "theJsonString",
        LastActionedUtc = DateTime.UtcNow
      };

      const string insert =
        @"INSERT Filter
                 (fk_CustomerUid, fk_UserUID, fk_ProjectUID, FilterUID,
                  Name, FilterJson, 
                  IsDeleted, LastActionedUTC)
            VALUES
              (@CustomerUid, @UserUID, @ProjectUID, @FilterUID,  
                  @Name, @FilterJson, 
                  @IsDeleted, @LastActionedUTC)";

      Console.WriteLine($"{DateTime.Now} {this.ToString()}: InsertString: {insert} filter: {JsonConvert.SerializeObject(filter)}");

      var insertedCount = dbConnection.Execute(insert, filter);

      Console.WriteLine($"{DateTime.Now} {this.ToString()}: insertedCount: {insertedCount}");
    }

  }
}
