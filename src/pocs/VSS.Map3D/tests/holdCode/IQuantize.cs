using System;
using System.Threading.Tasks;
using VSS.Map3D.Models;

namespace VSS.Map3D.Quantize
{
  public interface IQuantize
  {
    Task<byte[]> QuantizeDEMAsync(ElevationData ed);

  }
}
