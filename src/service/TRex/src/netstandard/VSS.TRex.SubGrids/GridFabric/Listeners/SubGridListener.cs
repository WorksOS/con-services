using Apache.Ignite.Core.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.SubGrids.GridFabric.Listeners
{
  /// <summary>
  /// SubGridListener implements a listening post for sub grid results being sent by processing nodes back
  /// to the local context for further processing when using a progressive style of sub grid requesting. 
  /// Sub grids are sent in groups as serialized streams held in memory streams to minimize serialization/deserialization overhead
  /// </summary>
  public class SubGridListener : IMessageListener<ISerialisedByteArrayWrapper>, IBinarizable, IFromToBinary
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridListener>();

    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// Count of the number of responses received by this listener
    /// </summary>
    private int responseCounter;

    /// <summary>
    /// Local reference to the client sub grid factory
    /// </summary>
    private IClientLeafSubGridFactory _clientLeafSubGridFactory;

    /// <summary>
    /// The reference to the TRexTask responsible for handling the returned sub grid information from the processing cluster
    /// </summary>
    private readonly ITRexTask TRexTask;

    /// <summary>
    /// Processes a response containing a set of sub grids from the sub grid processor for a request
    /// </summary>
    /// <param name="message"></param>
    private void ProcessResponse(ISerialisedByteArrayWrapper message)
    {
      using (var MS = new MemoryStream(message.Bytes))
      {
        using (var reader = new BinaryReader(MS, Encoding.UTF8, true))
        {
          // Read the number of sub grid present in the stream
          var responseCount = reader.ReadInt32();

          Log.LogDebug($"Sub grid listener processing a collection of {responseCount} sub grid results");

          // Create a single instance of the client grid. The approach here is that TransferResponse does not move ownership 
          // to the called context (it may clone the passed in client grid if desired)
          var clientGrids = new IClientLeafSubGrid[responseCount][];

          try
          {
            for (var i = 0; i < responseCount; i++)
            {
              var subGridCount = reader.ReadInt32();
              clientGrids[i] = new IClientLeafSubGrid[subGridCount];

              for (var j = 0; j < subGridCount; j++)
              {
                clientGrids[i][j] = _clientLeafSubGridFactory.GetSubGrid(TRexTask.GridDataType);

                // Check if the returned sub grid is null
                if (reader.ReadBoolean())
                  clientGrids[i][j].Read(reader);
               // else  - Remove to reduce unwanted log traffic
               //   Log.LogWarning($"Sub grid at position [{i},{j}] in sub grid response array is null");
              }
            }

            // Log.InfoFormat("Transferring response#{0} to processor (from thread {1})", thisResponseCount, System.Threading.Thread.CurrentThread.ManagedThreadId);

            // Send the decoded grid to the PipelinedTask, but ensure sub grids are serialized into the TRexTask
            // (no assumption of thread safety within the TRexTask itself)
            try
            {
              lock (TRexTask)
              {
                for (var i = 0; i < responseCount; i++)
                {
                  var thisResponseCount = ++responseCounter;

                  if (TRexTask.TransferResponse(clientGrids[i]))
                  {
                    // Log.DebugFormat("Processed response#{0} (from thread {1})", thisResponseCount, System.Threading.Thread.CurrentThread.ManagedThreadId);
                  }
                  else
                  {
                    Log.LogInformation($"Processing response#{thisResponseCount} FAILED (from thread {Thread.CurrentThread.ManagedThreadId})");
                  }
                }
              }
            }
            finally
            {
              // Tell the pipeline that a set of sub grid have been completely processed
              TRexTask.PipeLine.SubGridsProcessed(responseCount);
            }
          }
          finally
          {
            // Return the client grid to the factory for recycling now its role is complete here... when using SimpleConcurrentBag
            _clientLeafSubGridFactory.ReturnClientSubGrids(clientGrids, responseCount);
          }
        }
      }
    }

    /// <summary>
    /// The method called to announce the arrival of a message from a remote context in the cluster
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool Invoke(Guid nodeId, ISerialisedByteArrayWrapper message)
    {
      // Todo: Check if there are more performant approaches for handing this off asynchronously
      Task.Run(() => ProcessResponse(message));

      return true;
    }

    public SubGridListener() { }


    /// <summary>
    /// Constructor accepting a rexTask to pass sub grids into
    /// </summary>
    /// <param name="tRexTask"></param>
    public SubGridListener(ITRexTask tRexTask)
    {
      TRexTask = tRexTask;
      _clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubGridFactory>();
    }

    /// <summary>
    /// The sub grid response listener has no serializable state
    /// </summary>
    /// <param name="writer"></param>
    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    /// <summary>
    /// The sub grid response listener has no serializable state
    /// </summary>
    /// <param name="reader"></param>
    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);
    }
  }
}
