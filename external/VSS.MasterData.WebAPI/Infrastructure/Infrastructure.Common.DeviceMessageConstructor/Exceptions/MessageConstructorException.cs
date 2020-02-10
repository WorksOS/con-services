using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common.DeviceMessageConstructor.Exceptions
{
   public class MessageConstructorException : Exception
   {
      public MessageConstructorException(string message) : base(message)
      {
         
      }
   }
}
