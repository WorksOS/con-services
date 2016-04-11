using System;

namespace LandfillService.AcceptanceTests.Models.MasterData.Interfaces
{
    public interface IUserCustomerEvent
    {
        Guid CustomerUID { get; set; }
        Guid UserUID { get; set; }
        DateTime ActionUTC { get; set; }
        DateTime ReceivedUTC { get; set; }
    }
}