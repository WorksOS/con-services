using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common
{
  public class ScheduledReportTestBase : UnitTestBase
  {

    public string _scheduleTitle = "Test Schedule";
    public string _scheduleDescription = "Test Schedule Description";
    public int _scheduleEndKeyDate = DateTime.MaxValue.KeyDate();
    public ActiveUser _user = null;
    public SessionContext _session = null;
    public Customer _customer = null;
    public List<long> _contactIDs = null;

    private List<long> CreateContact(Customer customer, int noOfContacts)
    {
      List<long> contactIDs = new List<long>();
      for (int i = 0; i < noOfContacts; i++)
      {
        Contact ct = Entity.Contact.ForCustomer(customer).Save();
        contactIDs.Add(ct.ID);
      }
      return contactIDs;
    }

    public void SetupTestData(int noOfContacts)
    {
      _user = Entity.ActiveUser.ForUser(TestData.TestCorporateAdmin).Save();
      _session = Helpers.Sessions.GetContextFor(_user);

      _customer = (from c in Ctx.OpContext.CustomerReadOnly where c.ID == _session.CustomerID.Value select c).SingleOrDefault<Customer>();
      _contactIDs = CreateContact(_customer, noOfContacts);
    }

  }
}
