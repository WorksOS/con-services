using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Interfaces
{
    public interface IProductFamilyEvent
    {
        Guid ProductFamilyUID { get; set; }
        DateTime ActionUTC { get; set; }
        DateTime ReceivedUTC { get; set; }
    }
}