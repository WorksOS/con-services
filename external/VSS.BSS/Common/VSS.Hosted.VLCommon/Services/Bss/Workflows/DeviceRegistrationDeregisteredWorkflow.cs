using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceRegistrationDeregisteredWorkflow : Workflow
  {
    public DeviceStatusContext Context { get { return Inputs.Get<DeviceStatusContext>(); } }

    public DeviceRegistrationDeregisteredWorkflow(Inputs inputs) : base(inputs)
    {
      Do(new Validate<DeviceRegistration>(
        new DeviceRegistrationDataContractValidator()));

      Do(new MapDeviceRegistrationToDeviceAssetContext());

      Do(new Validate<DeviceStatusContext>(
        new DeviceStatusContextValidator(), 
        new DeviceStatusContextDeRegisteredValidator()));

      TransactionStart();

      //update the device state to deregister
      Do(new DeregisterDeviceState());

      //if the deregister message is received from store, then send an OTA command to the corresponding PL device
      If(() => Context.Status.IsStringEqual("DEREG_STORE"))
        .ThenDo(new DeRegisterDeviceStateOTA());

      TransactionCommit();
    }
  }
}
