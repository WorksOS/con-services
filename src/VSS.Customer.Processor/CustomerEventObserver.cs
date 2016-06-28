using VSS.Customer.Data.Interfaces;
using VSS.MasterData.Common.JsonConverters;
using VSS.MasterData.Common.Processor;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Customer.Processor
{
  public class CustomerEventObserver : EventObserverBase<ICustomerEvent, CustomerEventConverter>
  {
    private ICustomerService _customerService;

    public CustomerEventObserver(ICustomerService customerService)
    {
      _customerService = customerService;
      EventName = "Customer";
    }

    protected override bool ProcessEvent(ICustomerEvent evt)
    {
      int updatedCount = _customerService.StoreCustomer(evt);
      return updatedCount == 1;
    }
  }
}
