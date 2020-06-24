using System;
using System.Threading.Tasks;
using VSS.AWS.TransferProxy.Interfaces;

namespace VSS.TRex.Common.Interfaces.Interfaces
{
  public interface IS3FileTransfer
  {
    /// <summary>
    /// Reads a file from S3 and places it locally
    /// </summary>
    Task<bool> ReadFile(Guid siteModelUid, string fileName, string targetPath);

    /// <summary>
    /// Writes a file to S3
    ///  AWS Transfer Utility will create the 'directory' if not already there
    /// </summary>
    bool WriteFile(string localFullPath, string s3FullPath);

    /// <summary>
    /// Writes a file to S3 to a specific bucket
    ///  AWS Transfer Utility will create the 'directory' if not already there
    /// </summary>
    bool WriteFileToBucket(string localFullPath, string s3FullPath, string awsBucketName);

    /// <summary>
    /// Writes a file to S3
    ///  AWS Transfer Utility will create the 'directory' if not already there
    /// </summary>
    bool WriteFile(string sourcePath, Guid siteModelUid, string fileName, string destinationFileName = null);

    /// <summary>
    /// Writes a file to S3 and returns boolean & out url path to S3 location
    /// </summary>
    bool WriteFile(string localFullPath, Guid siteModelUid, out string preSignedUrl);

    /// <summary>
    ///  Remove file from storage
    /// </summary>
    bool RemoveFileFromBucket(Guid siteModelUid, string fileName);

    /// <summary>
    /// Generate url path to file located on S3
    /// </summary>
    string GeneratePreSignedUrl(string path);

    /// <summary>
    /// Returns a (possible incomplete) collection from the bucket that match the given prefix.
    /// The continuationToken is non-null/non-empty is there are more keys to query
    /// </summary>
    Task<(string[], string)> ListKeys(string prefix, int maxKeys, string continuationToken);

    /// <summary>
    /// Represents the underlying AWS S3 transfer proxy if require 
    /// </summary>
    ITransferProxy Proxy { get; }
  }
}
