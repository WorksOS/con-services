using System;
using VSS.MasterData.Project.WebAPI.Common.Helpers;

namespace VSS.MasterData.Project.WebAPI.Factories
{
  /// <summary>
  /// Interface to request Factory
  /// </summary>
  public interface IRequestFactory
    {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="action"></param>
      /// <typeparam name="T"></typeparam>
      /// <returns></returns>
      T Create<T>(Action<RequestFactory> action) where T : DataRequestBase, new();
      /// <summary>
      /// 
      /// </summary>
      /// <param name="customerUid"></param>
      /// <returns></returns>
      RequestFactory CustomerUid(string customerUid);
    }
}