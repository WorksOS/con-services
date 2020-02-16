using System;
using log4net.ObjectRenderer;
using System.Collections;
using System.IO;

namespace VSS.Nighthawk.Log4Net
{
  public class ExceptionRenderer : IObjectRenderer
  {
    public void RenderObject(RendererMap rendererMap, object obj, TextWriter writer)
    {
      Exception thrown = obj as Exception;
      while (thrown != null)
      {
        RenderException(thrown, writer);
        thrown = thrown.InnerException;
      }
    }

    private void RenderException(Exception ex, TextWriter writer)
    {
      writer.WriteLine(string.Format("Type: {0}", ex.GetType().FullName));
      writer.WriteLine(string.Format("Message: {0}", ex.Message));
      writer.WriteLine(string.Format("Source: {0}", ex.Source));
      RenderExceptionData(ex, writer);
      writer.WriteLine(string.Format("StackTrace: {0}", ex.StackTrace));
    }

    private void RenderExceptionData(Exception ex, TextWriter writer)
    {
      foreach (DictionaryEntry entry in ex.Data)
      {
        writer.WriteLine(string.Format("{0}: {1}", entry.Key, entry.Value));
      }
    }
  }
}
