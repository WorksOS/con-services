using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon.MTSMessages
{
   public abstract class UserDataMessage : PlatformMessage
   {
      [XmlIgnore]
      public PlatformMessage Parent 
      {
         get { return parent; }
         set { parent = value; }
      }

      protected PlatformMessage  parent;
   }

   public abstract class TrackerUserDataMessage : UserDataMessage
   {
      public override MessageCategory Category 
      {
         get { return MessageCategory.TrackerUserDataPayload; }
      }

      /// <summary>
      /// This method signature gives us implementation inheritance of the code to
      /// say that, yes, the event is published.  This is 99.9% the case.  Only those
      /// messages that have conditions to change it will implement an override.
      /// Note though that TrackerUserDataMessage does not implement IXmlEventFragment. It just
      /// provides this implementation so it is inherited; that doesn't corrupt
      /// the definition of a TrackerUserDataMessage since it is an abstract class that
      /// cannot exist anyway.  The proper test to know if a message implements
      /// a published event is to get its IXmlEventFragment interface; without it, the event
      /// cannot be published.
      /// </summary>
      public virtual bool EventIsPublished
      {
         get { return true; }
      }

      /// <summary>
      /// Most user data messages generate an EventID. The event ID can be Unparsed to mean
      /// that there is no special event with the message. Note that this method is also
      /// part of the IXmlEventFragment interface (leaving only the XmlEvent and CsvValues methods).
      /// </summary>
      public abstract UDEventID EventID { get; }
   }

   public abstract class BaseUserDataMessage : UserDataMessage
   {
      public override MessageCategory Category 
      {
         get { return MessageCategory.BaseUserDataPayload; }
      }
   }
}
