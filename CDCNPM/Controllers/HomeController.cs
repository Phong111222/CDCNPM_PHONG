using CDCNPM.Models;
using CDCNPM.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace CDCNPM.Controllers
{
    [Route("home")]
    public class HomeController:Controller
    {
        private ISqlTableRepository _sqlTableRepository;
        private readonly IConfiguration _configuration;

        public HomeController(ISqlTableRepository sqltableRepository,IConfiguration configuration)
        {
            this._sqlTableRepository = sqltableRepository;
            this._configuration = configuration;
        }
        [Route("~/")]
        [Route("homepage")]
        public ViewResult HomePage()
        {
            List<SqlTable> dataTables = new List<SqlTable>();


            /**
             * get list table name 
             **/
            dataTables = _sqlTableRepository.getAllTables(_configuration);

            /**
             * get list column of each table 
             * get list PKS of each table 
             * get list FKS of each table 
             **/

            foreach (SqlTable table in dataTables)
            {
                table.columns = _sqlTableRepository.getColsInTable(_configuration, table.table_name);
                table.FKs = _sqlTableRepository.getListFKsByTable(_configuration, table.table_name);
                table.PKs = _sqlTableRepository.getListPKsByTable(_configuration, table.table_name);
            }

            return View(dataTables);
        }
    }
}
