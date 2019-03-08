using System;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.GridFabric.Requests
{
  public class TestArgument
  {
    public string Arg { get; set; } = "AnArgument";
  }

  public class TestResult
  {
    public bool Result { get; set; } = false;
  }

  public class TestComputeFunc : IComputeFunc<TestResult>, IComputeFuncArgument<TestArgument>
  {
    public TestArgument Argument { get; set; }

    public TestResult Invoke()
    {
      Argument.Arg.Should().Be("AnArgument");
      return new TestResult
      {
        Result = true
      };
    }
  }

  public class TestRequest : GenericPSNodeSpatialAffinityRequest<TestArgument, TestComputeFunc, TestResult>
  {
  }

  public class GenericPSNodeSpatialAffinityRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var request = new TestRequest();
      request.Should().NotBeNull();
    }

    [Fact]
    public void Execute_Success_Synchronous()
    {
      IgniteMock.AddClusterComputeSpatialAffinityGridRouting<TestComputeFunc, TestArgument, TestResult>();

      var request = new TestRequest();
      var response = request.Execute(new TestArgument(), new SubGridSpatialAffinityKey());
      response.Result.Should().Be(true);
    }

    [Fact]
    public async void Execute_Success_Asynchronous()
    {
      IgniteMock.AddClusterComputeSpatialAffinityGridRouting<TestComputeFunc, TestArgument, TestResult>();

      var request = new TestRequest();
      var response = await request.ExecuteAsync(new TestArgument(), new SubGridSpatialAffinityKey());
      response.Result.Should().Be(true);
    }

    [Fact]
    public void Execute_FailureWithNoAffinityKey_NullKey_Synchronous()
    {
      var request = new TestRequest();

      Action act = () => request.Execute(new TestArgument(), null);
      act.Should().Throw<TRexException>().WithMessage("Affinity based result execution requires an affinity key");
    }

    [Fact]
    public void Execute_FailureWithNoAffinityKey_NullKey_Asynchronous()
    {
      var request = new TestRequest();

      Action act = () => request.ExecuteAsync(new TestArgument(), null);
      act.Should().Throw<TRexException>().WithMessage("Affinity based result execution requires an affinity key");
    }

    [Fact]
    public void Execute_FailureWithNoAffinityKey_Synchronous()
    {
      var request = new TestRequest();

      Action act = () => request.Execute(new TestArgument());
      act.Should().Throw<TRexException>().WithMessage("Affinity based result execution requires an affinity key");
    }

    [Fact]
    public void Execute_FailureWithNoAffinityKey_Asynchronous()
    {
      var request = new TestRequest();

      Action act = () => request.ExecuteAsync(new TestArgument());
      act.Should().Throw<TRexException>().WithMessage("Affinity based result execution requires an affinity key");
    }

  }
}
