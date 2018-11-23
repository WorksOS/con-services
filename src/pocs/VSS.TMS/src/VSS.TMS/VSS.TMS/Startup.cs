using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VSS.TMS.Controllers;
using VSS.TMS.TileSources;

namespace VSS.TMS
{
  class Startup
  {

    public static Dictionary<string, ITileSource> TileSources;


    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;

      Startup.TileSources = Utils.GetTileSetConfigurations(this.Configuration)
        .ToDictionary(c => c.Name, c => TileSourceFabric.CreateTileSource(c));
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      services.AddCors(options =>
      {
        options.AddPolicy("AllowSpecificOrigin",
          builder => builder.WithOrigins("http://localhost:8080"));
      });

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      // Shows UseCors with named policy.
      app.UseCors("AllowSpecificOrigin");
      app.UseStaticFiles();
      app.UseMvc();
    }
  }
}
