using JasonLibrary.Infrastructure.Enums;

namespace JasonLibrary.Providers.MySql.Enums
{
    public enum MySqlColumnTypeName
    {
        Unknown = 0,
        TinyInt,
        [EnumAlias("tinyint unsigned")]
        TinyIntUnsigned,
        SmallInt,
        [EnumAlias("smallint unsigned")]
        SmallIntUnsigned,
        MediumInt,
        [EnumAlias("mediumint unsigned")]
        MediumIntUnsigned,
        Int,
        [EnumAlias("int unsigned")]
        IntUnsigned,
        BigInt,
        [EnumAlias("bigint unsigned")]
        BigIntUnsigned,
        Decimal,
        [EnumAlias("decimal unsigned")]
        DecimalUnsigned,
        Float,
        [EnumAlias("float unsigned")]
        FloatUnsigned,
        Double,
        [EnumAlias("double unsigned")]
        DoubleUnsigned,
        Bit,
        Char,
        VarChar,
        TinyText,
        Text,
        MediumText,
        LongText,
        Binary,
        VarBinary,
        TinyBlob,
        Blob,
        MediumBlob,
        LongBlob,
        Enum,
        Set,
        Date,
        DateTime,
        Timestamp,
        Time,
        Year,
        Json,
        Geometry,
        Point,
        LineString,
        Polygon,
        MultiPolygon,
        GeometryCollection,
        MultiPoint,
        MultiLineString
    }
}
