using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace VSS.AWS.TransferProxy.Interfaces
{
  public interface ITransferProxy
  {
    Task<FileStreamResult> Download(string s3Key);

    void Upload(Stream stream, string s3Key);

    void Upload(Stream stream, string s3Key, string contentType);

    string GeneratePreSignedUrl(string s3Key);
  }
}
