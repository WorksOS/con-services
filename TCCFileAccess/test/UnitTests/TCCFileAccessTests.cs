using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4netExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TCCFileAccess;
using VSS.GenericConfiguration;

namespace UnitTests
{
    [TestClass]
    public class TCCFileAccessTests
    {
        public IServiceProvider serviceProvider = null;

        [TestInitialize]
        public virtual void InitTest()
        {
            var serviceCollection = new ServiceCollection();

            string loggerRepoName = "UnitTestLogTest";
            var logPath = System.IO.Directory.GetCurrentDirectory();
            Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddDebug();
            loggerFactory.AddLog4Net(loggerRepoName);

            serviceCollection.AddLogging();
            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
            serviceCollection.AddSingleton<IConfigurationStore, VSS.GenericConfiguration.GenericConfiguration>();
            serviceCollection.AddSingleton<IFileRepository,FileRepository>();
            serviceProvider = serviceCollection.BuildServiceProvider();
        }



        [TestMethod]
        public void CanCreateFileAccessService()
        {
            var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
            Assert.IsNotNull(fileaccess);
        }

        [TestMethod]
        public async Task CanListOrgs()
        {
            var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
            var orgs = await fileaccess.ListOrganizations();
            Assert.IsNotNull(orgs);
        }

        [TestMethod]
        public async Task CanListFoldersFiles()
        {
            var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
            var orgs = await fileaccess.ListOrganizations();
            var folders = await fileaccess.GetFolders(orgs.First(), DateTime.MinValue,"/");
            Assert.IsTrue(folders.entries.Length>0);
        }

        [TestMethod]
        public async Task CanUplpoadFile()
        {
            var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
            var orgs = await fileaccess.ListOrganizations();
            var fileStream = File.Open("appsettings.json", FileMode.Open);
            var fileuploadresult = await fileaccess.PutFile(orgs.First(), "/barney", "unittest.json", fileStream, fileStream.Length);
            Assert.IsNotNull(fileuploadresult);
            Assert.AreEqual(fileuploadresult.success,"true");
        }

        [TestMethod]
        public async Task CanDownloadFile()
        {
            var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
            var orgs = await fileaccess.ListOrganizations();
            var fileStream = File.Open("appsettings.json", FileMode.Open);
            var fileuploadresult = await fileaccess.PutFile(orgs.First(), "/unittest.json", "unittest.json", fileStream, fileStream.Length);
            var downloadFileResult = await fileaccess.GetFile(orgs.First(), "/unittest.json");
            Assert.AreEqual(downloadFileResult.Length, fileStream.Length);
        }

    }
}
