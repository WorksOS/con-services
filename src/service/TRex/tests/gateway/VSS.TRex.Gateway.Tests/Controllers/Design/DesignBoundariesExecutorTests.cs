using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.Design
{
  public class DesignBoundariesExecutorTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddDesignProfilerGridRouting() => IgniteMock.AddApplicationGridRouting<DesignFilterSubGridMaskComputeFunc, DesignSubGridFilterMaskArgument, DesignFilterSubGridMaskResponse>();

    [Fact]
    public void DesignBoundariesExecutor_SiteModelNotFound()
    {
      const double TOLERANCE = 1.2;
      const string FILE_NAME = "Test.ttm";

      var projectUid = Guid.NewGuid();

      var request = new TRexDesignBoundariesRequest(projectUid, Guid.NewGuid(), FILE_NAME, TOLERANCE);
      request.Validate();

      var executor = RequestExecutorContainer
        .Build<DesignBoundariesExecutor>(
          DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());

      var result = Assert.Throws<ServiceException>(() => executor.Process(request));

      Assert.Equal(HttpStatusCode.BadRequest, result.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.Equal($"Site model {projectUid} is unavailable", result.GetResult.Message);
    }

    [Fact]
    public void DesignBoundariesExecutor_ConvertBoundary()
    {
      const double TOLERANCE = 1.0;
      const string FILE_NAME = "Test.ttm";
      const string DIMENSIONS_2012_DC_CSIB = "QM0G000ZHC4000000000800BY7SN2W0EYST640036P3P1SV09C1G61CZZKJC976CNB295K7W7G30DA30A1N74ZJH1831E5V0CHJ60W295GMWT3E95154T3A85H5CRK9D94PJM1P9Q6R30E1C1E4Q173W9XDE923XGGHN8JR37B6RESPQ3ZHWW6YV5PFDGCTZYPWDSJEFE1G2THV3VAZVN28ECXY7ZNBYANFEG452TZZ3X2Q1GCYM8EWCRVGKWD5KANKTXA1MV0YWKRBKBAZYVXXJRM70WKCN2X1CX96TVXKFRW92YJBT5ZCFSVM37ZD5HKVFYYYMJVS05KA6TXFY6ZE4H6NQX8J3VAX79TTF82VPSV1KVR8W9V7BM1N3MEY5QHACSFNCK7VWPNY52RXGC1G9BPBS1QWA7ZVM6T2E0WMDY7P6CXJ68RB4CHJCDSVR6000047S29YVT08000";

      var boundary = new List<Fence>(){ new Fence() };

      boundary[0].Points.Add(new FencePoint(2700.20170260547, 1225.08445683629, 0.0));
      boundary[0].Points.Add(new FencePoint(2700.16517351542, 1224.38744027628, 0.0));
      boundary[0].Points.Add(new FencePoint(2700.10136538994, 1223.16990871245, 0.0));
      boundary[0].Points.Add(new FencePoint(2889.7599542129, 1178.36648123432, 0.0));
      boundary[0].Points.Add(new FencePoint(2889.75893340242, 1178.41010195536, 0.0));
      boundary[0].Points.Add(new FencePoint(2705.18796425658, 1224.6913839156, 0.0));
      boundary[0].Points.Add(new FencePoint(2700.20170260547, 1225.08445683629, 0.0));

      var designBoundaryResult = DesignBoundaryHelper.ConvertBoundary(boundary, TOLERANCE, TestConsts.CELL_SIZE, DIMENSIONS_2012_DC_CSIB, FILE_NAME);

      designBoundaryResult.Should().NotBeNull();
    }
  }
}
