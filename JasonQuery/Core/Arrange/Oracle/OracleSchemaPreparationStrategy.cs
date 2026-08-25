using JasonQuery.Core.Data.DataRows;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Arrange.Oracle
{
    public class OracleSchemaPreparationStrategy : CommonSchemaPreparationStrategy
    {
        public override void Prepare(ArrangeContext context)
        {
            context.DistinctTableNameViewName = string.Join(",", context.SchemaTable?.AsEnumerable()
                                                .Select(dr => dr.GetSafeString("BaseTableName"))
                                                .Where(name => !string.IsNullOrWhiteSpace(name))
                                                .Distinct());

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
