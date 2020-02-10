using System.Threading.Tasks;

namespace VSS.MasterData.WebAPI.Device.DeviceConfig
{
	public interface IStartUpObject
	{
		Task Initialize();
	}
}