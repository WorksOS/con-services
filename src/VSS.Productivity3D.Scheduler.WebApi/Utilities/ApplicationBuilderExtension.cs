// Package Owin.Builder 0.8.5 is not compatible with netcoreapp1.1 (.NETCoreApp,Version=v1.1). Package Owin.Builder 0.8.5 supports: net40 (.NETFramework,Version=v4.0)

//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.DataProtection;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.AspNetCore.Builder;

//using System;
//using Microsoft.AspNetCore.Builder;
//using Owin;

//namespace VSS.Productivity3D.Scheduler.WebApi
//{

//  using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

//  public static class IApplicationBuilderExtensions
//  {
//    public static IApplicationBuilder UseAppBuilder(this IApplicationBuilder app, Action<IAppBuilder> configure)
//    {
//      return app.UseOwin(addToPipeline =>
//      {
//        addToPipeline(next =>
//        {
//          AppBuilder appBuilder = new AppBuilder();
//          appBuilder.Properties["builder.DefaultApp"] = next;

//          configure(appBuilder);

//          return appBuilder.Build<AppFunc>();
//        });
//      });
//    }
//  }
//}
