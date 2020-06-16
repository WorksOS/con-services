using System;
using System.Text;
using TagFiles.Types;

namespace TagFiles.Parser
{

  public abstract class TagData
  {
    public const byte BITS_PER_NYBBLE = 4;
    public const byte BITS_PER_TWO_NYBBLES = 8;
    public const byte NYBBLES_PER_BYTE = 2;
    public TAGDataType DataType;
    public short DictID;
    public abstract void Read(ref NybbleStream stream);
    public abstract void Write(ref NybbleStream stream);
    public abstract string AsString();
  }

  public class TagData_Empty : TagData
  {
    public override void Read(ref NybbleStream stream)
    {

    }
    public override void Write(ref NybbleStream stream)
    {
    }

    public override string AsString()
    {
      return "";
    }
  }

  public class TagData_UnsignedInt : TagData
  {
    public uint Data;
    public override void Read(ref NybbleStream stream)
    {

    }
    public override void Write(ref NybbleStream stream)
    {

      switch (DataType)
      {
        case TAGDataType.t4bitUInt:
          stream.WriteFixedSizeUnsignedInt(Data, 1);
          break;
        case TAGDataType.t8bitUInt:
          stream.WriteFixedSizeUnsignedInt(Data, 2);
          break;
        case TAGDataType.t12bitUInt:
          stream.WriteFixedSizeUnsignedInt(Data, 3);
          break;
        case TAGDataType.t16bitUInt:
          stream.WriteFixedSizeUnsignedInt(Data, 4);
          break;
        case TAGDataType.t32bitUInt:
          stream.WriteFixedSizeUnsignedInt(Data, 8);
          break;
      }

    }

    public override string AsString()
    {
      return "";
    }
  }


  public class TagData_SignedInt : TagData
  {
    public int Data;
    public override void Read(ref NybbleStream stream)
    {
    }
    public override void Write(ref NybbleStream stream)
    {

    }

    public override string AsString()
    {
      return "";
    }
  }

  public class TagData_Double : TagData
  {
    public double Data;

    public override void Read(ref NybbleStream stream)
    {
    }

    public override void Write(ref NybbleStream stream)
    {
      var dBytes = BitConverter.GetBytes(Data);
      for (int i = 0; i < dBytes.Length; i++)
      {
        stream.WriteFixedSizeUnsignedInt(dBytes[i], 2);
      }
    }

    public override string AsString()
    {
      return "";
    }
  }


  public class TagData_String : TagData
  {
    public string Data;
    public override void Read(ref NybbleStream stream)
    {
    }

    public override void Write(ref NybbleStream stream)
    {

      byte[] bytes = Encoding.ASCII.GetBytes(Data);
      for (int i = 0; i < bytes.Length; i++)
      {
        stream.WriteFixedSizeUnsignedInt((uint)(bytes[i]), 2);
      }
      stream.WriteFixedSizeUnsignedInt(0, 2); // End of Text
    }

    public override string AsString()
    {
      return "";
    }
  }


  public class TagData_Unicode : TagData
  {
    public string Data;
    public override void Read(ref NybbleStream stream)
    {
    }

    public override void Write(ref NybbleStream stream)
    {
      for (int i = 0; i < Data.Length; i++)
      {
        stream.WriteFixedSizeUnsignedInt((uint)(Data[i]), 4);
      }
      stream.WriteFixedSizeUnsignedInt(0, 4);
    }

    public override string AsString()
    {
      return "";
    }
  }


}
