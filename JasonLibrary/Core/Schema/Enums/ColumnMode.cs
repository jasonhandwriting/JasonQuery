using System;

namespace JasonLibrary.Core.Schema.Enums
{
    [Flags]
    public enum ColumnMode
    {
        None = 0,
        ShowColumnType = 1 << 0,
        ShowColumnComment = 1 << 1,
        Grid = 1 << 10,
        Export = 1 << 11
    }
}