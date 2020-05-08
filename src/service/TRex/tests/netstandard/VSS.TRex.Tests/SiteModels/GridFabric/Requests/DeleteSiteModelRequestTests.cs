using FluentAssertions;
using VSS.TRex.SiteModels.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Requests
{
  [UnitTestCoveredRequest(RequestType = typeof(DeleteSiteModelRequest))]
  public class DeleteSiteModelRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<DeleteSiteModelRequestComputeFunc, DeleteSiteModelRequestArgument, DeleteSiteModelRequestResponse>();

    private void VerifyModelIsEmpty(ISiteModel model)
    {
      // Check that there are no elements in the storage proxy for the site model
      foreach (var cache in IgniteMock.MockedCacheDictionaries)
      {
        cache.Keys.Count.Should().Be(0, $"Cache {cache} should have had all elements removed");
      }
    }

    [Fact]
    public void Creation()
    {
      var req = new DeleteSiteModelRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public void DeleteEmptyModel()
    {
      AddApplicationGridRouting();

      var model = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      model.Should().NotBeNull();

      var modelId = model.ID;

      var request = new DeleteSiteModelRequest();
      var response = request.Execute(new DeleteSiteModelRequestArgument { ProjectID = modelId });

      response.Result.Should().Be(DeleteSiteModelResult.OK);

      VerifyModelIsEmpty(model);
    }
  }
}
