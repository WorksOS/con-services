using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IDevicePingStateChangerService
    {
        Task<DateTime> GetAcknowledgeTimeUTCFromDeviceACKMessageForValidPing(string assetUID, string deviceUID, DateTime dt);
        Task<bool> UpsertPing(string assetUID, string deviceUID, DateTime eventUTC);
    }
}
