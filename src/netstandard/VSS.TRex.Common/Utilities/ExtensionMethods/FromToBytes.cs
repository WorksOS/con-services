using System;
using System.IO;
using System.Text;
using VSS.TRex.Utilities.Interfaces;

namespace VSS.TRex.Utilities.ExtensionMethods
{
  /// <summary>
  /// Extension methods supporting serialisation and deserialisation to and from vanilla byte arrays.
  /// </summary>
  public static class FromToBytes
  {
    /*  An example that requires static extension methods to work...
            public static T FromBytes<T>(this T item, byte[] bytes) where T : class, IBinaryReaderWriter, new()
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        T newItem = new T();
                        newItem.Read(reader);
                        return newItem;
                    }
                }
            }
    */

    /// <summary>
    /// An extension method providing a FromBytes() semantic to deserialise a byte array via the class defined Read() implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="bytes"></param>
    public static void FromBytes<T>(this T item, byte[] bytes) where T : class, IBinaryReaderWriter => FromBytes(bytes, item.Read);

    /// <summary>
    /// An extension method providing a FromBytes() semantic to deserialise a byte array via the class defined Read() implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="stream"></param>
    public static void FromStream<T>(this T item, Stream stream) where T : class, IBinaryReaderWriter => FromStream(stream, item.Read);

    /// <summary>
    /// An extension method providing a ToBytes() semantic to serialise its state to a byte array via the class defined Write() implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    public static byte[] ToBytes<T>(this T item) where T : class, IBinaryReaderWriter => ToBytes(item.Write);

    /// <summary>
    /// An extension method providing a ToBytes() semantic to serialise its state to a byte array via the class defined Write() implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    public static MemoryStream ToStream<T>(this T item) where T : class, IBinaryReaderWriter => ToStream(item.Write);

    /// <summary>
    /// An extension method providing a ToBytes() semantic to serialise its state to a byte array via the class defined Write() implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static byte[] ToBytes<T>(this T item, byte[] buffer) where T : class, IBinaryReaderWriter => ToBytes(item.Write, buffer);

    /// <summary>
    /// A generic method providing a ToBytes() semantic to serialise its state to a byte array via the class defined Write() implementation
    /// </summary>
    /// <param name="serialiser"></param>
    /// <returns></returns>
    public static byte[] ToBytes(Action<BinaryWriter> serialiser)
    {
      using (MemoryStream ms = new MemoryStream())
      {
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
          serialiser(writer);
          return ms.ToArray();
        }
      }
    }

    /// <summary>
    /// A generic method providing a ToBytes() semantic to serialise its state to a byte array via the class defined Write() implementation
    /// </summary>
    /// <param name="serialiser"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static byte[] ToBytes(Action<BinaryWriter> serialiser, byte[] buffer)
    {
      using (MemoryStream ms = new MemoryStream())
      {
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
          serialiser(writer);
          return ms.ToArray();
        }
      }
    }

    /// <summary>
    /// A generic method providing a ToBytes() semantic to serialise its state to a byte array via the class defined Write() implementation
    /// </summary>
    /// <param name="serialiser"></param>
    /// <returns></returns>
    public static MemoryStream ToStream(Action<BinaryWriter> serialiser)
    {
      MemoryStream ms = new MemoryStream();

      using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true))
      {
        serialiser(writer);
      }

      ms.Position = 0;
      return ms;
    }

    /// <summary>
    /// A generic providing a FromBytes() semantic to deserialise a byte array via the class defined Read() implementation
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="serialiser"></param>
    public static void FromBytes(byte[] bytes, Action<BinaryReader> serialiser)
    {
      using (MemoryStream ms = new MemoryStream(bytes))
      {
        using (BinaryReader reader = new BinaryReader(ms))
        {
          serialiser(reader);
        }
      }
    }

    /// <summary>
    /// An extension method providing a FromBytes() semantic to deserialise a byte array via the class defined Read() implementation
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="serialiser"></param>
    public static void FromStream(Stream stream, Action<BinaryReader> serialiser)
    {
      using (BinaryReader reader = new BinaryReader(stream))
      {
        serialiser(reader);
      }
    }
  }
}
