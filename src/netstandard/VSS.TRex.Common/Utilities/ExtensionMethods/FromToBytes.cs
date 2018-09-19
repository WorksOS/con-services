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
    public static void FromBytes<T>(this T item, byte[] bytes) where T : class, IBinaryReaderWriter
    {
      using (MemoryStream ms = new MemoryStream(bytes))
      {
        using (BinaryReader reader = new BinaryReader(ms))
        {
          item.Read(reader);
        }
      }
    }

    /// <summary>
    /// An extension method providing a FromBytes() semantic to deserialise a byte array via the class defined Read() implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="stream"></param>
    public static void FromStream<T>(this T item, Stream stream) where T : class, IBinaryReaderWriter
    {
        using (BinaryReader reader = new BinaryReader(stream))
        {
          item.Read(reader);
        }
    }

    /// <summary>
    /// An extension method providing a ToBytes() semantic to serialise its state to a byte array via the class defined Write() implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    public static byte[] ToBytes<T>(this T item) where T : class, IBinaryReaderWriter
    {
      using (MemoryStream ms = new MemoryStream())
      {
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
          item.Write(writer);
          return ms.ToArray();
        }
      }
    }

    /// <summary>
    /// An extension method providing a ToBytes() semantic to serialise its state to a byte array via the class defined Write() implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    public static MemoryStream ToStream<T>(this T item) where T : class, IBinaryReaderWriter
    {
      MemoryStream ms = new MemoryStream();

      using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true))
      {
          item.Write(writer);
      }

      ms.Position = 0;
      return ms;
    }

    /// <summary>
    /// An extension method providing a ToBytes() semantic to serialise its state to a byte array via the class defined Write() implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static byte[] ToBytes<T>(this T item, byte[] buffer) where T : class, IBinaryReaderWriter
    {
      using (MemoryStream ms = new MemoryStream())
      {
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
          item.Write(writer, buffer);
          return ms.ToArray();
        }
      }
    }
  }
}
