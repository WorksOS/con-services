using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// TAG file domain object
  /// This is copied from ...\TagFileProcessing\Models\TAGFile.cs
  /// </summary>
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
    /// The project to process the TAG file into. This is optional. If not set, Raptor will determine automatically which project the TAG file should be processed into.
    /// </summary>
    public Guid? projectUid { get; set; }
    
    public long? orgId { get; set; }

    ///// <summary>
    ///// A flag to indicate if the TAG file should also be converted into a CSV file. Not currently available.
    ///// </summary>
    //public bool convertToCSV { get; set; }

    ///// <summary>
    ///// A flag to indicate if the TAG file should also be converted into a DXF file. Not currently available.
    ///// </summary>
    //public bool convertToDXF { get; set; } 
  }

  /// <summary>
  /// Represents response from the service after TAG file POST request
  /// </summary>
  public class CompactionTagFilePostResult : RequestResult, IEquatable<CompactionTagFilePostResult>
  {
    /// <summary>
    /// Constructor: success result by default
    /// </summary>
    public CompactionTagFilePostResult()
        : base("success")
    { }

    public bool Equals(CompactionTagFilePostResult other)
    {
      if (other == null)
        return false;

      return this.Code == other.Code && this.Message == other.Message;
    }

    public static bool operator ==(CompactionTagFilePostResult a, CompactionTagFilePostResult b)
    {
      if ((object)a == null || (object)b == null)
        return Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionTagFilePostResult a, CompactionTagFilePostResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionTagFilePostResult && this == (CompactionTagFilePostResult)obj;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation.</returns>
    public override string ToString()
    {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
  }
}