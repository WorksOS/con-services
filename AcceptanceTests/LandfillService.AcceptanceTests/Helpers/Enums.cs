namespace LandfillService.AcceptanceTests.Helpers
{
    public enum MessageType
    {
        Invalid,
        InternalQueue,
        HoursEvent,
        OdometerEvent,
        EngineOperatingStatusEvent,
        MovingEvent,
        WorkDefinition,
        SwitchStateEvent,
        CreateAssetEvent,
        UpdateAssetEvent,
        DeleteAssetEvent,
        CreateProjectEvent,
        UpdateProjectEvent,
        DeleteProjectEvent,
        CreateProjectSubscriptionEvent,
        CreateCustomerSubscriptionEvent,
        CreateAssetSubscriptionEvent,
        UpdateAssetSubscriptionEvent,
        UpdateProjectSubscriptionEvent,
        UpdateCustomerSubscriptionEvent,
        AssociateProjectSubscriptionEvent,
        DissociateProjectSubscriptionEvent
    }
}
