using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using GameKeeper;

namespace GameKeeper.Libraries.Tests
{
    [TestClass()]
    public class SteamLibraryTests
    {
        string _cwd;
        string _testdir;

        [TestInitialize()]
        public void Initialize()
        { // Testing will happen under C:\Users\<username>\AppData\Local\Temp or equivalent
            _cwd = Directory.GetCurrentDirectory();
            _testdir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_testdir);
            Directory.SetCurrentDirectory(_testdir);
        }

        [TestCleanup()]
        public void Cleanup()
        {   // Return to the original working directory and remove our test directory.
            // Note that recursive deletes dont play well with junctions if the target was removed first!
            Directory.SetCurrentDirectory(_cwd);
            Directory.Delete(_testdir, true);
        }

        [TestMethod()]
        public void GetHomePathTest()
        {   // Demonstrate that we can instantiate a steam library with mock dependency.
            var loc = new Mock<ILibraryLocator>();
            loc.Setup(l => l.GetLibraryPath()).Returns(_testdir);

            ILibrary lib = new GenericGameLibrary(loc.Object);
            Assert.AreEqual(lib.GetHomePath(), _testdir);
        }

        [TestMethod()]
        public void GetContentDirectoriesTest()
        {
            var loc = new Mock<ILibraryLocator>();
            loc.Setup(l => l.GetLibraryPath()).Returns(_testdir);
            Directory.CreateDirectory("ccc");
            Junctions.CreateJunction("aaa", "ccc");

            ILibrary lib = new GenericGameLibrary(loc.Object);
            Assert.AreEqual(1, lib.GetGameDirectories().Count);
            Assert.AreEqual("ccc", lib.GetGameDirectories()[0]);
            Junctions.DeleteJunction("aaa");
            Directory.Delete("ccc");
        }

        [TestMethod()]
        public void GetReparsePointsTest()
        {
            var loc = new Mock<ILibraryLocator>();
            loc.Setup(l => l.GetLibraryPath()).Returns(_testdir);
            Directory.CreateDirectory("ccc");
            Junctions.CreateJunction("aaa", "ccc");

            ILibrary lib = new GenericGameLibrary(loc.Object);
            Assert.AreEqual(1, lib.GetReparsePoints().Count);
            Assert.AreEqual("aaa", lib.GetReparsePoints()[0]);
            Junctions.DeleteJunction("aaa");
            Directory.Delete("ccc");
        }
    }
}