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
    [JsonProperty(PropertyName = "uid", Required = Required.Default)]
    public Guid? uid { get; private set; }

    /// <summary>
    /// The id of the design file
    /// </summary>
    [JsonProperty(PropertyName = "id", Required = Required.Default)]
    public long id { get; private set; }

    /// <summary>
    /// The description of where the file is located.
    /// </summary>
    [JsonProperty(PropertyName = "file", Required = Required.Default)]
    public FileDescriptor file { get; private set; }

    /// <summary>
    /// The offset in meters to use for a reference surface. The surface in the file will be offset by this amount.
    /// Only applicable when the file is a surface design file.
    /// </summary>
    [JsonProperty(PropertyName = "offset", Required = Required.Default)]
    public double offset { get; private set; }

    public bool ShouldSerializeuid() => uid.HasValue;

    /// <summary>
    /// Default private constructor
    /// </summary>
    private DesignDescriptor()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="file"></param>
    /// <param name="offset"></param>
    /// <param name="uid"></param>
    public DesignDescriptor(long id, FileDescriptor file, double offset, Guid? uid = null)
    {
      this.id = id;
      this.file = file;
      this.offset = offset;
      this.uid = uid;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (uid != null )
        return;

      if (id <= 0 && file == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Either the design id or file location is required"));
      }

      file?.Validate();
    }

    #region IEquatable
    public bool Equals(DesignDescriptor other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return id.Equals(other.id) && (file == null ? other.file == null : file.Equals(other.file)) && offset.Equals(other.offset);
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
        var hashCode = id.GetHashCode();
        hashCode = (hashCode * 397) ^ offset.GetHashCode();
        hashCode = (hashCode * 397) ^ (file != null ? file.GetHashCode() : 0);
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
