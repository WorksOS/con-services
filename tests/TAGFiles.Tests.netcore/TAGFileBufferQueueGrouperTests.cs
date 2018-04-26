using System;
using System.Collections.Generic;
using System.Linq;
using VSS.TRex.TAGFiles.Classes.Queues;
using Xunit;

namespace TAGFiles.Tests.netcore
{
    public class TAGFileBufferQueueGrouperTests
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
            Guid projectUID = Guid.NewGuid();
            Guid assetUID = Guid.NewGuid();

            TAGFileBufferQueueKey tagKey = new TAGFileBufferQueueKey(tagFileName, projectUID, assetUID);

            grouper.Add(tagKey);

            // Test the project is not returned if it is included in the avoid list
            var noTagFiles = grouper.Extract(new List<Guid>{projectUID}, out Guid noProjectUID)?.ToList();
            Assert.True(null == noTagFiles, $"Extract from grouper with avoided project {projectUID} returned a result for project {noProjectUID}");

            // Test the key is present in the extracted list of tag files for the given project
            var tagFiles = grouper.Extract(null, out Guid extractedProjectUID)?.ToList();

            Assert.True(null != tagFiles, "Returned list of grouped tag files is null");
            Assert.True(1 == tagFiles.Count, $"Returned list of grouped tag files does not have a single item (count = {tagFiles.Count}");

            Assert.True(extractedProjectUID == tagFiles[0].ProjectUID, $"Project UID does not match projhect UID out paramter from extract call {extractedProjectUID} versus {tagFiles[0].ProjectUID}");
            Assert.True(tagKey.AssetUID == tagFiles[0].AssetUID, $"Asset UIDs do not match {tagKey.AssetUID} versus {tagFiles[0].AssetUID}");
            Assert.True(tagKey.ProjectUID == tagFiles[0].ProjectUID, $"Project UIDs do not match {tagKey.ProjectUID} versus {tagFiles[0].ProjectUID}");
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
            Guid projectUID = Guid.NewGuid();
            Guid assetUID = Guid.NewGuid();

            // Add twice the limit of TAG files to the same project/asset combination to the grouper and ensure there are two full buckets returned
            for (int i = 0; i < 2 * TAGFileBufferQueueGrouper.kMaxNumberOfTAGFilesPerBucket; i++)
            {
                grouper.Add(new TAGFileBufferQueueKey($"{i} - {tagFileName}", projectUID, assetUID));
            }

            // Test the project is not returned if it is included in the avoid list
            var noTagFiles = grouper.Extract(new List<Guid> { projectUID }, out Guid noProjectUID)?.ToList();
            Assert.True(null == noTagFiles, $"Extract from grouper with avoided project {projectUID} returned a result for project {noProjectUID}");

            // Test there are two full groups, and no more
            var tagFilesGroup = grouper.Extract(null, out Guid _)?.ToList();
            Assert.True(null != tagFilesGroup, "Returned list of grouped tag files is null");
            Assert.True(TAGFileBufferQueueGrouper.kMaxNumberOfTAGFilesPerBucket == tagFilesGroup.Count, $"First returned list of grouped tag files does not have the grouper limit of TAG files {tagFilesGroup.Count} vs {TAGFileBufferQueueGrouper.kMaxNumberOfTAGFilesPerBucket}");

            tagFilesGroup = grouper.Extract(null, out Guid _)?.ToList();
            Assert.True(null != tagFilesGroup, "Returned list of grouped tag files is null");
            Assert.True(TAGFileBufferQueueGrouper.kMaxNumberOfTAGFilesPerBucket == tagFilesGroup.Count, $"Second returned list of grouped tag files does not have the grouper limit of TAG files {tagFilesGroup.Count} vs {TAGFileBufferQueueGrouper.kMaxNumberOfTAGFilesPerBucket}");

            //Test there are no more TAG files to extract from the grouper
            var tagFiles2 = grouper.Extract(null, out Guid _)?.ToList();
            Assert.True(null == tagFiles2, "Extract from empty grouper returned a non null result");
        }
    }
}
