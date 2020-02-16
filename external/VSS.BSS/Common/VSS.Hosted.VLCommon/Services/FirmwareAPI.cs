using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;

using log4net;
using System.Transactions;
using VSS.Hosted.VLCommon;
using System.Reflection;
using System.ServiceModel;

namespace VSS.Hosted.VLCommon
{
  internal class FirmwareAPI : IFirmwareAPI
  {
    private static readonly ILog log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

    public long Create(INH_OP ctx, long serviceProviderID, string versionName, string sourceFolder, string notes)
    {
      long id = -1;

      if (serviceProviderID <= 0)
        throw new InvalidOperationException("Service Provider ID is invalid");

      if (string.IsNullOrEmpty(sourceFolder))
        throw new InvalidOperationException("Source Folder is invalid");

      if (string.IsNullOrEmpty(versionName))
        throw new InvalidOperationException("Firmware Version name is null or empty");

      // check for duplicate firmware version here
      CheckValidVersion(ctx, versionName, serviceProviderID, sourceFolder);
      
      // add new firmware version here
      MTS500FirmwareVersion fwVersion = new MTS500FirmwareVersion{ Name=versionName, SourceFolder=sourceFolder, IsActive=true};
      fwVersion.fk_ServiceProviderID = serviceProviderID;
      fwVersion.Notes = notes;
      ctx.MTS500FirmwareVersion.AddObject(fwVersion);

      int result = ctx.SaveChanges();

      if (result < 0)
        throw new InvalidOperationException("Error creating Firmware Version");
      
      id = fwVersion.ID;
      
      return id;
    }

		/*removed as unused in BSS
    public bool Update(INH_OP ctx, long versionID, List<Param> modifiedProperties)
    {
      MTS500FirmwareVersion fw = (from ff in ctx.MTS500FirmwareVersion where ff.ID == versionID select ff).Single();
      return API.Update<MTS500FirmwareVersion>(ctx, fw, modifiedProperties) != null;
    }*/

    private void CheckValidVersion(INH_OP ctx, string versionName, long serviceProviderID, string sourceFolder)
    {
      string containsVersion = (from s in ctx.MTS500FirmwareVersionReadOnly
                                where (s.fk_ServiceProviderID == serviceProviderID 
                                    && s.Name == versionName 
                                    && s.SourceFolder == sourceFolder)
                                    && s.IsActive == true
                                select s.Name).FirstOrDefault<string>();
      
      if (!string.IsNullOrEmpty(containsVersion))
      {
        throw new InvalidOperationException("Firmware version already exists.", new IntentionallyThrownException());
      }
    }
  }
}