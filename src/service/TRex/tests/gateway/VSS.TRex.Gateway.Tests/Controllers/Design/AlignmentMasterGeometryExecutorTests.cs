using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CoreX.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Models.Designs;
using VSS.TRex.Common.Extensions;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Executors.Design;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

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
    public void AlignmentMasterGeometryExecutor_ConvertGeometryCoordinatesFromNEEToLLE()
    {
      const int DECIMALS = 6;

      const string DIMENSIONS_2012_DC_CSIB = "QM0G000ZHC4000000000800BY7SN2W0EYST640036P3P1SV09C1G61CZZKJC976CNB295K7W7G30DA30A1N74ZJH1831E5V0CHJ60W295GMWT3E95154T3A85H5CRK9D94PJM1P9Q6R30E1C1E4Q173W9XDE923XGGHN8JR37B6RESPQ3ZHWW6YV5PFDGCTZYPWDSJEFE1G2THV3VAZVN28ECXY7ZNBYANFEG452TZZ3X2Q1GCYM8EWCRVGKWD5KANKTXA1MV0YWKRBKBAZYVXXJRM70WKCN2X1CX96TVXKFRW92YJBT5ZCFSVM37ZD5HKVFYYYMJVS05KA6TXFY6ZE4H6NQX8J3VAX79TTF82VPSV1KVR8W9V7BM1N3MEY5QHACSFNCK7VWPNY52RXGC1G9BPBS1QWA7ZVM6T2E0WMDY7P6CXJ68RB4CHJCDSVR6000047S29YVT08000";

      // Vertices...
      var vertex00 = new[] { 2718.0231, 1172.2012, 0.0 };
      var vertex01 = new[] { 2741.4501, 1167.0194999999999, 24.0 };

      var array0 = new[] { vertex00, vertex01 };

      var vertex10 = new[] { 2779.3216, 1165.5918, 62.0 };
      var vertex11 = new[] { 2783.4665, 1166.0165, 66.166666666666671 };
      var vertex12 = new[] { 2804.0533, 1169.1917, 87.000000000000028 };

      var array1 = new[] { vertex10, vertex11, vertex12 };

      var vertices = new[] { array0, array1 };

      // Arcs...
      var arc = new AlignmentGeometryResponseArc(
        2779.32169461374,
        1165.5918144869775,
        0.0,
        2741.4500927432855,
        1167.0194702485303,
        0.0,
        2765.9912,
        1314.9983,
        1E+308,
        false);

      // Labels...
      var labels = new List<AlignmentGeometryResponseLabel>();

      labels.Add(new AlignmentGeometryResponseLabel(0.0, 2718.0231, 1172.2012, 1.3264603040333998));
      labels.Add(new AlignmentGeometryResponseLabel(10.0, 2727.7340448735927, 1169.8277460683921, 1.3403457438206368));
      labels.Add(new AlignmentGeometryResponseLabel(20.0, 2737.5150349254664, 1167.723162636835, 1.3819858321379428));
      labels.Add(new AlignmentGeometryResponseLabel(30.0, 2773.3363657610616, 1165.178246135564, -1.5218089644060191));
      labels.Add(new AlignmentGeometryResponseLabel(40.0, 2763.3394425012643, 1165.0217412244106, 4.6947096761067844));
      labels.Add(new AlignmentGeometryResponseLabel(50.0, 2753.3543004659637, 1165.5315519582812, 4.6280430094401517));
      labels.Add(new AlignmentGeometryResponseLabel(60.0, 2743.4253016301927, 1166.7054133507629, 4.5613763427738521));
      labels.Add(new AlignmentGeometryResponseLabel(70.0, 2787.2690825623372, 1166.4921795439566, 1.7045881732707571));
      labels.Add(new AlignmentGeometryResponseLabel(80.0, 2797.1558966323028, 1168.0067255362799, 1.7365783469590923));
      labels.Add(new AlignmentGeometryResponseLabel(87.000000000000028, 2804.0533, 1169.1917, 1.7431112347917908));

      // Geometry...
      var geometry = new AlignmentDesignGeometryResponse(
        DesignProfilerRequestResult.OK,
        vertices,
        new[] { arc },
        labels.ToArray()
        );

      // ConvertCoordinates expected result mock...
      var llhCoords = new List<XYZ>();

      llhCoords.Add(new XYZ(-115.02044094250364, 36.207027940562334, -1.4714844137459045E-05));
      llhCoords.Add(new XYZ(-115.02018046014511, 36.2069812416702, 23.999985285190935));
      llhCoords.Add(new XYZ(-115.01975936894772, 36.206968365690287, 61.9999852852006));
      llhCoords.Add(new XYZ(-115.0197132818117, 36.206972191444656, 66.1666519518644));
      llhCoords.Add(new XYZ(-115.01948437671714, 36.207000796998031, 86.99998528517628));
      llhCoords.Add(new XYZ(-115.01975936789569, 36.206968365820806, -1.4714799397041221E-05));
      llhCoords.Add(new XYZ(-115.02018046022583, 36.2069812414021, -1.4714809066554202E-05));
      llhCoords.Add(new XYZ(0.0, 0.0, 0.0));
      llhCoords.Add(new XYZ(-115.02044094250364, 36.207027940562334, -1.4714844137459045E-05));
      llhCoords.Add(new XYZ(-115.02033296755739, 36.207006550676034, -1.4714828073727506E-05));
      llhCoords.Add(new XYZ(-115.02022421375355, 36.206987583500194, -1.4714813829446455E-05));
      llhCoords.Add(new XYZ(-115.01982591867552, 36.206964640908289, -1.4714796599646234E-05));
      llhCoords.Add(new XYZ(-115.01993707410817, 36.206963233554013, -1.4714795542728612E-05));
      llhCoords.Add(new XYZ(-115.02004809831355, 36.206967830348162, -1.4714798994903312E-05));
      llhCoords.Add(new XYZ(-115.02015849805876, 36.206978410869162, -1.4714806940834526E-05));
      llhCoords.Add(new XYZ(-115.01967100086553, 36.206976476671556, -1.4714805488259375E-05));
      llhCoords.Add(new XYZ(-115.01956106916795, 36.206990121295007, -1.4714815735321082E-05));
      llhCoords.Add(new XYZ(-115.01948437671714, 36.207000796998024, -1.4714823752735629E-05));

      var convertCoordinatesMock = new Mock<ICoreXWrapper>();

      var expectedCoordinateConversionResult = llhCoords.ToArray().ToCoreX_XYZ();

      convertCoordinatesMock.Setup(x => x.NEEToLLH(It.IsAny<string>(), It.IsAny<CoreX.Models.XYZ[]>(), It.IsAny<CoreX.Types.ReturnAs>())).Returns(expectedCoordinateConversionResult);
      DIBuilder.Continue().Add(x => x.AddSingleton(convertCoordinatesMock.Object)).Complete();

      // Convert all coordinates from grid to lat/lon
      AlignmentMasterGeometryHelper.ConvertNEEToLLHCoords(DIMENSIONS_2012_DC_CSIB, geometry);

      geometry.Should().NotBeNull();
      geometry.Arcs.Should().NotBeNull();
      geometry.Labels.Should().NotBeNull();
      geometry.Vertices.Should().NotBeNull();

      geometry.Arcs.Should().HaveCount(1);
      geometry.Labels.Should().HaveCount(labels.Count);
      geometry.Vertices.Should().HaveCount(vertices.Length);

      var count = 0;

      // Vertices...
      for (var i = 0; i < geometry.Vertices.Length; i++)
      {
        geometry.Vertices[i].Should().HaveCount(vertices[i].Length);

        for (var n = 0; n < geometry.Vertices[i].Length; n++)
        {
          geometry.Vertices[i][n].Should().HaveCount(vertices[i][n].Length);

          geometry.Vertices[i][n][0].Should().BeApproximately(llhCoords[count].X, DECIMALS);
          geometry.Vertices[i][n][1].Should().BeApproximately(llhCoords[count].Y, DECIMALS);
          geometry.Vertices[i][n][2].Should().BeApproximately(llhCoords[count].Z, DECIMALS);

          count++;
        }
      }

      // Arc...
      geometry.Arcs[0].X1.Should().BeApproximately(llhCoords[count].X, DECIMALS);
      geometry.Arcs[0].Y1.Should().BeApproximately(llhCoords[count].Y, DECIMALS);
      geometry.Arcs[0].Z1.Should().BeApproximately(llhCoords[count].Z, DECIMALS);

      count++;

      geometry.Arcs[0].X2.Should().BeApproximately(llhCoords[count].X, DECIMALS);
      geometry.Arcs[0].Y2.Should().BeApproximately(llhCoords[count].Y, DECIMALS);
      geometry.Arcs[0].Z2.Should().BeApproximately(llhCoords[count].Z, DECIMALS);

      count++;

      geometry.Arcs[0].XC.Should().BeApproximately(llhCoords[count].X, DECIMALS);
      geometry.Arcs[0].YC.Should().BeApproximately(llhCoords[count].Y, DECIMALS);
      geometry.Arcs[0].ZC.Should().BeApproximately(llhCoords[count].Z, DECIMALS);

      count++;

      // Labels...
      for (var n = 0; n < geometry.Labels.Length; n++)
      {
        geometry.Labels[n].X.Should().BeApproximately(llhCoords[count].X, DECIMALS);
        geometry.Labels[n].Y.Should().BeApproximately(llhCoords[count].Y, DECIMALS);
      
        count++;
      }
    }
  }
}
