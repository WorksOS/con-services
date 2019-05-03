using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VSS.Map3D.DEM;

namespace VSS.Map3D.TMS
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
      Trace.Listeners.Add(new TextWriterTraceListener("c:\\temp\\trace.log"));
      Trace.AutoFlush = true;
      Trace.Indent();
      Trace.WriteLine("Entering Startup");
      Console.WriteLine("Hello World.");
      Trace.WriteLine("Exiting Startup");
      Trace.Unindent();
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      services.AddSingleton<IDEMSource, TRexDEMSource>();
      services.AddSingleton<IConfiguration>(Configuration);
      services.AddCors(options =>
      {
        options.AddPolicy("AllowAllOrigins",
          builder =>
          {
            builder.AllowAnyOrigin().AllowAnyMethod();
          });
      });


    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseMvc();
      app.UseCors("AllowAllOrigins");
      app.UseStaticFiles();
    }
  }
}
