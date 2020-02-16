using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
  [TestClass()]
  public class FirmwareAPITest : UnitTestBase
  {
    #region Create Method Tests

    [DatabaseTest]
    [TestMethod()]
    public void Create_SuccessFirmwareVersion()
    {
        long serviceProviderRecordID = CreateServiceProviderRecord(Ctx.OpContext);

        // create new firmware record to check
        string versionName = "Version 3.3.3";
        string sourceFolder = "Source Folder 3.3";
        string notes = "My notes here";

        FirmwareAPI fwApi = new FirmwareAPI();
        long newFwVersionId = fwApi.Create(Ctx.OpContext,
                                           serviceProviderRecordID, 
                                           versionName, 
                                           sourceFolder, 
                                           notes);

        Assert.IsTrue(newFwVersionId > 0, "Invalid firmware version ID");
    }

    [DatabaseTest]
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException), "Firmware Version name is null or empty")]
    public void Create_FailureFirmwareVersionWithInvalidVersionName()
    {
      long serviceProviderRecordID = CreateServiceProviderRecord(Ctx.OpContext);

        // create new firmware record to check
        string versionName = string.Empty;
        string sourceFolder = "Source Folder 3.3";
        string notes = "My notes here";

        FirmwareAPI fwApi = new FirmwareAPI();
        long newFwVersionId = fwApi.Create(Ctx.OpContext,
                                           serviceProviderRecordID, 
                                           versionName, 
                                           sourceFolder, 
                                           notes);
    }

    [DatabaseTest]
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException), "Service Provider ID is invalid")]
    public void Create_FailureFirmwareVersionWithInvalidProviderId()
    {
        long serviceProviderRecordID = -1;

        // create new firmware record to check
        string versionName = "Version 3.3.3";
        string sourceFolder = "Source Folder 3.3";
        string notes = "My notes here";

        FirmwareAPI fwApi = new FirmwareAPI();
        long newFwVersionId = fwApi.Create(Ctx.OpContext,
                                           serviceProviderRecordID,
                                           versionName,
                                           sourceFolder,
                                           notes);
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException), "Source Folder is invalid")]
    public void Create_FailureFirmwareVersionWithEmptySourceFolder()
    {
      long serviceProviderRecordID = CreateServiceProviderRecord(Ctx.OpContext);

        // create new firmware record to check
        string versionName = "Version 3.3.3";
        string sourceFolder = String.Empty;
        string notes = "My notes here";

        FirmwareAPI fwApi = new FirmwareAPI();
        long newFwVersionId = fwApi.Create(Ctx.OpContext,
                                           serviceProviderRecordID,
                                           versionName,
                                           sourceFolder,
                                           notes);
    }

    [DatabaseTest]
    [TestMethod]
    [Ignore]
    [ExpectedException(typeof(InvalidOperationException), "Service Provider ID is invalid")]
    public void Create_FailureSaveChangesException()
    {

    }

        #endregion

    #region Update Method Tests
        /* unused
    [DatabaseTest]
    [TestMethod()]
    public void Update_SuccessFirmwareVersion()
    {
      long serviceProviderRecordID = CreateServiceProviderRecord(Ctx.OpContext);

      // create new firmware record to check
      string versionName = "Version 3.3.3";
      string sourceFolder = "Source Folder 3.3";
      string notes = "My notes here";

      FirmwareAPI fwApi = new FirmwareAPI();
      long newFwVersionId = fwApi.Create(Ctx.OpContext,
                                         serviceProviderRecordID,
                                         versionName,
                                         sourceFolder,
                                         notes);

      Param p = new Param();
      p.Name = "Name";
      p.Value ="New Name";
      List<Param> lst = new List<Param>();
      lst.Add(p);

      bool updatedVersionID = fwApi.Update(Ctx.OpContext, newFwVersionId, lst);

      Assert.IsTrue(updatedVersionID, "Update not successful.");
    }

    [Ignore()]
    [ExpectedException(typeof(InvalidOperationException), "Firmware Version name is null or empty")]
    public void Update_FirmwareVersionWithInvalidFirmwareVersion()
    {
      
    }

    [Ignore]
    [ExpectedException(typeof(InvalidOperationException), "Service Provider ID is invalid")]
    public void Update_FirmwareVersionWithInvalidServiceProviderId()
    {

    }

    [Ignore()]
    [ExpectedException(typeof(InvalidOperationException), "Source Folder is invalid")]
    public void Update_FirmwareVersionWithInvalidSourceFolder()
    {

    }

    [Ignore()]
    [ExpectedException(typeof(InvalidOperationException), "Firmware version already exists.")]
    public void CheckValidVersion_FailureFirmwareVersionNotFound()
    {

    }
    */
        #endregion

    #region Helpers

    private long CreateServiceProviderRecord(INH_OP ctx)
    {
      // create a provider for the new firmware record
      ServiceProvider sp = new ServiceProvider();
      sp.ProviderName = "MyProviderName";
      sp.ServerIPAddress = "1.2.3.4";
      sp.UserName = "Keith Richards";
      sp.Password = "Stoner";
      sp.UpdateUTC = DateTime.UtcNow;

      ctx.ServiceProvider.AddObject(sp);
      bool isSuccessful = ctx.SaveChanges() >= 0;

      if (!isSuccessful)
        throw new InvalidOperationException("Error creating Service Provider record");

      var serviceProviderRecord = (from spr in ctx.ServiceProviderReadOnly
                                    where spr.ProviderName == sp.ProviderName
                                    select spr).FirstOrDefault();

      return serviceProviderRecord.ID;
    }

    private long CreateMts500FirmwareVersion(INH_OP ctx)
    {
      long serviceProviderRecordID = CreateServiceProviderRecord(ctx);

      // create initial record to check against
      string versionName = "Version 3.3.3";
      string sourceFolder = "Source Folder 3.3";
      string notes = "My notes here";

      FirmwareAPI fwApi = new FirmwareAPI();
      long newFwVersionId = fwApi.Create(ctx,
                                         serviceProviderRecordID,
                                         versionName,
                                         sourceFolder,
                                         notes);
      return newFwVersionId;
    }

    #endregion
  }
}
