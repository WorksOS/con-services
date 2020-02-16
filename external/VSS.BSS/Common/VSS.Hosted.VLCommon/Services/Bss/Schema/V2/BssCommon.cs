using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace VSS.Hosted.VLCommon.Bss.Schema.V2
{
  [DataContract]
  public class BssCommon
  {
    public static string WriteXML<T>(T payload) where T : BssCommon
    {
      StringBuilder builder = new StringBuilder();

      using (XmlWriter writer = XmlWriter.Create(builder))
      {
        DataContractSerializer ser = new DataContractSerializer(typeof(T));
        // serialize the data and write it to the instance.
        ser.WriteObject(writer, payload);
        writer.Flush();
      }
      return builder.ToString();
    }

    public static T ReadXML<T>(string payload) where T : BssCommon, new()
    {
      T value = new T();
      using (XmlReader reader = XmlReader.Create(new StringReader(payload)))
      {
        DataContractSerializer ser = new DataContractSerializer(typeof(T));
        // Deserialize the data and read it from the instance.
        value = ser.ReadObject(reader, true) as T;
      }

      return value;
    }
  }

  public enum ActionEnum
  {
    Created = 0,
    Deleted = 1,
    Updated = 2,
    Deactivated = 3,
    Reactivated = 4,
    Swapped = 5,
    Replaced = 6,
    Activated = 7,
    Cancelled = 8,
    UpdatedMerge = 9,
    Registered = 10,
    Deregistered = 11,
  }
}
