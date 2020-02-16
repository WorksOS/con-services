using System;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class RelationshipBuilder 
  {
    #region Account Fields

    private string _bssRelationshipId = Math.Abs(Guid.NewGuid().GetHashCode()).ToString();
    private Customer _parent;
    private Customer _child;
    #endregion

    public RelationshipBuilder(Customer parent, Customer child)
    {
      _parent = parent;
      _child = child;
    }
    public RelationshipBuilder BssRelationshipId(string bssRelationshipId)
    {
      _bssRelationshipId = bssRelationshipId;
      return this;
    }

    public CustomerRelationship Build()
    {
      CustomerRelationship account = new CustomerRelationship();

      ValidateRelationship(_parent, _child);

      account.fk_ParentCustomerID = _parent.ID;
      account.fk_ClientCustomerID = _child.ID;
      account.BSSRelationshipID = _bssRelationshipId;
      account.fk_CustomerRelationshipTypeID = (int)GetRelationshipType((CustomerTypeEnum)_parent.fk_CustomerTypeID);

      return account;
    }

    public CustomerRelationship Save()
    {
      CustomerRelationship account = Build();

      ContextContainer.Current.OpContext.CustomerRelationship.AddObject(account);
      ContextContainer.Current.OpContext.SaveChanges();

      return account;
    }

    private static void ValidateRelationship(Customer parent, Customer child)
    {
      
      if (parent.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer &&
         (child.fk_CustomerTypeID == (int)CustomerTypeEnum.Dealer ||
          child.fk_CustomerTypeID == (int)CustomerTypeEnum.Account))
        return;

      if (parent.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer &&
         (child.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer ||
          child.fk_CustomerTypeID == (int)CustomerTypeEnum.Account))
        return;

      throw new InvalidOperationException("AccountBuilder Invalid Parent Child Relationship");
    }

    private static CustomerRelationshipTypeEnum GetRelationshipType(CustomerTypeEnum parentCustomerType)
    {
      return parentCustomerType == CustomerTypeEnum.Dealer ? CustomerRelationshipTypeEnum.TCSDealer : CustomerRelationshipTypeEnum.TCSCustomer;
    }
  }
}
