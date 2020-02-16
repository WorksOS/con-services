using System;


namespace VSS.Hosted.VLCommon.Bss
{
  public static class CustomerContextExtensions
  {
    ////ParentChildRelationshipIsValid
    //public static bool ParentChildRelationshipIsValid(this CustomerContext context)
    //{
    //  Require.IsNotNull(context, "CustomerContext");
    //  Require.IsNotNull(context.New, "CustomerContext.New");
    //  Require.IsNotNull(context.NewParent, "CustomerContext.NewParent");

    //  if (context.NewParent.BssId.IsNotDefined())
    //    return true;

    //  var child = context.New.Type;
    //  var parent = context.NewParent.Type;

    //  // Account can have a Dealer or a Customer as a parent
    //  if (child == CustomerTypeEnum.Account &&
    //     (parent == CustomerTypeEnum.Dealer ||
    //      parent == CustomerTypeEnum.Customer))
    //    return true;

    //  // Dealer can only have a Dealer as a parent
    //  if (child == CustomerTypeEnum.Dealer &&
    //      parent == CustomerTypeEnum.Dealer)
    //    return true;

    //  // Customer can only have a Customer as a parent
    //  // Warning
    //  if (child == CustomerTypeEnum.Customer &&
    //     parent == CustomerTypeEnum.Customer)
    //    return true;

    //  // All other relationship combinations are invalid
    //  return false;
    //}

    ////RelationshipTypeCantExistForCustomer
    //public static bool RelationshipTypeExistsForCustomer(this CustomerContext context)
    //{
    //  if (context.NewParent.BssId.IsNotDefined())
    //    return false;

    //  return (context.NewParent.Type == CustomerTypeEnum.Dealer && context.ParentDealer.Exists ||
    //          context.NewParent.Type == CustomerTypeEnum.Customer && context.ParentCustomer.Exists);
    //}

    ////RelationshipTypeCantExistForCustomer
    //public static bool RelationshipIdExistsForCustomer(this CustomerContext context)
    //{
    //  if (context.NewParent.RelationshipId.IsNotDefined())
    //    return false;

    //  if (context.ParentDealer.RelationshipId.IsDefined() &&
    //     context.ParentDealer.RelationshipId == context.NewParent.RelationshipId)
    //    return true;

    //  if (context.ParentCustomer.RelationshipId.IsDefined() &&
    //     context.ParentCustomer.RelationshipId == context.NewParent.RelationshipId)
    //    return true;

    //  return false;
    //}
  }
}