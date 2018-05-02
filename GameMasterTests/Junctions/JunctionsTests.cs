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
        { // Return to the original working directory and remove our test directory.
            Directory.SetCurrentDirectory(_cwd);
            Directory.Delete(_testdir, true);        
        }

        [TestMethod()]
        public void CreateJunctionTest()
        {
            Directory.CreateDirectory( Path.Combine( _testdir, "target" ));
            try
            {
                Junctions.CreateJunction("foobar", Path.Combine(_testdir, "target" ));
            } catch (Win32Exception)
            {
                Assert.Fail();
            }
            Directory.Delete("foobar");
            Directory.Delete("target");
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