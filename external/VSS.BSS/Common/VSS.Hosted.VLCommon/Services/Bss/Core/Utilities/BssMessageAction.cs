using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class BssMessageAction
  {
    public static IDictionary<Type, ActionEnum[]> MessageActions = new Dictionary<Type, ActionEnum[]>
    {
      {typeof(AccountHierarchy), new [] {
          ActionEnum.Created, 
          ActionEnum.Updated,
          ActionEnum.Deleted, 
          ActionEnum.Deactivated, 
          ActionEnum.Reactivated
        }},
      {typeof(InstallBase), new [] {
          ActionEnum.Created, 
          ActionEnum.Updated,
          ActionEnum.UpdatedMerge
        }},
      {typeof(DeviceReplacement), new[]{ 
          ActionEnum.Replaced, 
          ActionEnum.Swapped 
        }},
      {typeof(ServicePlan), new[]{
          ActionEnum.Activated,
          ActionEnum.Updated,
          ActionEnum.Cancelled
        }},
      {typeof(DeviceRegistration), new []
      { 
        ActionEnum.Registered, 
        ActionEnum.Deregistered 
      }}
    };

    public static bool IsValidForMessage<TMessage>(object actionAsObj, TMessage message)
    {
      var action = actionAsObj.ToString().ToEnum<ActionEnum>();

      ActionEnum[] actions;
      return MessageActions.TryGetValue(message.GetType(), out actions) && actions.Contains(action);
    }
  }
}