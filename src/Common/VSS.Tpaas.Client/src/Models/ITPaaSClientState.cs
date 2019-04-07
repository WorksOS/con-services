using System;

namespace VSS.Tpaas.Client.Models
{
  public interface ITPaaSClientState
  {
    string TokenType { get; set; }
    string TPaaSToken { get; set; }
    DateTime TPaaSTokenExpiry { get; set; }
  }
}