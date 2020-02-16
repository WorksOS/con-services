using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class InvalidAction : IActivity
  {
    private readonly object _action;
    private readonly Type _messageType;

    public InvalidAction(object action, Type messageType)
    {
      _action = action;
      _messageType = messageType;
    }

    public ActivityResult Execute(Inputs inputs)
    {
      string errorMessage = string.Format(BssConstants.ACTION_INVALID_FOR_MESSAGE, _action, _messageType.Name);

      return new BssErrorResult
      {
        FailureCode = BssFailureCode.ActionInvalid, 
        Summary = errorMessage
      };
    }
  }
}
