using System;
using System.Collections.Generic;

namespace CDCNPM.Models
{
    public class SqlTable
    {
        public List<string> PKs { get; set; }
        public List<string> FKs { get; set; }
        public string table_name { get; set; }
        public List<SqlColumn> columns { get; set; }
       
    }
}
