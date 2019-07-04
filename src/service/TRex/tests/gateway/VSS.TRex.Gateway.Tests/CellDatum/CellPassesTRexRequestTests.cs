using System;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using Xunit;

namespace VSS.TRex.Gateway.Tests.CellDatum
{
  public class CellPassesTRexRequestTests
  {
    [Fact]
    public void CellPassesRequest_ValidateSuccess()
    {
      var req = new CellPassesTRexRequest(Guid.NewGuid(),  new Point(123.456, 987.654), null);
      req.Validate();

      var req2 = new CellPassesTRexRequest(Guid.NewGuid(), new WGSPoint(0.01, 0.01), null);
      req2.Validate();
    }

    [Fact]
    public void CellPassesRequest_ValidateMissingProjectUid()
    {
      var req = new CellPassesTRexRequest(Guid.Empty, new Point(123.456, 987.654), null);
      Assert.Throws<ServiceException>(() => req.Validate());
    }

    [Fact]
    public void CellPassesRequest_ValidateMissingPoint()
    {
      var req = new CellPassesTRexRequest(Guid.NewGuid(), (Point)null, null);
      Assert.Throws<ServiceException>(() => req.Validate());
    }
  }
}
