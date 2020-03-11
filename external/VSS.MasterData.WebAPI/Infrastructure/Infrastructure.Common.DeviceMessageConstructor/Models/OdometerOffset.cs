using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using System;

namespace Infrastructure.Common.DeviceMessageConstructor.Models
{
	public class OdometerOffset : IVssKafkaMessage
    {
        public Guid AssetId { get; set; }

        public double Offset { get; set; }

        public string GetKafkaTopicName()
        {
            return "Telematics_OdometerOffset";
        }

        public bool IsMessageValid()
        {
            return true;
        }

        public string GetKey()
        {
            return AssetId.ToString();
        }

        //this property auto-generated in Source.Objects.csproj, do not modify
        //this property auto-generated in Source.Objects.csproj, do not modify
        public string MessageHash { get; set; } = "C4E8F050CFD662519C6370C9D66356F8";

    }
}
