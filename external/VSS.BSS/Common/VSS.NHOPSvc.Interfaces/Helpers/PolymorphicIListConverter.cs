using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon;
using System.Text;
using Newtonsoft.Json;
using VSS.Nighthawk.MassTransit;
using System.Collections;

namespace VSS.Nighthawk.NHOPSvc.Interfaces.Helpers
{
  public class PolymorphicIListConverter<TListType> : PolymorphicChildListConverter where TListType : class, IList
  {
    public PolymorphicIListConverter()
      : base(typeof(TListType))
    {
    }
  }
}
