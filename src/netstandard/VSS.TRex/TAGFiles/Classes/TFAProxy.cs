using System;
using System.IO;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.TAGFiles.Classes.Validator;

namespace VSS.TRex.TAGFiles.Classes
{
    public class TFAProxy : ITFAProxy
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

      /// <summary>
      ///  Default no-arg constructor
      /// </summary>
      public TFAProxy()
      {

      }

        /// <summary>
        /// Calls Tagfile Auth Service to lookup project details and check assett is licensed
        /// </summary>
        /// <param name="tccOrgId"></param>
        /// <param name="radioSerial"></param>
        /// <param name="radioType"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="timeOfPosition"></param>
        /// <param name="projectId"></param>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public ValidationResult ValidateTagfile(Guid? submittedProjectId, Guid tccOrgId, string radioSerial, int radioType, double lat, double lon, DateTime timeOfPosition, ref Guid? projectId, out Guid? assetId)
        {
            ValidationResult result = ValidationResult.Unknown;

            assetId = null;
            // dont waste the services time if you dont have any details
            //if (tccOrgId == string.Empty && radioType == string.Empty)
            //   return ValidationResult.BadRequest;
              Log.LogInformation($"Details passed to TFA servce. ProjectID:{projectId}, TCCOrgId:{tccOrgId}, radioSerial:{radioSerial}, radioType:{radioType}, lat:{lat}, lon:{lon}, DateTime:{timeOfPosition}");

            if (radioSerial == String.Empty && tccOrgId == Guid.Empty && submittedProjectId == Guid.Empty)
            {
                Log.LogWarning($"Must have either a valid TCCOrgID or RadioSerialNo or ProjectID");
                return ValidationResult.BadRequest;
            }


            TFARequest req = new TFARequest()
                             {
                                radioSerial = radioSerial,
                                deviceType = radioType,
                                latitude = lat,
                                longitude = lon,
                                timeOfPosition = timeOfPosition,
                                tccOrgUid = tccOrgId,
                                projectUid = submittedProjectId
                             };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(req);

            // This part will need to be refactored if you want to mock it
            string URL = TRexConfig.TFAServiceURL + TRexConfig.TFAServiceGetProjectID;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = json.Length;
            StreamWriter requestWriter = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
            requestWriter.Write(json);
            requestWriter.Close();

            try
            {
                WebResponse webResponse = request.GetResponse();
                Stream webStream = webResponse.GetResponseStream();
                StreamReader responseReader = new StreamReader(webStream);
                string response = responseReader.ReadToEnd();
                Console.Out.WriteLine(response);
                var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<TFAReponse>(response);
                responseReader.Close();
                if (responseObj.ResultCode == 0)
                {
                    result = ValidationResult.Valid;
                    // if not overriding take TFA projectid
                    if ((projectId == null) && (Guid.Parse(responseObj.projectUid) != Guid.Empty))
                    {
                        projectId = Guid.Parse(responseObj.projectUid);
                    }
                    // take what TFA gives us including a empty guid
                    assetId = (Guid.Parse(responseObj.assetUid));
                }
                else
                {
                    // Todo assigned correct values from new service once written
                    result = ValidationResult.Invalid;
                }
                    
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("-----------------");
                Console.Out.WriteLine(e.Message);
                Log.LogError($"#Exception# Unexpected exception occured calling TFA service ProjectId:{projectId}, TCCOrgId:{tccOrgId}, radioSerial:{radioSerial}, radioType:{radioType}, lat:{lat}, lon:{lon}, DateTime:{timeOfPosition} {e.Message}");
                //result = ValidationResult.Unknown;
                return result;
            }

            return result;
        }

    }
}
