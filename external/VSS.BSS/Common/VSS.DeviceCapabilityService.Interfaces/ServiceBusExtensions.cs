using System.Linq;
using Magnum.Extensions;
using MassTransit;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces
{
  public static class ServiceBusExtensions
  {
    /// <summary>
    /// Publish a message of a type that is the most-specialized derivative of type T.
    /// </summary>
    /// <typeparam name="T">The base or superclass type.</typeparam>
    /// <param name="bus">An instance of MassTransit.IServiceBus</param>
    /// <param name="message">The published message, which derives from generic argument T.</param>
    public static void PublishSpecificOf<T>(this IServiceBus bus, T message)
    {
      var interfaces = message.GetType().GetInterfaces().Where(type => type.Implements(typeof(T)) && (type != typeof(T))).ToList();
      var msgType = interfaces.Any() ? interfaces.First() : typeof(T);
      bus.Publish(message, msgType);
    }
  }
}
