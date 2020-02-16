using System;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.Collections;
using System.Collections.Specialized;

namespace VSS.Hosted.VLCommon.MTSMessages
{
   public sealed class PlatformEvents
   {
      public sealed class Constants 
      {
         /// <summary>
         /// The Date Format that all dates should use when serializing themselves to XML
         /// </summary>
         public static readonly string  DateFormat = "dddd, MMMM d, yyyy HH:mm:ss UTC";

         public static readonly string  RealtimeNamespace    = "urn:schemas-tms:RealTimeAssetInfo";
         public static readonly string  V1CoherencyNamespace = "urn:schemas-tms:Coherency";

         public static readonly string SpeedingInformationCapability = "CI.OPTION.SPEEDING_EVENT";
      }

      public static Int64 ObfusticateIDNumber(Int64 id)
      {
         // Make the number opaque for things like position-harvester where we want an opaque number to track
         // events/positions for a particular vehicle (the opaque number) without knowing exactly which vehicle it
         // is on the platform.

         //@@@@
         return id;
      }

#if LEGACY
      public static IHighLevelPlatformEvent[] RetrieveEventsByTracker(Int64 trackerID, bool isAssetID, DateTime startGmt, DateTime endGmt, out DateTime realEndGmt) 
      {
         return RetrieveEventsByTracker(trackerID, isAssetID, startGmt, endGmt, out realEndGmt, true);
      }

      public static IHighLevelPlatformEvent[] RetrieveEventsByTracker(Int64 trackerID, bool isAssetID, DateTime startGmt, DateTime endGmt, out DateTime realEndGmt, bool onlyPublished) 
      {
         try 
         {
            IDataReader eventRdr = PlatformEventQueries.GetEventsByTracker(trackerID, isAssetID, startGmt, endGmt, out realEndGmt);

            return createEventsFromEventQuery(eventRdr, startGmt, endGmt, onlyPublished);
         } 
         catch (Exception)
         {
            realEndGmt = endGmt;
            return null;
         }
      }

      public static IHighLevelPlatformEvent[] RetrieveGlobalEvents(DateTime startGmt, DateTime endGmt)
      {
         return RetrieveGlobalEvents(startGmt, endGmt, true);
      }

      public static IHighLevelPlatformEvent[] RetrieveGlobalEvents(DateTime startGmt, DateTime endGmt, bool onlyPublished)
      {
         try
         {
            IDataReader eventRdr = PlatformEventQueries.GetGlobalEvents(startGmt, endGmt);

            return createEventsFromEventQuery(eventRdr, startGmt, endGmt, onlyPublished);
         }
         catch (Exception e)
         {
            // I just want to document the fact that while testing on Dev an exception
            // occurred stating that the state of package Event_Feed had changed or been
            // discarded.  Yet no one was even accessing the database at the time.

            Log.Exception("Exception in RetrieveGlobalEvents:", e);
            return null;
         }
      }

      /// <summary>
      /// Although the database stored procedure has a parameter for "fetch size in seconds",
      /// we currently only need a method where that parameter is null.  If that parameter ever
      /// is needed, an overloaded method can be created.
      /// </summary>
      /// <param name="namedBookmark">The name of the bookmark to use that has the last queried date range.</param>
      /// <returns>An array of publishable events.</returns>
      public static IHighLevelPlatformEvent[] RetrieveGlobalEvents(string namedBookmark)
      {
         DateTime unusedStartGmt;
         DateTime unusedEndGmt;

         return RetrieveGlobalEvents(namedBookmark, out unusedStartGmt, out unusedEndGmt);
      }

      public static IHighLevelPlatformEvent[] RetrieveGlobalEvents(string namedBookmark,
                                                                   out DateTime startGmt, out DateTime endGmt)
      {
         return RetrieveGlobalEvents(namedBookmark, out startGmt, out endGmt, true);
      }

      public static IHighLevelPlatformEvent[] RetrieveGlobalEvents(string namedBookmark,
                                                                   out DateTime startGmt, out DateTime endGmt,
                                                                   bool onlyPublished)
      {
         startGmt = DateTime.MinValue;
         endGmt   = DateTime.MinValue;

         try
         {
            IDataReader eventRdr = PlatformEventQueries.GetGlobalEvents(namedBookmark, out startGmt, out endGmt);

            return createEventsFromEventQuery(eventRdr, startGmt, endGmt, onlyPublished);
         }
         catch (Exception e)
         {
            // I just want to document the fact that while testing on Dev an exception
            // occurred stating that the state of package Event_Feed had changed or been
            // discarded.  Yet no one was even accessing the database at the time.

            Log.Exception("Exception in RetrieveGlobalEvents:", e);
            return null;
         }
      }

