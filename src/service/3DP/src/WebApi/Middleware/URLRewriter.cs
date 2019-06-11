using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;

namespace VSS.Productivity3D.WebApi.Middleware
{
  /// <summary>
  /// URL rewriting is the act of modifying request URLs based on one or more predefined rules.
  /// This class provides functionality allowing us custom rewrite behaviour.
  /// </summary>
  public class URLRewriter
  {
    private static readonly Regex _regex = new Regex(@"(?<!:)(\/\/+)");

    /// <summary>
    /// Custom URL rewriter: Replaces all occurrences of // with /.
    /// </summary>
    /// <remarks>
    /// This catch all is needed to work around errant behaviour in TBC where invalid path strings are sent to 3DP.
    /// If that issue is addressed this rewrite could be removed.
    /// Note: Custom rewriter is required as framework AddRewrite() method doesn't cater for our needs.
    /// </remarks>
    public static void RewriteMalformedPath(RewriteContext context)
    {
      var request = context.HttpContext.Request;

      if (!_regex.IsMatch(request.Path.Value))
      {
        return;
      }

      request.Path = new PathString(_regex.Replace(request.Path.Value, "/"));
    }
  }
}
