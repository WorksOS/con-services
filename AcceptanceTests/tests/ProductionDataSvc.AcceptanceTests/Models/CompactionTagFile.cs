using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{ 
  public class CompactionTagFilePostParameter
  {
    /// <summary>
    /// The name of the TAG file.
    /// </summary>
    public string fileName { get; set; }

    /// <summary>
    /// The content of the TAG file as an array of bytes.
    /// </summary>
    public byte[] data { get; set; }

    /// <summary>
    /// The project to process the TAG file into. 
    /// </summary>
    public Guid projectUid { get; set; }
  }
}
