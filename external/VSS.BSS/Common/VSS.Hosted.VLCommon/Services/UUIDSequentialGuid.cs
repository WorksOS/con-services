using System;
using System.Runtime.InteropServices;

namespace VSS.Hosted.VLCommon 
{
  public class UUIDSequentialGuid : IUUIDSequentialGuid
  {
    //This will compute the guid that is will go into the asset for VLReady-store
    //http://stackoverflow.com/questions/1752004/sequential-guid-generator-c-sharp/#answer-9538870

    [DllImport("rpcrt4.dll", SetLastError = true)]
    private static extern int UuidCreateSequential(out Guid value);

    [Flags]
    private enum RetUuidCodes
    {
      RPC_S_OK = 0,//The call succeeded.
      RPC_S_UUID_LOCAL_ONLY = 1824, //The UUID is guaranteed to be unique to this computer only.
      RPC_S_UUID_NO_ADDRESS = 1739 //Cannot get Ethernet or token-ring hardware address for this computer.
    }

    public Guid CreateGuid()
    {
      Guid guid;
      int result = UuidCreateSequential(out guid);
      if (result == (int)RetUuidCodes.RPC_S_OK)
        return guid;

      throw new InvalidOperationException("Could Not Create a new Uuid");
    }
  }
}
