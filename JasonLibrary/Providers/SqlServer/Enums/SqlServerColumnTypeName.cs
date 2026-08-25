using JasonLibrary.Infrastructure.Enums;

namespace JasonLibrary.Providers.SqlServer.Enums
{
    public enum SqlServerColumnTypeName
    {
        Unknown = 0,
        BigInt,
        Binary,
        Bit,
        Char,
        Date,
        DateTime,
        DateTime2,
        DateTimeOffset,
        Decimal,
        Float,
        Geography,
        Geometry,
        HierarchyId,
        Image,
        Int,
        Integer,
        Json,
        Money,
        NChar,
        NText,
        Numeric,
        NVarchar,
        Real,
        RowVersion, //8字節的二進制欄位型態，通常用於行版本控制，與日期時間無關
        SmallDateTime,
        SmallInt,
        SmallMoney,
        [EnumAlias("sql_variant")]
        SqlVariant,
        Text,
        Time,
        Timestamp, //8字節的二進制欄位型態，通常用於行版本控制，與日期時間無關
        TinyInt,
        UniqueIdentifier,
        VarBinary,
        VarChar,
        Vector,
        Xml
    }
}
