using System;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.IO;
using Xunit;

namespace VSS.TRex.Tests.IO
{
  public class TResSpanTests
  {
    [Fact]
    public void Creation_Default()
    {
      var span = new TRexSpan<CellPass>();

      span.Count.Should().Be(0);
      span.Capacity.Should().Be(0);
      span.Offset.Should().Be(0);
      span.Elements.Should().BeNull();
      span.PoolAllocated.Should().BeFalse();
      span.OffsetPlusCount.Should().Be(0);
    }

    [Fact]
    public void Creation_Specific_NonPoolAllocated()
    {
      var span = new TRexSpan<CellPass>(new CellPass[2], 0, 2, false, false);

      span.Count.Should().Be(0);
      span.Capacity.Should().Be(2);
      span.Offset.Should().Be(0);
      span.Elements.Should().NotBeNull();
      span.PoolAllocated.Should().BeFalse();
      span.OffsetPlusCount.Should().Be(0);
    }

    [Fact]
    public void Creation_Specific_PoolAllocated()
    {
      var span = new TRexSpan<CellPass>(new CellPass[2], 0, 2, true, false);

      span.Count.Should().Be(0);
      span.Capacity.Should().Be(2);
      span.Offset.Should().Be(0);
      span.Elements.Should().NotBeNull();
      span.PoolAllocated.Should().BeTrue();
      span.OffsetPlusCount.Should().Be(0);
    }

    [Fact]
    public void Add_SimpleSpan_One()
    {
      var baseTime = DateTime.UtcNow;
      var span = new TRexSpan<CellPass>(new CellPass[2], 0, 2, false, false);
      var cp = new CellPass
      {
        Time = baseTime
      };

      span.Add(cp);

      span.Count.Should().Be(1);
      span.First().Should().BeEquivalentTo(cp);
      span.Last().Should().BeEquivalentTo(cp);
      span.GetElement(0).Should().BeEquivalentTo(cp);

      span.OffsetPlusCount.Should().Be(1);
    }

    [Fact]
    public void Add_SimpleSpanTwo()
    {
      var baseTime = DateTime.UtcNow;
      var span = new TRexSpan<CellPass>(new CellPass[2], 0, 2, false, false);
      var cp1 = new CellPass
      {
        Time = baseTime
      };
      var cp2 = new CellPass
      {
        Time = baseTime.AddMinutes(1)
      };

      span.Add(cp1);
      span.Add(cp2);

      span.Count.Should().Be(2);
      span.First().Should().BeEquivalentTo(cp1);
      span.Last().Should().BeEquivalentTo(cp2);

      span.GetElement(0).Should().BeEquivalentTo(cp1);
      span.GetElement(1).Should().BeEquivalentTo(cp2);

      span.OffsetPlusCount.Should().Be(2);
    }


    [Theory]
    [InlineData(10, 0, 2)]
    [InlineData(10, 5, 2)]
    [InlineData(10, 8, 2)]
    public void Add_CentralSpanTwo(int poolSize, int spanOffset, int spanCapacity)
    {
      var baseTime = DateTime.UtcNow;
      var span = new TRexSpan<CellPass>(new CellPass[poolSize], spanOffset, spanCapacity, false, false);
      var cp1 = new CellPass
      {
        Time = baseTime
      };
      var cp2 = new CellPass
      {
        Time = baseTime.AddMinutes(1)
      };

      span.Add(cp1);
      span.Add(cp2);

      span.Count.Should().Be(2);
      span.First().Should().BeEquivalentTo(cp1);
      span.Last().Should().BeEquivalentTo(cp2);

      span.GetElement(0).Should().BeEquivalentTo(cp1);
      span.GetElement(1).Should().BeEquivalentTo(cp2);

      span.OffsetPlusCount.Should().Be(spanOffset + span.Count);
    }

    [Fact]
    public void Insert_AtBeginning()
    {
      var baseTime = DateTime.UtcNow;
      var span = new TRexSpan<CellPass>(new CellPass[2], 0, 2, false, false);
      var cp1 = new CellPass
      {
        Time = baseTime
      };
      var cp2 = new CellPass
      {
        Time = baseTime.AddMinutes(1)
      };

      span.Add(cp1);
      span.Insert(cp2, 0);

      span.Count.Should().Be(2);
      span.First().Should().BeEquivalentTo(cp2);
      span.Last().Should().BeEquivalentTo(cp1);

      span.GetElement(0).Should().BeEquivalentTo(cp2);
      span.GetElement(1).Should().BeEquivalentTo(cp1);

      span.OffsetPlusCount.Should().Be(2);
    }

