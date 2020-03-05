using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common.DeviceMessageConstructor.Attributes
{
   public class GroupAttribute : Attribute
   {
      private readonly string _groupName;
      public GroupAttribute(string groupName)
      {
         _groupName = groupName;
      }

      public string GroupName
      {
         get
         {
            return _groupName;
         }
      }
   }
}
