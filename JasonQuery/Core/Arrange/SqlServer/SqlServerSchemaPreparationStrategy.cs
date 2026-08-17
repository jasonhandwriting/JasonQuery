using JasonQuery.Core.Data.DataRows;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Arrange.SqlServer
{
    public class SqlServerSchemaPreparationStrategy : CommonSchemaPreparationStrategy
    {
        public override void Prepare(ArrangeContext context)
        {
            context.DistinctTableNameViewName = string.Join(";", context.SchemaTable?.AsEnumerable()
                                                .Select(dr => $"{dr.GetSafeString("BaseSchemaName")},{dr.GetSafeString("BaseTableName")}")
                                                .Where(name => !string.IsNullOrWhiteSpace(name) && !name.EndsWith(","))
                                                .Distinct() ?? Enumerable.Empty<string>());

            context.TableNameForUpdateComment = context.SchemaTable?.AsEnumerable()
                   .Select
                    (
                        r => (
                                 Schema: r.GetSafeString("BaseSchemaName"),
                                 Table: r.GetSafeString("BaseTableName")
                             )
                    )
                   .Where(t => !string.IsNullOrWhiteSpace(t.Schema) && !string.IsNullOrWhiteSpace(t.Table))
                   .Distinct()
                   .ToList();

            base.Prepare(context);
        }
    }
}