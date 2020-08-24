using System.Net.Http;
using System.Threading.Tasks;
using CCSS.IntegrationTests.Utils;

namespace AccuracyTests
{
  public class TestDataGenerator
  {
    private readonly IRestClient _restClient;

    public TestDataGenerator(IRestClient restClient)
    {
      _restClient = restClient;
    }

    //public async Task<HttpResponseMessage> GetById(string id) =>
    //  await _restClient.SendAsync(
    //    $"/api/organisation/{id}",
    //    HttpMethod.Get);

    //public async Task<HttpResponseMessage> AddOrganisation(OrganisationRequestDto request) =>
    //  await _restClient.SendAsync(
    //    "/api/organisation",
    //    HttpMethod.Post,
    //    body: request);

    public async Task<HttpResponseMessage> GetTestData() =>
      await _restClient.SendAsync(
        $"https://jsonplaceholder.typicode.com/todos/1",
        HttpMethod.Get);
  }
}
