using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSP.MasterData.Common.KafkaWrapper;
using VSS.Subscription.Processor.Interfaces;

namespace VSS.Subscription.Processor
{
    public class SubscriptionEventConsumer : IConsumer
    {
        private readonly IConsumerWrapper _consumerWrapper;

        public SubscriptionEventConsumer(IConsumerWrapper consumerWrapper)
        {
            _consumerWrapper = consumerWrapper;
        }
        public void Process()
        {
            _consumerWrapper.Consume(skipFailedMessages: true);
        }
    }
}
