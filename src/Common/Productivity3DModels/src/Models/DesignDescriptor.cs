using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Description to identify a design file either by id or by its location in TCC.
  /// </summary>
  public class DesignDescriptor : IEquatable<DesignDescriptor>
  {
    /// <summary>
    /// The unique id of the design file
    /// </summary>
    [JsonProperty(PropertyName = "fileUid", Required = Required.Default)]
    public Guid? FileUid { get; private set; }

    /// <summary>
    /// The id of the design file
    /// </summary>
    [JsonProperty(PropertyName = "id", Required = Required.Default)]
    public long Id { get; private set; }

    /// <summary>
    /// The description of where the file is located.
    /// </summary>
    [JsonProperty(PropertyName = "file", Required = Required.Default)]
    public FileDescriptor File { get; }

    /// <summary>
    /// The offset in meters to use for a reference surface. The surface in the file will be offset by this amount.
    /// Only applicable when the file is a surface design file.
    /// </summary>
    [JsonProperty(PropertyName = "offset", Required = Required.Default)]
    public double Offset { get; }

    public bool ShouldSerializeuid() => FileUid.HasValue;

    /// <summary>
    /// Default private constructor
    /// </summary>
    private DesignDescriptor()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public DesignDescriptor(long id, FileDescriptor file, double offset, Guid? fileUid = null)
    {
      Id = id;
      File = file;
      Offset = offset;
      FileUid = fileUid;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (FileUid != null )
        return;

      if (Id <= 0 && File == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Either the design id or file location is required"));
      }

      File?.Validate();
    }

    #region IEquatable
    public bool Equals(DesignDescriptor other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Id.Equals(other.Id) && (File == null ? other.File == null : File.Equals(other.File)) && Offset.Equals(other.Offset);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((DesignDescriptor)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = Id.GetHashCode();
        hashCode = (hashCode * 397) ^ Offset.GetHashCode();
        hashCode = (hashCode * 397) ^ (File != null ? File.GetHashCode() : 0);
        return hashCode;
      }
    }

    public static bool operator ==(DesignDescriptor left, DesignDescriptor right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(DesignDescriptor left, DesignDescriptor right)
    {
      return !Equals(left, right);
    }
    #endregion
  }
}
