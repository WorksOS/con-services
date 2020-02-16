using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon.Bss.Schema.V2;


namespace VSS.Hosted.VLCommon.Bss
{
  public class ServicePlanCancelledWorkflow : Workflow
  {
    public DeviceServiceContext context { get { return Inputs.Get<DeviceServiceContext>(); } }

    public ServicePlanCancelledWorkflow(Inputs inputs) : base(inputs)
    {
      Do(new Validate<ServicePlan>(
        new ServicePlanDataContractValidator()));

      Do(new MapServicePlanToDeviceServiceContext());

      Do(new Validate<DeviceServiceContext>(
        new DeviceServiceContextValidator(),
        new ServicePlanCancelledValidator()));

      TransactionStart();

      //terminate the service requested
      Do(new ServicePlanTerminate());

      //if no active core services exist for the device, change the device state to 'Provisioned'
      If(() => context.IsCoreService())
        .ThenDo(new DeActivateDeviceState());

      TransactionCommit();

      //reconfigure the device reporting parameters based on the available active service plans
      Do(new CancelledServicePlanConfiguration());
    }
  }
}
