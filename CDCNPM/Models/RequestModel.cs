using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CDCNPM.Models
{
    public class RequestModel
    {
        public List<QueryObject> objectList { get; set; }

        public List<SqlTable> tableList { get; set; }

    }
}
