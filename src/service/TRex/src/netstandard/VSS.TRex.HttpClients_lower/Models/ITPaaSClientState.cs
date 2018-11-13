using System;

namespace VSS.TRex.HttpClients.Models
{
  public interface ITPaaSClientState
  {
    string TokenType { get; set; }
    string TPaaSToken { get; set; }
    DateTime TPaaSTokenExpiry { get; set; }
  }
}