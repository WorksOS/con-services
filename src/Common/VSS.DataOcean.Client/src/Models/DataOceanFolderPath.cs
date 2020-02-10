using System.Collections.Generic;

namespace VSS.DataOcean.Client.Models
{ 
  public class DataOceanFolderPath
  {
    public string DataOceanFolderId { get; }

    // subfolders and it FolderIds
    public readonly IDictionary<string, DataOceanFolderPath> Nodes;

    public DataOceanFolderPath(string dataOceanFolderId, IDictionary<string, DataOceanFolderPath> nodes)
    {
      DataOceanFolderId = dataOceanFolderId;
      Nodes = nodes;
    }

    public DataOceanFolderPath CreateNode(string parentId, string folderName)
    {
      var folderPath = new DataOceanFolderPath(parentId, new Dictionary<string, DataOceanFolderPath>());

      lock (Nodes)
      {
        if (Nodes.TryGetValue(folderName, out var retrievedCurrentDataOceanFolderPath)) 
          return retrievedCurrentDataOceanFolderPath;
        Nodes.Add(folderName, folderPath);
        return folderPath;
      }
    }
  }
}
