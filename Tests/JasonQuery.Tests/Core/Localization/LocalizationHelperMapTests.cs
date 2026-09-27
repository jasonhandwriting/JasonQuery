using JasonQuery.Core.Localization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace JasonQuery.Tests.Core.Localization
{
    [TestClass]
    public class LocalizationHelperMapTests
    {
        [TestMethod]
        public void InitializeLocalizationMap_ParsesSupportedLocalizations()
        {
            var originalList = LocalizationHelper.LocalizationList;
            var originalMap = LocalizationHelper.LocalizationMap;

            try
            {
                LocalizationHelper.LocalizationList = "English;english.xml`Chinese (Traditional) - 中文繁體;chinese-cht.xml`Chinese (Simplified) - 中文简体;chinese-chs.xml";

                LocalizationHelper.InitializeLocalizationMap();

                Assert.HasCount(3, LocalizationHelper.LocalizationMap);
                Assert.AreEqual("english.xml", LocalizationHelper.LocalizationMap["English"]);
                Assert.AreEqual("chinese-cht.xml", LocalizationHelper.LocalizationMap["Chinese (Traditional) - 中文繁體"]);
                Assert.AreEqual("chinese-chs.xml", LocalizationHelper.LocalizationMap["Chinese (Simplified) - 中文简体"]);
            }
            finally
            {
                LocalizationHelper.LocalizationList = originalList;
                LocalizationHelper.LocalizationMap = originalMap ?? new Dictionary<string, string>();
            }
        }

        [TestMethod]
        public void InitializeLocalizationMap_IgnoresMalformedItems()
        {
            var originalList = LocalizationHelper.LocalizationList;
            var originalMap = LocalizationHelper.LocalizationMap;

            try
            {
                LocalizationHelper.LocalizationList = "English;english.xml`Malformed` ;missing-name.xml`MissingFile; ";

                LocalizationHelper.InitializeLocalizationMap();

                Assert.HasCount(1, LocalizationHelper.LocalizationMap);
                Assert.AreEqual("english.xml", LocalizationHelper.LocalizationMap["English"]);
            }
            finally
            {
                LocalizationHelper.LocalizationList = originalList;
                LocalizationHelper.LocalizationMap = originalMap ?? new Dictionary<string, string>();
            }
        }
    }
}
