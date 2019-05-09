using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Interfaces
{
    public interface ISalesModelEvent
    {
        Guid SalesModelUID { get; set; }
        DateTime ActionUTC { get; set; }
        DateTime ReceivedUTC { get; set; }
    }
}