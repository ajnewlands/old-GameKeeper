using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameMaster.Junctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.InteropServices;
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
            Assert.ThrowsException<CreationFailedException>( () => Junctions.CreateJunction("foobar", "target") );
        }

        [TestMethod()]
        public void getJunctionTargetTest()
        {
            string target;
            var r = Junctions.getJunctionTarget("C:\\Users\\ajn\\gamemaster_test\\source\\test", out target);

            Assert.AreEqual(r,0);
            Assert.AreEqual(target, "C:\\Users\\ajn\\gamemaster_test\\dest\\test");
        }

        [TestMethod()]
        public void deleteJunctionTest()
        {
            var r = Junctions.deleteJunction("C:\\Users\\ajn\\gamemaster_test\\source\\test");
            Assert.AreEqual(r, 0);
        }
    }
}