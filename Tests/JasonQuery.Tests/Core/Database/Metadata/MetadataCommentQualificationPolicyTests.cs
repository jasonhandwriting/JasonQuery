using JasonQuery.Core.Database.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.Core.Database.Metadata
{
    [TestClass]
    public sealed class MetadataCommentQualificationPolicyTests
    {
        private static DataTable Metadata(object comment, object hasExisting)
        {
            var data = new DataTable();

            data.Columns.Add("MetadataComment", typeof(string));
            data.Columns.Add("HasExistingComment", typeof(object));
            data.Rows.Add(comment, hasExisting);

            return data;
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G01_G02_ExactlyOneCommentedObject_Qualifies()
        {
            var data = Metadata("Existing comment", 1);

            Assert.IsTrue(MetadataCommentQualificationPolicy.TryQualify(data, out var result));
            Assert.AreEqual("Existing comment", result.Comment);
            Assert.IsTrue(result.HasExistingComment);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G05_ExistingColumnWithoutExtendedProperty_QualifiesAsAdd()
        {
            var data = Metadata(DBNull.Value, 0);

            Assert.IsTrue(MetadataCommentQualificationPolicy.TryQualify(data, out var result));
            Assert.AreEqual(string.Empty, result.Comment);
            Assert.IsFalse(result.HasExistingComment);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G06_ExistingExtendedPropertyWithNullValue_UsesUpdate()
        {
            var data = Metadata(DBNull.Value, 1);

            Assert.IsTrue(MetadataCommentQualificationPolicy.TryQualify(data, out var result));
            Assert.AreEqual(string.Empty, result.Comment);
            Assert.IsTrue(result.HasExistingComment);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G11_G19_ExistingColumnWithEmptyComment_IsNotMissingObject()
        {
            var data = Metadata(string.Empty, 0);

            Assert.IsTrue(MetadataCommentQualificationPolicy.TryQualify(data, out var result));
            Assert.AreEqual(string.Empty, result.Comment);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G12_DuplicateRows_FailsClosed()
        {
            var data = Metadata("first", 1);

            data.Rows.Add("other", 1);
            Assert.IsFalse(MetadataCommentQualificationPolicy.TryQualify(data, out var result));
            Assert.IsNull(result);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G13_ZeroRows_FailsClosed()
        {
            var data = Metadata("x", 1);

            data.Rows.Clear();
            Assert.IsFalse(MetadataCommentQualificationPolicy.TryQualify(data, out var result));
            Assert.IsNull(result);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void G14_NullReaderOutput_FailsClosed()
        {
            Assert.IsFalse(MetadataCommentQualificationPolicy.TryQualify(null, out var result));
            Assert.IsNull(result);
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void MissingResultShape_FailsClosed()
        {
            var data = new DataTable();

            data.Columns.Add("MetadataComment");
            data.Rows.Add("comment");
            Assert.IsFalse(MetadataCommentQualificationPolicy.TryQualify(data, out _));
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        [DataRow("unknown")]
        [DataRow("2")]
        [DataRow("")]
        public void InvalidPropertyFlag_FailsClosed(string value)
        {
            var data = Metadata("comment", value);

            Assert.IsFalse(MetadataCommentQualificationPolicy.TryQualify(data, out _));
        }

        [TestMethod]
        [TestCategory("Golden")]
        [TestCategory("D1B1")]
        public void BooleanPropertyFlag_IsAccepted()
        {
            var data = Metadata("comment", true);

            Assert.IsTrue(MetadataCommentQualificationPolicy.TryQualify(data, out var result));
            Assert.IsTrue(result.HasExistingComment);
        }
    }
}
