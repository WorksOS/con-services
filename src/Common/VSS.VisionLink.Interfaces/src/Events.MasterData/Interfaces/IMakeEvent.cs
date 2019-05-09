using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Interfaces
{
    public interface IMakeEvent
    {
        Guid MakeUID { get; set; }
        DateTime ActionUTC { get; set; }
        DateTime ReceivedUTC { get; set; }
    }
}