using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class ServicePlanUpdatedWorkflow : Workflow
  {
    public DeviceServiceContext context { get { return Inputs.Get<DeviceServiceContext>(); } }

    public ServicePlanUpdatedWorkflow(Inputs inputs) : base(inputs)
    {
      Do(new Validate<ServicePlan>(
        new ServicePlanDataContractValidator()));

      Do(new MapServicePlanToDeviceServiceContext());

      Do(new Validate<DeviceServiceContext>(
        new DeviceServiceContextValidator(),
        new ServicePlanUpdatedValidator()));

      TransactionStart();

      Do(new ManageCustomerServiceViews());

      TransactionCommit();
    }
  }
}