      private static IHighLevelPlatformEvent[] createEventsFromEventQuery(IDataReader rdr, DateTime startGmt, DateTime endGmt, bool onlyPublished) 
      {
         ArrayList events = new ArrayList();

         using (IDataReader eventRdr = rdr) 
         {
            // Pull in the event objects that we deal with.  Our returned cursor has 3
            // result sets.  The order of the result sets is UserDataMessage then
            // StateMessage then MsgResponse.

            hydrateEventFromOEMQuery         (events, eventRdr, startGmt, endGmt, onlyPublished);
            eventRdr.NextResult();
            hydrateEventFromStateQuery       (events, eventRdr, startGmt, endGmt, onlyPublished);
            eventRdr.NextResult();
            hydrateEventFromMsgResponseQuery (events, eventRdr, startGmt, endGmt, onlyPublished);
            eventRdr.NextResult();
            hydrateEventFromCoherencyMsgQuery(events, eventRdr, startGmt, endGmt, onlyPublished);
         }

         if (events.Count == 0) 
         {
            return null;
         }

         // Create our result array.  Note that the events are in no particular order.

         IHighLevelPlatformEvent[] result = new IHighLevelPlatformEvent[events.Count];

         events.CopyTo(result);

         // Sort the resulting array in ascending packet time.  We require a stable
         // sort so events from the same sources in the same second retain their order.

         Array.Sort(result, HighLevelPlatformEventComparer.CompareByPacketGmt);

         return result;
      }
#endif

      private static string ToTrueFalse(bool value, int version) 
      {
         // In version 1.0 of the realtime feed, we produce non-standard XML using True and False instead of true and false in the stream.
         // For 2.0, we follow XML.

         if (version == 1) 
         {
            return value ? "True" : "False";
         }

         return XmlConvert.ToString(value);
      }

      #region Support methods for event types

      /// <summary>
      /// Determines if the current connection has access to and is therefore supposed to receive
      /// events for the specified sub org (asset group).
      /// </summary>
      /// <param name="orgID">The orgID to check.</param>
      /// <returns><c>True</c> if the current connection has access to the specified orgID.  <c>False</c> otherwise.</returns>
      /// <remarks>
      /// A connections list of visible orgs is static.  It is initialized when the connection
      /// is made and is never refreshed.  Therefore, if the user's access changes, those changes will
      /// not be reflected until the user disconnects and reconnects.
      /// </remarks>
      private static bool canViewAssetGroup(Int64 orgID, AssetGroupOwnerPair[] assetGroupPairs)
      {
         if (assetGroupPairs != null) 
         {
            for (int i = 0; i < assetGroupPairs.Length; i++)
            {
               if (orgID == assetGroupPairs[i].AssetGroupID) 
               {
                  return true;
               }
            }
         }

         return false;
      }

      private static bool canViewGlobalEvent(Int64 orgID, AssetGroupOwnerPair[] assetGroupPairs) 
      {
         if (orgID == -1) 
         {
            return false;
         }

         if (assetGroupPairs != null) 
         {
            for (int i = 0; i < assetGroupPairs.Length; i++)
            {
               if (orgID == assetGroupPairs[i].OwningOrgID) 
               {
                  return true;
               }
            }
         }

         return false;
      }

      #endregion

      #region UserDataMessage Events (From Outgoing_Event_Msg)

#pragma warning disable 0649
      private class oemEventColumns 
      {
         public int  EventOrigin;
         public int  SequenceID;
         public int  AssetID;
         public int  AssetName;
         public int  DeviceID;
         public int  PacketGmt;
         public int  EventGmt;
         public int  EventID;
         
         public int  UserDataBin;
         public int  UserDataDesc;
         public int  DescTemplate;
         public int  DataParms;

         public int  LocationID;
         public int  LocationName;

         public int  Latitude;
         public int  Longitude;
         
         public int  Mileage;
         public int  Speed;
         public int  Heading;

         public int  DriverPresent;
         public int  DriverID;
         public int  MdtDriverID;
         public int  DriverDisplayName;

         public int  SiteStatusSource;

         public int  OwnerOrgID;

         public int  Street;
         public int  City;
         public int  State;
         public int  Zip;
         public int  Country;
      }
#pragma warning restore 0649

      #endregion

      #region State (Position) Events (From Outgoing_State_Msg)

#pragma warning disable 0649
      private class stateEventColumns
      {
         public int     EventOrigin;
         public int     SequenceID;
         public int     AssetID;
         public int     AssetName;
         public int     DeviceID;
         public int     PacketGmt;
         public int     EventGmt;

         public int     Latitude;
         public int     Longitude;
         
         public int     Mileage;
         public int     Speed;
         public int     Heading;
         public int     PostedSpeedLimit;

