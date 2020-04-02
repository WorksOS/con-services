using System;

namespace VSS.Common.Abstractions.Exceptions
{
  public abstract class ProductivityException : Exception
  {
    protected ProductivityException(string message) : base(message)
    {
      
    }

    protected ProductivityException(string message, Exception innerException) : base(message, innerException)
    {
      
    }
  }
}
