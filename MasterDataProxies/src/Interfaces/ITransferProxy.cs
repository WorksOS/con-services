using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ITransferProxy
  {
    Task<FileStreamResult> Download(string s3Key);

    void Upload(Stream stream, string s3Key);

    string GeneratePreSignedUrl(string s3Key);
  }
}
