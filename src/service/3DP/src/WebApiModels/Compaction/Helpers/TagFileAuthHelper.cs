using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// To support getting identifying project
  /// </summary>
  public class TagFileAuthHelper : ITagFileAuthHelper
  {
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfigurationStore _configStore;
    private readonly ITagFileAuthProjectProxy _tagFileAuthProjectV2Proxy;

    public TagFileAuthHelper(ILoggerFactory loggerFactory, IConfigurationStore configStore,
      ITagFileAuthProjectProxy tagFileAuthProjectV2Proxy
      )
    {
      _loggerFactory = loggerFactory;
      _configStore = configStore;
      _tagFileAuthProjectV2Proxy = tagFileAuthProjectV2Proxy;
    }

    public async Task<GetProjectAndAssetUidsResult> GetProjectUid(GetProjectAndAssetUidsRequest tfaRequest)
    {
      return await _tagFileAuthProjectV2Proxy.GetProjectAndAssetUids(tfaRequest); 
    }
  }
}