    [Fact]
    public void Insert_InMiddle()
    {
      var baseTime = DateTime.UtcNow;
      var span = new TRexSpan<CellPass>(new CellPass[3], 0, 3, false, false);
      var cp1 = new CellPass
      {
        Time = baseTime
      };
      var cp2 = new CellPass
      {
        Time = baseTime.AddMinutes(1)
      };

      span.Add(cp1);
      span.Add(cp1);
      span.Insert(cp2, 1);

      span.Count.Should().Be(3);
      span.First().Should().BeEquivalentTo(cp1);
      span.Last().Should().BeEquivalentTo(cp1);

      span.GetElement(0).Should().BeEquivalentTo(cp1);
      span.GetElement(1).Should().BeEquivalentTo(cp2);

      span.OffsetPlusCount.Should().Be(3);
    }

    [Fact]
    public void GetElement_Empty_Fail()
    {
      var span = new TRexSpan<CellPass>(new CellPass[3], 0, 3, false, false);
      Action act = () => span.GetElement(0);
      act.Should().Throw<ArgumentException>("Index out of range");
    }

    [Fact]
    public void GetElement_SingleElement_Success()
    {
      var baseTime = DateTime.UtcNow;
      var span = new TRexSpan<CellPass>(new CellPass[3], 0, 3, false, false);

      var cp = new CellPass
      {
        Time = baseTime
      };

      span.Add(cp);

      span.GetElement(0).Should().BeEquivalentTo(cp);
    }

    [Fact]
    public void GetElement_SingleElement_RangeFailure()
    {
      var baseTime = DateTime.UtcNow;
      var span = new TRexSpan<CellPass>(new CellPass[3], 0, 3, false, false);

      var cp = new CellPass
      {
        Time = baseTime
      };

      span.Add(cp);

      Action act = () => span.GetElement(-1);
      act.Should().Throw<ArgumentException>("Index out of range");

      act = () => span.GetElement(1);
      act.Should().Throw<ArgumentException>("Index out of range");
    }

    [Fact]
    public void SetElement_Empty_Fail()
    {
      var span = new TRexSpan<CellPass>(new CellPass[3], 0, 3, false, false);
      Action act = () => span.SetElement(new CellPass(), 0);
      act.Should().Throw<ArgumentException>("Index out of range");
    }


    [Fact]
    public void SetElement_SingleElement_Success()
    {
      var baseTime = DateTime.UtcNow;
      var span = new TRexSpan<CellPass>(new CellPass[3], 0, 3, false, false);

      var cp = new CellPass
      {
        Time = baseTime
      };
      var cp2 = new CellPass
      {
        Time = baseTime.AddMinutes(1)
      };

      span.Add(cp);
      span.SetElement(cp2, 0);
        
      span.GetElement(0).Should().BeEquivalentTo(cp2);
    }

    [Fact]
    public void SetElement_SingleElement_RangeFailure()
    {
      var baseTime = DateTime.UtcNow;
      var span = new TRexSpan<CellPass>(new CellPass[3], 0, 3, false, false);

      var cp = new CellPass
      {
        Time = baseTime
      };

      span.Add(cp);

      Action act = () => span.SetElement(cp, -1);
      act.Should().Throw<ArgumentException>("Index out of range");

      act = () => span.SetElement(cp, 1);
      act.Should().Throw<ArgumentException>("Index out of range");
    }

    [Fact]
    public void Copy_Simple()
    {
      var baseTime = DateTime.UtcNow;
      var span = new TRexSpan<CellPass>(new CellPass[2], 0, 2, false, false);
      var cp1 = new CellPass
      {
        Time = baseTime
      };
      var cp2 = new CellPass
      {
        Time = baseTime.AddMinutes(1)
      };

      span.Add(cp1);
      span.Add(cp2);

      var span2 = new TRexSpan<CellPass>(new CellPass[2], 0, 2, false, false);
      span2.Copy(span, 2);

      span2.Count.Should().Be(2);
      span2.GetElement(0).Should().BeEquivalentTo(cp1);
      span2.GetElement(1).Should().BeEquivalentTo(cp2);
    }

    [Fact]
    public void Copy_Central()
    {
      var baseTime = DateTime.UtcNow;
      var span = new TRexSpan<CellPass>(new CellPass[12], 5, 2, false, false);
      var cp1 = new CellPass
      {
        Time = baseTime
      };
      var cp2 = new CellPass
      {
        Time = baseTime.AddMinutes(1)
      };

      span.Add(cp1);
      span.Add(cp2);

      var span2 = new TRexSpan<CellPass>(new CellPass[8], 0, 8, false, false);
      span2.Copy(span, 2);

      span2.Count.Should().Be(2);
      span2.GetElement(0).Should().BeEquivalentTo(cp1);
      span2.GetElement(1).Should().BeEquivalentTo(cp2);
    }
  }
}
