using System;
using System.Collections.Generic;

namespace CDCNPM.Models
{
    public class QueryObject
    {
        public SqlTable sql_table { get; set; }
        public SqlColumn sql_column { get; set; }

        public string reName { get; set; }

        public string total { get; set; }

        public string condition { get; set; }

        public List<string> orConditionList { get; set; }

        public bool isShow { get; set; }

        public string isSort { get; set; }

        public bool isGroupBy { get; set; }

    }
}
