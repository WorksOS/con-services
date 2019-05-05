using System;
using System.Runtime.Serialization;

namespace VSS.Tpaas.Client.Models
{
  [Serializable]
  public class TPaaSAuthenticatedRequestHandlerException : Exception
  {
  //  public TPaaSAuthenticatedRequestHandlerException()
  //  {
  //  }

  //  public TPaaSAuthenticatedRequestHandlerException(string message) : base(message)
  //  {
  //  }

    public TPaaSAuthenticatedRequestHandlerException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected TPaaSAuthenticatedRequestHandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}
