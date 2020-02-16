using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class CoreConstants
  {
    public const string WORKFLOW_NOT_IWORKFLOW = "Workflow: {0} does not implement {1}. See InnerException for details.";
    
    public const string WORKFLOW_CANNOT_BE_INITIALIZED = "Workflow cannot be initialized by {0}. Tried {1} and {2}. See InnerException for details.";


    public const string INPUT_DICTIONARY_KEY_NOT_FOUND = "InputDictionary does not contain a value for type \"{0}\".";
    public const string INPUT_DICTIONARY_ARGUMENT_EXCEPTION = "InputDictionary cannot return a non-null value for type \"{0}\".";
    public const string INPUT_DICTIONARY_INVALID_CAST = "InputDictionary cannot cast \"{0}\" to \"{1}\".";


    public const string VALIDATION_PASSED = "{0} validation passed.";
    public const string VALIDATION_PASSED_WITH_WARNINGS = VALIDATION_PASSED + "WARNINGS: {1}";
    public const string VALIDATION_FAILED = "{0} FAILED validation. ERRORS: {1}";


    public const string WORKFLOW_COMPLETED_SUCCESSFULLY = "Workflow completed successfully!";
    public const string WORKFLOW_FAILED = "Workflow FAILED! Detail: {0}";
    public const string WORKFLOW_HAS_NO_ACTIVITY_SEQUENCES = "Workflow has no AcitivitySequences to execute.";
    public const string TRANSACTION_STARTED = "Transaction started.";
    public const string TRANSACTION_COMMITED = "Transaction committed.";
    public const string TRANSACTION_ROLLED_BACK = "Transaction rolled back.";
  }
}
