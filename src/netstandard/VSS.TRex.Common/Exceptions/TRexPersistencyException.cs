using System;
using VSS.TRex.Exceptions;

namespace VSS.TRex.Common.Exceptions
{
  class TRexPersistencyException : TRexException
  {
    public TRexPersistencyException(string message) : base(message)
    {
    }

    public TRexPersistencyException(string message, Exception E) : base(message, E)
    {
    }
  }
}
