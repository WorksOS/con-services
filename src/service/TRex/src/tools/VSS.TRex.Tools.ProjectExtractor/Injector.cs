using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Tools.ProjectExtractor
{
  public class Injector
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<Extractor>();

    private readonly ISiteModel _siteModel;
    private readonly string _projectInputPath;

    public Injector(ISiteModel siteModel, string projectInputPath)
    {
      _siteModel = siteModel;
      _projectInputPath = projectInputPath;
    }
  }
}
