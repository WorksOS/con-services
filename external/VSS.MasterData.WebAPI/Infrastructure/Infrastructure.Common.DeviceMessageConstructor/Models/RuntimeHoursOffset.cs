using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using System;

namespace Infrastructure.Common.DeviceMessageConstructor.Models
{
	public class RuntimeHoursOffset : IVssKafkaMessage
    {
        public Guid AssetId { get; set; }

        public double Offset { get; set; }

        public string GetKafkaTopicName()
        {
            return "Telematics_RuntimeHoursOffset";
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
        public string MessageHash { get; set; } = "D9C5D0BCFDEBD0410A30E37E3E8631EF";

    }
}
