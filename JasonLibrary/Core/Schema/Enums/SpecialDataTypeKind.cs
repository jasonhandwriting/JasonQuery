namespace JasonLibrary.Core.Schema.Enums
{
    public enum SpecialDataTypeKind
    {
        String,
        Array,
        DateTime,
        DateTimeOffset, //SQL Server, 有時區，格式為YYYY-MM-DD HH:mm:ss[.nnnnnnn]
        Number,
        Bit,
        Boolean,
        BooleanArray,
        BoxArray,
        Char,
        CharArray,
        CharacterArray,
        Date,
        Enum,
        Time, //SQL Server: HH:mm:ss[.nnnnnnn]
        TimeWithTimeZone,
        TimeWithoutTimeZone,
        Timestamp, //Oracle；SQLServer：Binary 格式；MySQL: 年月日時分秒毫秒，毫秒最多 6位數
        RowVersion, //SQL Server, 8字節的二進制欄位型態，通常用於行版本控制，與日期時間無關
        TimestampWithLocalTimeZone,
        TimestampWithTimeZone,
        TimestampWithoutTimeZone,
        Bytea, //PostgreSQL, 大型欄位二進制型態
        IntervalDayToSecond,
        IntervalYearToMonth,
        Blob, //Oracle, 大型欄位二進制型態
        Clob, //Oracle, 大型欄位文字型態
        NClob, //Oracle, 大型欄位文字型態
        NString,
        Raw, //Oracle, 大型欄位二進制型態
        Long, //Oracle, 大型欄位文字型態
        LongRaw, //Oracle, 大型欄位二進制型態
        Oid, //PostgreSQL, 大型欄位二進制型態
        ByteaArray, //大型欄位二進制型態
        Binary, //Binary 格式
        VarBinary, //Binary 格式
        Image, //SQL Server, Binary 格式
        TinyBlob,
        MediumBlob,
        LongBlob
    }
}
