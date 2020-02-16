using System;
using log4net;
namespace VSS.Hosted.VLCommon.Utilities
{
  public delegate T SpringProxyGetter<T>();

  /// <summary>
  /// Class to automatically manage proxy referencing from inside of proxied classes.
  /// </summary>
  /// <typeparam name="T">Type of class that is proxied.</typeparam>
  public class SpringProxyHandle<T> where T : class
  {
    /* ************************ NOTE ************************
     * 
     * By convention, any class that implements a member of this type should initialize it likewise:
     * ProxyHandle<ProxiedObjectType> handle = new ProxyHandle<ProxiedObjectType>(this, () => (ProxiedObjectType)AopContext.CurrentProxy))
     * 
     * Use this field / property accessor pair pattern when implementing:
     * 
     *     private ProxyHandle<ProxiedTestObject> _proxyHandle;
     *     
     *     protected virtual ProxyHandle<ProxiedTestObject> ProxyHandle
     *     {
     *        get
     *        {
     *            return _proxyHandle ?? (_proxyHandle = new ProxyHandle<ProxiedTestObject>(this, () => (ProxiedTestObject)AopContext.CurrentProxy));
     *        }
     *     }
     * 
     * Be sure to configure Spring.Aop.Framework.ProxyFactoryObject with ExposeProxy EQ True:
     * 
     *  <object id="ProxiedObject" type="Spring.Aop.Framework.ProxyFactoryObject">
     *    <property name="ExposeProxy" value="true" />
     *    <property name="Target">
     *      ...
     *    </property>
     *    <property name="InterceptorNames">
     *      <list>
     *      ...
     *      </list>
     *    </property>
     *  </object>
     * 
     */
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly SpringProxyGetter<T> _proxyGetter;
    private readonly T _proxyTarget;
    private T _proxy;

    /// <summary>
    /// SpringProxyHandle CTOR.
    /// </summary>
    /// <param name="proxyTarget">The proxied class.</param>
    /// <param name="proxyGetter">A callback delegate, returning the proxy.</param>
    public SpringProxyHandle(T proxyTarget, SpringProxyGetter<T> proxyGetter)
    {
      _proxyTarget = proxyTarget;
      _proxyGetter = proxyGetter;
    }

    public T Proxy
    {
      get
      {
        if (_proxy == null)
        {
          try
          {
            _proxy = _proxyGetter();
          }
          catch (Exception ex)
          {
            log.IfError("Encountered exception getting spring proxy handle", ex);
          }
        }
        return _proxy ?? (_proxy = _proxyTarget);
      }
    }

    public static explicit operator T(SpringProxyHandle<T> proxyHandle)
    {
      return proxyHandle.Proxy;
    }
  }
}