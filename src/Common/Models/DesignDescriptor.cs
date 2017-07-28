using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// Description to identify a design file either by id or by its location in TCC.
  /// </summary>
  public class DesignDescriptor
  {
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

      /// <summary>
    /// Private constructor
    /// </summary>
    private DesignDescriptor()
    {}

    /// <summary>
    /// Create instance of FileDescriptor
    /// </summary>
    public static DesignDescriptor CreateDesignDescriptor
        (
          long id,
          FileDescriptor file,
          double offset
        )
    {
      return new DesignDescriptor
             {
               id = id,
               file = file,
               offset = offset
             };
    }

    /// <summary>
    /// Create example instance of DesignDescriptor to display in Help documentation.
    /// </summary>
    public static DesignDescriptor HelpSample => new DesignDescriptor
    {
      id = 1234,
      file = FileDescriptor.HelpSample,
      offset = 0
    };

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (id <= 0 && file == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
             new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                 "Either the design id or file location is required"));        
      }

      file?.Validate();
    }
  }
}