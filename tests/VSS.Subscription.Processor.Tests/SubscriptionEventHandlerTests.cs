//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Xunit;
//using VSS.Subscription.Data.MySql;
//using NSubstitute;
//using VSS.Subscription.Model.Interfaces;
//using VSP.MasterData.Common.KafkaWrapper.Models;
//using VSS.Subscription.Data.Models;

//namespace VSS.Subscription.Processor.Tests
//{
//    public class SubscriptionEventHandlerTests
//    {
//        [Fact]
//        public void EventHandler_SendCreateAssetSubscriptionEvent_CallsServiceWithCreate()
//        {
//            var mockProducerWrapper = Substitute.For<ISubscriptionService>();

//            var eventHandler = new SubscriptionEventHandler(mockProducerWrapper);

//            var kafkaMsg = new KafkaMessage()
//            {
//                Value = "{\"CreateAssetSubscriptionEvent\":{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\",\"CustomerUID\":\"9cd89089-c850-42ee-bd2a-13a1406bac71\",\"AssetUID\":\"5b811c51-6156-4547-acb5-031ddda1a1ff\",\"SubscriptionType\":\"Essentials\",\"StartDate\":\"2015-11-18T05:58:12.0856191Z\",\"EndDate\":\"2015-11-18T05:58:12.0856191Z\",\"ActionUTC\":\"2020-08-25T23:40:00Z\",\"ReceivedUTC\":\"2015-11-16T08:31:40.4129206Z\"}}"
//            };
//            eventHandler.Handle(kafkaMsg);
//            mockProducerWrapper.Received(1).CreateAssetSubscription(Arg.Is<CreateAssetSubscriptionEvent>(x => x.SubscriptionUID == new Guid("431e9c00-802b-403e-a9c8-3a5dca5e4bfa")));

//        }


//        [Fact]
//        public void EventHandler_SendUpdateAssetSubscriptionEvent_CallsServiceWithUpdate()
//        {
//            var mockProducerWrapper = Substitute.For<ISubscriptionService>();

//            var eventHandler = new SubscriptionEventHandler(mockProducerWrapper);

//            var kafkaMsg = new KafkaMessage()
//            {
//                Value = "{\"UpdateAssetSubscriptionEvent\":{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\",\"StartDate\":\"2015-11-18T05:58:12.0856191Z\",\"EndDate\":\"2015-11-18T05:58:12.0856191Z\",\"ActionUTC\":\"2020-08-25T23:40:00Z\",\"ReceivedUTC\":\"2015-11-16T08:31:40.4129206Z\"}}"
//            };
//            eventHandler.Handle(kafkaMsg);
//            mockProducerWrapper.Received(1).UpdateAssetSubscription(Arg.Is<UpdateAssetSubscriptionEvent>(x => x.SubscriptionUID == new Guid("431e9c00-802b-403e-a9c8-3a5dca5e4bfa")));
//        }

//        [Fact]
//        public void EventHandler_SendCreateProjectSubscriptionEvent_CallsServiceWithCreate()
//        {
//            var mockProducerWrapper = Substitute.For<ISubscriptionService>();

//            var eventHandler = new SubscriptionEventHandler(mockProducerWrapper);

//            var kafkaMsg = new KafkaMessage()
//            {
//                Value = "{\"CreateProjectSubscriptionEvent\":{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\",\"CustomerUID\":\"9cd89089-c850-42ee-bd2a-13a1406bac71\",\"SubscriptionType\":\"Manual 3D Project Monitoring\",\"StartDate\":\"2015-11-18T05:58:12.0856191Z\",\"EndDate\":\"2015-11-18T05:58:12.0856191Z\",\"ActionUTC\":\"2020-08-25T23:40:00Z\",\"ReceivedUTC\":\"2015-11-16T08:31:40.4129206Z\"}}"
//            };
//            eventHandler.Handle(kafkaMsg);
//            mockProducerWrapper.Received(1).CreateProjectSubscription(Arg.Is<CreateProjectSubscriptionEvent>(x => x.SubscriptionUID == new Guid("431e9c00-802b-403e-a9c8-3a5dca5e4bfa")));

//        }

//        [Fact]
//        public void EventHandler_SendUpdateProjectSubscriptionEvent_CallsServiceWithUpdate()
//        {
//            var mockProducerWrapper = Substitute.For<ISubscriptionService>();

//            var eventHandler = new SubscriptionEventHandler(mockProducerWrapper);

//            var kafkaMsg = new KafkaMessage()
//            {
//                Value = "{\"UpdateProjectSubscriptionEvent\":{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\",\"StartDate\":\"2015-11-18T05:58:12.0856191Z\",\"EndDate\":\"2015-11-18T05:58:12.0856191Z\",\"ActionUTC\":\"2020-08-25T23:40:00Z\",\"ReceivedUTC\":\"2015-11-16T08:31:40.4129206Z\"}}"
//            };
//            eventHandler.Handle(kafkaMsg);
//            mockProducerWrapper.Received(1).UpdateProjectSubscription(Arg.Is<UpdateProjectSubscriptionEvent>(x => x.SubscriptionUID == new Guid("431e9c00-802b-403e-a9c8-3a5dca5e4bfa")));
//        }

