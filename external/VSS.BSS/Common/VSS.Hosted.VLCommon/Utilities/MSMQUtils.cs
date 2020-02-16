using System;
using System.Messaging;

namespace VSS.Hosted.VLCommon
{
  public static class MSMQUtils
  {
    public static void EnsureMSMQ(string name, string path, bool transactional)
    {
      if (!path.StartsWith("."))
        return;

      try
      {
        if (MessageQueue.Exists(path))
          return;
      }
      catch (MessageQueueException)
      {
        return;
      }

      MessageQueue result = MessageQueue.Create(path, transactional);
      if (result == null)
        throw new ApplicationException(String.Format("Unable to create queue ({0}) at {1}",
           name, path));

      result.Label = name;
      result.QueueName = name;

      result.Close();
      result = new MessageQueue(path);

      result.SetPermissions(new MessageQueueAccessControlEntry(
         new Trustee("SYSTEM", null, TrusteeType.User),
         MessageQueueAccessRights.FullControl));

      result.SetPermissions(new MessageQueueAccessControlEntry(
         new Trustee("Administrators", null, TrusteeType.Group),
         MessageQueueAccessRights.FullControl));

      // For a remote computer to access this queue, we need to give it specific permissions here
      result.SetPermissions(new MessageQueueAccessControlEntry(
         new Trustee("Everyone", null, TrusteeType.Group),
         MessageQueueAccessRights.DeleteJournalMessage |
         MessageQueueAccessRights.DeleteMessage |
         MessageQueueAccessRights.GenericRead |
         MessageQueueAccessRights.GenericWrite |
         MessageQueueAccessRights.PeekMessage));

      // Flush the permission changes.
      result.Close();
      result = new MessageQueue(path);
    }

    public static string QueuePath(string queueName)
    {
      return string.Format(@".\private$\{0}", queueName);
    }
  }
}
