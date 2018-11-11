using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using src.Utils;
using VSS.MasterData.Models.FIlters;

namespace MockProjectWebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddCors(options =>
            {
              options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
                      .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization", "X-VisionLink-CustomerUid", "X-VisionLink-UserUid")
                      .WithMethods("OPTIONS", "TRACE", "GET", "POST", "DELETE", "PUT", "HEAD"));
            });

            services.AddMvc();
          }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseExceptionTrap();
            app.UseCors("VSS");
            app.UseExceptionDummyPostMiddleware();

            app.UseMvc();
        }
    }
}