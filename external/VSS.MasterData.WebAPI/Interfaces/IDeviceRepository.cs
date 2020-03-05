using DbModel;
using DbModel.DeviceConfig;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
	public interface IDeviceRepository
	{
		Task<DeviceData> Fetch(string serialNumber);
	}
}
