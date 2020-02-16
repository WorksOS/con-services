using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  /// <summary>
  /// This presents a BitArray replacement that adds many enhancements we specifically need to deal
  /// with our bit vectors.
  /// </summary>
  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
  public struct BitArray
  {
    public byte[] Data;

    public void Create(int bitCount)
    {
      Data = new byte[(bitCount + 7) / 8];
    }

    public void Create(List<bool> bitValues)
    {      
      if (bitValues != null)
      {
        Create(bitValues.Count);
        for (int i = 0; i < bitValues.Count; i++)
        {
          this[i] = bitValues[i];
        }
      }
    }
    
    public void Create(bool[] bitValues)
    {
      Create(bitValues.Length);
      if (bitValues != null)
      {
        for (int i = 0; i < bitValues.Length; i++)
        {
          this[i] = bitValues[i];
        }
      }
    }
  
    public bool IsNull
    {
      get { return Data == null; }
    }

    public bool this[int i]
    {
      get
      {
        return (Data[i / 8] & (1 << (i % 8))) != 0;
      }

      set
      {
        if (value)
        {
          Data[i / 8] |= (byte)(1 << (i % 8));
        }
        else
        {
          Data[i / 8] &= (byte)~(1 << (i % 8));
        }
      }
    }

    public int FindFirst(int startBitIndex, bool value)
    {
      int bitCount = Data.Length * 8 - startBitIndex;

      while (bitCount > 0)
      {
        byte checkByte = Data[startBitIndex / 8];

        int shift = startBitIndex % 8;

        if (shift > 0)
        {
          checkByte >>= shift;
        }

        int mask = (bitCount >= 8) ? 0xff : (0xff >> (8 - bitCount));

        // Check for the target value in the mask of bits

        if ((checkByte & mask) != (value ? 0 : mask))
        {
          while (true)
          {
            if ((checkByte & 1) == (value ? 1 : 0))
            {
              // This is it

              return startBitIndex;
            }

            startBitIndex++;
            checkByte >>= 1;
          }
        }

        // We didn't find a value in that byte, continue

        startBitIndex += 8 - shift;
        bitCount -= 8 - shift;
      }

      return -1;
    }

    public void SetRange(int startBitIndex, int bitCount, bool value)
    {
      if ((startBitIndex + bitCount) > Data.Length * 8)
      {
        throw new ArgumentException("bit range exceeds array", "bitCount");
      }

      if ((startBitIndex % 8) != 0)
      {
        int bitsToDo = Math.Min(bitCount, 8 - (startBitIndex % 8));

        // Mask of 'bitsToDo's worth of 1s right justified (e.g. for 3 results in 0x07)

        int mask = (0xff >> (8 - bitsToDo));

        if (value)
        {
          Data[startBitIndex / 8] |= (byte)((mask) << (startBitIndex % 8));
        }
        else
        {
          Data[startBitIndex / 8] &= (byte)~((mask) << (startBitIndex % 8));
        }

        startBitIndex += bitsToDo;
        bitCount -= bitsToDo;
      }

      // Do as many whole bytes as possible.

      while (bitCount > 8)
      {
        Data[startBitIndex / 8] = (byte)(value ? 0xff : 0x00);

        startBitIndex += 8;
        bitCount -= 8;
      }

      // To the remainder.

      if (bitCount > 0)
      {
        int mask = (0xff >> (8 - bitCount));

        if (value)
        {
          Data[startBitIndex / 8] |= (byte)mask;
        }
        else
        {
          Data[startBitIndex / 8] &= (byte)~mask;
        }
      }
    }
  }
}
