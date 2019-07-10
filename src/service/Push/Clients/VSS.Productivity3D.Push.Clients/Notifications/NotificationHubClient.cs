using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Productivity.Push.Models.Attributes;
using VSS.Productivity.Push.Models.Enums;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Abstractions.Notifications;

namespace VSS.Productivity3D.Push.Clients.Notifications
{
  /// <summary>
  /// A notification hub client that will call methods decorated with the NotificationAttribute
  /// Also allows for sending notifications to other hub clients in other services (or even the currently running service if subscribed)
  /// </summary>
  public class NotificationHubClient : BaseClient, INotificationHubClient
  {
    private List<MethodInfo> methodInfoCache = null;
    private readonly object methodInfoLock = new object();

    private readonly IServiceProvider serviceProvider;

    public NotificationHubClient(IServiceProvider serviceProvider, IConfigurationStore configuration,
      IServiceResolution resolution,
      ILoggerFactory loggerFactory) : base(configuration, resolution, loggerFactory)
    {
      this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public override string HubRoute => HubRoutes.NOTIFICATIONS;

    /// <inheritdoc />
    /// <summary>
    /// NOTE: Connection.On Does not validate that the method name, return type or parameters match the interface
    /// </summary>
    public override void SetupCallbacks()
    {
      Connection.On<Notification>(nameof(INotificationHub.Notify), ProcessNotification); 
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
    /// Processes an incoming notification and return the tasks generated
    /// </summary>
    /// <param name="notification">The notification to be processed</param>
    public List<Task> ProcessNotificationAsTasks(Notification notification)
    {
      if (notification == null)
        return new List<Task>();

      var results = new List<Task>();
      var methods = GetTypes(notification.Type, notification.Key);
      foreach (var methodInfo in methods)
      {
        if (methodInfo.DeclaringType == null)
          continue;
 
        var task = Invoke(methodInfo, notification.Parameters ?? notification.Uid);
        results.Add(task);
      }
      Logger.LogInformation($"Got a notification with key {JsonConvert.SerializeObject(notification)}. Total {results.Count} Tasks run/queued");

      return results;
    }

    /// <summary>
    /// Process an incoming notification, but don't return anything
    /// Required for SignalR
    /// </summary>
    public void ProcessNotification(Notification notification)
    {
      ProcessNotificationAsTasks(notification);
    }

    /// <summary>
    /// This method will attempt to invoke the Method with a Notification Attribute attached
    /// The method must have a signature 'void MethodName(Guid)' or 'void MethodName(object)'
    /// </summary>
    private Task Invoke(MethodInfo method, object uidOrParams)
    {
      var classInstance = ActivatorUtilities.CreateInstance(serviceProvider, method.DeclaringType);

      if (classInstance == null)
      {
        Logger.LogError($"Got a notification that would execute a method {method.Name} on class " +
                        $"{method.DeclaringType.FullName} but cannot resolve the class using Service Resolution.");
        return Task.CompletedTask;
      }

      if (!ValidateMethod(method))
      {
        Logger.LogError($"Attempting to execute {method.DeclaringType.FullName}::{method.Name}, but its signature is incorrect. " +
                        $"It should be {typeof(void).Name}/{typeof(Task).Name} {method.Name}({typeof(Guid).FullName}) " +
                        $"or {typeof(void).Name}/{typeof(Task).Name} {method.Name}({typeof(object).FullName})");
        return Task.CompletedTask;
      }


      // Make sure we handle exceptions here here, and log them
      // Also pass up in case someone wants the exception (highly likely not, as this is called via SignalR)
      return method.ReturnType == typeof(Task)
        ? Task.Run(async () =>
        {
          try
          {
            await (Task) method.Invoke(classInstance, new[] {uidOrParams});
            Logger.LogInformation($"Successfully called async method {method.DeclaringType.FullName}::{method.Name}");
          }
          catch (Exception e)
          {
            Logger.LogError(e, $"Failed to notify async method {method.DeclaringType.FullName}::{method.Name} due to error.");
            throw;
          }
        })
        : Task.Run(() =>
        {
          try
          {
            method.Invoke(classInstance, new[] {uidOrParams});
            Logger.LogInformation($"Successfully called method {method.DeclaringType.FullName}::{method.Name}");
          }
          catch (Exception e)
          {
            Logger.LogError(e, $"Failed to notify method {method.DeclaringType.FullName}::{method.Name} due to error.");
            throw;
          }
        });
    }

    /// <summary>
    /// Validate method signature.
    /// Ensure that we only have one parameter, and it is a Guid or an object
    /// And the return type is void or Task.
    /// </summary>
    private bool ValidateMethod(MethodInfo method)
    {
      if (method == null)
        return false;

      var parameters = method.GetParameters();

      if (parameters.Length != 1)
        return false;

      if (parameters[0].ParameterType != typeof(Guid) && parameters[0].ParameterType != typeof(object))
        return false;

      if (method.ReturnType != typeof(void) && method.ReturnType != typeof(Task))
        return false;

      return true;
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
          // Load the methods for the notification hub into a cached list.
          // Validate these at generation as well.
          var assemblies = AppDomain.CurrentDomain.GetAssemblies();
          methodInfoCache = new List<MethodInfo>();
          foreach (var assembly in assemblies)
          {
            try
            {
              var localMethods = assembly
                .GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(NotificationAttribute), false).Length > 0)
                .ToList();

              foreach (var method in localMethods)
              {
                if (!ValidateMethod(method))
                {
                  // We want to log an error, but still add the method to the cache
                  // That way we still get errors if/when the event that triggers the method is generated.
                  Logger.LogError($"Method {method.DeclaringType.FullName}::{method.Name} is invalid. " +
                                  $"It should be {typeof(void).Name}/{typeof(Task).Name} {method.Name}({typeof(Guid).FullName}) " +
                                  $"or {typeof(void).Name}/{typeof(Task).Name} {method.Name}({typeof(object).FullName})");
                }
              }

              methodInfoCache.AddRange(localMethods);

              if (localMethods.Count > 0)
                Logger.LogInformation($"Assembly: {assembly.GetName().Name} has Notification Methods: {string.Join(", ", localMethods.Select(m => m.Name))}");
            }
            catch (ReflectionTypeLoadException e)
            {
              // This is perfectly acceptable, happens with some .NET assemblies
              Logger.LogWarning($"Failed to load assembly {assembly.FullName} due to load exception, {e.Message}");
            }
          }
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
