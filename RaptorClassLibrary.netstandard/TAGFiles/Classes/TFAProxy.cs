using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using log4net;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.TAGFiles.Models;
using VSS.Velociraptor.DesignProfiling.Servers.Client;
using VSS.TRex;

namespace VSS.TRex.TAGFiles.Classes
{
    public class TFAProxy : ITFAProxy
    {

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


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
        public ValidationResult ValidateTagfile(Guid submittedProjectId, Guid tccOrgId, string radioSerial, int radioType, double lat, double lon, DateTime timeOfPosition, out Guid projectId, out Guid assetId)
        {

            ValidationResult result = ValidationResult.Unknown;

            // dont waste the services time if you dont have any details
            //if (tccOrgId == string.Empty && radioType == string.Empty)
             //   return ValidationResult.BadRequest;
            Log.Info($"#Info# Details passed to TFA servce. ProjectID:{projectId}, AssetId:{assetId}, TCCOrgId:{tccOrgId}, radioSerial:{radioSerial}, radioType:{radioType}, lat:{lat}, lon:{lon}, DateTime:{timeOfPosition}");

            // Todo This code can be refactored to suit needs
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

            // Update port # in the following line.
            string URL = RaptorConfig.TFAServiceURL + RaptorConfig.TFAServiceGetProjectID;
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
                    result = ValidationResult.Valid;
                else
                {
                    // Todo assigned correct values
                    result = ValidationResult.Invalid;
                }
                    
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("-----------------");
                Console.Out.WriteLine(e.Message);
                Log.Error($"#Exception# Unexpected exception occured calling TFA service ProjectId:{projectId}, TCCOrgId:{tccOrgId}, radioSerial:{radioSerial}, radioType:{radioType}, lat:{lat}, lon:{lon}, DateTime:{timeOfPosition} {e.Message}");
                return result;
            }

            return result;
        }

    }
}
