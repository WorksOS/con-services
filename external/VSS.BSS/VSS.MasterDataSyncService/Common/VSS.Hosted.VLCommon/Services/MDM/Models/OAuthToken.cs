﻿namespace VSS.Hosted.VLCommon.Services.MDM.Models
{
  public class OAuthToken
  {
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string access_token { get; set; }
  }
}