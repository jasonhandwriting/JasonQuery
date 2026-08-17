using JasonQuery.Core.Database.Diagnostics.MySql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.Diagnostics.MySql
{
    [TestClass]
    public sealed class MySqlErrorMessageParserTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        [DataRow("Unknown column 'c1' in 'where clause'", (int)MySqlErrorMessageKind.UnknownColumn, "c1", "where clause", 0)]
        [DataRow("Unknown column 'a.c1' in 'field list'", (int)MySqlErrorMessageKind.UnknownColumn, "a.c1", "field list", 0)]
        [DataRow("Column 'c1' in where clause is ambiguous", (int)MySqlErrorMessageKind.AmbiguousColumn, "c1", "where clause", 0)]
        [DataRow("Duplicate column name 'last_update'", (int)MySqlErrorMessageKind.DuplicateColumn, "last_update", "", 0)]
        [DataRow("Table 'sakila.film_actor2' doesn't exist", (int)MySqlErrorMessageKind.TableNotFound, "sakila.film_actor2", "", 0)]
        [DataRow("Unknown database 'missing_db'", (int)MySqlErrorMessageKind.UnknownDatabase, "missing_db", "", 0)]
        [DataRow("No database selected", (int)MySqlErrorMessageKind.NoDatabaseSelected, "", "", 0)]
        [DataRow("You have an error in your SQL syntax at line 2", (int)MySqlErrorMessageKind.SyntaxError, "", "", 2)]
        public void Parse_KnownMessage_ReturnsExpectedInfo(string message, int kindValue, string target, string clause, int line)
        {
            var result = MySqlErrorMessageParser.Parse(message);

            Assert.AreEqual((MySqlErrorMessageKind)kindValue, result.Kind);
            Assert.AreEqual(target, result.TargetText);
            Assert.AreEqual(clause, result.ClauseName);
            Assert.AreEqual(line, result.LineNumber);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Parse_SyntaxMessageWithNearText_ReturnsNearText()
        {
            var result = MySqlErrorMessageParser.Parse
            (
                "You have an error in your SQL syntax; check the manual near '--, r.last_update' at line 1"
            );

            Assert.AreEqual(MySqlErrorMessageKind.SyntaxError, result.Kind);
            Assert.AreEqual(1, result.LineNumber);
            Assert.AreEqual("--, r.last_update", result.NearText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("Some other provider error")]
        public void Parse_UnsupportedMessage_ReturnsNone(string message)
        {
            Assert.AreEqual
            (
                MySqlErrorMessageKind.None,
                MySqlErrorMessageParser.Parse(message).Kind
            );
        }
    }
}