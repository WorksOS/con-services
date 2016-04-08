namespace LandfillService.AcceptanceTests
{
    public enum EventType
    {
        CreateProjectEvent,
        UpdateProjectEvent,
        DeleteProjectEvent,
        CreateCustomerEvent,
        CreateProjectSubscriptionEvent,
        CreateCustomerSubscriptionEvent,
        UpdateProjectSubscriptionEvent,
        UpdateCustomerSubscriptionEvent,
        AssociateProjectSubscriptionEvent,
        AssociateProjectCustomerEvent,
        AssociateCustomerUserEvent,
        DissociateProjectSubscriptionEvent
    }

    public enum CustomerType
    {
        Customer = 0,
        Dealer = 1,
        Operations = 2,
        Corporate = 3
    }

    public enum ProjectType
    {
        Full3D = 0,
        LandFill = 1,
    }

    public enum RelationType
    {
        Owner = 0,
        Customer = 1,
        Dealer = 2,
        Operations = 3,
        Corporate = 4,
        SharedOwner = 5
    }
}
