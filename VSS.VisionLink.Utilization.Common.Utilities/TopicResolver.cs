using System;

namespace VSS.VisionLink.Landfill.Common.Utilities
{
  /// <summary>
  ///   This class resolves topic name to specific Kafka queue
  /// </summary>
  public static class TopicResolver
  {
    /// <summary>
    ///   Resolves the kafka topic.
    /// </summary>
    /// <param name="topicName">Name of the topic.</param>
    /// <returns>Type of generic IKafkaQueue to resolve by dependency container</returns>
    public static Type ResolveKafkaTopic(string topicName)
    {
      Type T;
      try
      {
        var typename =
          string.Format(
            "VSS.VisionLink.Landfill.Common.Interfaces.IKafkaQueue`1[[{0},{1}]],VSS.VisionLink.Landfill.Common.Interfaces, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            topicName,
            "VSS.VisionLink.Interfaces.Events.Telematics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        T =
          Type.GetType(typename, false);

        if(T==null)
        {
          typename =
            string.Format(
            "VSS.VisionLink.Landfill.Common.Interfaces.IKafkaQueue`1[[{0},{1}]],VSS.VisionLink.Landfill.Common.Interfaces, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            topicName,
            "VSS.VisionLink.Interfaces.Events.MasterData, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
          T =
            Type.GetType(typename, false);
        }
      }
      catch
      {
        return null;
      }
      return T;
    }
  }
}