using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common.DeviceMessageConstructor.Settings
{
   // Please maintain the alignment for readablity
   public static class EnumMapper
   {
      // Primary Dictionary Key should be Requestbase Enum Name 
      // Nested Dictionary Key should be Requestbase from Input
      // Nested Dictionary Value should be KafkaEvent Property Name
      public static Dictionary<string, Dictionary<string, string>> Container = new Dictionary<string, Dictionary<string, string>>
      {
         //{
         //   "EnableMaintenanceModeEvent", new Dictionary<string, string>
         //   {
         //      {"StartTime", "StartUtc"},
         //      {"MaintenanceModeDuration", "Duration"}
         //   }
         //}
      };
   }
}
