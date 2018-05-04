using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GameMaster.Junctions.Tests
{
    [TestClass()]
    public class JunctionsTests
    {
        private string _cwd;
        private string _testdir;

        [TestInitialize()]
        public void Initialize()
        { // Testing will happen under C:\Users\<username>\AppData\Local\Temp or equivalent
            _cwd = Directory.GetCurrentDirectory();           
            _testdir = Path.Combine( Path.GetTempPath(), Path.GetRandomFileName() );
            Directory.CreateDirectory( _testdir );
            Directory.SetCurrentDirectory( _testdir );
        }

        [TestCleanup()]
        public void Cleanup()
        {   // Return to the original working directory and remove our test directory.
            // Note that recursive deletes dont play well with junctions if the target was removed first!
            Directory.SetCurrentDirectory(_cwd);
            Directory.Delete(_testdir, true);        
        }

        [TestMethod()]
        public void CreateJunctionTest()
        {
            Directory.CreateDirectory( Path.Combine( _testdir, "target" ));
            Junctions.CreateJunction("foobar", Path.Combine(_testdir, "target" ));
            Directory.Delete("foobar");
            Directory.Delete("target");
        }

        [TestMethod()]
        public void CreateJunctionRelativePathTest()
        {
            Directory.CreateDirectory("target");
            Junctions.CreateJunction("foobar",  "target");
            Directory.Delete("foobar");
            Directory.Delete("target");
        }

        [TestMethod()]
        public void CreateJunctionMissingLinkTargetTest()
        {
            Assert.ThrowsException<CreationFailedException>( 
                () => Junctions.CreateJunction("foobar", "target") 
            );
        }

        [TestMethod()]
        public void GetJunctionTargetTest()
        {
            Directory.CreateDirectory("target");
            Junctions.CreateJunction("foo", "target");
            Junctions.GetJunctionTarget("foo", out string target);
            Assert.AreEqual(Path.Combine(Directory.GetCurrentDirectory(), "target"), target);
            Directory.Delete("foo");
            Directory.Delete("target");
        }

        [TestMethod()]
        public void GetJunctionInvalidLinkTest()
        {
            Assert.ThrowsException<DereferenceFailedException> (
                () => Junctions.GetJunctionTarget("foo", out string target)
            );
        }

        [TestMethod()]
        public void DeleteJunctionTest()
        {
            Directory.CreateDirectory("target");
            Junctions.CreateJunction("foo", "target");
            Junctions.DeleteJunction("foo");
            Directory.Delete("target");
            Assert.ThrowsException<System.IO.DirectoryNotFoundException>(
                () => Directory.Delete("foo") // Already deleted
            );
        }

        [TestMethod()]
        public void DeleteJunctionDirectoryTargetTest()
        {
            Directory.CreateDirectory("test_dir");
            Assert.ThrowsException<DeletionFailedException>(
                () => Junctions.DeleteJunction("test_dir")
            );
            Directory.Delete("test_dir"); // still here - didn't get nuked.
        }
    }
}