using System;
using System.IO;
using Apache.Ignite.Core;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Servers;
using VSS.TRex.Servers.Client;
using Xunit;

namespace TAGFiles.Tests.netcore
{
    public class TAGFileSubmissionTests
    {
        private static MutableClientServer TAGClientServer;
        private static IIgnite ignite;

        private static void EnsureServer()
        {
            try
            {
                ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());
            }
            catch
            {
                TAGClientServer = TAGClientServer ?? new MutableClientServer(ServerRoles.TAG_PROCESSING_NODE_CLIENT);
                ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());
            }
        }

        [Fact(Skip = "Requires live Ignite node")]
        public void Test_TAGFileSubmission_Creation()
        {
            EnsureServer();

            SubmitTAGFileRequest submission = new SubmitTAGFileRequest();

            Assert.True(null != submission, "Failed to create SubmitTAGFileRequest instance");
        }

        [Fact(Skip = "Requires live Ignite node")]
        public void Test_TAGFileSubmission_SubmitTAGFile()
        {
            EnsureServer();

            SubmitTAGFileRequest submission = new SubmitTAGFileRequest();

            Assert.True(null != submission, "Failed to create SubmitTAGFileRequest instance");

            string tagFileName = "TestTAGFile-TAGFile-Read-Stream.tag";
            //Guid projectUID = Guid.NewGuid();
            Guid assetID = Guid.NewGuid();

            byte[] tagContent;
            using (FileStream tagFileStream =
                new FileStream(Path.Combine("TestData", "TAGFiles", tagFileName),
                    FileMode.Open, FileAccess.Read))
            {
                tagContent = new byte[tagFileStream.Length];
                tagFileStream.Read(tagContent, 0, (int)tagFileStream.Length);
            }

            SubmitTAGFileResponse response = submission.Execute(new SubmitTAGFileRequestArgument()
            {
                ProjectID = Guid.NewGuid(),
                AssetID = assetID,
                TagFileContent = tagContent,
                TAGFileName = tagFileName,
                TCCOrgID = ""
            });

            Assert.True(response.Success, $"Response is not successful. Filename={response.FileName}, exception={response.Exception}");
        }
    }
}
