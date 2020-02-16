#region Imports

using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using log4net;
using Microsoft.ServiceModel.Web;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects.Factory;
using Spring.ServiceModel.Support;
using Spring.Util;


#endregion

namespace VSS.Hosted.VLCommon
{
  /// <summary>
  /// Static Factory class to construct instances of various classes deriving from System.ServiceModel.ServiceHost and instrumented with Spring.NET.
  /// </summary>
  /// <remarks>
  /// NOTE: this class is a VERBATIM copy of Spring.ServiceModel.SpringWebServiceHost/Spring.ServiceModel.SpringServiceHost 
  /// with the exception that it does not extend System.ServiceModel.Web.WebServiceHost/System.ServiceModel.ServiceHost respectively
  /// and implements ctor logic of SpringWebServiceHost/SpringServiceHost instead to construct Spring-instrumented instances of classes
  /// deriving from ServiceHost. 
  /// 
  /// TO EXTEND: add desired Factory method for a new or existing type in the ServiceHost class hierarchy with a desired parameter list. Preferably, 
  /// re-use one of the existing Factory methods as a basis for this extension or follow existing pattern to implement Factory methods for a new
  /// ServiceHost type. The pattern for factory methods follows that of SpringWebServiceHost/SpringServiceHost constructors.
  /// 
  /// Original Apache License disclaimer from Spring.NET SpringWebServiceHost/SpringServiceHost source is included above.
  /// </remarks>
  public static class SpringServiceHostFactory
  {
    #region ServiceHost Factory Methods

