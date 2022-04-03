using System;
using CDCNPM.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace CDCNPM.Repositories
{
    public interface ISqlTableRepository
    {
        public List<SqlTable> getAllTables(IConfiguration configuration);
        public List<string> getListFKsByTable(IConfiguration configuration,string table_name);
        public List<string> getListPKsByTable(IConfiguration configuration, string table_name);
        public List<SqlColumn> getColsInTable(IConfiguration configuration, string table_name);
        public DataSet getDataRawQuery(IConfiguration config, string queryString);


    }
}
