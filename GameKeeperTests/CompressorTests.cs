using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameKeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GameKeeper.Tests
{
    [TestClass()]
    public class CompressorTests
    {
        private string _cwd;
        private string _testdir;

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
        public void CompressTest()
        {
            Directory.CreateDirectory("test");
            Compressor.Compress("test");
            Compressor.GetCompressionState("Test", out bool compressed);
            Assert.AreEqual(true, compressed);
        }

        [TestMethod()]
        public void DecompressTest()
        {
            Directory.CreateDirectory("test");
            Compressor.Compress("test");
            Compressor.GetCompressionState("Test", out bool compressed);
            Assert.AreEqual(true, compressed);
            Compressor.Decompress("test");
            Compressor.GetCompressionState("Test", out compressed);
            Assert.AreEqual(false, compressed);
        }
    }
}