         public int     IsSpeeding;

         public int     Street;
         public int     City;
         public int     State;
         public int     Zip;
         public int     Country;

         public int     OwnerOrgID;
      }
#pragma warning restore 0649

      #endregion

      #region Response Events (From Msg_Response)

#pragma warning disable 0649
      private class msgResponseEventColumns 
      {
         public int     EventOrigin;
         public int     AssetID;
         public int     AssetName;
         public int     DeviceID;

         public int     MessageText;
         public int     ResponseChoices;
         public int     ResponseText;
         public int     ResponseSequenceID;

         public int     RtnReceiptPacketGmt;
         public int     ResponsePacketGmt;
         public int     AckPacketGmt;

         public int     RtnReceiptGmt;
         public int     ResponseGmt;
         public int     AckGmt;

         public int     OwnerOrgID;

         public int     LastName;
         public int     FirstName;
         public int     MiddleInitial;
         public int     Subject;
         public int     AppData;

         public int     SentGmt;

         public int     MessageTypeId;
         public int     MessageTypeDesc;
      }
#pragma warning restore 0649

      #endregion

      #region Coherency Events (From Coherency_Event_Msg)
#pragma warning disable 0649
      private class coherencyMsgEventColumns
      {
         public int EventOrigin;
         public int ChangeGMT;
         public int ChangeType;
         public int ChangeCategory;
         public int ID;
         public int RealtimeData;
      }
#pragma warning restore 0649

       #endregion