    /// <summary>
    /// Creates a new instance of the <see cref="System.ServiceModel.ServiceHost"/> class.
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="System.ServiceModel.ServiceHost"/> instance.</returns>
    public static ServiceHost BuildServiceHost(Type serviceType, params Uri[] baseAddresses)
    {
      return BuildServiceHost(serviceType, (string)null, baseAddresses);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="System.ServiceModel.ServiceHost"/> class.
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="contextName">The name of the Spring context to use.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="System.ServiceModel.ServiceHost"/> instance.</returns>
    public static ServiceHost BuildServiceHost(Type serviceType, string contextName, params Uri[] baseAddresses)
    {
      ServiceHost host;
      try
      {
        IObjectFactory objectFactory = GetApplicationContext(contextName);
        host = BuildServiceHost(serviceType, objectFactory, baseAddresses);
      }
      catch (Exception)
      {
        host = BuildDefaultServiceHostInstance(serviceType, baseAddresses);
      }
      return host;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="System.ServiceModel.ServiceHost"/> class.
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="objectFactory">The <see cref="IObjectFactory"/> to use.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="System.ServiceModel.ServiceHost"/> instance.</returns>
    public static ServiceHost BuildServiceHost(Type serviceType, IObjectFactory objectFactory, params Uri[] baseAddresses)
    {
      return BuildServiceHost(serviceType, objectFactory, true, baseAddresses);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="System.ServiceModel.ServiceHost"/> class.
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="objectFactory">The <see cref="IObjectFactory"/> to use.</param>
    /// <param name="useServiceProxyTypeCache">Whether to cache the generated service proxy type.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="System.ServiceModel.ServiceHost"/> instance.</returns>
    public static ServiceHost BuildServiceHost(Type serviceType, IObjectFactory objectFactory, bool useServiceProxyTypeCache, params Uri[] baseAddresses)
    {
      ServiceHost host;
      if (serviceType == null)
      {
        throw new ArgumentException("The service type cannot be null.", "serviceType");
      }
      try
      {
        // public ServiceHost(Type serviceType, params Uri[] baseAddresses)
        host = new ServiceHost(CreateServiceType(serviceType.FullName, objectFactory, useServiceProxyTypeCache), baseAddresses);
      }
      catch (Exception)
      {
        host = BuildDefaultServiceHostInstance(serviceType, baseAddresses);
      }
      return host;
    }

    #endregion // ServiceHost Factory Methods

    #region WebServiceHost Factory Methods

    /// <summary>
    /// Creates a new instance of the <see cref="System.ServiceModel.Web.WebServiceHost"/> class.
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="System.ServiceModel.Web.WebServiceHost"/> instance.</returns>
    public static WebServiceHost BuildWebServiceHost(Type serviceType, params Uri[] baseAddresses)
    {
      return BuildWebServiceHost(serviceType, (string)null, baseAddresses);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="System.ServiceModel.Web.WebServiceHost"/> class.
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="contextName">The name of the Spring context to use.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="System.ServiceModel.Web.WebServiceHost"/> instance.</returns>
    public static WebServiceHost BuildWebServiceHost(Type serviceType, string contextName, params Uri[] baseAddresses)
    {
      WebServiceHost host;
      try
      {
        IObjectFactory objectFactory = GetApplicationContext(contextName);
        host = BuildWebServiceHost(serviceType, objectFactory, baseAddresses);
      }
      catch (Exception)
      {
        host = BuildDefaultWebServiceHostInstance(serviceType, baseAddresses);
      }
      return host;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="System.ServiceModel.Web.WebServiceHost"/> class.
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="objectFactory">The <see cref="IObjectFactory"/> to use.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="System.ServiceModel.Web.WebServiceHost"/> instance.</returns>
    public static WebServiceHost BuildWebServiceHost(Type serviceType, IObjectFactory objectFactory, params Uri[] baseAddresses)
    {
      return BuildWebServiceHost(serviceType, objectFactory, true, baseAddresses);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="System.ServiceModel.Web.WebServiceHost"/> class.
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="objectFactory">The <see cref="IObjectFactory"/> to use.</param>
    /// <param name="useServiceProxyTypeCache">Whether to cache the generated service proxy type.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="System.ServiceModel.Web.WebServiceHost"/> instance.</returns>
    public static WebServiceHost BuildWebServiceHost(Type serviceType, IObjectFactory objectFactory, bool useServiceProxyTypeCache, params Uri[] baseAddresses)
    {
      WebServiceHost host;
      if (serviceType == null)
      {
        throw new ArgumentException("The service type cannot be null.", "serviceType");
      }
      try
      {
        // public WebServiceHost(Type serviceType, params Uri[] baseAddresses)
        host = new WebServiceHost(CreateServiceType(serviceType.FullName, objectFactory, useServiceProxyTypeCache), baseAddresses);
      }
      catch (Exception)
      {
        host = BuildDefaultWebServiceHostInstance(serviceType, baseAddresses);
      }
      return host;
    }

    #endregion // WebServiceHost Factory Methods

    #region WebServiceHost2 Factory Methods

    /// <summary>
    /// Creates a new instance of the <see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> class.
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> instance.</returns>
    public static WebServiceHost2 BuildWebServiceHost2(Type serviceType, params Uri[] baseAddresses)
    {
      return BuildWebServiceHost2(serviceType, (string)null, baseAddresses);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> class.
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="contextName">The name of the Spring context to use.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> instance.</returns>
    public static WebServiceHost2 BuildWebServiceHost2(Type serviceType, string contextName, params Uri[] baseAddresses)
    {
      WebServiceHost2 host;
      try
      { 
        IObjectFactory objectFactory = GetApplicationContext(contextName);
        host = BuildWebServiceHost2(serviceType, objectFactory, baseAddresses);
      }
      catch (Exception)
      {
        host = BuildDefaultWebServiceHost2Instance(serviceType, baseAddresses);
      }
      return host;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> class.
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="objectFactory">The <see cref="IObjectFactory"/> to use.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> instance.</returns>
    public static WebServiceHost2 BuildWebServiceHost2(Type serviceType, IObjectFactory objectFactory, params Uri[] baseAddresses)
    {
      return BuildWebServiceHost2(serviceType, objectFactory, true, baseAddresses);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> class.
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="objectFactory">The <see cref="IObjectFactory"/> to use.</param>
    /// <param name="useServiceProxyTypeCache">Whether to cache the generated service proxy type.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> instance.</returns>
    public static WebServiceHost2 BuildWebServiceHost2(Type serviceType, IObjectFactory objectFactory, bool useServiceProxyTypeCache, params Uri[] baseAddresses)
    {
      WebServiceHost2 host;
      if (serviceType == null)
      {
        throw new ArgumentException("The service type cannot be null.", "serviceType");
      }
      try
      {
        // public WebServiceHost2(Type serviceType, bool dummy, Uri[] baseAddresses) : base(serviceType, baseAddresses)
        host = new WebServiceHost2(CreateServiceType(serviceType.FullName, objectFactory, useServiceProxyTypeCache), false, baseAddresses);
      }
      catch (Exception)
      {
        host = BuildDefaultWebServiceHost2Instance(serviceType, baseAddresses);
      }
      return host;
    }

    #endregion // WebServiceHost2 Factory Methods

    #region WebServiceHost2 Singleton Factory Methods

    /// <summary>
    /// Creates a new instance of the <see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> class. 
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> instance.</returns>
    public static WebServiceHost2 BuildWebServiceHost2Singleton(Type serviceType, params Uri[] baseAddresses)
    {
      return BuildWebServiceHost2Singleton(serviceType, (string)null, baseAddresses);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> class. 
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="contextName">The name of the Spring context to use.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> instance.</returns>
    public static WebServiceHost2 BuildWebServiceHost2Singleton(Type serviceType, string contextName, params Uri[] baseAddresses)
    {
      WebServiceHost2 host;
      try
      {
        IObjectFactory objectFactory = GetApplicationContext(contextName);
        host = BuildWebServiceHost2Singleton(serviceType, objectFactory, baseAddresses);
      }
      catch (Exception)
      {
        host = BuildDefaultWebServiceHost2SingletonInstance(serviceType, baseAddresses);
      }
      return host;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> class. 
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="objectFactory">The <see cref="IObjectFactory"/> to use.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> instance.</returns>
    public static WebServiceHost2 BuildWebServiceHost2Singleton(Type serviceType, IObjectFactory objectFactory, params Uri[] baseAddresses)
    {
      return BuildWebServiceHost2Singleton(serviceType, objectFactory, true, baseAddresses);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> class.
    /// </summary>
    /// <param name="serviceType">The type of the service within Spring's IoC container.</param>
    /// <param name="objectFactory">The <see cref="IObjectFactory"/> to use.</param>
    /// <param name="useServiceProxyTypeCache">Whether to cache the generated service proxy type.</param>
    /// <param name="baseAddresses">The base addresses for the hosted service.</param>
    /// <returns><see cref="Microsoft.ServiceModel.Web.WebServiceHost2"/> instance.</returns>
    public static WebServiceHost2 BuildWebServiceHost2Singleton(Type serviceType, IObjectFactory objectFactory, bool useServiceProxyTypeCache, params Uri[] baseAddresses)
    {
      WebServiceHost2 host;
      if (serviceType == null)
      {
        throw new ArgumentException("The service type cannot be null.", "serviceType");
      }
      try
      {
        // public WebServiceHost2(object singletonInstance, Uri[] baseAddresses) : base(singletonInstance, baseAddresses)
        host = new WebServiceHost2(Activator.CreateInstance(CreateServiceType(serviceType.FullName, objectFactory, useServiceProxyTypeCache)), baseAddresses);
      }
      catch (Exception)
      {
        host = BuildDefaultWebServiceHost2SingletonInstance(serviceType, baseAddresses);
      }
      return host;
    }

    #endregion // WebServiceHost2 Singleton Factory Methods

    #region Misc Utility Methods

    private static void LogDebug(string format, params string[] msgs)
    {
      if (!String.IsNullOrEmpty(format))
      {
        ILog log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);
        log.IfDebug(String.Format(format, msgs));
      }
    }

    private static ServiceHost BuildDefaultServiceHostInstance(Type serviceType, params Uri[] baseAddresses)
    {
      LogDebug("Using non-IOC version of service type {0}", serviceType.FullName);
      return new ServiceHost(serviceType, baseAddresses);
    }

    private static WebServiceHost BuildDefaultWebServiceHostInstance(Type serviceType, params Uri[] baseAddresses)
    {
      LogDebug("Using non-IOC version of service type {0}", serviceType.FullName);
      return new WebServiceHost(serviceType, baseAddresses);
    }

    private static WebServiceHost2 BuildDefaultWebServiceHost2Instance(Type serviceType, params Uri[] baseAddresses)
    {
      LogDebug("Using non-IOC version of service type {0}", serviceType.FullName);
      return new WebServiceHost2(serviceType, true, baseAddresses);
    }

    private static WebServiceHost2 BuildDefaultWebServiceHost2SingletonInstance(Type serviceType, params Uri[] baseAddresses)
    {
      LogDebug("Using non-IOC version of service type {0}", serviceType.FullName);
      object singletonInstance = Activator.CreateInstance(serviceType);
      return new WebServiceHost2(singletonInstance, baseAddresses);
    }

    #endregion // Misc Utility Methods

    #region SpringServiceHost Utility Code

    private static IApplicationContext GetApplicationContext(string contextName)
    {
      if (StringUtils.IsNullOrEmpty(contextName))
      {
        return ContextRegistry.GetContext();
      }
      else
      {
        return ContextRegistry.GetContext(contextName);
      }
    }

    private static Type CreateServiceType(string serviceName, IObjectFactory objectFactory, bool useServiceProxyTypeCache)
    {
      if (StringUtils.IsNullOrEmpty(serviceName))
      {
        throw new ArgumentException("The service name cannot be null or an empty string.", "serviceName");
      }

      if (objectFactory.IsTypeMatch(serviceName, typeof(Type)))
      {
        return objectFactory.GetObject(serviceName) as Type;
      }

      return new ServiceProxyTypeBuilder(serviceName, objectFactory, useServiceProxyTypeCache).BuildProxyType();
    }

    #endregion // SpringServiceHost Utility Code
  }
}
