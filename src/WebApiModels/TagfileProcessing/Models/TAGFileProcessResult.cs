

namespace VSS.Productivity3D.WebApiModels.TagfileProcessing.Models
{
    /// <summary>
    /// The set of response codes returned by TAG file submission
    /// </summary>
    /// 
    public enum TagFileProcessResult
    {
        /// <summary>
        /// TAG file submitted with no error
        /// </summary>
        OK = 0,

        /// <summary>
        /// An unknown condition has prevented acceptance of the TAG file
        /// </summary>
        Unknown = 1,

        /// <summary>
        /// 
        /// </summary>
        OnSubmissionBaseConnectionFailure = 2,

        /// <summary>
        /// 
        /// </summary>
        OnSubmissionVerbConnectionFailure = 3,

        /// <summary>
        /// 
        /// </summary>
        OnSubmissionResultConnectionFailure = 4,

        /// <summary>
        /// The TAG file was found to be corrupted on its pre-processing scan
        /// </summary>
        FileReaderCorruptedTAGFileData = 5,

        /// <summary>
        /// The TAG file processor was unable to locate a machine/asset to process the TAG file against. This may happen when the TAG file identifies a telematics device
        /// such as a SNM940, but the telematics device is not known to the system.
        /// </summary>
        OnChooseMachineUnknownMachine = 6,

        /// <summary>
        /// The TAG file was found to be invalid when attempting to locate the machine/asset to process the TAG file against
        /// </summary>
        OnChooseMachineInvalidTagFile = 7,

        /// <summary>
        /// The machine/asset to process the TAG file against, or the customer that owns it, does not have sufficient subscriptions to allow processing the TAG file into the project.
        /// </summary>
        OnChooseMachineInvalidSubscriptions = 8,

        /// <summary>
        /// The TAG file processor was unable to determine the machine/asset to process the TAG file againsdt
        /// </summary>
        OnChooseMachineUnableToDetermineMachine = 9,

        /// <summary>
        /// The TAG file processor was unable to determine the project to process the TAG file into
        /// </summary>
        OnChooseDataModelUnableToDetermineDataModel = 10,

        /// <summary>
        ///  The TAg file processor could not convert the WGS84 project boundary into the project grid coordinate system. This may be due to no coordinate system assigned, 
        ///  or a coordinate system that is not relevant to the project.
        /// </summary>
        OnChooseDataModelCouldNotConvertDataModelBoundaryToGrid = 11,

        /// <summary>
        /// The TAG file contained no measured epochs from which to process gridded cell pass data.
        /// </summary>
        OnChooseDataModelNoGridEpochsFoundInTAGFile = 12,

        /// <summary>
        /// The project boundary supplied to the TAG file processor did not contain sufficient vertices.
        /// </summary>
        OnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices = 13,

        /// <summary>
        /// The first measured blade epoch in the TAG file did not lie within the supplied project boundary.
        /// </summary>
        OnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary = 14
    }
}