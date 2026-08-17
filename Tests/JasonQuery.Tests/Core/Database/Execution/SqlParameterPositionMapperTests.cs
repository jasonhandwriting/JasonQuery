using JasonQuery.Core.Database.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.Execution
{
    [TestClass]
    public sealed class SqlParameterPositionMapperTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_WithEmptyMapping_ReturnsFalseAndPreservesPosition()
        {
            var success = SqlParameterPositionMapper.TryMapExecutedSqlPositionToOriginalSqlPosition
            (
                string.Empty,
                0,
                25,
                out int originalPosition,
                out string parameterName
            );

            Assert.IsFalse(success);
            Assert.AreEqual(25, originalPosition);
            Assert.AreEqual(string.Empty, parameterName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_WithMalformedItem_ReturnsFalse()
        {
            var success =SqlParameterPositionMapper.TryMapExecutedSqlPositionToOriginalSqlPosition
            (
                ":id|10",
                0,
                25,
                out int originalPosition,
                out string parameterName
            );

            Assert.IsFalse(success);
            Assert.AreEqual(25, originalPosition);
            Assert.AreEqual(string.Empty, parameterName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_WithInvalidRelativePosition_ReturnsFalse()
        {
            var success = SqlParameterPositionMapper.TryMapExecutedSqlPositionToOriginalSqlPosition
            (
                ":id|invalid|123",
                0,
                25,
                out int originalPosition,
                out string parameterName
            );

            Assert.IsFalse(success);
            Assert.AreEqual(25, originalPosition);
            Assert.AreEqual(string.Empty, parameterName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_BeforeFirstParameter_PreservesPosition()
        {
            AssertMappedPosition(":id|10|12345", 0, 5, 5, string.Empty);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_AtParameterStart_MapsToParameterName()
        {
            AssertMappedPosition(":id|10|12345", 0, 10, 10, ":id");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_InsideParameterValue_MapsToParameterName()
        {
            AssertMappedPosition(":id|10|12345", 0, 13, 10, ":id");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_AtParameterValueEnd_MapsAfterOriginalParameterName()
        {
            AssertMappedPosition(":id|10|12345", 0, 15, 13, string.Empty);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_AfterLongerReplacement_SubtractsLengthDifference()
        {
            AssertMappedPosition(":id|10|12345", 0, 20, 18, string.Empty);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_AfterShorterReplacement_AddsLengthDifference()
        {
            AssertMappedPosition(":long|10|1", 0, 11, 15, string.Empty);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_BetweenTwoParameters_AdjustsOnlyFirstParameter()
        {
            AssertMappedPosition(":a|10|111`:bb|20|XYZW", 0, 15, 14, string.Empty);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_InsideSecondParameter_MapsToSecondParameterName()
        {
            AssertMappedPosition(":a|10|111`:bb|20|XYZW", 0, 22, 20, ":bb");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_AfterTwoParameters_AdjustsCumulativeOffset()
        {
            AssertMappedPosition(":a|10|111`:bb|20|XYZW", 0, 30, 28, string.Empty);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_WithOutOfOrderMapping_SortsByOriginalPosition()
        {
            AssertMappedPosition(":bb|20|XYZW`:a|10|111", 0, 22, 20, ":bb");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_WithParameterStartPosition_AddsBaseOffset()
        {
            AssertMappedPosition(":p|5|VALUE", 100, 107, 105, ":p");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_WithPipeInsideValue_PreservesWholeValue()
        {
            AssertMappedPosition(":p|10|a|b", 0, 12, 10, ":p");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryMapExecutedSqlPositionToOriginalSqlPosition_WithInvalidAndValidItems_UsesValidItem()
        {
            AssertMappedPosition("invalid`:id|10|12345", 0, 12, 10, ":id");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FindNearestTextPosition_WithEmptyText_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, SqlParameterPositionMapper.FindNearestTextPosition(string.Empty, "abc", 0));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FindNearestTextPosition_WithEmptyValue_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, SqlParameterPositionMapper.FindNearestTextPosition("abc", string.Empty, 0));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FindNearestTextPosition_IsCaseInsensitive()
        {
            Assert.AreEqual(7, SqlParameterPositionMapper.FindNearestTextPosition("prefix TARGET suffix", "target", 0));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FindNearestTextPosition_WithRepeatedValue_ReturnsNearestOccurrence()
        {
            const string text = "target middle target end";

            Assert.AreEqual
            (
                text.LastIndexOf("target"),
                SqlParameterPositionMapper.FindNearestTextPosition
                (
                    text,
                    "target",
                    text.Length
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FindNearestTextPosition_WithEqualDistance_ReturnsFirstOccurrence()
        {
            const string text = "abc---abc";

            Assert.AreEqual
            (
                0,
                SqlParameterPositionMapper.FindNearestTextPosition
                (
                    text,
                    "abc",
                    3
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ResolveSqlServerExecutedPositionForParameterMapping_WhenTargetExists_PrefersNearestTarget()
        {
            const string sql = "select missing, other, missing";

            var actual = SqlParameterPositionMapper.ResolveSqlServerExecutedPositionForParameterMapping
            (
                sql,
                "missing",
                "3",
                sql.Length
            );

            Assert.AreEqual(sql.LastIndexOf("missing"), actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ResolveSqlServerExecutedPositionForParameterMapping_WhenTargetMissing_UsesParsedPosition()
        {
            var actual = SqlParameterPositionMapper.ResolveSqlServerExecutedPositionForParameterMapping
            (
                "select 1",
                "missing",
                "12",
                5
            );

            Assert.AreEqual(12, actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ResolveSqlServerExecutedPositionForParameterMapping_WhenPositionInvalid_UsesFallback()
        {
            var actual = SqlParameterPositionMapper.ResolveSqlServerExecutedPositionForParameterMapping
            (
                "select 1",
                "missing",
                "invalid",
                5
            );

            Assert.AreEqual(5, actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ResolveSqlServerExecutedPositionForParameterMapping_WithEmptyTarget_UsesParsedPosition()
        {
            var actual = SqlParameterPositionMapper.ResolveSqlServerExecutedPositionForParameterMapping
            (
                "select 1",
                string.Empty,
                "7",
                5
            );

            Assert.AreEqual(7, actual);
        }

        private static void AssertMappedPosition(string mapping, int parameterStartPosition, int executedPosition,
                                                 int expectedOriginalPosition, string expectedParameterName)
        {
            var success = SqlParameterPositionMapper.TryMapExecutedSqlPositionToOriginalSqlPosition
            (
                mapping,
                parameterStartPosition,
                executedPosition,
                out int originalPosition,
                out string parameterName
            );

            Assert.IsTrue(success);
            Assert.AreEqual(expectedOriginalPosition, originalPosition);
            Assert.AreEqual(expectedParameterName, parameterName);
        }
    }
}