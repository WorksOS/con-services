using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.UnitTest.Common.Contexts;
using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common
{
  public class NHOpHelper
  {
    #region BSS
    public long CreateResponseEndpoint()
    {
      BSSResponseEndPoint endPoint = new BSSResponseEndPoint();
      endPoint.ID = IdGen.GetId();
      endPoint.DestinationURI = "Test";
      endPoint.SenderIP = "127.0.0";
      endPoint.UserName = null;
      endPoint.Password = null;
      ContextContainer.Current.OpContext.BSSResponseEndPoint.AddObject(endPoint);
      ContextContainer.Current.OpContext.SaveChanges();
      return endPoint.ID;
    }
    #endregion

  }
}
