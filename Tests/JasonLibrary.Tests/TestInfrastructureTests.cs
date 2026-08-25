using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests
{
    [TestClass]
    public sealed class TestInfrastructureTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Smoke")]
        public void Mstest_ShouldDiscoverAndExecuteTests()
        {
            const bool expected = true;

            var actual = true;

            TestContext.WriteLine("MSTest discovery and execution are working.");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Environment")]
        public void TestHost_ShouldRunAs64Bit()
        {
            TestContext.WriteLine($"Is64BitProcess: {Environment.Is64BitProcess}");

            Assert.IsTrue
            (
                Environment.Is64BitProcess,
                "The testhost is running as x86. Check JasonQuery.Tests.runsettings."
            );
        }

        [TestMethod]
        [TestCategory("Environment")]
        public void TestHost_ShouldRunOnDotNetFramework()
        {
            TestContext.WriteLine($"CLR Version: {Environment.Version}");

            Assert.AreEqual
            (
                4,
                Environment.Version.Major,
                "The testhost is not running on the expected .NET Framework CLR."
            );
        }
    }
}
