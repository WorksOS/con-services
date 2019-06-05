using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace VSS.SeriLog.Extensions
{
  class SerilogProvider : ILoggerProvider
  {
    private IHttpContextAccessor accessor;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="accessor">Optionally sets the HTTP context accessor.</param>
    public SerilogProvider(IHttpContextAccessor accessor = null)
    {
      this.accessor = accessor;
    }

    public void Dispose()
    { }

    public ILogger CreateLogger(string categoryName)
    {
      throw new NotImplementedException();
    }
  }
}
