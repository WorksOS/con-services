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
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using GeometryRequest = VSS.TRex.Designs.GridFabric.Requests.AlignmentDesignGeometryRequest;

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
      const string FILE_NAME = "Test.svl";
      const string DIMENSIONS_2012_DC_CSIB = "QM0G000ZHC4000000000800BY7SN2W0EYST640036P3P1SV09C1G61CZZKJC976CNB295K7W7G30DA30A1N74ZJH1831E5V0CHJ60W295GMWT3E95154T3A85H5CRK9D94PJM1P9Q6R30E1C1E4Q173W9XDE923XGGHN8JR37B6RESPQ3ZHWW6YV5PFDGCTZYPWDSJEFE1G2THV3VAZVN28ECXY7ZNBYANFEG452TZZ3X2Q1GCYM8EWCRVGKWD5KANKTXA1MV0YWKRBKBAZYVXXJRM70WKCN2X1CX96TVXKFRW92YJBT5ZCFSVM37ZD5HKVFYYYMJVS05KA6TXFY6ZE4H6NQX8J3VAX79TTF82VPSV1KVR8W9V7BM1N3MEY5QHACSFNCK7VWPNY52RXGC1G9BPBS1QWA7ZVM6T2E0WMDY7P6CXJ68RB4CHJCDSVR6000047S29YVT08000";

    }
  }
}
