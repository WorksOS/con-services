using FluentAssertions;
using VSS.TRex.Volumes;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
  public class CutFillVolumeTests
  {
    [Fact]
    public void CutFillVolumeTest_Creation_NonNull()
    {
      var cv = new CutFillVolume(0, 0);
      cv.CutVolume.Should().Be(0);
      cv.FillVolume.Should().Be(0);
    }

    [Fact]
    public void CutFillVolumeTest_Creation_WithNull()
    {
      var cv = new CutFillVolume();
      cv.CutVolume.Should().Be(0);
      cv.FillVolume.Should().Be(0);
    }

    [Fact]
    public void AccumulatedVolumeTest()
    {
      var cv = new CutFillVolume();

      cv.AccumulatedVolume().Should().Be(0);

      cv.AddCutVolume(10);
      cv.AccumulatedVolume().Should().Be(10);
      cv.AddCutVolume(10);
      cv.AccumulatedVolume().Should().Be(20);
    }

    [Fact]
    public void AccumulatedVolume_BulkShrinkAdjustedTest()
    {
      var cv = new CutFillVolume();

      cv.AddCutFillVolume(100, 200);

      cv.AccumulatedVolume_BulkShrinkAdjusted(1.1, 0.8).Should().Be(100 * 1.1 + 200 * 0.8);
    }

    [Fact]
    public void AddCutFillVolumeTest()
    {
      var cv = new CutFillVolume();
      cv.AddCutFillVolume(100, 200);

      cv.AccumulatedVolume().Should().Be(300);
    }

    [Fact]
    public void AddCutVolumeTest()
    {
      var cv = new CutFillVolume();
      cv.AddCutVolume(100);

      cv.CutVolume.Should().Be(100);
      cv.FillVolume.Should().Be(0);
    }

    [Fact]
    public void AddFillVolumeTest()
    {
      var cv = new CutFillVolume();
      cv.AddFillVolume(100);

      cv.CutVolume.Should().Be(0);
      cv.FillVolume.Should().Be(100);
    }

    [Fact]
    public void AddVolumeTest()
    {
      var cv = new CutFillVolume();
      cv.AddVolume(new CutFillVolume(100, 200));

      cv.CutVolume.Should().Be(100);
      cv.FillVolume.Should().Be(200);

      cv.AddVolume(new CutFillVolume());

      cv.CutVolume.Should().Be(100);
      cv.FillVolume.Should().Be(200);
    }

    [Fact]
    public void AssignTest()
    {
      var cv = new CutFillVolume();
      cv.AddVolume(new CutFillVolume(100, 200));

      var cv2 = new CutFillVolume();
      cv2.Assign(cv);

      cv2.CutVolume.Should().Be(100);
      cv2.FillVolume.Should().Be(200);
    }

    [Fact]
    public void ExcessVolumeTest()
    {
      var cv = new CutFillVolume();
      cv.ExcessVolume().Should().Be(0);

      cv.AddCutFillVolume(100, 200);
      cv.ExcessVolume().Should().Be(cv.CutVolume - cv.FillVolume);
      cv.ExcessVolume().Should().Be(-100);
    }

    [Fact]
    public void ExcessVolume_BulkShrinkAdjustedTest()
    {
      var cv = new CutFillVolume();

      cv.AddCutFillVolume(100, 200);
      cv.ExcessVolume_BulkShrinkAdjusted(1.1, 0.8).Should().Be(100 * 1.1 - 200 * 0.8);
    }

    [Fact]
    public void CutVolume_BulkageAdjustedTest()
    {
      var cv = new CutFillVolume();

      cv.AddCutFillVolume(100, 200);
      cv.CutVolume_BulkageAdjusted(1.1).Should().Be(100 * 1.1);
    }

    [Fact]
    public void FillVolume_ShrinkageAdjustedTest()
    {
      var cv = new CutFillVolume();

      cv.AddCutFillVolume(100, 200);
      cv.FillVolume_ShrinkageAdjusted(0.8).Should().Be(200 * 0.8);
    }

    [Fact]
    public void FillVolume_HasAccumulatedVolumeTest()
    {
      var cv = new CutFillVolume();

      cv.HasAccumulatedVolume.Should().BeFalse();

      cv.AddCutVolume(100);
      cv.HasAccumulatedVolume.Should().BeTrue();

      cv = new CutFillVolume();
      cv.AddFillVolume(100);
      cv.HasAccumulatedVolume.Should().BeTrue();

      cv = new CutFillVolume();
      cv.AddCutFillVolume(100, 200);
      cv.HasAccumulatedVolume.Should().BeTrue();
    }

    [Fact]
    public void HasCutVolume()
    {
      var cv = new CutFillVolume();
      cv.HasCutVolume.Should().BeFalse();

      cv.AddCutVolume(100);
      cv.HasCutVolume.Should().BeTrue();
    }

    [Fact]
    public void HasFillVolume()
    {
      var cv = new CutFillVolume();
      cv.HasFillVolume.Should().BeFalse();

      cv.AddFillVolume(100);
      cv.HasFillVolume.Should().BeTrue();
    }
  }
}
