using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGrids
{
  public class SubGridQOSTaskSchedulerTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void Creation()
    {
      var scheduler = new SubGridQOSTaskScheduler();
      scheduler.Should().NotBeNull();
    }

    [Fact]
    public void Creation2()
    {
      var scheduler = new SubGridQOSTaskScheduler(1, 1);
      scheduler.Should().NotBeNull();
    }

    [Fact]
    public void SchedulesSingleTaskWithTaskLimitOfOne()
    {
      var calledCount = 0;

      void ProcessorMethod(SubGridCellAddress[] addresses)
      {
        calledCount++;
      }

      var scheduler = new SubGridQOSTaskScheduler();
      scheduler.Schedule(new List<SubGridCellAddress[]> {new [] {new SubGridCellAddress(0, 0)}}, ProcessorMethod, 1).Should().BeTrue();

      calledCount.Should().Be(1);
    }

    [Fact]
    public void LimitsConcurrentSchedulersToMaximum()
    {
      var calledCount = 0;
      var lockObj = new object();

      void ProcessorMethod(SubGridCellAddress[] addresses)
      {
        lock (lockObj)
        {
          calledCount++;
        }
      }

      var tasks = new List<Task>();

      // Create a scheduler with one concurrent session and one concurrent tasks per session.
      // Schedule two sessions with a single tasks each and validate just one runs before blocking
      var scheduler = new SubGridQOSTaskScheduler(1, 1);
      lock (lockObj)
      {
        for (var i = 0; i < scheduler.MaxConcurrentSchedulerSessions + 1; i++)
        {
          tasks.Add(Task.Run(() => scheduler.Schedule(new List<SubGridCellAddress[]> {new [] {new SubGridCellAddress(0, 0)}}, ProcessorMethod, 1).Should().BeTrue()));
        }

        calledCount.Should().Be(0);
        // Sleep a little to allow all tasks to reach blockers (task waits or locks)
        Thread.Sleep(1000);

        scheduler.TotalSchedulerSessions.Should().Be(scheduler.MaxConcurrentSchedulerSessions + 1);
        scheduler.CurrentExecutingSessionCount.Should().Be(1);
      }

      Task.WaitAll(tasks.ToArray());

      // The lock has now released and all tasks waited, all pending and executing sessions should now complete
      scheduler.CurrentExecutingSessionCount.Should().Be(0);
      scheduler.TotalSchedulerSessions.Should().Be(0);
      calledCount.Should().Be(scheduler.MaxConcurrentSchedulerSessions + 1);
    }

    [Fact]
    public void LimitsConcurrentTasksToMaximum()
    {
      var calledCount = 0;
      var lockObj = new object();

      void ProcessorMethod(SubGridCellAddress[] addresses)
      {
        lock (lockObj)
        {
          calledCount++;
        }
      }

      var tasks = new List<Task>();

      // Create a scheduler with one concurrent session and 1 concurrent tasks per session.
      // Schedule a session with two tasks and validate just one runs before blocking
      var scheduler = new SubGridQOSTaskScheduler(1, 1);
      lock (lockObj)
      {
        tasks.Add(Task.Run(() => scheduler.Schedule(new List<SubGridCellAddress[]>
        {
          new[]
          {
            new SubGridCellAddress(0, 0),
          },
          new[]
          {
            new SubGridCellAddress(1, 1)
          }
        }, ProcessorMethod, 1).Should().BeTrue()));

        calledCount.Should().Be(0);
        // Sleep a little to allow all tasks to reach blockers (task waits or locks)
        Thread.Sleep(1000);

        scheduler.TotalSchedulerSessions.Should().Be(1);
        scheduler.CurrentExecutingSessionCount.Should().Be(1);
        scheduler.CurrentExecutingTaskCount.Should().Be(1);
      }

      Task.WaitAll(tasks.ToArray());

      // The lock has now released and all tasks waited, all pending and executing sessions should now complete
      scheduler.CurrentExecutingSessionCount.Should().Be(0);
      scheduler.TotalSchedulerSessions.Should().Be(0);
      calledCount.Should().Be(2);
    }
  }
}

