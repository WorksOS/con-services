using System;

namespace CoordinateSystemFileResolver.Interfaces
{
  public interface IResolver
  {
    IResolver ResolveCSIB(Guid projectUid, Guid customerUid);
    void GetCoordSysInfoFromCSIB64();
  }
}
