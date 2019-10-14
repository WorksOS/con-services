using System.Text;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean.Utils
{
  public class CalibrationFileHelper
  {
    private readonly byte[] _coordSystemFileContent;

    public CalibrationFileHelper(byte[] calibrationFileContent)
    {
      _coordSystemFileContent = calibrationFileContent;
    }

    public DxfUnitsType GetDxfUnitsType()
    {
      var dxfUnitsType = Encoding.UTF8.GetString(_coordSystemFileContent).Substring(41, 1);
      int.TryParse(dxfUnitsType, out var dxfUnits);

      return (DxfUnitsType)dxfUnits - 1;
    }
  }
}
