using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class UpdateWorkerLocalizationTests
    {
        [TestMethod]
        [TestCategory("Update")]
        public void SourceFiles_ContainLocalizedWorkerProgressMessages()
        {
            var expectedByFile = new Dictionary<string, Dictionary<string, string>>
            {
                ["english.xml"] = new Dictionary<string, string>
                {
                    ["WorkerPreparingUpdate"] = "Preparing the update. Please wait.",
                    ["WorkerCreatingBackup"] = "Creating and verifying a backup of the previous version.",
                    ["WorkerApplyingUpdate"] = "Installing and verifying the update files.",
                    ["WorkerRestoringBackup"] = "The update failed. Restoring and verifying the previous version.",
                    ["WorkerFinalizingUpdate"] = "Finalizing the update and organizing old backups.",
                    ["WorkerDoNotClose"] = "Do not close this window or start JasonQuery until the update is complete."
                },
                ["chinese-cht.xml"] = new Dictionary<string, string>
                {
                    ["WorkerPreparingUpdate"] = "正在準備更新，請稍候。",
                    ["WorkerCreatingBackup"] = "正在建立並驗證前一版本備份。",
                    ["WorkerApplyingUpdate"] = "正在安裝並驗證更新檔案。",
                    ["WorkerRestoringBackup"] = "更新失敗，正在還原並驗證前一版本。",
                    ["WorkerFinalizingUpdate"] = "正在完成更新並整理舊備份。",
                    ["WorkerDoNotClose"] = "更新完成前，請勿關閉此視窗或啟動 JasonQuery。"
                },
                ["chinese-chs.xml"] = new Dictionary<string, string>
                {
                    ["WorkerPreparingUpdate"] = "正在准备更新，请稍候。",
                    ["WorkerCreatingBackup"] = "正在创建并验证前一版本备份。",
                    ["WorkerApplyingUpdate"] = "正在安装并验证更新文件。",
                    ["WorkerRestoringBackup"] = "更新失败，正在恢复并验证前一版本。",
                    ["WorkerFinalizingUpdate"] = "正在完成更新并整理旧备份。",
                    ["WorkerDoNotClose"] = "更新完成前，请勿关闭此窗口或启动 JasonQuery。"
                }
            };

            var localizationRoot = FindLocalizationRoot();

            foreach (var expectedFile in expectedByFile)
            {
                var document = new XmlDocument();

                document.Load(Path.Combine(localizationRoot, expectedFile.Key));

                foreach (var expectedEntry in expectedFile.Value)
                {
                    var nodes = document.SelectNodes
                    (
                        $"/JasonQuery/language[@category='form' and @class='UpdaterForm' and @type='msg' and @id='{expectedEntry.Key}' and @attribute='Text']"
                    );

                    Assert.HasCount(
                        1,
                        nodes,
                        $"{expectedFile.Key} must contain exactly one {expectedEntry.Key} entry."
                    );

                    Assert.AreEqual
                    (
                        expectedEntry.Value,
                        nodes[0].Attributes["name"]?.Value,
                        $"{expectedFile.Key} contains unexpected text for {expectedEntry.Key}."
                    );
                }
            }
        }

        private static string FindLocalizationRoot()
        {
            var startingDirectories = new[]
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Environment.CurrentDirectory
            };

            foreach (var startingDirectory in startingDirectories)
            {
                var current = new DirectoryInfo(startingDirectory);

                while (current != null)
                {
                    var candidate = Path.Combine(current.FullName, "JasonQuery", "localization");

                    if (File.Exists(Path.Combine(candidate, "english.xml")))
                    {
                        return candidate;
                    }

                    current = current.Parent;
                }
            }

            Assert.Fail("Could not locate JasonQuery\\localization from the test output directory.");
            return string.Empty;
        }
    }
}
