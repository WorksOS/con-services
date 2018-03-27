using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Rendering.Abstractions;

namespace RaptorClassLibrary.netstandard.DI
{
    public static class DIContext
    {
        public static IRenderingFactory RenderingFactory { get; internal set; }

        public static void Inject(IServiceProvider serviceProvider)
        {
            RenderingFactory = serviceProvider.GetService<IRenderingFactory>();
        }
    }
}
