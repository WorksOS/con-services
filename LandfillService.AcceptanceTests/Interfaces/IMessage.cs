namespace LandfillService.AcceptanceTests.Helpers
{
    public interface IMessage
    {
        MessageType GetMessageType();
        object Send();
    }
}
