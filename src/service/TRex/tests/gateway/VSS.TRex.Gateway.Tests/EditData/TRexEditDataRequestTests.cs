using System;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Models.Models;
using Xunit;

namespace VSS.TRex.Gateway.Tests.EditData
{
  public class TRexEditDataRequestTests
  {
    [Theory]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", "408A150C-B606-E311-9E53-0050568824D7", "2018-10-12T09:30:00.000000Z", "2018-10-12T11:00:15.000000Z", "design 1", 1)]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", "408A150C-B606-E311-9E53-0050568824D7", "2018-10-12T09:30:00.000000Z", "2018-10-12T11:00:15.000000Z", "design 1", null)]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", "408A150C-B606-E311-9E53-0050568824D7", "2018-10-12T09:30:00.000000Z", "2018-10-12T11:00:15.000000Z", null, 1)]
    public void TRexEditDataRequestValidation_HappyPath(string projectUid, string assetUid, DateTime startUtc, DateTime endUtc, string machineDesignName, int? liftNumber)
    {
      var request = new TRexEditDataRequest(Guid.Parse(projectUid), Guid.Parse(assetUid), startUtc, endUtc, machineDesignName, liftNumber);
      request.Validate();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "408A150C-B606-E311-9E53-0050568824D7", "2018-10-12T09:30:00.000000Z", "2018-10-12T11:00:15.000000Z", "design 1", 1, -1, "Missing ProjectUid")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", "00000000-0000-0000-0000-000000000000", "2018-10-12T09:30:00.000000Z", "2018-10-12T11:00:15.000000Z", "design 1", 1, -1, "Missing AssetUid")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", "408A150C-B606-E311-9E53-0050568824D7", "0001-01-01T00:00:00.000000Z", "2018-10-12T11:00:15.000000Z", "design 1", 1, -1, "Invalid override date range")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", "408A150C-B606-E311-9E53-0050568824D7", "2018-10-12T09:30:00.000000Z", "0001-01-01T00:00:00.000000Z", "design 1", 1, -1, "Invalid override date range")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", "408A150C-B606-E311-9E53-0050568824D7", "2018-10-12T11:00:15.000000Z", "2018-10-12T09:30:00.000000Z", "design 1", 1, -1, "Invalid override date range")]
    [InlineData("203A150C-B606-E311-9E53-0050568824D7", "408A150C-B606-E311-9E53-0050568824D7", "2018-10-12T09:30:00.000000Z", "2018-10-12T11:00:15.000000Z", null, null, -1, "Nothing to edit")]
    public void TRexEditDataRequestValidation_Errors(string projectUid, string assetUid, DateTime startUtc, DateTime endUtc, string machineDesignName, int? liftNumber,
      int expectedCode, string expectedMessage)
    {
      var request = new TRexEditDataRequest(Guid.Parse(projectUid), Guid.Parse(assetUid), startUtc.ToUniversalTime(), endUtc.ToUniversalTime(), machineDesignName, liftNumber);
      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.Equal(expectedCode, ex.GetResult.Code);
      Assert.Equal(expectedMessage, ex.GetResult.Message);
    }
  }
}
