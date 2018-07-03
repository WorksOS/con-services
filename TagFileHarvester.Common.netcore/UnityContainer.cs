using SimpleInjector;
using TagFileHarvester;

namespace VSS.Nighthawk.ThreeDCommon.ThreeDAPIs.ProjectDataServer
{
  public class UnityContainer : IUnityContainer
  {
    private static readonly Container container = new Container();

    public T Resolve<T>() where T : class
    {
      return container.GetInstance<T>();
    }

    public UnityContainer RegisterType<T, T1>() where T : class where T1 : class, T
    {
      container.Register<T, T1>();
      return this;
    }

    public UnityContainer RegisterInstance<T>(T instance)
    {
      container.RegisterInstance(typeof(T), instance);
      return this;
    }
  }
}