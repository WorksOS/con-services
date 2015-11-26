using System;
using System.Collections;
using System.IO;
using log4net.ObjectRenderer;

namespace VSS.VisionLink.Utilization.Common.Utilities
{
  public class ExceptionRenderer : IObjectRenderer
  {
    public void RenderObject(RendererMap rendererMap, object obj, TextWriter writer)
    {
      var thrown = obj as Exception;
      while (thrown != null)
      {
        RenderException(thrown, writer);
        thrown = thrown.InnerException;
      }
    }

    private void RenderException(Exception ex, TextWriter writer)
    {
      writer.WriteLine("Type: {0}", ex.GetType().FullName);
      writer.WriteLine("Message: {0}", ex.Message);
      writer.WriteLine("Source: {0}", ex.Source);
      RenderExceptionData(ex, writer);
      writer.WriteLine("StackTrace: {0}", ex.StackTrace);
    }

    private void RenderExceptionData(Exception ex, TextWriter writer)
    {
      foreach (DictionaryEntry entry in ex.Data)
      {
        writer.WriteLine("{0}: {1}", entry.Key, entry.Value);
      }
    }
  }
}