//        [Fact]
//        public void EventHandler_SendAssociateProjectSubscriptionEvent_CallsService()
//        {
//            var mockProducerWrapper = Substitute.For<ISubscriptionService>();

//            var eventHandler = new SubscriptionEventHandler(mockProducerWrapper);

//            var kafkaMsg = new KafkaMessage()
//            {
//                Value = "{\"AssociateProjectSubscriptionEvent\":{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\",\"ProjectUID\":\"d6b1b168-f04f-4f9d-b80a-0aebf36926c2\",\"EffectiveDate\":\"2015-11-18T05:58:12.0856191Z\",\"ActionUTC\":\"2020-08-25T23:40:00Z\",\"ReceivedUTC\":\"2015-11-16T08:31:40.4129206Z\"}}"
//            };
//            eventHandler.Handle(kafkaMsg);
//            mockProducerWrapper.Received(1).AssociateProjectSubscription(Arg.Is<AssociateProjectSubscriptionEvent>(x => x.SubscriptionUID == new Guid("431e9c00-802b-403e-a9c8-3a5dca5e4bfa")));
//        }

//        [Fact]
//        public void EventHandler_SendDissaciateProjectSubscriptionEvent_CallsService()
//        {
//            var mockProducerWrapper = Substitute.For<ISubscriptionService>();

//            var eventHandler = new SubscriptionEventHandler(mockProducerWrapper);

//            var kafkaMsg = new KafkaMessage()
//            {
//                Value = "{\"DissociateProjectSubscriptionEvent\":{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\",\"ProjectUID\":\"d6b1b168-f04f-4f9d-b80a-0aebf36926c2\",\"EffectiveDate\":\"2015-11-18T05:58:12.0856191Z\",\"ActionUTC\":\"2020-08-25T23:40:00Z\",\"ReceivedUTC\":\"2015-11-16T08:31:40.4129206Z\"}}"
//            };
//            eventHandler.Handle(kafkaMsg);
//            mockProducerWrapper.Received(1).DissociateProjectSubscription(Arg.Is<DissociateProjectSubscriptionEvent>(x => x.SubscriptionUID == new Guid("431e9c00-802b-403e-a9c8-3a5dca5e4bfa")));
//        }

//        [Fact]
//        public void EventHandler_SendCreateCustomerSubscriptionEvent_CallsServiceWithCreate()
//        {
//            var mockProducerWrapper = Substitute.For<ISubscriptionService>();

//            var eventHandler = new SubscriptionEventHandler(mockProducerWrapper);

//            var kafkaMsg = new KafkaMessage()
//            {
//                Value = "{\"CreateCustomerSubscriptionEvent\":{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\",\"CustomerUID\":\"9cd89089-c850-42ee-bd2a-13a1406bac71\",\"SubscriptionType\":\"Manual 3D Project Monitoring\",\"StartDate\":\"2015-11-18T05:58:12.0856191Z\",\"EndDate\":\"2015-11-18T05:58:12.0856191Z\",\"ActionUTC\":\"2020-08-25T23:40:00Z\",\"ReceivedUTC\":\"2015-11-16T08:31:40.4129206Z\"}}"
//            };
//            eventHandler.Handle(kafkaMsg);
//            mockProducerWrapper.Received(1).CreateCustomerSubscription(Arg.Is<CreateCustomerSubscriptionEvent>(x => x.SubscriptionUID == new Guid("431e9c00-802b-403e-a9c8-3a5dca5e4bfa")));

//        }

//        [Fact]
//        public void EventHandler_SendUpdateCustomerSubscriptionEvent_CallsServiceWithUpdate()
//        {
//            var mockProducerWrapper = Substitute.For<ISubscriptionService>();

//            var eventHandler = new SubscriptionEventHandler(mockProducerWrapper);

//            var kafkaMsg = new KafkaMessage()
//            {
//                Value = "{\"UpdateCustomerSubscriptionEvent\":{\"SubscriptionUID\":\"431e9c00-802b-403e-a9c8-3a5dca5e4bfa\",\"StartDate\":\"2015-11-18T05:58:12.0856191Z\",\"EndDate\":\"2015-11-18T05:58:12.0856191Z\",\"ActionUTC\":\"2020-08-25T23:40:00Z\",\"ReceivedUTC\":\"2015-11-16T08:31:40.4129206Z\"}}"
//            };
//            eventHandler.Handle(kafkaMsg);
//            mockProducerWrapper.Received(1).UpdateCustomerSubscription(Arg.Is<UpdateCustomerSubscriptionEvent>(x => x.SubscriptionUID == new Guid("431e9c00-802b-403e-a9c8-3a5dca5e4bfa")));
//        }
//    }
//}
