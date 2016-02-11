﻿namespace LandfillService.AcceptanceTests.Helpers
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
        CreateCustomerEvent,
        CreateProjectSubscriptionEvent,
        CreateCustomerSubscriptionEvent,
        UpdateProjectSubscriptionEvent,
        UpdateCustomerSubscriptionEvent,
        AssociateProjectSubscriptionEvent,
        AssociateProjectCustomerEvent,
        DissociateProjectSubscriptionEvent
    }
}
