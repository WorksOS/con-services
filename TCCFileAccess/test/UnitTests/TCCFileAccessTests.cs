using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4netExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TCCFileAccess.Implementation;
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
            serviceCollection.AddTransient<IFileRepository,FileRepository>();
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
            var folders = await fileaccess.ListFolders(orgs[0], DateTime.MinValue);
            var files = await fileaccess.GetFiles(orgs[0].filespaceId, folders[0], DateTime.MinValue);
            Assert.IsNotNull(files);
        }

    }
}
