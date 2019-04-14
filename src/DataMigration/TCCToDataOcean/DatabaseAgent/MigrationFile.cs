using VSS.MasterData.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean.DatabaseAgent
{
  public class MigrationFile : MigrationObj
  {
    public string ProjectUid { get; set; }
    public string ImportedFileUid { get; set; }
    public string CustomerUid { get; set; }
    public ImportedFileType ImportedFileType { get; set; }
    public string Filename { get; set; }
    public MigrationState MigrationState { get; set; }
    public long Length { get; set; }

    public MigrationFile()
    { }

    public MigrationFile(FileData file)
    {
      Id = (int)file.LegacyFileId;
      ProjectUid = file.ProjectUid;
      ImportedFileType = file.ImportedFileType;
      CustomerUid = file.CustomerUid;
      ImportedFileUid = file.ImportedFileUid;
      Filename = file.Name;
    }
  }
}
