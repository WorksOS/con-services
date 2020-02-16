using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;

namespace VSS.Nighthawk.ReferenceIdentifierService.Data
{
  public class NHOpContextFactory : INHOpContextFactory
  {
    public INH_OP CreateContext()
    {
      return ObjectContextFactory.NewNHContext<INH_OP>();
    }
  }

}
