using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.FileAccess;
using VSS.Common.Abstractions.FileAccess.Enums;
using VSS.Common.Abstractions.FileAccess.Interfaces;

namespace VSS.Common.Abstractions.UnitTests
{
  [TestClass]
  public class DefaultVirtualFileSystemFactoryTests
  {
    private IServiceCollection serviceCollection;

    [TestInitialize]
    public void InitTest()
    {
      serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug().AddConsole();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection
        .AddTransient<IVirtualFileSystemFactory, DefaultVirtualFileSystemFactory>(); // This is the class we are testing

    }

    [TestMethod]
    public void TestCreate()
    {
      var factory = serviceCollection.BuildServiceProvider().GetService<IVirtualFileSystemFactory>();
      Assert.IsNotNull(factory);
      Assert.IsInstanceOfType(factory, typeof(DefaultVirtualFileSystemFactory));
    }

    [TestMethod]
    public void TestTags()
    {
      const FileSystemEntries expectedTag = FileSystemEntries.DesignFiles;
      var factory = serviceCollection.BuildServiceProvider().GetService<IVirtualFileSystemFactory>();
      Assert.IsNotNull(factory);

      var tags = factory.GetAvailableTags();
      Assert.IsTrue(!tags.Any(), "There should be not tags with no file systems registered");

      var mock = new Mock<IVirtualFileSystemRegistryEntry>();
      mock.Setup(m => m.Tag).Returns(expectedTag);

      var result = factory.RegisterFileSystem(mock.Object).Result;
      Assert.IsTrue(result);

      var tags2 = factory.GetAvailableTags().ToList();
      Assert.IsTrue(tags2.Count == 1, "Should have exactly one tag");
      Assert.AreEqual(expectedTag, tags2[0]);
    }

    [TestMethod]
    public void TestCreateFilesystem()
    {
      const FileSystemEntries expectedTag = FileSystemEntries.DesignFiles;
      var factory = serviceCollection.BuildServiceProvider().GetService<IVirtualFileSystemFactory>();
      Assert.IsNotNull(factory);

      // Create 2 file system entries, when we register an existing type it should replace the already registered entry
      var mockFilesystem1 = new Mock<IVirtualFileSystem>();
      var mockFilesystem2 = new Mock<IVirtualFileSystem>();

      var mockEntry1 = new Mock<IVirtualFileSystemRegistryEntry>();
      mockEntry1.Setup(m => m.Tag).Returns(expectedTag);
      mockEntry1.Setup(m => m.Create()).Returns(Task.FromResult(mockFilesystem1.Object));

      var mockEntry2 = new Mock<IVirtualFileSystemRegistryEntry>();
      mockEntry2.Setup(m => m.Tag).Returns(expectedTag);
      mockEntry2.Setup(m => m.Create()).Returns(Task.FromResult(mockFilesystem2.Object));

      // Do the test now
      factory.RegisterFileSystem(mockEntry1.Object);
      Assert.IsTrue(factory.GetAvailableTags().Count() == 1);

      var filesystem1 = factory.GetFileSystem(expectedTag).Result;
      Assert.AreSame(mockFilesystem1.Object, filesystem1);

      // Register the second file system now, ensure we only have one and it's the second one
      factory.RegisterFileSystem(mockEntry2.Object);
      Assert.IsTrue(factory.GetAvailableTags().Count() == 1);

      var filesystem2 = factory.GetFileSystem(expectedTag).Result;
      Assert.AreSame(mockFilesystem2.Object, filesystem2);

      // Just to confirm, make sure they're different
      Assert.AreNotSame(filesystem2, filesystem1);
      
    }

    [TestMethod]
    public void TestCreateDifferentFilesystem()
    {
      const FileSystemEntries expectedTag1 = FileSystemEntries.DesignFiles;
      const FileSystemEntries expectedTag2 = FileSystemEntries.DirectTagFile;

      Assert.AreNotEqual(expectedTag2, expectedTag1);

      var factory = serviceCollection.BuildServiceProvider().GetService<IVirtualFileSystemFactory>();
      Assert.IsNotNull(factory);

      var mockFilesystem1 = new Mock<IVirtualFileSystem>();
      var mockFilesystem2 = new Mock<IVirtualFileSystem>();

      var mockEntry1 = new Mock<IVirtualFileSystemRegistryEntry>();
      mockEntry1.Setup(m => m.Tag).Returns(expectedTag1);
      mockEntry1.Setup(m => m.Create()).Returns(Task.FromResult(mockFilesystem1.Object));

      var mockEntry2 = new Mock<IVirtualFileSystemRegistryEntry>();
      mockEntry2.Setup(m => m.Tag).Returns(expectedTag2);
      mockEntry2.Setup(m => m.Create()).Returns(Task.FromResult(mockFilesystem2.Object));

      // Register the entries
      factory.RegisterFileSystem(mockEntry1.Object);
      factory.RegisterFileSystem(mockEntry2.Object);

      Assert.IsTrue(factory.GetAvailableTags().Count() == 2);
      var entry1 = factory.GetFileSystem(expectedTag1).Result;
      var entry2 = factory.GetFileSystem(expectedTag2).Result;

      Assert.IsNotNull(entry1);
      Assert.IsNotNull(entry2);

      Assert.AreSame(mockFilesystem1.Object, entry1);
      Assert.AreSame(mockFilesystem2.Object, entry2);

      Assert.AreNotSame(entry2, entry1);


    }

    [TestMethod]
    public void TestCreateNoEntry()
    {
      var factory = serviceCollection.BuildServiceProvider().GetService<IVirtualFileSystemFactory>();
      Assert.IsNotNull(factory);

      var emptyResult = factory.GetFileSystem(FileSystemEntries.DesignFiles).Result;

      Assert.IsNull(emptyResult);

    }
  }
}
