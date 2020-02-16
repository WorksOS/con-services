using System.Collections.Generic;
using System.Linq;
using System.Text;
using MassTransit;
using VSS.Nighthawk.Instrumentation.Interfaces;
using VSS.Hosted.VLCommon;
using System;

namespace VSS.Nighthawk.NHOPSvc.Interfaces.Events
{
  public interface INewNhOPDataObjectEvent : CorrelatedBy<Guid>, IAuditableMessage
  {
    INHOPDataObject Message { get; set; }
  }
}


