namespace VSS.Productivity3D.Project.Abstractions.Models
{
  public class CwsCalibrationFileUpdateDto
  {
    public string AccountTrn { get; set; }

    public string ProjectTrn { get; set; }

    public int ProjectType { get; set; }

    public string CoordinateSystemFileContent { get; set; }

    public string CoordinateSystemFileName { get; set; }
  }
}