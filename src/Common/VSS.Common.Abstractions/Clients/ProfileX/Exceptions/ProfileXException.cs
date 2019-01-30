using System;
using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.ProfileX.Exceptions
{
  public class ProfileXException : Exception
  {
    [JsonProperty("code", Required = Required.Always)]
    public string Code { get; set; }

    [JsonProperty("message")]
    public string CustomMessage { get; set; }

    [JsonProperty("exceptionName")]
    public string ExceptionName { get; set; }

    public override string Message => $"{Code} {Message} {ExceptionName}";
  }
}