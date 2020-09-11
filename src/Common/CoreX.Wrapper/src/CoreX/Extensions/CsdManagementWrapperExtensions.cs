using System;
using Trimble.CsdManagementWrapper;
using Trimble.GeodeticXWrapper;

namespace CoreX.Wrapper.Extensions
{
  internal static class CsdManagementWrapperExtensions
  {
    internal static bool Validate(this csmErrorCode code, string message) => code switch
    {
      (int)csmErrorCode.cecSuccess => true,
      _ => throw new Exception($"Error '{code}' {message}")
    };

    internal static bool Validate(this geoErrorCode code, string message) => code switch
    {
      (int)geoErrorCode.gecSuccess => true,
      _ => throw new Exception($"Error '{code}' {message}")
    };
  }
}
