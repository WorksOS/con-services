using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Rendering.Abstractions;

namespace VSS.VisionLink.Raptor.DI
{
    public static class DIContext
    {
        private static IServiceProvider ServiceProvider { get; set; }

        public static IRenderingFactory RenderingFactory { get; internal set; }
//        public static ILoggerFactory LoggerFactory { get; internal set; }
//        public static ILogger DefaultLogger { get; internal set; }

        public static void Inject(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;

            RenderingFactory = serviceProvider.GetService<IRenderingFactory>();
//            LoggerFactory = serviceProvider.GetService<ILoggerFactory>();
//            DefaultLogger = serviceProvider.GetService<ILogger>();
        }
    }
}
