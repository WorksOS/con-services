
namespace VSS.Raptor.Service.WebApiModels.ProductionData.Models
{
    public class DesignNames 
    {
      /// <summary>
      /// The name of the design.
      /// </summary>
      /// <value>
      /// The name of the design.
      /// </value>
        public string designName { get; private set; }
        /// <summary>
        ///The Raptor design identifier.
        /// </summary>
        /// <value>
        /// The design identifier.
        /// </value>
        public long designId { get; private set; }

        public static DesignNames CreateDesignNames(string name, long id)
        {
            return new DesignNames() {designId = id, designName = name};
        }

        /// <summary>
        /// Create example instance of MachineExecutionResult to display in Help documentation.
        /// </summary>
        public static DesignNames HelpSample
        {
            get
            {
                return new DesignNames()
                {
                    designName = "The very best design",
                    designId = 1024,
                };
            }
        }

    }
}