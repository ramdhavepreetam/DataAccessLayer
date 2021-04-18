using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataAccessLayer
{
    public interface IDBManager
    {
        DataProvider ProviderType
        {
            get;
            set;
        }

        string ConnectionString
        {
            get;
            set;
        }

        IDbConnection Connection
        {
            get;
        }
        IDbTransaction Transaction
        {
            get;
        }

        IDataReader DataReader
        {
            get;
        }
        IDbCommand Command
        {
            get;
        }

        IDbDataParameter[] Parameters
        {
            get;
        }

        void Open();
        void BeginTransaction();
        void CommitTransaction();
        void RollBack();
        void CreateParameters(int paramsCount);
        void AddParameters(int index, string paramName, object objValue, ParameterDirection parameterDirection, DbType dbType);

        IDataReader ExecuteReader(CommandType commandType, string commandText);
        DataSet ExecuteDataSet(CommandType commandType, string commandText);
        object ExecuteScalar(CommandType commandType, string commandText);
        int ExecuteNonQuery(CommandType commandType, string commandText);
        DataTable ExecuteDataTable(CommandType commandType, string commandText);
        void BulkUpdateByDataSet(CommandType commandType, string commandText,
              DataTable ds, CommandBuilderType commandBuilderType = CommandBuilderType.NONE);

        void CloseReader();
        void Close();
        void Dispose();
    }
}