      private static oemEventColumns          oemOrdinals          = new oemEventColumns();
      private static stateEventColumns        stateOrdinals        = new stateEventColumns();
      private static msgResponseEventColumns  msgResponseOrdinals  = new msgResponseEventColumns();
      private static coherencyMsgEventColumns coherencyMsgOrdinals = new coherencyMsgEventColumns();

#if LEGACY
      private static void hydrateEventFromMsgResponseQuery(ArrayList events, IDataReader rdr, DateTime startGMT, DateTime endGMT, bool onlyPublished) 
      {
         // See if the event columns have been initialized yet; do it if necessary.
         // When uninitialized, all indices are zero and no two columns can have the
         // same ordinal so that's how we tell.

         IDataRecord row = (IDataRecord) rdr;

         if (rdr.FieldCount == 0) 
         {
            // If there are no fields, we have no data.

            return;
         }

         if (msgResponseOrdinals.AssetID == msgResponseOrdinals.MessageText) 
         {
            QueryHelpers.FillInColumnIndices(msgResponseOrdinals, row);
         }

         while (rdr.Read()) 
         {
            msgResponseEvent msg = new msgResponseEvent();

            // Ensure we have the correct datareader.

            if (Convert.ToInt32(row[msgResponseOrdinals.EventOrigin]) != (int) Origin.MessageResponseMessage) 
            {
               throw new ApplicationException("Wrong datareader for hydrateEventFromMsgResponseQuery");
            }

            // Fill in all the available fields from the cursor.

            if (!row.IsDBNull(msgResponseOrdinals.AssetID))
            {
               msg._AssetID            = Convert.ToInt64(row[msgResponseOrdinals.AssetID]);
            }
            if (!row.IsDBNull(msgResponseOrdinals.AssetName))
            {
               msg._AssetName          = Convert.ToString(row[msgResponseOrdinals.AssetName]);
            }
            if (!row.IsDBNull(msgResponseOrdinals.DeviceID))
            {
               msg._DeviceID           = Convert.ToInt64(row[msgResponseOrdinals.DeviceID]);
            }

            if (!row.IsDBNull(msgResponseOrdinals.MessageText))
            {
               msg.MessageText         = Convert.ToString(row[msgResponseOrdinals.MessageText]);
            }
            if (!row.IsDBNull(msgResponseOrdinals.ResponseChoices))
            {
               msg.ResponseChoices     = Convert.ToString(row[msgResponseOrdinals.ResponseChoices]);
            }
            if (!row.IsDBNull(msgResponseOrdinals.ResponseText))
            {
               msg.ResponseText        = Convert.ToString(row[msgResponseOrdinals.ResponseText]);
            }
            if (!row.IsDBNull(msgResponseOrdinals.ResponseSequenceID))
            {
               msg.ResponseSequenceID  = Convert.ToInt64(row[msgResponseOrdinals.ResponseSequenceID]);
            }
            if (!row.IsDBNull(msgResponseOrdinals.OwnerOrgID))
            {
               msg._OwnerOrgID         = Convert.ToInt64(row[msgResponseOrdinals.OwnerOrgID]);
            }
            if (!row.IsDBNull(msgResponseOrdinals.LastName))
            {
               msg.LastName            = Convert.ToString(row[msgResponseOrdinals.LastName]);
            }
            if (!row.IsDBNull(msgResponseOrdinals.FirstName))
            {
               msg.FirstName           = Convert.ToString(row[msgResponseOrdinals.FirstName]);
            }
            if (!row.IsDBNull(msgResponseOrdinals.MiddleInitial))
            {
               msg.MiddleInitial       = Convert.ToString(row[msgResponseOrdinals.MiddleInitial]);
            }
            if (!row.IsDBNull(msgResponseOrdinals.Subject))
            {
               msg.Subject             = Convert.ToString(row[msgResponseOrdinals.Subject]);
            }
            if (!row.IsDBNull(msgResponseOrdinals.AppData))
            {
               msg.AppData             = Convert.ToString(row[msgResponseOrdinals.AppData]);
            }

            if (!row.IsDBNull(msgResponseOrdinals.MessageTypeId))
            {
               msg.MessageTypeId       = Convert.ToInt64(row[msgResponseOrdinals.MessageTypeId]);
            }
            if (!row.IsDBNull(msgResponseOrdinals.MessageTypeDesc))
            {
               msg.MessageTypeDesc     = Convert.ToString(row[msgResponseOrdinals.MessageTypeDesc]);
            }

            // A single message response row can generate from 1 to 4 events depending on the times
            // of the three GMT members (SentGmt, AckPacketGmt, ResponsePacketGmt, RtnReceiptPacketGmt).
            // Go through each and generate events as needed.

            DateTime PacketGmt;

            // Check for the posting date of the message

            if (!row.IsDBNull(msgResponseOrdinals.SentGmt))
            {
               PacketGmt = row.GetDateTime(msgResponseOrdinals.SentGmt);

               if (PacketGmt.CompareTo(startGMT) >= 0  &&  PacketGmt.CompareTo(endGMT) < 0) 
               {
                  // The send occurred during our period.  Generate the event.

                  msgResponseEvent sendEvent = (msgResponseEvent) msg.Clone();

                  sendEvent.ResponseEventType = msgResponseEvent.ResponseType.InitialSend;
                  sendEvent._PacketGmt = PacketGmt;
                  sendEvent._EventGmt  = PacketGmt;

                  if (!onlyPublished  ||  sendEvent.EventIsPublished) 
                  {
                     events.Add(sendEvent);
                  }
               }
            }

            // Check for an acknowledgement event.

            if (!row.IsDBNull(msgResponseOrdinals.AckPacketGmt))
            {
               PacketGmt = row.GetDateTime(msgResponseOrdinals.AckPacketGmt);

               if (PacketGmt.CompareTo(startGMT) >= 0  &&  PacketGmt.CompareTo(endGMT) < 0) 
               {
                  // The acknowledgement occurred during our period.  Generate the event.

                  msgResponseEvent ackEvent = (msgResponseEvent) msg.Clone();

                  ackEvent.ResponseEventType = msgResponseEvent.ResponseType.Acknowledgement;
                  ackEvent._PacketGmt = PacketGmt;
                  ackEvent._EventGmt  = row.GetDateTime(msgResponseOrdinals.AckGmt);

                  if (!onlyPublished  ||  ackEvent.EventIsPublished) 
                  {
                     events.Add(ackEvent);
                  }
               }
            }

            // Check for a return receipt.

            if (!row.IsDBNull(msgResponseOrdinals.RtnReceiptPacketGmt))
            {
               PacketGmt = row.GetDateTime(msgResponseOrdinals.RtnReceiptPacketGmt);

               if (PacketGmt.CompareTo(startGMT) >= 0  &&  PacketGmt.CompareTo(endGMT) < 0) 
               {
                  // The return receipt occurred during our period.  Generate the event.

                  msgResponseEvent rrEvent = (msgResponseEvent) msg.Clone();

                  rrEvent.ResponseEventType = msgResponseEvent.ResponseType.ReturnReceipt;
                  rrEvent._PacketGmt = PacketGmt;
                  rrEvent._EventGmt  = row.GetDateTime(msgResponseOrdinals.RtnReceiptGmt);

                  if (!onlyPublished  ||  rrEvent.EventIsPublished) 
                  {
                     events.Add(rrEvent);
                  }
               }
            }

            // Check for a response from the user.

            if (!row.IsDBNull(msgResponseOrdinals.ResponsePacketGmt))
            {
               PacketGmt = row.GetDateTime(msgResponseOrdinals.ResponsePacketGmt);

               if (PacketGmt.CompareTo(startGMT) >= 0  &&  PacketGmt.CompareTo(endGMT) < 0) 
               {
                  // The response occurred during our period.  Generate the event.

                  msgResponseEvent respEvent = msg;  // No need to clone, there is no other msg type after this.
                  msg = null; // ...So if we do add another case, it will get a run-time error.

                  respEvent.ResponseEventType = msgResponseEvent.ResponseType.Response;
                  respEvent._PacketGmt = PacketGmt;
                  respEvent._EventGmt  = row.GetDateTime(msgResponseOrdinals.ResponseGmt);

                  if (!onlyPublished  ||  respEvent.EventIsPublished) 
                  {
                     events.Add(respEvent);
                  }
               }
            }
         }
      }

