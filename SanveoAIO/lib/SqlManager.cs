using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace telmvc
{
    public class SqlManager
    {
        
        public static SqlConnection OpenConnection()
        {
            SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString);
            sqlConnection.Open();
            return sqlConnection;
        }

        public static bool ExecuteNonQuery(string command)
        {
            SqlConnection sqlConnection = OpenConnection();
            SqlCommand sqlCommand = new SqlCommand(command, sqlConnection);
            sqlCommand.CommandTimeout = 0;
            int returnValue = sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
            return returnValue != -1 ? false : true;
        }

        public static bool ExecuteNonQuery(string strCommandText, SqlParameter[] paramArray)
        {
            SqlCommand sqlCommand = new SqlCommand();

            sqlCommand.CommandTimeout = 0;
            sqlCommand = PrepareCommand(strCommandText, paramArray);
            int returnValue = sqlCommand.ExecuteNonQuery();
            sqlCommand.Parameters.Clear();
            sqlCommand.Connection.Close();
            return returnValue != -1 ? true : false;
        }
        public static bool ExecuteNonQuery(string strCommandText, SqlParameter[] paramArray, bool clearParameters)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandTimeout = 0;
            sqlCommand = PrepareCommand(strCommandText, paramArray);
            int returnValue = sqlCommand.ExecuteNonQuery();
            if (clearParameters)
                sqlCommand.Parameters.Clear();
            return returnValue != -1 ? true : false;
        }

        public static DataSet ExecuteDataSet(string command, SqlParameter[] paramArray)
        {
            DataSet ds = new DataSet();
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandTimeout = 0;
            sqlCommand = PrepareCommand(command, paramArray);
            SqlDataAdapter sqlDa = new SqlDataAdapter(sqlCommand);
            sqlDa.Fill(ds);
            sqlCommand.Parameters.Clear();

            return ds;

        }
        private static SqlCommand PrepareCommand(string command, SqlParameter[] param)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandTimeout = 0;
            sqlCommand.CommandText = command;
            sqlCommand.CommandType = CommandType.StoredProcedure;
            SqlConnection sqlconnect = OpenConnection();
            sqlCommand.Connection = sqlconnect;
            sqlCommand.Parameters.AddRange(param);
            return sqlCommand;
        }

        public static DataSet ExecuteDataSet(string command)
        {
            SqlConnection sqlConnection = OpenConnection();
            DataSet dataset = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(command, sqlConnection);
            da.SelectCommand.CommandTimeout = 0;
            da.Fill(dataset);
            da.Dispose();
            sqlConnection.Close();
            return dataset;
        }

        public static SqlDataReader ExecuteReader(string command)
        {
            SqlConnection sqlConnection = OpenConnection();
            SqlCommand sqlCommand = new SqlCommand(command, sqlConnection);
            sqlCommand.CommandTimeout = 0;
            SqlDataReader dr = sqlCommand.ExecuteReader();
            sqlConnection.Close();
            return dr;
        }

        public static SqlDataReader ExecuteReader(string strCommandText, SqlParameter[] paramArray)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandTimeout = 0;
            sqlCommand = PrepareCommand(strCommandText, paramArray);
            SqlDataReader dr = sqlCommand.ExecuteReader();
            sqlCommand.Parameters.Clear();
            sqlCommand.Connection.Close();
            return dr;
        }
        public static SqlDataReader ExecuteReader(string strCommandText, SqlParameter[] paramArray, bool clearParameters)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandTimeout = 0;
            sqlCommand = PrepareCommand(strCommandText, paramArray);
            SqlDataReader dr = sqlCommand.ExecuteReader();
            if (clearParameters)
                sqlCommand.Parameters.Clear();
            return dr;
        }

    }

    public class ResultData
    {
        public object Data;
        public int Count;

        public ResultData()
        {
            this.Data = new DataTable();
            this.Count = 0;
        }
    }
}