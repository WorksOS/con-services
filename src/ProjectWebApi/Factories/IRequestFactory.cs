using System;
using System.Collections.Generic;
using VSS.MasterData.Project.WebAPI.Common.Helpers;

namespace VSS.MasterData.Project.WebAPI.Factories
{
  public interface IRequestFactory
    {
      T Create<T>(Action<RequestFactory> action) where T : DataRequestBase, new();
      RequestFactory Headers(IDictionary<string, string> headers);
    }
}