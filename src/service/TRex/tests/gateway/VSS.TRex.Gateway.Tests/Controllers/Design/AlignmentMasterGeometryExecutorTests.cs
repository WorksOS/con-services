using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Executors.Design;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using VSS.Productivity3D.Productivity3D.Models.Designs;
using GeometryRequest = VSS.TRex.Designs.GridFabric.Requests.AlignmentDesignGeometryRequest;
using FluentAssertions;
using VSS.Productivity3D.Models.Models.MapHandling;

namespace VSS.TRex.Gateway.Tests.Controllers.Design
{
  public class AlignmentMasterGeometryExecutorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public async Task AlignmentMasterGeometryExecutor_SiteModelNotFound()
    {
      const string FILE_NAME = "Test.svl";

      var projectUid = Guid.NewGuid();

      var request = new AlignmentDesignGeometryRequest(projectUid, Guid.NewGuid(), FILE_NAME);
      request.Validate();

      var executor = RequestExecutorContainer
        .Build<AlignmentMasterGeometryExecutor>(
          DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());

      var result = await Assert.ThrowsAsync<ServiceException>(() => executor.ProcessAsync(request));

      Assert.Equal(HttpStatusCode.BadRequest, result.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.Equal($"Site model {projectUid} is unavailable", result.GetResult.Message);
    }

    [Fact]
    public async Task AlignmentMasterGeometryExecutor_ConvertGeometry()
    {
      const int DECIMALS = 6;

      var designUid = new Guid("3ead0c55-1e1f-4d30-aaf8-873526a2ab82");
      const string FILE_NAME = "Test.svl";

      var vertex00 = new [] { -115.02044094250364, 36.207027940562334, 0.0 };
      var vertex01 = new [] { -115.02018046014511, 36.2069812416702, 24.0 };

      var array0 = new [] { vertex00, vertex01 };

      var vertex10 = new [] { -115.01975936894772, 36.206968365690287, 62.0 };
      var vertex11 = new [] { -115.0197132818117, 36.206972191444656, 66.166666666666671 };
      var vertex12 = new [] { -115.01948437671714, 36.207000796998031, 87.000000000000028 };

      var array1 = new [] { vertex10, vertex11, vertex12 };

      var vertices = new[] { array0, array1 };

      var arc = new AlignmentGeometryResultArc(
        36.206968365820806,
        -115.01975936789569,
        -1.4714809066554202E-05,
        36.2069812414021,
        -115.02018046022583,
        0.0,
        0.0,
        0.0,
        0.0,
        false);

      var alignmentGeometry = new AlignmentGeometryResult
        (ContractExecutionStatesEnum.ExecutedSuccessfully,
          designUid,
          vertices,
          new[] { arc },
          null);

      var alignmentGeometryResult = await AlignmentGeometryHelper.ConvertGeometry(alignmentGeometry, FILE_NAME);

      alignmentGeometryResult.Should().NotBeNull();
      alignmentGeometryResult.GeoJSON.Should().NotBeNull();
      alignmentGeometryResult.GeoJSON.Features.Count.Should().Be(1);

      var geometry = alignmentGeometryResult.GeoJSON.Features[0].Geometry as CenterlineGeometry;

      geometry.CenterlineCoordinates.Count.Should().Be(vertices[0].Length + vertices[1].Length + 2);

      var count = 0;
      foreach (var v in array0)
      {
        geometry.CenterlineCoordinates[count][0].Should().BeApproximately(v[0], DECIMALS);
        geometry.CenterlineCoordinates[count][1].Should().BeApproximately(v[1], DECIMALS);
        count++;
      }

      foreach (var v in array1)
      {
        geometry.CenterlineCoordinates[count][0].Should().BeApproximately(v[0], DECIMALS);
        geometry.CenterlineCoordinates[count][1].Should().BeApproximately(v[1], DECIMALS);
        count++;
      }

      geometry.CenterlineCoordinates[count][0].Should().BeApproximately(arc.Lon1, DECIMALS);
      geometry.CenterlineCoordinates[count][1].Should().BeApproximately(arc.Lat1, DECIMALS);

      count++;

      geometry.CenterlineCoordinates[count][0].Should().BeApproximately(arc.Lon2, DECIMALS);
      geometry.CenterlineCoordinates[count][1].Should().BeApproximately(arc.Lat2, DECIMALS);

      alignmentGeometryResult.GeoJSON.Features[0].Properties.Name.Should().Be(FILE_NAME);
    }
  }
}
