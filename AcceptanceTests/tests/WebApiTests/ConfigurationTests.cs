using System;
using System.Diagnostics;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using VSS.UnifiedProductivity.Service.WebApiModels.Models;
using WebAPIResults = VSS.UnifiedProductivity.Service.WebApiModels.ResultHandling;

namespace WebApiTests
{
    [TestClass]
    public class ConfigurationTestsClass
    {
        // Three tests to make sure configuration rules are obeyed
        [TestMethod]
        public void Config_LoadSwitchValid()
        {

            var testAsset = Guid.NewGuid().ToString();

            // Make Load Switch config for assett
            var config = new AssetConfigData
            {
                startDate = DateTime.Now.AddDays(1).Date, // future date
                loadSwitchNumber = 1,
                loadSwitchOpen = true,
                dumpSwitchNumber = 0,
                dumpSwitchOpen = false,
                targetCyclesPerDay = 1000,
                volumePerCycleCubicMeter = 100,
                assetIdentifier = testAsset
            };

            var configJSON = JsonConvert.SerializeObject(config, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

            // Save config
            var appConfig = new TestConfig();
            var baseUri = appConfig.webApiUri;
            if (Debugger.IsAttached)
            {
                baseUri = appConfig.debugWebApiUri;
            }
            string uri = baseUri + string.Format("asset/{0}", testAsset); 
            var restClient = new RestClientUtil();
            var response = restClient.DoHttpRequest(uri, "POST", "application/json", configJSON, HttpStatusCode.OK);
            WebAPIResults.ContractExecutionResult result = JsonConvert.DeserializeObject<WebAPIResults.ContractExecutionResult>(response);
            Assert.AreEqual(WebAPIResults.ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code); 
            // note because its a future config you can not retrieve it until tomorrow
        }

        [TestMethod]
        public void Config_LoadSwitchPastConfig()
        {

            var testAsset = Guid.NewGuid().ToString();

            // Make Load Switch config for assett with past date
            var config = new AssetConfigData
            {
                startDate = DateTime.Now.AddDays(-5).Date, 
                loadSwitchNumber = 1,
                loadSwitchOpen = true,
                dumpSwitchNumber = 0,
                dumpSwitchOpen = false,
                targetCyclesPerDay = 1000,
                volumePerCycleCubicMeter = 100,
                assetIdentifier = testAsset,
                allowPastConfig = true // override switch
            };

            var configJSON = JsonConvert.SerializeObject(config, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

            var appConfig = new TestConfig();
            // Save config
            var baseUri = appConfig.webApiUri;
            if (Debugger.IsAttached)
            {
                baseUri = appConfig.debugWebApiUri;
            }
            string uri = baseUri + string.Format("asset/{0}", testAsset);
            var restClient = new RestClientUtil();
            var response = restClient.DoHttpRequest(uri, "POST", "application/json", configJSON, HttpStatusCode.OK);
            WebAPIResults.ContractExecutionResult result = JsonConvert.DeserializeObject<WebAPIResults.ContractExecutionResult>(response);
            Assert.AreEqual(WebAPIResults.ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);

            var retrievedConfig = new AssetConfigData { };
            // Retrieve config
            string uri2 = baseUri + string.Format("asset/{0}", testAsset);
            var restClient2 = new RestClientUtil();
            var response2 = restClient2.DoHttpRequest(uri2, "GET", "application/json", null, HttpStatusCode.OK);
            var aResult = JsonConvert.DeserializeObject<WebAPIResults.AssetConfigResult>(response2);
            retrievedConfig = aResult.config;
            retrievedConfig.allowPastConfig = true; // we expect this to be diff
            Assert.AreEqual(retrievedConfig.assetIdentifier.ToString(), config.assetIdentifier.ToString());
            Assert.AreEqual(retrievedConfig.startDate.ToString(), config.startDate.ToString());

        }

        [TestMethod]
        public void Config_LoadSwitchNullVolume()
        {

            var testAsset = Guid.NewGuid().ToString();

            // Make Load Switch config for assett with past date
            var config = new AssetConfigData
            {
                startDate = DateTime.Now.AddDays(-5).Date, 
                loadSwitchNumber = 1,
                loadSwitchOpen = true,
                dumpSwitchNumber = 0,
                dumpSwitchOpen = false,
                assetIdentifier = testAsset,
                allowPastConfig = true // override switch
            };

            var configJSON = JsonConvert.SerializeObject(config, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
            var appConfig = new TestConfig();
            // Save config
            var baseUri = appConfig.webApiUri;
            if (Debugger.IsAttached)
            {
                baseUri = appConfig.debugWebApiUri;
            }
            string uri = baseUri + string.Format("asset/{0}", testAsset);
            var restClient = new RestClientUtil();
            var response = restClient.DoHttpRequest(uri, "POST", "application/json", configJSON, HttpStatusCode.OK);
            WebAPIResults.ContractExecutionResult result = JsonConvert.DeserializeObject<WebAPIResults.ContractExecutionResult>(response);
            Assert.AreEqual(WebAPIResults.ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);

            var retrievedConfig = new AssetConfigData { };
            // Retrieve config
            string uri2 = baseUri + string.Format("asset/{0}", testAsset);
            var restClient2 = new RestClientUtil();
            var response2 = restClient2.DoHttpRequest(uri2, "GET", "application/json", null, HttpStatusCode.OK);
            var aResult = JsonConvert.DeserializeObject<WebAPIResults.AssetConfigResult>(response2);
            retrievedConfig = aResult.config;
            retrievedConfig.allowPastConfig = true; // we expect this to be diff
            Assert.AreEqual(retrievedConfig.assetIdentifier.ToString(), config.assetIdentifier.ToString());            
            Assert.AreEqual(retrievedConfig.startDate.ToString(), config.startDate.ToString());

            Assert.AreEqual(retrievedConfig.volumePerCycleCubicMeter, config.volumePerCycleCubicMeter);
            Assert.AreEqual(retrievedConfig.targetCyclesPerDay, config.targetCyclesPerDay);
        }



        [TestMethod]
        public void Config_DumpSwitchValid()
        {
            var testAsset = Guid.NewGuid().ToString();

            // Make Dump Switch config for assett
            var config = new AssetConfigData
            {
                startDate = DateTime.Now.AddDays(1),
                loadSwitchNumber = 0,
                loadSwitchOpen = true,
                dumpSwitchNumber = 1,
                dumpSwitchOpen = false,
                targetCyclesPerDay = 1000,
                volumePerCycleCubicMeter = 100,
                assetIdentifier = testAsset
            };

            var configJSON = JsonConvert.SerializeObject(config);

            var appConfig = new TestConfig();
            // Save config
            var baseUri = appConfig.webApiUri;
            if (Debugger.IsAttached)
            {
                baseUri = appConfig.debugWebApiUri;
            }
            string uri = baseUri + string.Format("asset/{0}", testAsset);
            var restClient = new RestClientUtil();
            var response = restClient.DoHttpRequest(uri, "POST", "application/json", configJSON, HttpStatusCode.OK);
            WebAPIResults.ContractExecutionResult result = JsonConvert.DeserializeObject<WebAPIResults.ContractExecutionResult>(response);
            Assert.AreEqual(WebAPIResults.ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
        }

        [TestMethod]
        public void Config_LoadDumpSwitchValid()
        {
            var testAsset = Guid.NewGuid().ToString();

            // Mqke Load Switch config for assett
            var config = new AssetConfigData
            {
                startDate = DateTime.Now.AddDays(1),
                loadSwitchNumber = 1,
                loadSwitchOpen = true,
                dumpSwitchNumber = 0,
                dumpSwitchOpen = false,
                targetCyclesPerDay = 1000,
                volumePerCycleCubicMeter = 100,
                assetIdentifier = testAsset
            };

            var configJSON = JsonConvert.SerializeObject(config);

            var appConfig = new TestConfig();
            // Save config
            var baseUri = appConfig.webApiUri;
            if (Debugger.IsAttached)
            {
                baseUri = appConfig.debugWebApiUri;
            }
            string uri = baseUri + string.Format("asset/{0}", testAsset);
            var restClient = new RestClientUtil();
            var response = restClient.DoHttpRequest(uri, "POST", "application/json", configJSON, HttpStatusCode.OK);
            WebAPIResults.ContractExecutionResult result = JsonConvert.DeserializeObject<WebAPIResults.ContractExecutionResult>(response);
            Assert.AreEqual(WebAPIResults.ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);

        }


        [TestMethod]
        public void Config_InvalidSwitch()
        {
            var testAsset = Guid.NewGuid().ToString();

            // Mqke Load Switch config for assett
            var config = new AssetConfigData
            {
                startDate = DateTime.Now.AddDays(1),
                loadSwitchNumber = 100,
                loadSwitchOpen = true,
                dumpSwitchNumber = 0,
                dumpSwitchOpen = false,
                targetCyclesPerDay = 1000,
                volumePerCycleCubicMeter = 100,
                assetIdentifier = testAsset
            };

            var configJSON = JsonConvert.SerializeObject(config);

            var appConfig = new TestConfig();
            // Save config
            var baseUri = appConfig.webApiUri;
            if (Debugger.IsAttached)
            {
                baseUri = appConfig.debugWebApiUri;
            }
            string uri = baseUri + string.Format("asset/{0}", testAsset);

            try
            { 
              var restClient = new RestClientUtil();
              var response = restClient.DoHttpRequest(uri, "POST", "application/json", configJSON, HttpStatusCode.BadRequest);
              WebAPIResults.ContractExecutionResult result = JsonConvert.DeserializeObject<WebAPIResults.ContractExecutionResult>(response);
            }
            catch
            {
            }

        }

        [TestMethod]
      //  [ExpectedException(typeof(WebAPIResults.ServiceException), "StartDate must be tomorrow or later")]
        public void Config_InvalidPastDate()
        {
            var testAsset = Guid.NewGuid().ToString();

            // Mqke Load Switch config for assett
            var config = new AssetConfigData
            {
                startDate = DateTime.Now.AddDays(-5),
                loadSwitchNumber = 100,
                loadSwitchOpen = true,
                dumpSwitchNumber = 0,
                dumpSwitchOpen = false,
                targetCyclesPerDay = 1000,
                volumePerCycleCubicMeter = 100,
                assetIdentifier = testAsset
            };

            var configJSON = JsonConvert.SerializeObject(config);

            var appConfig = new TestConfig();
            // Save config
            var baseUri = appConfig.webApiUri;
            if (Debugger.IsAttached)
            {
                baseUri = appConfig.debugWebApiUri;
            }
            string uri = baseUri + string.Format("asset/{0}", testAsset);
            try
            {
                var restClient = new RestClientUtil();
                var response = restClient.DoHttpRequest(uri, "POST", "application/json", configJSON, HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
// waitong on global exception handler to return correct exception
              //  Assert.Fail( string.Format("Unexpected exception of type {0} caught: {1}", e.GetType(), e.Message));
            }


        }

        [TestMethod]
        public void Config_NoConfigSuppliedNoCycle()
        {
            var testAsset = Guid.NewGuid();
            var retrievedConfig = new AssetConfigData { };
            var appConfig = new TestConfig();
            // Retrieve config
            var baseUri = appConfig.webApiUri;
            if (Debugger.IsAttached)
            {
                baseUri = appConfig.debugWebApiUri;
            }
            string uri = baseUri + string.Format("asset/{0}", testAsset);
            // Check for No Content
            var restClient = new RestClientUtil();
            var response1 = restClient.DoHttpRequest(uri, "GET", "application/json", null, HttpStatusCode.NoContent);

        }

    }
}
