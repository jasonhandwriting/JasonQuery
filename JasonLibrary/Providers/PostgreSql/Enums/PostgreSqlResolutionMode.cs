using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JasonLibrary.Providers.PostgreSql.Enums
{
    public class PostgreSqlResolutionMode
    {
        public enum ResolutionMode
        {
            Direct, //Catalog 1：直接 Mapping，例如 16 -> boolean)
            WithColumnSize, //Catalog 2：帶入 Column 長度，例如 1056 -> bit(n); 如果 n=0, 則最終結果為 bit
            WithPrecisionSize, //Catalog 2：帶入長度，例如 1186 -> interval(n); 如果 n=0, 則最終結果為 interval
            WithSize, //Catalog 2：帶入長度，例如 1700 -> numeric(10,2)
            Catalog //Catalog 1：Need to search from Catalog, ex. 23, 1043
        }
    }
}