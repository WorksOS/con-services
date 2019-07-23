using System;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using Xunit;

namespace VSS.TRex.Gateway.Tests.CellDatum
{
  public class CellDatumTRexRequestTests
  {
    [Fact]
    public void CellDatumRequest_ValidateSuccess()
    {
      var request = new CellDatumTRexRequest(Guid.NewGuid(), DisplayMode.PassCount, null, new Point(123.456, 987.654), null, null, 0, null);
      request.Validate();
    }

    [Fact]
    public void CellDatumRequest_ValidateMissingPoint()
    {
      var request = new CellDatumTRexRequest(Guid.NewGuid(), DisplayMode.PassCount, null, null, null, null, 0, null);
      Assert.Throws<ServiceException>(() => request.Validate());
    }

    [Fact]
    public void CellDatumRequest_ValidateTooManyPoints()
    {
      var request = new CellDatumTRexRequest(Guid.NewGuid(), DisplayMode.PassCount, new WGSPoint(123.456, 987.654), new Point(123.456, 987.654), null, null, 0, null);
      Assert.Throws<ServiceException>(() => request.Validate());
    }

    [Fact]
    public void CellDatumRequest_ValidateInvalidProjectUId()
    {
      var request = new CellDatumTRexRequest(Guid.Empty, DisplayMode.PassCount, new WGSPoint(123.456, 987.654), new Point(123.456, 987.654), null, null, 0, null);
      Assert.Throws<ServiceException>(() => request.Validate());
    }

    [Fact]
    public void CellDatumRequest_ValidateInvalidDesignUid()
    {
      var request = new CellDatumTRexRequest(Guid.NewGuid(), DisplayMode.PassCount, new WGSPoint(123.456, 987.654), new Point(123.456, 987.654), null, Guid.Empty, 0, null);
      Assert.Throws<ServiceException>(() => request.Validate());
    }
  }
}
