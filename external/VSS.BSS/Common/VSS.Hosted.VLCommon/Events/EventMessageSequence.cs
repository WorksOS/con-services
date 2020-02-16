using System;
using System.Collections.Generic;
using MassTransit;

namespace VSS.Hosted.VLCommon.Events
{
  public class EventMessageSequence : List<CorrelatedBy<Guid>>
  {
  }
}
