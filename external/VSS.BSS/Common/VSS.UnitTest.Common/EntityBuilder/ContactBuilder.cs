using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.UnitTest.Common.Contexts;
using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class ContactBuilder
  {
    private long _id = IdGen.GetId();
    private Customer _customer;
    private int _language = 1;
    private string _email = IdGen.GetId() + "@testemail.com";
    private string _name = "TEST_CONTACT_" + IdGen.GetId();
    private bool _isSMS = false;

    public ContactBuilder ForCustomer(Customer customer)
    {
      _customer = customer;
      return this;
    }
    public ContactBuilder Language(int languageID)
    {
      _language = languageID;
      return this;
    }
    public ContactBuilder Email(string email)
    {
      _email = email;
      return this;
    }
    public ContactBuilder Name(string name)
    {
      _name = name;
      return this;
    }
    public ContactBuilder IsSMS(bool isSMS)
    {
      _isSMS = isSMS;
      return this;
    }

    public Contact Build()
    {
      Contact contact = new Contact();
      contact.ID = _id;
      contact.fk_CustomerID = _customer.ID;
      contact.Email = _email;
      contact.fk_LanguageID = _language;
      contact.Name = _name;
      contact.IsSMS = _isSMS;
      return contact;
    }
    public Contact Save()
    {
      var contact = Build();
      ContextContainer.Current.OpContext.Contact.AddObject(contact);
      ContextContainer.Current.OpContext.SaveChanges();
      return contact;
    }
  }
}
