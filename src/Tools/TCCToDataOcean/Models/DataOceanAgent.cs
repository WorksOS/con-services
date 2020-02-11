using System.Net.Http;
using System.Threading.Tasks;
using TCCToDataOcean.Interfaces;
using TCCToDataOcean.Types;
using TCCToDataOcean.Utils;
using VSS.DataOcean.Client.Models;

namespace TCCToDataOcean.Models
{
  public class DataOceanKeysetMeta
  {
    public long Key_Offset { get;set; }
  }

  public class DataOceanKeysetDirectory
  {
    public DataOceanDirectory[] Directories;
    public DataOceanKeysetMeta Meta;
  }

  public class DataOceanKeysetFile
  {
    public DataOceanFile[] Files;
    public DataOceanKeysetMeta Meta;
  }

  public interface IDataOceanAgent
  {
    /// <summary>
    /// Gets the Customer folder Id by name.
    /// </summary>
    Task<DataOceanKeysetDirectory> GetCustomerByName(string customerUid);

    /// <summary>
    /// Get the Project subfolders of the DataOcean Customer folder.
    /// </summary>
    Task<DataOceanKeysetDirectory> GetProjectForCustomerById(string customerId, string projectUid);

    /// <summary>
    /// Get the files Project folder by the DataOcean Id.
    /// </summary>
    Task<DataOceanKeysetFile> GetFilesForProjectById(string projectId, long metaKeyOffset);
  }

  public class DataOceanAgent : IDataOceanAgent
  {
    private readonly IRestClient _restClient;
    private readonly string _dataOceanApiUrl;
    private readonly string _dataOceanRootId;

    public DataOceanAgent(IRestClient restClient, IEnvironmentHelper environmentHelper)
    {
      _restClient = restClient;

      _dataOceanApiUrl = environmentHelper.GetVariable("DATA_OCEAN_API_URL", 1);
      _dataOceanRootId = environmentHelper.GetVariable("DATA_OCEAN_ROOT_ID", 1);
    }

    public Task<DataOceanKeysetDirectory> GetCustomerByName(string customerUid) =>
      _restClient.SendHttpClientRequest<DataOceanKeysetDirectory>($"{_dataOceanApiUrl}/api/browse/keyset_directories?parent_id={_dataOceanRootId}&name={customerUid}",
                                                                  HttpMethod.Get,
                                                                  MediaType.APPLICATION_JSON,
                                                                  MediaType.APPLICATION_JSON,
                                                                  setJWTHeader: false);

    public Task<DataOceanKeysetDirectory> GetProjectForCustomerById(string customerId, string projectUid) =>
      _restClient.SendHttpClientRequest<DataOceanKeysetDirectory>($"{_dataOceanApiUrl}/api/browse/keyset_directories?parent_id={customerId}&name={projectUid}",
                                                                  HttpMethod.Get,
                                                                  MediaType.APPLICATION_JSON,
                                                                  MediaType.APPLICATION_JSON,
                                                                  setJWTHeader: false);

    public Task<DataOceanKeysetFile> GetFilesForProjectById(string projectId, long metaKeyOffset) =>
      _restClient.SendHttpClientRequest<DataOceanKeysetFile>($"{_dataOceanApiUrl}/api/browse/keyset_files?parent_id={projectId}{(metaKeyOffset > 0 ? $"=key_offset={metaKeyOffset}" : null)}",
                                                             HttpMethod.Get,
                                                             MediaType.APPLICATION_JSON,
                                                             MediaType.APPLICATION_JSON,
                                                             setJWTHeader: false);
  }
}
