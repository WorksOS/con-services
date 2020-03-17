﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace VSS.AWS.TransferProxy.Interfaces
{
  public interface ITransferProxy
  {
    Task<FileStreamResult> DownloadFromBucket(string s3Key, string bucketName);
    Task<FileStreamResult> Download(string s3Key);
   
    void UploadToBucket(Stream stream, string s3Key, string bucketName);
    void Upload(Stream stream, string s3Key);

    void RemoveFromBucket(string s3Key);

    string Upload(Stream stream, string s3Key, string contentType);

    string GeneratePreSignedUrl(string s3Key);
  }
}
