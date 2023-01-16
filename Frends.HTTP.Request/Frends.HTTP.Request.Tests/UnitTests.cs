using System;
using System.Threading.Tasks;
using Frends.HTTP.Request.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.HTTP.Request.Tests;

public class UnitTests
{
    [TestClass]
    public class HTTPRequestTests
    {
        [TestInitialize]
        public async Task OneTimeSetUp()
        {

        }

        [TestCleanup]
        public async Task OneTimeTearDown()
        {

        }

        [TestMethod]
        public async Task TestCallStoredProcedure()
        {
            Assert.IsTrue(true);
        }
    }
}
