using System;

namespace LandfillService.AcceptanceTests.Models.MasterData.Interfaces
{
    public interface IProjectEvent
    {
        Guid ProjectUID { get; set; }
        DateTime ActionUTC { get; set; }
        DateTime ReceivedUTC { get; set; }
    }
}
