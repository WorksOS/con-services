using System;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class CustomerBuilder 
  {
    #region Customer Fields

    private long _id = IdGen.GetId();
    private CustomerTypeEnum _customerType;
    private string _name = "CUSTOMER_" + IdGen.GetId();
    private string _bssId = "BSS_ID_" + IdGen.GetId();
    private DealerNetworkEnum _dealerNetwork = DealerNetworkEnum.None;
    private DateTime _updateUtc = DateTime.UtcNow;
    private string _networkDealerCode;
    private string _networkCustomerCode;
    private string _dealerAccountCode;
    private bool _isActivated = true;

    #endregion

    public CustomerBuilder(CustomerTypeEnum customerType)
    {
      _customerType = customerType;
      if (customerType == CustomerTypeEnum.Dealer)
      {
        _dealerNetwork = DealerNetworkEnum.CAT;
        _networkDealerCode = "NETWORK_DEALER_CODE_" + IdGen.GetId();
      }
      else if (customerType == CustomerTypeEnum.Account)
      {
        _dealerAccountCode = "DEALER_ACCOUNT_CODE_" + IdGen.GetId();
        _networkCustomerCode = "NETWORK_CUSTOMER_CODE_" + IdGen.GetId();
      }
      else if (customerType == CustomerTypeEnum.Corporate)
      {
        _networkCustomerCode = String.Empty;
        _networkDealerCode = String.Empty;
      }
      
    }
    public CustomerBuilder Id(long id)
    {
      _id = id;
      return this;
    }
    public CustomerBuilder Name(string name)
    {
      _name = name;
      return this;
    }
    public CustomerBuilder BssId(string bssId)
    {
      _bssId = bssId;
      return this;
    }
    public CustomerBuilder DealerNetwork(DealerNetworkEnum dealerNetwork)
    {
      _dealerNetwork = dealerNetwork;
      return this;
    }
    public CustomerBuilder NetworkDealerCode(string networkDealerCode)
    {
      _networkDealerCode = networkDealerCode;
      return this;
    }
    public CustomerBuilder NetworkCustomerCode(string networkCustomerCode)
    {
      _networkCustomerCode = networkCustomerCode;
      return this;
    }
    public CustomerBuilder DealerAccountCode(string dealerAccountCode)
    {
      _dealerAccountCode = dealerAccountCode;
      return this;
    }
    public CustomerBuilder UpdateUtc(DateTime udpateUtc)
    {
      _updateUtc = udpateUtc;
      return this;
    }
    public CustomerBuilder IsActivated(bool isActivated)
    {
      _isActivated = isActivated;
      return this;
    }

    public CustomerBuilder SyncWithRpt()
    {
      // disabled the sync as part of cleanup activity
      return this;
    }

    public Customer Build()
    {
      Customer customer =  new Customer();
      customer.ID = _id;
      customer.Name = _name;
      customer.fk_CustomerTypeID = (int)_customerType;
      customer.fk_DealerNetworkID = (int) _dealerNetwork;
      customer.BSSID = _bssId;
      customer.UpdateUTC = _updateUtc;
      customer.NetworkCustomerCode = _networkCustomerCode;
      customer.NetworkDealerCode = _networkDealerCode;
      customer.DealerAccountCode = _dealerAccountCode;
      customer.IsActivated = _isActivated;
      customer.CustomerUID = Guid.NewGuid();
      return customer;
    }
    public Customer Save()
    {
      Customer customer = Build();

      ContextContainer.Current.OpContext.Customer.AddObject(customer);
      ContextContainer.Current.OpContext.SaveChanges();

      return customer;
    }
  }
}
