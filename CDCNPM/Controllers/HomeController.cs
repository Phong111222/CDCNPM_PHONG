using CDCNPM.Models;
using CDCNPM.Reports;
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

        [HttpPost]
        [Route("CreateQuery")]
        public JsonResult ResultQuery([FromBody] RequestModel request)
        {
            string queryResult = generateQuery(request).Replace("\n", "  ");
            Response.Cookies.Append(
                "query",
                queryResult,
                new CookieOptions()
                {
                    Expires = DateTime.Now.AddSeconds(60 * 5)
                });
            return Json(queryResult);
        }

        [Route("report")]
        public ViewResult Report()
        {
            DataSet dataSet = _sqlTableRepository.getDataRawQuery(_configuration, Request.Cookies["query"]);
            QueryReport report = new QueryReport();
            report.DataSource = dataSet;
            QueryReport.InitBands(report);
            QueryReport.InitDetailsBaseXRTable(report, dataSet, "Report");
            return View(report);
        }

        public static string generateQuery(RequestModel requestModel)
        {
            String queryString = "";
            queryString += "SELECT ";
            foreach (QueryObject item in requestModel.objectList)
            {
                if (item.isShow)
                {
                    if (string.IsNullOrEmpty(item.total) || string.IsNullOrWhiteSpace(item.total) || "None".Equals(item.total))
                    {
                        if (string.IsNullOrEmpty(item.reName) || string.IsNullOrWhiteSpace(item.reName))
                        {
                            queryString += String.Format("{0}.{1}, ", item.sql_column.table_name, item.sql_column.column_name);
                        }
                        else
                        {
                            queryString += String.Format("{0}.{1} as {2}, ", item.sql_column.table_name, item.sql_column.column_name, item.reName);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.reName) || string.IsNullOrWhiteSpace(item.reName))
                        {
                            queryString += String.Format("{0}({1}.{2}), ", item.total, item.sql_column.table_name, item.sql_column.column_name);
                        }
                        else
                        {
                            queryString += String.Format("{0}({1}.{2}) as {3}, ", item.total, item.sql_column.table_name, item.sql_column.column_name,
                                item.reName);
                        }
                    }
                }
            }
            queryString = queryString.Trim();
            queryString = queryString.Remove(queryString.Length - 1);
            queryString += "\n ";

            queryString += "FROM ";
            foreach (SqlTable table in requestModel.tableList)
            {
                queryString += String.Format("{0}, ", table.table_name);
            }
            queryString = queryString.Trim();
            queryString = queryString.Remove(queryString.Length - 1);
            queryString += "\n ";


            bool FKConstaintFlag = false;
            bool ConditionFlag = false;
            foreach (SqlTable table in requestModel.tableList)
            {
                if (table.FKs.Count > 0)
                {
                    foreach (string item in table.FKs)
                    {
                        string[] listData = item.Split("-");
                        if (requestModel.tableList.Find(ele => ele.table_name.Equals(listData[1])) != null)
                        {
                            FKConstaintFlag = true;
                            break;
                        }
                    }
                    if (FKConstaintFlag) break;
                }
            }
            foreach (QueryObject item in requestModel.objectList)
            {
                if (!(string.IsNullOrEmpty(item.condition) || string.IsNullOrWhiteSpace(item.condition)))
                {
                    ConditionFlag = true;
                    break;
                }
                if (item.orConditionList != null)
                {
                    if (item.orConditionList.Count > 0)
                    {
                        ConditionFlag = true;
                        break;
                    }
                }
            }
            if (FKConstaintFlag || ConditionFlag)
            {
                queryString += "WHERE ";
                foreach (SqlTable table in requestModel.tableList)
                {
                    if (table.FKs.Count > 0)
                    {
                        foreach (string item in table.FKs)
                        {
                            string[] listData = item.Split("-");
                            SqlTable checkConnect = requestModel.tableList.Find(ele => ele.table_name.Equals(listData[2]));
                            if (checkConnect != null)
                            {
                                queryString += String.Format("({0}.{1} = {2}.{3}) AND ",
                                listData[0], listData[1],
                                listData[2], listData[3]);
                            }
                        }
                    }
                }
                foreach (QueryObject item in requestModel.objectList)
                {
                    if (!(string.IsNullOrEmpty(item.condition) ||
                        string.IsNullOrWhiteSpace(item.condition)))
                    {
                        if (item.orConditionList != null && item.orConditionList.Count > 0)
                        {
                            queryString += String.Format("({0}.{1} = \'{2}\' OR ", item.sql_table.table_name, item.sql_column.column_name, item.condition);
                            foreach (string orCondition in item.orConditionList)
                            {
                                if (!string.IsNullOrWhiteSpace(orCondition))
                                {
                                    queryString += String.Format("{0}.{1} = \'{2}\' OR ", item.sql_table.table_name, item.sql_column.column_name, orCondition);
                                }
                            }
                            queryString = queryString.Trim();
                            queryString = queryString.Remove(queryString.Length - 3);
                            queryString += ") AND ";
                        }
                        else
                        {
                            queryString += String.Format("{0}.{1} = \'{2}\' AND ", item.sql_table.table_name, item.sql_column.column_name, item.condition);
                        }
                    }
                    else
                    {
                        if (item.orConditionList != null && item.orConditionList.Count > 0)
                        {
                            queryString += "(";
                            foreach (string orCondition in item.orConditionList)
                            {
                                if (!string.IsNullOrWhiteSpace(orCondition))
                                {
                                    queryString += String.Format("{0}.{1} = \'{2}\' OR ", item.sql_table.table_name, item.sql_column.column_name, orCondition);
                                }
                            }
                            queryString = queryString.Trim();
                            queryString = queryString.Remove(queryString.Length - 3);
                            queryString += ") AND ";
                        }
                    }
                }
                queryString = queryString.Trim();
                queryString = queryString.Remove(queryString.Length - 4);
                queryString += "\n ";
            }


            bool GroupByFlag = false;
            foreach (QueryObject item in requestModel.objectList)
            {
                if (item.isGroupBy)
                {
                    GroupByFlag = true;
                    break;
                }
            }
            if (GroupByFlag)
            {
                queryString += "GROUP BY ";
                foreach (QueryObject item in requestModel.objectList)
                {
                    if (item.isGroupBy)
                    {
                        queryString += String.Format("{0}.{1}, ", item.sql_table.table_name, item.sql_column.column_name);
                    }
                }
                queryString = queryString.Trim();
                queryString = queryString.Remove(queryString.Length - 1);
                queryString += "\n ";
            }

            bool OrderTypeFlag = false;
            foreach (QueryObject item in requestModel.objectList)
            {
                if (!(string.IsNullOrEmpty(item.isSort) || string.IsNullOrWhiteSpace(item.isSort) || "None".Equals(item.isSort)))
                {
                    OrderTypeFlag = true;
                    break;
                }
            }
            if (OrderTypeFlag)
            {
                queryString += "ORDER BY ";
                foreach (QueryObject item in requestModel.objectList)
                {
                    switch (item.isSort)
                    {
                        case "ASC":
                            queryString += String.Format("{0}.{1}, ", item.sql_table.table_name, item.sql_column.column_name);
                            break;
                        case "DESC":
                            queryString += String.Format("{0}.{1} DESC, ", item.sql_table.table_name, item.sql_column.column_name);
                            break;
                        default:
                            break;
                    }
                }
                queryString = queryString.Trim();
                queryString = queryString.Remove(queryString.Length - 1);
                queryString += "\n ";
            }
            return queryString;
        }
    }
}
