using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// Manages the life cycle of activities in the project rebuilder across the set of projects being rebuilt
  /// </summary>
  public class SiteModelRebuilderManager
  {
    private static ILogger _log = Logging.Logger.CreateLogger<SiteModelRebuilderManager>();

    public SiteModelRebuilderManager()
    {

    }
  }
}