      private static void hydrateEventFromOEMQuery(ArrayList events, IDataReader rdr, DateTime startGMT, DateTime endGMT, bool onlyPublished) 
      {
         // See if the event columns have been initialized yet; do it if necessary.
         // When uninitialized, all indices are zero and no two columns can have the
         // same ordinal so that's how we tell.

         IDataRecord row = (IDataRecord) rdr;

         if (rdr.FieldCount == 0) 
         {
            // If there are no fields, we have no data.

            return;
         }

         if (oemOrdinals.AssetID == oemOrdinals.PacketGmt) 
         {
            QueryHelpers.FillInColumnIndices(oemOrdinals, row);
         }

         while (rdr.Read()) 
         {
            oemEvent msg = new oemEvent();

            // Ensure we have the correct datareader.

            if (Convert.ToInt32(row[oemOrdinals.EventOrigin]) != (int) Origin.OutgoingEventMessage) 
            {
               throw new ApplicationException("Wrong datareader for hydrateEventFromOEMQuery");
            }

            // Fill in all the available fields from the cursor.

            if (!row.IsDBNull(oemOrdinals.SequenceID))
            {
               msg._SequenceID       = Convert.ToInt64(row[oemOrdinals.SequenceID]);
            }

            if (!row.IsDBNull(oemOrdinals.AssetID))
            {
               msg._AssetID          = Convert.ToInt64(row[oemOrdinals.AssetID]);
            }
            if (!row.IsDBNull(oemOrdinals.AssetName))
            {
               msg._AssetName        = Convert.ToString(row[oemOrdinals.AssetName]);
            }
            if (!row.IsDBNull(oemOrdinals.DeviceID))
            {
               msg._DeviceID         = Convert.ToInt64(row[oemOrdinals.DeviceID]);
            }

            if (!row.IsDBNull(oemOrdinals.PacketGmt))
            {
               msg._PacketGmt        = row.GetDateTime(oemOrdinals.PacketGmt);
            }
            if (!row.IsDBNull(oemOrdinals.EventGmt))
            {
               msg._EventGmt         = row.GetDateTime(oemOrdinals.EventGmt);
            }

            if (!row.IsDBNull(oemOrdinals.EventID))
            {
               msg._EventID          = Convert.ToInt32(row[oemOrdinals.EventID]);
            }
      
            if (!row.IsDBNull(oemOrdinals.UserDataDesc))
            {
               msg.UserDataDescription   = Convert.ToString(row[oemOrdinals.UserDataDesc]);
            }
            if (!row.IsDBNull(oemOrdinals.DescTemplate))
            {
               msg.DescriptionTemplate   = Convert.ToString(row[oemOrdinals.DescTemplate]);
            }
            if (!row.IsDBNull(oemOrdinals.DataParms))
            {
               msg.DescriptionParameters = Convert.ToString(row[oemOrdinals.DataParms]);
            }

            if (!row.IsDBNull(oemOrdinals.LocationID))
            {
               msg.LocationID        = Convert.ToInt64(row[oemOrdinals.LocationID]);
            }
            if (!row.IsDBNull(oemOrdinals.LocationName))
            {
               msg.LocationName      = Convert.ToString(row[oemOrdinals.LocationName]);
            }

            if (!row.IsDBNull(oemOrdinals.Latitude))
            {
               msg._Latitude         = Convert.ToDouble(row[oemOrdinals.Latitude]);
            }
            if (!row.IsDBNull(oemOrdinals.Longitude))
            {
               msg._Longitude        = Convert.ToDouble(row[oemOrdinals.Longitude]);
            }

            if (!row.IsDBNull(oemOrdinals.Mileage))
            {
               msg._Mileage          = Convert.ToDouble(row[oemOrdinals.Mileage]);
            }
            if (!row.IsDBNull(oemOrdinals.Speed))
            {
               msg._Speed            = Convert.ToInt32(Convert.ToDouble(row[oemOrdinals.Speed]));
            }
            if (!row.IsDBNull(oemOrdinals.Heading))
            {
               msg._Heading          = Convert.ToDouble(row[oemOrdinals.Heading]);
            }

            if (!row.IsDBNull(oemOrdinals.DriverPresent))
            {
               msg.DriverPresent     = Convert.ToInt32(row[oemOrdinals.DriverPresent]) != 0;
            }
            if (!row.IsDBNull(oemOrdinals.DriverID))
            {
               msg.DriverID          = Convert.ToInt64(row[oemOrdinals.DriverID]);
            }
            if (!row.IsDBNull(oemOrdinals.MdtDriverID))
            {
               msg.MdtDriverID       = Convert.ToString(row[oemOrdinals.MdtDriverID]);
            }
            if (!row.IsDBNull(oemOrdinals.DriverDisplayName))
            {
               msg.DriverDisplayName = Convert.ToString(row[oemOrdinals.DriverDisplayName]);
            }
            if (!row.IsDBNull(oemOrdinals.SiteStatusSource))
            {
               msg.SiteStatusSource = Convert.ToString(row[oemOrdinals.SiteStatusSource]);
            }
            if (!row.IsDBNull(oemOrdinals.OwnerOrgID))
            {
               msg._OwnerOrgID      = Convert.ToInt64(row[oemOrdinals.OwnerOrgID]);
            }

            if (!row.IsDBNull(oemOrdinals.Street))
            {
               msg._Street = Convert.ToString(row[oemOrdinals.Street]);
            }
            if (!row.IsDBNull(oemOrdinals.City))
            {
               msg._City = Convert.ToString(row[oemOrdinals.City]);
            }
            if (!row.IsDBNull(oemOrdinals.State))
            {
               msg._State = Convert.ToString(row[oemOrdinals.State]);
            }
            if (!row.IsDBNull(oemOrdinals.Zip))
            {
               msg._Zip = Convert.ToString(row[oemOrdinals.Zip]);
            }
            if (!row.IsDBNull(oemOrdinals.Country))
            {
               msg._Country = Convert.ToString(row[oemOrdinals.Country]);
            }


            if (!row.IsDBNull(oemOrdinals.UserDataBin)) 
            {
               // Get the user_data_bin column as a byte array and hydrate the nested user-data packet.

               long   userDataBinLength = row.GetBytes(oemOrdinals.UserDataBin, 0, null, 0, 0);

               if (userDataBinLength > 0) 
               {
                  // Only hydrate the tracker UserDataMessage if there is user-data to build it from.  Otherwise,
                  // this is one of those other non-user-data messages that produces events.

                  byte[] userDataBin = new byte[userDataBinLength];

                  row.GetBytes(oemOrdinals.UserDataBin, 0, userDataBin, 0, userDataBin.Length);

                  try 
                  {
                     msg.UserDataMessage = PlatformMessage.HydrateTrackerUserDataMessageFromUserDataBinary(userDataBin);
                  } 
                  catch 
                  {
                     // In this case, the message was malformed.

                     msg.UserDataMessage = null;
                  }

                  if (msg.UserDataMessage != null) 
                  {
                     // Sometimes, the event ID doesn't match!

                     msg._EventID = (int) msg.UserDataMessage.EventID;
                  }
               }
            }

            // Special case for the discrete inputs.  Supply the correct default "{1} {2}"
            // (useful when looking at a platform that doesn't have that change yet).

            if (msg.EventID >= UDEventID.Input1On  &&
                msg.EventID <= UDEventID.Input3Off) 
            {
               msg.DescriptionTemplate = "{1} {2}";
            }

            // After all that, it might be an event that isn't published.  Find out.

            if (!onlyPublished  ||  msg.EventIsPublished) 
            {
               events.Add(msg);
            }
         }
      }

