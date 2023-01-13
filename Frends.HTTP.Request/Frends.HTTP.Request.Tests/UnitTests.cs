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
        /// <summary>
        /// Setup MySQL to docker:
        /// docker run -p 3306:3306 -e MYSQL_ROOT_PASSWORD=my-secret-pw -d mysql
        /// </summary>

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
