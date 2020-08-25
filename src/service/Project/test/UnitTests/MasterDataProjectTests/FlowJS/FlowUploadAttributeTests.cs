using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.MasterData.ProjectTests.FlowJS.Utils;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using Xunit;

namespace VSS.MasterData.ProjectTests.FlowJS
{
  public class FlowUploadAttributeTests : IClassFixture<TestClientProviderFixture>
  {
    private readonly FlowFileUploader _fileUploader;

    private const string SOURCE_FILE = "KubernetesClient.dll"; // Any file large enough to be chunked.
    private const string UPLOAD_URL = "http://localhost/api/v6/importedfile"; // Any POST URI that uses FlowUploadAttribute.

    public FlowUploadAttributeTests(TestClientProviderFixture fixture)
    {
      _fileUploader = new FlowFileUploader(fixture.RestClient);
    }

    [Fact]
    public async Task FlowJS_Should_fail_on_invalid_file_extension()
    {
      var result = await _fileUploader.UploadFileData<ProjectV6DescriptorsSingleResult>(
        File.ReadAllBytes(SOURCE_FILE),
        "mock.xxx",
        UPLOAD_URL);

      result.Should().BeNull();
    }

    [Fact]
    public async Task FlowJS_Should_chunk_uploaded_file()
    {
      var result = await _fileUploader.UploadFileData<ProjectV6DescriptorsSingleResult>(
        File.ReadAllBytes(SOURCE_FILE),
        "mock.dxf",
        UPLOAD_URL);

      result.Should().NotBeNull();
      result.Message.Should().Be("Missing ProjectUID.");
    }
  }
}