      private static void hydrateEventFromStateQuery(ArrayList events, IDataReader rdr, DateTime startGMT, DateTime endGMT, bool onlyPublished) 
      {
         // See if the position columns have been initialized yet; do it if necessary.
         // When uninitialized, all indices are zero and no two columns can have the
         // same ordinal so that's how we tell.

         IDataRecord row = (IDataRecord) rdr;

         if (rdr.FieldCount == 0) 
         {
            // If there are no fields, we have no data.

            return;
         }

         if (stateOrdinals.AssetID == stateOrdinals.PacketGmt) 
         {
            QueryHelpers.FillInColumnIndices(stateOrdinals, row);
         }

         while (rdr.Read()) 
         {
            stateEvent msg = new stateEvent();

            // Ensure we have the correct datareader.

            if (Convert.ToInt32(row[stateOrdinals.EventOrigin]) != (int) Origin.OutgoingStateMessage) 
            {
               throw new ApplicationException("Wrong datareader for hydrateEventFromStateQuery");
            }

            // Fill in all the available fields from the cursor.

            if (!row.IsDBNull(stateOrdinals.SequenceID))
            {
               msg._SequenceID       = Convert.ToInt64(row[stateOrdinals.SequenceID]);
            }

            if (!row.IsDBNull(stateOrdinals.AssetID))
            {
               msg._AssetID          = Convert.ToInt64(row[stateOrdinals.AssetID]);
            }
            if (!row.IsDBNull(stateOrdinals.AssetName))
            {
               msg._AssetName        = Convert.ToString(row[stateOrdinals.AssetName]);
            }
            if (!row.IsDBNull(stateOrdinals.DeviceID))
            {
               msg._DeviceID         = Convert.ToInt64(row[stateOrdinals.DeviceID]);
            }

            if (!row.IsDBNull(stateOrdinals.PacketGmt))
            {
               msg._PacketGmt        = row.GetDateTime(stateOrdinals.PacketGmt);
            }
            if (!row.IsDBNull(stateOrdinals.EventGmt))
            {
               msg._EventGmt         = row.GetDateTime(stateOrdinals.EventGmt);
            }


            if (!row.IsDBNull(stateOrdinals.Latitude))
            {
               msg._Latitude         = Convert.ToDouble(row[stateOrdinals.Latitude]);
            }
            if (!row.IsDBNull(stateOrdinals.Longitude))
            {
               msg._Longitude        = Convert.ToDouble(row[stateOrdinals.Longitude]);
            }

            if (!row.IsDBNull(stateOrdinals.Speed))
            {
               //msg._Speed            = Convert.ToDouble(row[stateOrdinals.Speed]);
              
               //The Least Significant Bit (LSB) on speed from the CrossCheck is 0.5m/sec.  
               //This causes the output in mph when converted to be in increments of ~1.118mph 
               //So, the value of the speed would be converted to an Int32 value to remove the precision.
               //In future the value that DBWriter saves into the database for Speed would be rounded               
               msg._Speed              = Convert.ToInt32(Convert.ToDouble(row[stateOrdinals.Speed]));
            } 
            if (!row.IsDBNull(stateOrdinals.Heading))
            {
               msg._Heading          = Convert.ToInt32(row[stateOrdinals.Heading]);
            }
            if (!row.IsDBNull(stateOrdinals.Mileage))
            {
               msg._Mileage          = Convert.ToDouble(row[stateOrdinals.Mileage]);
            }
            if (!row.IsDBNull(stateOrdinals.PostedSpeedLimit))
            {
               msg._PostedSpeedLimit = Convert.ToInt32(row[stateOrdinals.PostedSpeedLimit]);
            }
            if (!row.IsDBNull(stateOrdinals.IsSpeeding))
            {
               msg._IsSpeeding = Convert.ToInt32(row[stateOrdinals.IsSpeeding]) != 0;
            }

            if (!row.IsDBNull(stateOrdinals.OwnerOrgID))
            {
               msg._OwnerOrgID       = Convert.ToInt64(row[stateOrdinals.OwnerOrgID]);
            }

            // After all that, it might be an event that isn't published.  Find out.

            if (!onlyPublished  ||  msg.EventIsPublished) 
            {
               events.Add(msg);
            }
         }
      }

