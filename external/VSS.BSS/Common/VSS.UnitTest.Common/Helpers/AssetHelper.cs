using System;
using System.Collections.Generic;
using System.Linq;

using VSS.UnitTest.Common.Contexts;
using VSS.UnitTest.Common.EntityBuilder;
using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common 
{
  public class AssetHelper : UnitTestBase
  {
    private const string PRODUCT_FAMILY = "Wheel Loader";
    private const string SERIAL_NUMBER = "123123456";

    public User GetActiveUsersUser(long activeUserID)
    {
      return (from user in Ctx.OpContext.UserReadOnly where user.ID == activeUserID select user).FirstOrDefault();
    }
  }
}