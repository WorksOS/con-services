using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.Productivity.Push.Models.Attributes;
using VSS.Productivity.Push.Models.Enums;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity3D.Push.Abstractions;

namespace VSS.Productivity3D.Push.Clients
{
  /// <summary>
  /// A notification hub client that will call methods decorated with the NotificationAttribute
  /// Also allows for sending notifications to other hub clients in other services (or even the currently running service if subscribed)
  /// </summary>
  public class NotificationHubClient : BaseClient, INotificationHubClient
  {
    private const string URL_KEY = "NOTIFICATION_HUB_URL";

    private List<MethodInfo> methodInfoCache = null;
    private readonly object methodInfoLock = new object();

    private readonly IServiceProvider serviceProvider;
    
    public NotificationHubClient(IServiceProvider serviceProvider, IConfigurationStore configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
    {
      this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public override string UrlKey => URL_KEY;

    /// <inheritdoc />
    /// <summary>
    /// NOTE: Connection.On Does not validate that the method name, return type or parameters match the interface
    /// </summary>
    public override void SetupCallbacks()
    {
      Connection.On<Notification>(nameof(INotificationHub.Notify), (key) =>
      {
        var methods = GetTypes(key.Type, key.Key);
        foreach (var methodInfo in methods)
        {
          if (methodInfo.DeclaringType == null)
            continue;

          Invoke(methodInfo, key.Uid);
         
        }
        Logger.LogInformation($"Got a notification with key {JsonConvert.SerializeObject(key)}");
      }); 
    }

    /// <inheritdoc />
    public Task Notify(Notification notification)
    {
      if(Connected)
        return Connection.InvokeAsync(nameof(INotificationHub.Notify), notification);

      // We could queue this up if it becomes a problem
      Logger.LogWarning("Attempt to send message while client disconnected. Notification not sent.");
      return Task.CompletedTask;
    }

    /// <summary>
    /// This method will attempt to invoke the Method with a Notification Attribute attached
    /// The method must have a signature 'void MethodName(Guid)'
    /// </summary>
    private void Invoke(MethodInfo method, Guid uid)
    {
      var classInstance = ActivatorUtilities.CreateInstance(serviceProvider, method.DeclaringType);

      if (classInstance == null)
      {
        Logger.LogError($"Got a notification that would execute a method {method.Name} on class " +
                        $"{method.DeclaringType.FullName} but cannot resolve the class using Service Resolution.");
      }

      var parameters = method.GetParameters();
      // Validate that we only have one parameter, and it is a Guid
      if (parameters.Length != 1 || parameters[0].ParameterType != typeof(Guid) || method.ReturnType != typeof(void))
      {
        Logger.LogError($"Attempting to execute {method.DeclaringType.FullName}::{method.Name}, but it's signature is incorrect. " +
                        $"It should be {typeof(void).Name} {method.Name}({typeof(Guid).FullName})");
      }

      Task.Run(() => method.Invoke(classInstance, new object[] {uid}));
    }

    /// <summary>
    /// Finds all methods in the currently loaded assemblies that match the Notification Type requested.
    /// </summary>
    private IEnumerable<MethodInfo> GetTypes(NotificationUidType uidType, string key)
    {
      List<MethodInfo> methods;
      lock (methodInfoLock)
      {
        if (methodInfoCache == null)
        {
          methodInfoCache = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes(typeof(NotificationAttribute), false).Length > 0)
            .ToList();
        }

        methods = methodInfoCache;
      }

      foreach (var methodInfo in methods)
      {
        if (methodInfo.GetCustomAttributes<NotificationAttribute>().Any(a =>
          a.Type == uidType && string.Compare(a.Key, key, StringComparison.OrdinalIgnoreCase) == 0))
        {
          yield return methodInfo;
        }
      }
    }
  }
}