      private static void hydrateEventFromCoherencyMsgQuery(ArrayList events, IDataReader rdr, DateTime startGMT, DateTime endGMT, bool onlyPublished) 
      {
         // See if the position columns have been initialized yet; do it if necessary.
         // When uninitialized, all indices are zero and no two columns can have the
         // same ordinal so that's how we tell.

         IDataRecord row = (IDataRecord) rdr;

         if (rdr.FieldCount == 0) 
         {
            // If there are no fields, we have no data.

            return;
         }

         if (coherencyMsgOrdinals.ChangeCategory == coherencyMsgOrdinals.ChangeType) 
         {
            QueryHelpers.FillInColumnIndices(coherencyMsgOrdinals, row);
         }

         while (rdr.Read()) 
         {
            coherencyMsgEvent msg = new coherencyMsgEvent();

            // Ensure we have the correct datareader.
            
            int eventOrigin = Convert.ToInt32(row[coherencyMsgOrdinals.EventOrigin]);

            if (eventOrigin < (int) Origin.CoherencyEventAssetMessage ||
                eventOrigin > (int) Origin.CoherencyEventDriver)
            {
               throw new ApplicationException("Wrong datareader for hydrateEventFromCoherencyMsgQuery");
            }

            // Fill in all the available fields from the cursor.

            if (!row.IsDBNull(coherencyMsgOrdinals.ChangeGMT))
            {
               // EventGMT & PacketGMT are the same for coherency events.

               msg._EventGmt   = row.GetDateTime(coherencyMsgOrdinals.ChangeGMT);
            }

            if (!row.IsDBNull(coherencyMsgOrdinals.ChangeType))
            {
               // 'ChangeType' from the database is our 'ChangeAction'
               msg.ChangeAction = (coherencyMsgEvent.CoherencyAction) Convert.ToInt32(row[coherencyMsgOrdinals.ChangeType]);
            }

            if (!row.IsDBNull(coherencyMsgOrdinals.ChangeCategory))
            {
               // 'ChangeType' from the database is our 'ChangeAction'
               msg.ChangeCategory = (coherencyMsgEvent.CoherencyCategory) Convert.ToInt32(row[coherencyMsgOrdinals.ChangeCategory]);
            }

            if (!row.IsDBNull(coherencyMsgOrdinals.ID))
            {
               msg._ID   = Convert.ToInt64(row[coherencyMsgOrdinals.ID]);
            }

            string[] dataParts = null;

            if (!row.IsDBNull(coherencyMsgOrdinals.RealtimeData))
            {
               dataParts = Convert.ToString(row[coherencyMsgOrdinals.RealtimeData]).Split('|');
            }

            // Unpacking the dataParts depends on the coherency type. This is to avoid having 15+ columns in CEM that aren't used 90% of the time.
            // Although I'm not a fan of opaque data...

            int i;

            switch (msg.ChangeCategory)
            {
               case coherencyMsgEvent.CoherencyCategory.Asset:
                  // Model:
                  // assetName | OrgID | OrgName

                  i = 0;

                  msg._AssetName = dataParts[i++];
                  msg._OwnerOrgID = Convert.ToInt64(dataParts[i++]);
                  msg.OwnerOrgName = dataParts[i++];
                  break;

               case coherencyMsgEvent.CoherencyCategory.Driver:
                  // Model:
                  // driverDispName | driverName | EmployeeID | MDT_Driver_ID_Text | OrgID | OrgName

                  i = 0;

                  msg.StreetOrString1 = dataParts[i++]; // Name
                  msg._AssetName = dataParts[i++]; // ShortName
                  msg.StreetOrString2 = dataParts[i++]; // Employee ID
                  msg.CityOrString3 = dataParts[i++]; // MDT ID
                  msg._OwnerOrgID = Convert.ToInt64(dataParts[i++]);
                  msg.OwnerOrgName = dataParts[i++];
                  break;

               case coherencyMsgEvent.CoherencyCategory.MonitoringUser:
                  // Model:
                  // ID1,OrgName1,ID2,OrgName2...

                  msg.AssetGroupPairs = new AssetGroupOwnerPair[dataParts.Length/2];

                  i = 0;
                  while (i < dataParts.Length)
                  {
                     AssetGroupOwnerPair pair = new AssetGroupOwnerPair();

                     msg.AssetGroupPairs[i/2] = pair;

                     pair.AssetGroupID = Convert.ToInt64(dataParts[i++]);
                     pair.AssetGroupName = dataParts[i++];
                  }
                  break;

               case coherencyMsgEvent.CoherencyCategory.WorkSite:
                  // Model:
                  // Name | Type_Desc | Map_Color | Timeout | Latitude_S | Longitude_W | Latitude_N | Longitude_E | Street1 | Street2 | City | State_CD | Zipcode

                  i = 0;

                  msg.WorksiteName = dataParts[i++];
                  msg._OwnerOrgID = Convert.ToInt64(dataParts[i++]);
                  try
                  {
                     msg.WorksiteType = (PlatformMessage.DeviceSiteType) Enum.Parse(typeof(PlatformMessage.DeviceSiteType), dataParts[i++], true);
                  }
                  catch
                  {
                     msg.WorksiteType = PlatformMessage.DeviceSiteType.Invalid;
                  }

                  msg.MapColor            = Convert.ToInt32(dataParts[i++]);
                  msg.Timeout             = Convert.ToInt32(dataParts[i++]);

                  msg.LatitudeSouth       = Convert.ToDouble(dataParts[i++]);
                  msg.LongitudeWest       = Convert.ToDouble(dataParts[i++]);
                  msg.LatitudeNorth       = Convert.ToDouble(dataParts[i++]);
                  msg.LongitudeEast       = Convert.ToDouble(dataParts[i++]);


                  msg.StreetOrString1     = dataParts[i++];
                  msg.StreetOrString2     = dataParts[i++];
                  msg.CityOrString3       = dataParts[i++];
                  msg.StateOrString4      = dataParts[i++];
                  msg.PostalCodeOrString5 = dataParts[i++];
                  msg.CountryOrString6 = dataParts[i++];
                  
                  break;

               default:
                  continue;  // Skip this item
            }


            if (!onlyPublished  ||  msg.EventIsPublished) 
            {
               events.Add(msg);
            }
         }
      }
#endif
   }
}
