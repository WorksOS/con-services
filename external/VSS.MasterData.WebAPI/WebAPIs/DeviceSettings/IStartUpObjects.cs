using System.Threading.Tasks;

namespace DeviceSettings
{
	public interface IStartUpObject
	{
		Task Initialize();
	}
}