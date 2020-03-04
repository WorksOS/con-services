using System;
using System.Threading.Tasks;
using Utilities.IOC;

namespace AssetSettings
{
	public interface IStartUpObject
	{

		Task Initialize();
	}
}