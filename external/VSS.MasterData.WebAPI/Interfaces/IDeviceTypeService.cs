using System;
using System.Collections.Generic;
using System.Text;
using VSS.MasterData.WebAPI.DbModel.Device;

namespace VSS.MasterData.WebAPI.Interfaces
{
	public interface IDeviceTypeService
	{
		Dictionary<string, DbDeviceType> GetDeviceType();
	}
}