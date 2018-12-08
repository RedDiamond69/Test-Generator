using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestGeneratorConsole.TestSourceFiles1;

namespace TestGeneratorConsole.TestSourceFiles1.Test
{
    [TestClass]
    public class NotSimpleClass1Test
    {
        private NotSimpleClass1 notSimpleClass1TestInstance; 

        [TestInitialize]
        public void TestInitialize()
        {
            int p1 = default(int);
            double p2 = default(double);
            notSimpleClass1TestInstance = new NotSimpleClass1(p1, p2);
        }

        [TestMethod]
        public void GetIntTest()
        {
            int actual = notSimpleClass1TestInstance.GetInt(); 

            int expected = default(int);
            Assert.AreEqual(expected, actual);
            Assert.Fail("autogenerated");
        }

        [TestMethod]
        public void DoSomethingTest()
        {
            int param1 = default(int);
            double param2 = default(double); 

            Assert.Fail("autogenerated");
        }
    }
}