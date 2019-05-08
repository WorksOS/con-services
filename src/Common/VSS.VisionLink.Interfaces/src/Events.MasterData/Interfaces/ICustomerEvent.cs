﻿using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Interfaces
{
  public interface ICustomerEvent
  {
    Guid CustomerUID { get; set; }
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}