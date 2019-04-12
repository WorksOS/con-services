﻿using System;
using Newtonsoft.Json;

namespace VSS.TRex.ConnectedSite.Gateway.Abstractions
{
  public interface IL1ConnectedSiteMessage : IConnectedSiteMessage
  {
    [JsonProperty("ts")]
    DateTime? Timestamp { get; set; }
  }
}