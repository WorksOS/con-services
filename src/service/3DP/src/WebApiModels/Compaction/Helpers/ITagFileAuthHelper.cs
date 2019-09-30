using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public interface ITagFileAuthHelper
  {
    Task<GetProjectAndAssetUidsResult> GetProjectUid(GetProjectAndAssetUidsRequest tfaRequest);
  }
}
