
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiTests.Common.Contracts
{
    [TestClass]
    public class GeneralTests
    {
    /* TODO: Fix when JsonTextFormatter replaced
        [TestMethod]
        public void CanSerializeDesrializeDomainObject()
        {
          BoundingBox2DGrid box = BoundingBox2DGrid.CreateBoundingBox2DGrid(1, 1, 10, 10);
            MemoryStream stream = new MemoryStream();
            JsonTextFormatter formatter = new JsonTextFormatter();
            var writeTask = formatter.WriteToStreamAsync(typeof (BoundingBox2DGrid), box,stream, null, null);
            writeTask.Wait();
            stream.Position=0;
            var readTask = formatter.ReadFromStreamAsync(typeof(BoundingBox2DGrid), stream, null, null);
            readTask.Wait();

            Assert.IsInstanceOfType(readTask.Result, typeof(BoundingBox2DGrid));
            var result = readTask.Result as BoundingBox2DGrid;

            Assert.AreEqual(box.topRightX,result.topRightX);


        }
        */

        class TestContainer : RequestExecutorContainer
        {
            protected override ContractExecutionResult ProcessEx<T>(T item)
            {
                ContractExecutionStates.ClearDynamic();
                return new ContractExecutionResult(1,"test result");
            }

            protected override void ProcessErrorCodes()
            {
                ContractExecutionStates.DynamicAddwithOffset("OnSubmissionResult. ConnectionFailure.",
                        -10);
                ContractExecutionStates.DynamicAddwithOffset(
                        "The TAG file was found to be corrupted on its pre-processing scan.",
                        15);
            }
        }

        [TestMethod()]
        public void GenerateErrorlistTest()
        {
            TestContainer container = new TestContainer();
            Assert.AreEqual(8,container.GenerateErrorlist().Count);
            container.Process(WGSPoint.CreatePoint(1,1));
            Assert.AreEqual(6, container.GenerateErrorlist().Count);
        }

    }
}
