using VSP.MasterData.Common.KafkaWrapper;
using VSS.MasterData.Customer.Processor.Interfaces;

namespace VSS.MasterData.Customer.Processor
{
    public class CustomerProcessor : ICustomerProcessor
    {
        private readonly IConsumerWrapper _consumerWrapper;

        public CustomerProcessor(IConsumerWrapper consumerWrapper)
        {
            _consumerWrapper = consumerWrapper;
        }
        public void Process()
        {
            _consumerWrapper.Consume(skipFailedMessages: true);
        }
    }
}
