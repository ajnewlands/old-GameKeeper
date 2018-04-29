using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameMaster.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace GameMaster.Libraries.Tests
{
    [TestClass()]
    public class SteamLibraryTests
    {
        [TestMethod()]
        public void GetHomePathTest()
        {   // Demonstrate that we can instantiate a steam library with mock dependency.
            var loc = new Mock<ILibraryLocator>();
            loc.Setup(l => l.GetLibraryPath()).Returns("G:\\");

            ILibrary lib = new SteamLibrary(loc.Object);
            Assert.AreEqual(lib.GetHomePath(), "G:\\");
        }

        [TestMethod()]
        public void GetContentDirectoriesTest()
        {
            var loc = new Mock<ILibraryLocator>();
            loc.Setup(l => l.GetLibraryPath()).Returns("G:\\Bulk Storage\\Program Files (x86)\\Steam\\SteamApps\\common");

            ILibrary lib = new SteamLibrary(loc.Object);
            Assert.AreEqual(lib.GetGameDirectories()[0], "alien swarm");
        }

        [TestMethod()]
        public void GetReparsePointsTest()
        {
            var loc = new Mock<ILibraryLocator>();
            loc.Setup(l => l.GetLibraryPath()).Returns("G:\\Bulk Storage\\Program Files (x86)\\Steam\\SteamApps\\common");

            ILibrary lib = new SteamLibrary(loc.Object);
            Assert.AreEqual(lib.GetReparsePoints()[0], "Prey");
        }
    }
}