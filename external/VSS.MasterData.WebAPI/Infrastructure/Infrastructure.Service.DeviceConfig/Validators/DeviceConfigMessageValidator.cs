using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;
using Infrastructure.Service.DeviceConfig.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public class DeviceConfigMessageValidator : IDeviceConfigMessageValidator
    {
        private readonly ILoggingService _loggingService;

        public DeviceConfigMessageValidator(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _loggingService.CreateLogger(this.GetType());
        }

        public bool ValidateDeviceConfigMessage(DateTime messageEventUTC, DateTime? lastAttrEventUTC)
        {
            if (lastAttrEventUTC == null || messageEventUTC > lastAttrEventUTC.Value)
                return true;
            else
                return false;
        }
    }
}
