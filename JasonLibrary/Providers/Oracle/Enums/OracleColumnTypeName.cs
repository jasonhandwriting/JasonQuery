using JasonLibrary.Infrastructure.Enums;

namespace JasonLibrary.Providers.Oracle.Enums
{
    public enum OracleColumnTypeName
    {
        Unknown = 0,
        AnsiString,
        Anydata,
        BFileXmlType,
        [EnumAlias("binary_double")]
        BinaryDouble,
        [EnumAlias("binary_float")]
        BinaryFloat,
        Blob,
        Boolean,
        Char,
        Clob,
        Date,
        Float,
        Integer,
        [EnumAlias("interval day to second")]
        IntervalDayToSecond,
        [EnumAlias("interval year to month")]
        IntervalYearToMonth,
        Json,
        Long,
        [EnumAlias("long raw")]
        LongRaw,
        MlsLabel,
        NChar,
        NClob,
        Number,
        NVarchar2,
        Raw,
        Real,
        RowID,
        Sdo_Geometry,
        Timestamp,
        [EnumAlias("timestamp with local time zone")]
        TimestampWithLocalTimeZone,
        [EnumAlias("timestamp with time zone")]
        TimestampWithTimeZone,
        UriType,
        URowID,
        Varchar2,
        Vector,
        XmlType
    }
}
