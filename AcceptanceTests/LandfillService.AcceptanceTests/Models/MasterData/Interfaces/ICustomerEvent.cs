using System;

namespace LandfillService.AcceptanceTests.Models.MasterData.Interfaces
{
    public interface ICustomerEvent
    {
        Guid CustomerUID { get; set; }
        DateTime ActionUTC { get; set; }
        DateTime ReceivedUTC { get; set; }
    }
}