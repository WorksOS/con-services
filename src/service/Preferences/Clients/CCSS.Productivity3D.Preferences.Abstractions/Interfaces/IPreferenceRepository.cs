using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.Preference.Interfaces;

namespace CCSS.Productivity3D.Preferences.Abstractions.Interfaces
{
  public interface IPreferenceRepository
  {
    Task<int> StoreEvent(IPreferenceEvent evt);
  }
}
