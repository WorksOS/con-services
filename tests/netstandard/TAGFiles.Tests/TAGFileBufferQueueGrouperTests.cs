using System;
using System.Collections.Generic;
using System.Linq;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
    public class TAGFileBufferQueueGrouperTests : IClassFixture<DILoggingFixture>
  {
        [Fact]
        public void Test_TAGFileBufferQueueGrouper_Creation()
        {
            TAGFileBufferQueueGrouper grouper = new TAGFileBufferQueueGrouper();

            Assert.NotNull(grouper);
        }

        [Fact]
        public void Test_TAGFileBufferQueueGrouper_AddAndExtractTagFile()
        {
            TAGFileBufferQueueGrouper grouper = new TAGFileBufferQueueGrouper();

            const string tagFileName = "TestTAGFile - TAGFile - Read - Stream.tag";
            Guid projectID = Guid.NewGuid();
            Guid assetID = Guid.NewGuid();

            TAGFileBufferQueueKey tagKey = new TAGFileBufferQueueKey(tagFileName, projectID, assetID);

            grouper.Add(tagKey);

            // Test the project is not returned if it is included in the avoid list
            var noTagFiles = grouper.Extract(new List<Guid>{projectID}, out Guid noProjectUID)?.ToList();
            Assert.True(null == noTagFiles, $"Extract from grouper with avoided project {projectID} returned a result for project {noProjectUID}");

            // Test the key is present in the extracted list of tag files for the given project
            var tagFiles = grouper.Extract(new List<Guid>(), out Guid extractedProjectID)?.ToList();

            Assert.True(null != tagFiles, "Returned list of grouped tag files is null");
            Assert.True(1 == tagFiles.Count, $"Returned list of grouped tag files does not have a single item (count = {tagFiles.Count}");

            Assert.True(extractedProjectID == tagFiles[0].ProjectID, $"Project UID does not match projhect UID out paramter from extract call {extractedProjectID} versus {tagFiles[0].ProjectID}");
            Assert.True(tagKey.AssetID == tagFiles[0].AssetID, $"Asset UIDs do not match {tagKey.AssetID} versus {tagFiles[0].AssetID}");
            Assert.True(tagKey.ProjectID == tagFiles[0].ProjectID, $"Project UIDs do not match {tagKey.ProjectID} versus {tagFiles[0].ProjectID}");
            Assert.True(tagKey.FileName == tagFiles[0].FileName, $"Filenames do not match {tagKey.FileName} versus {tagFiles[0].FileName}");

            //Test there are no more TAG files to extract from the grouper
            var tagFiles2 = grouper.Extract(null, out Guid _)?.ToList();
            
            Assert.True(null == tagFiles2, "Extract from empty grouper returned a non null result");
        }

        [Fact]
        public void Test_TAGFileBufferQueueGrouper_AddAndExtractTagFiles()
        {
            TAGFileBufferQueueGrouper grouper = new TAGFileBufferQueueGrouper();

            const string tagFileName = "TestTAGFile - TAGFile - Read - Stream.tag";
            Guid projectID = Guid.NewGuid();
            Guid assetID = Guid.NewGuid();

            // Add twice the limit of TAG files to the same project/asset combination to the grouper and ensure there are two full buckets returned
            for (int i = 0; i < 2 * TAGFileBufferQueueGrouper.kMaxNumberOfTAGFilesPerBucket; i++)
            {
                grouper.Add(new TAGFileBufferQueueKey($"{i} - {tagFileName}", projectID, assetID));
            }

            // Test the project is not returned if it is included in the avoid list
            var noTagFiles = grouper.Extract(new List<Guid> { projectID }, out Guid noProjectUID)?.ToList();
            Assert.True(null == noTagFiles, $"Extract from grouper with avoided project {projectID} returned a result for project {noProjectUID}");

            // Test there are two full groups, and no more
            var tagFilesGroup = grouper.Extract(new List<Guid>(), out Guid _)?.ToList();
            Assert.True(null != tagFilesGroup, "Returned list of grouped tag files is null");
            Assert.True(TAGFileBufferQueueGrouper.kMaxNumberOfTAGFilesPerBucket == tagFilesGroup.Count, $"First returned list of grouped tag files does not have the grouper limit of TAG files {tagFilesGroup.Count} vs {TAGFileBufferQueueGrouper.kMaxNumberOfTAGFilesPerBucket}");

            tagFilesGroup = grouper.Extract(new List<Guid>(), out Guid _)?.ToList();
            Assert.True(null != tagFilesGroup, "Returned list of grouped tag files is null");
            Assert.True(TAGFileBufferQueueGrouper.kMaxNumberOfTAGFilesPerBucket == tagFilesGroup.Count, $"Second returned list of grouped tag files does not have the grouper limit of TAG files {tagFilesGroup.Count} vs {TAGFileBufferQueueGrouper.kMaxNumberOfTAGFilesPerBucket}");

            //Test there are no more TAG files to extract from the grouper
            var tagFiles2 = grouper.Extract(new List<Guid>(), out Guid _)?.ToList();
            Assert.True(null == tagFiles2, "Extract from empty grouper returned a non null result");
        }
    }
}
