using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CDCNPM.Models;
using Microsoft.Extensions.Configuration;

namespace CDCNPM.Repositories
{
    public class SqlTableRepositoryImpl : ISqlTableRepository
    {
        private SqlConnection sqlConnection;

        public SqlTableRepositoryImpl()
        {

        }

        public SqlConnection GetSqlConnection(IConfiguration configuration)
        {
            string connection_string = configuration.GetConnectionString("MSSQLConnection");
            if (sqlConnection == null)
            {
                sqlConnection = new SqlConnection(connection_string);

            }
            return sqlConnection;

        }


        List<SqlTable> ISqlTableRepository.getAllTables(IConfiguration configuration)
        {
            List<SqlTable> tables = new List<SqlTable>();

            SqlConnection connection = GetSqlConnection(configuration);


            string query = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME NOT LIKE 'sysdiagrams';";

            connection.Open();

            SqlCommand sqlCommand = new SqlCommand(query, connection);

            SqlDataReader result = sqlCommand.ExecuteReader();
            if (result.HasRows)
            {
                while (result.Read())
                {
                    SqlTable table = new SqlTable()
                    {
                        table_name = result.GetString(2)
                    };
                    tables.Add(table);
                }
                result.Close();
            }
            else
            {
                result.Close();
            }
            connection.Close();
            return tables;
        }

        List<SqlColumn> ISqlTableRepository.getColsInTable(IConfiguration configuration, string the_table_name)
        {
            SqlConnection connection = GetSqlConnection(configuration);

            if (the_table_name == null)
            {
                return null;
            }

            List<SqlColumn> columns = new List<SqlColumn>();


            try
            {
                connection.Open();

                string queryString = String.Format(
                  "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", the_table_name);

                SqlCommand sqlCommand = new SqlCommand(queryString, connection);

                System.Diagnostics.Debug.WriteLine(queryString);

                SqlDataReader result = sqlCommand.ExecuteReader();

                if (result.HasRows)
                {

                    while (result.Read())
                    {
                        SqlColumn column = new SqlColumn()
                        {
                            column_name = result.GetString(3),
                            table_name = the_table_name
                        };

                        columns.Add(column);
                    }
                    result.Close();
                }
                else
                {
                    result.Close();
                }

                connection.Close();
                return columns;

            }
            catch (Exception exception)
            {
                connection.Close();
                System.Diagnostics.Debug.WriteLine(exception.StackTrace);
                return null;

            }

        }

        List<string> ISqlTableRepository.getListFKsByTable(IConfiguration configuration, string table_name)
        {
            SqlConnection sqlConnection = GetSqlConnection(configuration);

            List<string> FKs = new List<string>();

            string query = "SELECT OBJECT_NAME(fkeys.constraint_object_id) foreign_key_name "
                            + ",OBJECT_NAME(fkeys.parent_object_id) referencing_table_name " +
                            ",COL_NAME(fkeys.parent_object_id, fkeys.parent_column_id) referencing_column_name " +
                            ",OBJECT_SCHEMA_NAME(fkeys.parent_object_id) referencing_schema_name" +
                            ",OBJECT_NAME (fkeys.referenced_object_id) referenced_table_name" +
                            ",COL_NAME(fkeys.referenced_object_id, fkeys.referenced_column_id) referenced_column_name " +
                            ",OBJECT_SCHEMA_NAME(fkeys.referenced_object_id) referenced_schema_name " +
                            "FROM sys.foreign_key_columns AS fkeys " +
                            "WHERE OBJECT_NAME(fkeys.parent_object_id) = '{0}'";
            SqlCommand sqlCommand = new SqlCommand(String.Format(query, table_name), sqlConnection);

            sqlConnection.Open();

            SqlDataReader result = sqlCommand.ExecuteReader();

            if (result.HasRows)
            {
                while (result.Read())
                {
                    string resultStr = String.
                         Format("{0}-{1}-{2}-{3}", result.GetString(1), result.GetString(2), result.GetString(4), result.GetString(5));

                    System.Diagnostics.Debug.WriteLine(resultStr);
                    FKs.Add(resultStr);
                }
                result.Close();
            }
            else
            {
                result.Close();
            }
            sqlConnection.Close();
            return FKs;
        } 

        List<string> ISqlTableRepository.getListPKsByTable(IConfiguration configuration, string table_name)
        {
            SqlConnection sqlConnection = GetSqlConnection(configuration);

            List<string> PKs = new List<string>();

            string query = "SELECT Col.Column_Name from "
                            + "INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, "
                            + "INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col "

                            + "WHERE  Col.Constraint_Name = Tab.Constraint_Name "
                            + "AND Col.Table_Name = Tab.Table_Name "
                            + "AND Constraint_Type = 'PRIMARY KEY' "
                            + "AND Col.Table_Name = '{0}'";
            SqlCommand sqlCommand = new SqlCommand(String.Format(query, table_name), sqlConnection);

            sqlConnection.Open();

            SqlDataReader result = sqlCommand.ExecuteReader();

            if (result.HasRows)
            {
                while (result.Read())
                {
                    PKs.Add(result.GetString(0));
                }
                result.Close();
            }
            else
            {
                result.Close();
            }
            sqlConnection.Close();
            return PKs;
        }

        public DataSet getDataRawQuery(IConfiguration config, string queryString)
        {
            SqlConnection myConn = null;
            DataSet result = new DataSet();
            try
            {
                string conString = config.GetConnectionString("MSSQLConnection");
                myConn = new SqlConnection(conString);

                myConn.Open();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter()
                {
                    SelectCommand = new SqlCommand(queryString, myConn)
                };
                sqlDataAdapter.Fill(result);
            }
            catch (Exception err)
            {
                result = null;
            }
            finally
            {
                if (myConn != null) myConn.Close();
            }
            return result;
        }
    }
}
