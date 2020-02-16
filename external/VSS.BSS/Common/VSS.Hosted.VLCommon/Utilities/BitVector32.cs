using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  /// <summary>
  /// This presents a working BitVector32 replacement.
  /// </summary>
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public struct BitVector32
  {
    public BitVector32(Int32 data)
    {
      this.Data = data;
    }

    public Int32 Data;

    public bool this[int i]
    {
      get
      {
        if (i < 0 || i > 31)
        {
          return false;
        }

        return ((Data >> i) & 1) != 0;
      }

      set
      {
        if (value)
        {
          Data = Data | (1 << i);
        }
        else
        {
          Data = Data & ~(1 << i);
        }
      }
    }
  }
}
