using System.Collections.Generic;

namespace VSS.Productivity3D.Now3D.Models
{
  public class ProjectDisplayModel
  {
    public ProjectDisplayModel()
    {
      Files = new List<FileDisplayModel>();
    }
    
    public string ProjectUid { get; set; }
    public string ProjectName { get; set; }

    public List<FileDisplayModel> Files { get; set; }
  }
}