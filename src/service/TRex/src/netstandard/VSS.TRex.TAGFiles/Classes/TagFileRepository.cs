﻿using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.TAGFiles.Classes.Validator;

namespace VSS.TRex.TAGFiles.Classes
{
  /// <summary>
  /// Archiving will initially write tag files to a local folder before another process either internal or external will move the files to a S3 bucket on Amazon
  /// Ideas. 
  /// </summary>

  public class TagfileMetaData
  {
    [XmlAttribute]
    public Guid? projectId;
    public Guid? assetId;
    public string tagFileName;
    public string tccOrgId;
    public bool IsJohnDoe;
  }


  public static class TagFileRepository
  {

    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);


    private static string MakePath(TagFileDetail td)
    {
      var config = DIContext.Obtain<IConfigurationStore>();
      string tagFileArchiveFolder = config.GetValueString("TAGFILE_ARCHIVE_FOLDER");
      if (!string.IsNullOrEmpty(tagFileArchiveFolder))
        return Path.Combine(tagFileArchiveFolder, td.projectId.ToString(), td.assetId.ToString());

      return Path.Combine(Path.GetTempPath(), "TRexIgniteData", "TagFileArchive", td.projectId.ToString(), td.assetId.ToString());
    }

    /// <summary>
    /// Archives successfully processed tag files for reprocessing when required
    /// </summary>
    /// <param name="tagDetail"></param>
    /// <returns></returns>
    public static bool ArchiveTagfile(TagFileDetail tagDetail)
    {
      string thePath = MakePath(tagDetail);
      if (!Directory.Exists(thePath))
        Directory.CreateDirectory(thePath);

      string fType = "tagfile";

      string ArchiveTagfilePath = Path.Combine(thePath, tagDetail.tagFileName);

      // We don't keep duplicates in TRex
      if (File.Exists(ArchiveTagfilePath))
        File.Delete(ArchiveTagfilePath);

      try
      {
        using (FileStream file = new FileStream(ArchiveTagfilePath, FileMode.Create, System.IO.FileAccess.Write))
        {
          file.Write(tagDetail.tagFileContent, 0, tagDetail.tagFileContent.Length);
        }

        Log.LogDebug($"Tagfile archived to {ArchiveTagfilePath}");

        /* This feature is not required in TRex. Plus not sure if under netcore the serializer is working probably so commented out for now
          leaving code here in case we change our minds in future

        IConfigurationStore config = DIContext.Obtain<IConfigurationStore>();
        if (config.GetValue<bool>("ENABLE_TAGFILE_ARCHIVING_METADATA", false))
        {
          fType = "metafile";
          string ArchiveTagfileMetaDataPath = Path.ChangeExtension(ArchiveTagfilePath, ".xml");

          if (File.Exists(ArchiveTagfileMetaDataPath))
            File.Delete(ArchiveTagfileMetaDataPath);

          // Tagfile MetaData
          TagfileMetaData tmd = new TagfileMetaData()
          {
            projectId = tagDetail.projectId,
            assetId = tagDetail.assetId,
            tccOrgId = tagDetail.tccOrgId,
            tagFileName = tagDetail.tagFileName,
            IsJohnDoe = tagDetail.IsJohnDoe
          };

          using (FileStream file = new FileStream(ArchiveTagfileMetaDataPath, FileMode.Create,
                  System.IO.FileAccess.Write))
          {
            new XmlSerializer(typeof(TagfileMetaData)).Serialize(file, tmd);
          }
          

        } */

        // Another process should move tagfiles eventually to S3 bucket

        return true;
      }

      catch (System.Exception e)
      {
        Log.LogWarning($"Exception occured saving {fType}. error:{e.Message}");
        return false;
      }
    }

    public static bool MoveToUnableToProcess(TagFileDetail tagDetail)
    {
      // todo: Should be moved to a common location. To preserve state we could save all details as a json file which includes the state and binary content. 
      return true;
    }

    /// <summary>
    /// Returns tag file content and meta data for an archived tag file. Input requires filename and project id
    /// </summary>
    /// <param name="tagDetail"></param>
    /// <returns></returns>
    public static TagFileDetail GetTagFile(TagFileDetail tagDetail)
    {
      // just requires the project id and tag file name to be set
      string ArchiveTagfilePath = Path.Combine(MakePath(tagDetail), tagDetail.tagFileName);
      string ArchiveTagfileMetaDataPath = Path.ChangeExtension(ArchiveTagfilePath, ".xml");

      if (!File.Exists(ArchiveTagfilePath))
        return tagDetail;

      using (FileStream file = new FileStream(ArchiveTagfilePath, FileMode.Open, FileAccess.Read))
      {
        tagDetail.tagFileContent = new byte[(int)file.Length];
        file.Read(tagDetail.tagFileContent, 0, (int)file.Length);
      }

      // load xml data ArchiveTagFileMetaDataPath and put into tagDetail
      // if using location only for metadata then you would have to extract it from the path

      var enableArchivingMetadata = DIContext.Obtain<IConfigurationStore>().GetValueBool("ENABLE_TAGFILE_ARCHIVING_METADATA", Consts.ENABLE_TAGFILE_ARCHIVING_METADATA);
      if (enableArchivingMetadata && File.Exists(ArchiveTagfileMetaDataPath))
      {
        FileStream ReadFileStream = new FileStream(ArchiveTagfileMetaDataPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        XmlSerializer SerializerObj = new XmlSerializer(typeof(TagfileMetaData));

        // Load the object saved above by using the Deserialize function
        TagfileMetaData tmd = (TagfileMetaData)SerializerObj.Deserialize(ReadFileStream);
        tagDetail.IsJohnDoe = tmd.IsJohnDoe;
        tagDetail.projectId = tmd.projectId;
        tagDetail.assetId = tmd.assetId;
        tagDetail.tagFileName = tmd.tagFileName;
        tagDetail.tccOrgId = tmd.tccOrgId;

        // Cleanup
        ReadFileStream.Close();
      }

      return tagDetail;
    }

  }
}
