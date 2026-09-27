using JasonQuery.Core.SystemInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.SystemInfo
{
    [TestClass]
    public class WindowsVersionInfoTests
    {
        [TestMethod]
        public void Create_Windows10_ReturnsShortAndFullNames()
        {
            var actual = WindowsVersionInfo.Create("Windows 10 Pro", "22H2", "2009", 10, 0, 19045, 4046, string.Empty);

            Assert.AreEqual("Win10", actual.ShortName);
            Assert.AreEqual("Windows 10 Pro 22H2 (Build 19045.4046)", actual.FullName);
        }

        [TestMethod]
        public void Create_Windows11Build_CorrectsLegacyWindows10ProductName()
        {
            var actual = WindowsVersionInfo.Create("Windows 10 Pro", "23H2", string.Empty, 10, 0, 22631, 3155, string.Empty);

            Assert.AreEqual("Win11", actual.ShortName);
            Assert.AreEqual("Windows 11 Pro 23H2 (Build 22631.3155)", actual.FullName);
        }

        [TestMethod]
        public void Create_WindowsServer2022_ReturnsUnambiguousShortName()
        {
            var actual = WindowsVersionInfo.Create("Windows Server 2022 Standard", "21H2", string.Empty, 10, 0, 20348, 2402, string.Empty);

            Assert.AreEqual("WinServer2022", actual.ShortName);
            Assert.AreEqual("Windows Server 2022 Standard 21H2 (Build 20348.2402)", actual.FullName);
        }

        [TestMethod]
        public void Create_WindowsServer2012R2_PreservesR2InShortName()
        {
            var actual = WindowsVersionInfo.Create("Windows Server 2012 R2 Standard", string.Empty, string.Empty, 6, 3, 9600, 0, string.Empty);

            Assert.AreEqual("WinServer2012R2", actual.ShortName);
        }

        [TestMethod]
        public void Create_FutureWindowsServerYear_UsesProductNameYear()
        {
            var actual = WindowsVersionInfo.Create("Windows Server 2028 Datacenter", string.Empty, string.Empty, 10, 0, 30000, 12, string.Empty);

            Assert.AreEqual("WinServer2028", actual.ShortName);
        }

        [TestMethod]
        public void Create_DisplayVersionMissing_UsesReleaseId()
        {
            var actual = WindowsVersionInfo.Create("Windows 10 Enterprise", string.Empty, "1909", 10, 0, 18363, 2274, string.Empty);

            Assert.AreEqual("Windows 10 Enterprise 1909 (Build 18363.2274)", actual.FullName);
        }

        [TestMethod]
        public void Create_NoReliableVersionData_UsesSafeFallbackWithoutGuessingWindows8()
        {
            var actual = WindowsVersionInfo.Create(string.Empty, string.Empty, string.Empty, 0, 0, 0, -1, "Microsoft Windows NT 6.2.9200.0");

            Assert.AreEqual("Windows", actual.ShortName);
            Assert.AreEqual("Microsoft Windows NT 6.2.9200.0", actual.FullName);
        }
    }
}
