using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common.DeviceMessageConstructor.Interfaces
{
    public interface IVssKafkaMessage
    {
        bool IsMessageValid();

        string GetKafkaTopicName();

        string GetKey();

        /// <summary>
        /// You must implement this field, but just accept VS' default implementation (public string MessageHash { get; set; }).  It will be auto-populated at build time with a MD5 hash of the entire class, allowing us to verify the class used to publish is the same class used to consume for data consistency and/or version consumers in the future.
        /// </summary>
        string MessageHash { get; set; }
    }
}
