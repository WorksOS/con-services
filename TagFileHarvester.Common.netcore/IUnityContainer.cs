namespace TagFileHarvester
{
  public interface IUnityContainer
  {
    T Resolve<T>() where T : class;
  }
}