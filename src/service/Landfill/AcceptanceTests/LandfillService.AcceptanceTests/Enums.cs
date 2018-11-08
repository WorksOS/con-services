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

    public enum GeofenceType
    {
        Generic = 0,
        Project = 1,
        Borrow = 2,
        Waste = 3,
        AvoidanceZone = 4,
        Stockpile = 5,
        CutZone = 6,
        FillZone = 7,
        Import = 8,
        Export = 9,
        Landfill = 10
    }
}
