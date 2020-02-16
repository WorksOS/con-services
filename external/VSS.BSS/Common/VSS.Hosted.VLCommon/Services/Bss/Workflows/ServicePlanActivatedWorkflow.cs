
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ServicePlanActivatedWorkflow : Workflow
  {
    public DeviceServiceContext Context { get { return Inputs.Get<DeviceServiceContext>(); } }

    public ServicePlanActivatedWorkflow(Inputs inputs) : base(inputs)
    {
      Do(new Validate<ServicePlan>(
        new ServicePlanDataContractValidator()));

      Do(new MapServicePlanToDeviceServiceContext());

      Do(new Validate<DeviceServiceContext>(
        new DeviceServiceContextValidator(), 
        new ServicePlanActivatedValidator()));

      TransactionStart();

      Do(new ServicePlanActivate());

      If(() => !Context.IsDeviceDeregistered() && Context.IsCoreService())
        .ThenDo(new UpdateDeviceState());

      TransactionCommit();

      If(() => !Context.IsDeviceDeregistered() && Context.ExistingDeviceAsset.Type != DeviceTypeEnum.MANUALDEVICE)
        .ThenDo(new ActivatedServicePlanConfiguration());
    }
  }
}
