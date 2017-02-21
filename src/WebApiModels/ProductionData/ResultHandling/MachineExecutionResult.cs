using System.Linq;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling
{
  public class MachineExecutionResult : ContractExecutionResult
    {

    public MachineStatus[] MachineStatuses { get; private set; }

      /// <summary>
    /// Private constructor
    /// </summary>
        private MachineExecutionResult()
    {}

        /// <summary>
        /// Create instance of MachineExecutionResult
        /// </summary>
        public static MachineExecutionResult CreateMachineExecutionResult(MachineStatus[] machineDetails)
        {
          return new MachineExecutionResult
          {
            MachineStatuses = machineDetails,
          };
        }


        public void FilterByMachineId(long machineId)
        {
          MachineStatuses = MachineStatuses.Where(m => m.assetID == machineId).ToArray();
        }

        /// <summary>
        /// Create example instance of MachineExecutionResult to display in Help documentation.
        /// </summary>
        public static MachineExecutionResult HelpSample
        {
          get
          {
            return new MachineExecutionResult()
            {
                MachineStatuses = new []{Models.MachineStatus.HelpSample}
            };
          }
        }

    }
}