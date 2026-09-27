using JasonQuery.Core.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace JasonQuery.Tests.Core.Logging
{
    [TestClass]
    [DoNotParallelize]
    public class TraceCsvFormatterTests
    {
        [TestMethod]
        public void EscapeText_QuotesCommaQuoteAndLineBreak()
        {
            var value = "alpha,\"beta\"\r\ngamma";
            var result = TraceCsvFormatter.EscapeText(value);

            Assert.AreEqual("\"alpha,\"\"beta\"\"\r\ngamma\"", result);
        }

        [TestMethod]
        public void EscapeText_NeutralizesSpreadsheetFormula()
        {
            Assert.AreEqual("\"'=1+1\"", TraceCsvFormatter.EscapeText("=1+1"));
            Assert.AreEqual("\"  '@SUM(A1:A2)\"", TraceCsvFormatter.EscapeText("  @SUM(A1:A2)"));
        }

        [TestMethod]
        public void Format_UsesInvariantCultureForNumericFields()
        {
            var originalCulture = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

                var entry = new TraceLogEntry
                {
                    Timestamp = new DateTimeOffset(2026, 8, 30, 10, 11, 12, 345, TimeSpan.FromHours(8)),
                    Sequence = 1,
                    Depth = 0,
                    DurationMs = 1234,
                    PrivateMemoryMb = 123.45,
                    MemoryDeltaMb = -1.25,
                    ThreadId = 7,
                    SourceLine = 42
                };

                var result = TraceCsvFormatter.Format(entry);

                Assert.Contains(",1234,123.45,-1.25,", result);
                Assert.StartsWith("\"2026-08-30T10:11:12.345+08:00\",1,", result);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [TestMethod]
        public void Header_HasSameColumnCountAsFormattedEntry()
        {
            var entry = new TraceLogEntry
            {
                Timestamp = DateTimeOffset.Now
            };

            var headerColumnCount = TraceCsvFormatter.Header.Split(',').Length;
            var dataColumnCount = TraceCsvFormatter.Format(entry).Split(',').Length;

            Assert.AreEqual(headerColumnCount, dataColumnCount);
            Assert.AreEqual(28, headerColumnCount);
        }
    }
}
