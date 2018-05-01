using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameMaster.Junctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace GameMaster.Junctions.Tests
{
    [TestClass()]
    public class JunctionsTests
    {
        [TestMethod()]
        public void CreateJunctionTest()
        {
            var r = Junctions.CreateJunction("C:\\Users\\ajn\\gamemaster_test\\source\\test", "C:\\Users\\ajn\\gamemaster_test\\dest\\test");
            Assert.AreEqual(new Win32Exception(r).Message, "The operation completed successfully");
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