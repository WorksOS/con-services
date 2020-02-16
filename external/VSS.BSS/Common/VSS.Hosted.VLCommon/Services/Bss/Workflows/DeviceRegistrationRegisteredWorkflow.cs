using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceRegistrationRegisteredWorkflow : Workflow
  {
    public DeviceStatusContext context { get { return Inputs.Get<DeviceStatusContext>(); } }

    public DeviceRegistrationRegisteredWorkflow(Inputs inputs) : base(inputs)
    {
      Do(new Validate<DeviceRegistration>(new DeviceRegistrationDataContractValidator()));

      Do(new MapDeviceRegistrationToDeviceAssetContext());

      Do(new Validate<DeviceStatusContext>(new DeviceStatusContextValidator(), new DeviceStatusContextRegisteredValidator()));

      TransactionStart();

      Do(new RegisterDeviceState());

      TransactionCommit();
    }
  }
}
