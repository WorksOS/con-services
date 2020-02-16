using System.Data.Entity.Core.Objects;
using System.Linq;

namespace VSS.Hosted.VLCommon
{
  /// <summary>
  /// Additional extension for interface <code>IQueryableExtension</code>, to
  /// allow includes on <code>IObjectSet</code> when using mocking contexts.
  /// </summary>
  public static class IQueryableExtension
  {
    public static IQueryable<T> Include<T>
        (this IQueryable<T> source, string path)
        where T : class
    {
      ObjectQuery<T> objectQuery = source as ObjectQuery<T>;
      if (objectQuery != null)
      {
        return objectQuery.Include(path);
      }
      return source;
    }
  }
}
