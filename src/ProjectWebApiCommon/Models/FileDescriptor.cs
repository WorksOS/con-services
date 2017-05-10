using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using ProjectWebApiCommon.ResultsHandling;
using TCCFileAccess.Models;

namespace ProjectWebApiCommon.Models
{
    /// <summary>
    /// Description to identify a file by its location in TCC.
    /// </summary>
    public class FileDescriptor 
    {
      /// <summary>
      /// The id of the filespace in TCC where the file is located.
      /// </summary>
      [JsonProperty(PropertyName = "filespaceId", Required = Required.Always)]
      [Required]
      public string filespaceId { get; private set; }

      /// <summary>
      /// The full path of the file.
      /// </summary>
      [JsonProperty(PropertyName = "path", Required = Required.Always)]
      public string path { get; private set; }

      /// <summary>
      /// The name of the file.
      /// </summary>
      [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
      public string fileName { get; private set; }

      /// <summary>
      /// Private constructor
      /// </summary>
      private FileDescriptor()
      { }

      /// <summary>
      /// Create instance of FileDescriptor
      /// </summary>
      public static FileDescriptor CreateFileDescriptor
      (
        string filespaceId,
        string path,
        string fileName
      )
      {
        return new FileDescriptor
        {
          filespaceId = filespaceId,
          path = path,
          fileName = fileName
        };
      }

      public static FileDescriptor EmptyFileDescriptor
      {
        get { return emptyDescriptor; }
      }

      /// <summary>
      /// Create example instance of FileDescriptor to display in Help documentation.
      /// </summary>
      public static FileDescriptor HelpSample
      {
        get
        {
          return new FileDescriptor()
          {
            filespaceId = "u72003136-d859-4be8-86de-c559c841bf10",
            path = "BC Data/Sites/Integration10/Designs",
            fileName = "Cycleway.ttm"
          };
        }
      }

      /// <summary>
      /// Validates all properties
      /// </summary>
      public void Validate()
      {
        if (string.IsNullOrEmpty(this.filespaceId) || string.IsNullOrEmpty(this.path) ||
            string.IsNullOrEmpty(this.fileName))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Filespace Id, filespace name, path and file name are all required"));
        }

      }

      private static FileDescriptor emptyDescriptor = new FileDescriptor
      {
        filespaceId = string.Empty,
        path = string.Empty,
        fileName = string.Empty
      };

    }
  }