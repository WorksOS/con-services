using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceReplacementReplacedWorkflow : Workflow
  {
    public DeviceAssetContext Context { get { return Inputs.Get<DeviceAssetContext>(); } }

    public DeviceReplacementReplacedWorkflow(Inputs inputs) : base(inputs)
    {
      Do(new Validate<DeviceReplacement>(
        new DeviceReplacementDataContractValidator()));

      Do(new MapDeviceReplacementToDeviceAssetContext());

      Do(new Validate<DeviceAssetContext>(
        new DeviceAssetContextValidator(),
        new DeviceAssetContextReplacedValidator()));

      TransactionStart();

      //call the service transfer which 
      //will transfer active services 
      //from old ibkey to new ibkey
      Do(new ServiceTransfer());

      Do(new ServiceViewCreateForDeviceReplacement());

      // For US 22511
      Do(new ResetAssetOnOffTrackingForDeviceReplacement());
      TransactionCommit();

      Do(new DeviceReconfigure());
    }
  }
}
