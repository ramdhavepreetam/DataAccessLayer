using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace DataAccessLayer
{

    public enum DataProvider
    {
        SqlServer, OleDb, Odbc
    }
    public enum CommandBuilderType
    {
        InsertCommand,
        DeleteCommand,
        UpdateCommand,
        InsertUpdateCommand,
        NONE

    }
    public sealed class DBManager : IDBManager, IDisposable
    {

        private IDbConnection idbConnection;
        private IDataReader idataReader;
        private IDbCommand idbCommand;
        private DataProvider providerType;
        private IDbTransaction idbTransaction = null;
        private IDbDataParameter[] idbParameters = null;
        private DbCommandBuilder iDbCommandBuilder = null;
        public DataSet UpdateDataSet = null;
        private string strConnection;

        public DBManager()
        {

        }

        public DBManager(DataProvider providerType)
        {
            this.providerType = providerType;
        }

        public DBManager(DataProvider providerType, string
         connectionString)
        {
            this.providerType = providerType;
            this.strConnection = connectionString;
        }

        public DBManager(DataProvider providerType, IDbConnection
       dbConnection, string connectionString)
        {
            this.providerType = providerType;
            this.idbConnection = dbConnection;
            this.strConnection = connectionString;
        }
        public IDbConnection Connection
        {
            get
            {
                return idbConnection;
            }
        }

        public IDataReader DataReader
        {
            get
            {
                return idataReader;
            }
            set
            {
                idataReader = value;
            }
        }

        public DataProvider ProviderType
        {
            get
            {
                return providerType;
            }
            set
            {
                providerType = value;
            }
        }

        public string ConnectionString
        {
            get
            {
                return strConnection;
            }
            set
            {
                strConnection = value;
            }
        }

        public IDbCommand Command
        {
            get
            {
                return idbCommand;
            }
        }

        public IDbTransaction Transaction
        {
            get
            {
                return idbTransaction;
            }
        }

        public IDbDataParameter[] Parameters
        {
            get
            {
                return idbParameters;
            }
        }

        public void Open()
        {
            idbConnection =
            DBManagerFactory.GetConnection(this.providerType);
            idbConnection.ConnectionString = this.ConnectionString;
            if (idbConnection.State != ConnectionState.Open)
                idbConnection.Open();
            this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
        }

        public void Close()
        {
            if (idbConnection!=null && idbConnection.State != ConnectionState.Closed)
                idbConnection.Close();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Close();
            this.idbCommand = null;
            this.idbTransaction = null;
            this.idbConnection = null;
        }

        public void CreateParameters(int paramsCount)
        {
            idbParameters = new IDbDataParameter[paramsCount];
            idbParameters = DBManagerFactory.GetParameters(this.ProviderType,
              paramsCount);
        }

        public void AddParameters(int index, string paramName, object
         objValue, ParameterDirection parameterDirection = ParameterDirection.Input, DbType dbType = DbType.String)
        {


            if (index < idbParameters.Length)
            {
                idbParameters[index].ParameterName = paramName;
                idbParameters[index].Value = objValue;
                idbParameters[index].Direction = parameterDirection;
                idbParameters[index].DbType = dbType;

            }
        }

        public void BeginTransaction()
        {
            if (this.idbTransaction == null)
            {

                //DBManagerFactory.GetTransaction(this.ProviderType);

                if (this.Connection != null && this.Connection.State == ConnectionState.Open)
                {
                }

                idbTransaction = this.Connection.BeginTransaction();
                this.idbCommand.Transaction = idbTransaction;
            }
        }

        public void CommitTransaction()
        {
            if (this.idbCommand.Transaction != null)
                this.idbTransaction.Commit();
            idbTransaction = null;
        }

        public IDataReader ExecuteReader(CommandType commandType, string
          commandText)
        {
            this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
            idbCommand.Connection = this.Connection;
            PrepareCommand(idbCommand, this.Connection, this.Transaction,
             commandType,
              commandText, this.Parameters);
            this.DataReader = idbCommand.ExecuteReader();
            idbCommand.Parameters.Clear();
            return this.DataReader;
        }

        public void CloseReader()
        {
            if (this.DataReader != null)
                this.DataReader.Close();
        }

        private void AttachParameters(IDbCommand command,
          IDbDataParameter[] commandParameters)
        {
            foreach (IDbDataParameter idbParameter in commandParameters)
            {
                if ((idbParameter.Direction == ParameterDirection.InputOutput)
                &&
                  (idbParameter.Value == null))
                {
                    idbParameter.Value = DBNull.Value;
                }
                command.Parameters.Add(idbParameter);
            }
        }

        private void PrepareCommand(IDbCommand command, IDbConnection
          connection,
          IDbTransaction transaction, CommandType commandType, string
          commandText,
          IDbDataParameter[] commandParameters, CommandBuilderType commandBuilderType = CommandBuilderType.NONE)
        {
            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = commandType;

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }

            if (iDbCommandBuilder != null)
            {
                switch (commandBuilderType)
                {
                    case CommandBuilderType.InsertCommand:
                        iDbCommandBuilder.GetInsertCommand();
                        break;
                    case CommandBuilderType.DeleteCommand:
                        iDbCommandBuilder.GetDeleteCommand();
                        break;
                    case CommandBuilderType.UpdateCommand:
                        iDbCommandBuilder.GetUpdateCommand();
                        break;
                    case CommandBuilderType.InsertUpdateCommand:
                        iDbCommandBuilder.GetInsertCommand();
                        iDbCommandBuilder.GetUpdateCommand();
                        break;
                    case CommandBuilderType.NONE:
                        break;
                    default:
                        break;
                }
            }

        }

        public int ExecuteNonQuery(CommandType commandType, string
        commandText)
        {
            try
            {
                this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
                PrepareCommand(idbCommand, this.Connection, this.Transaction,
                commandType, commandText, this.Parameters);
                int returnValue = idbCommand.ExecuteNonQuery();

                idbCommand.Parameters.Clear();
                return returnValue;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public object ExecuteScalar(CommandType commandType, string
          commandText)
        {
            try
            {
                this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
                PrepareCommand(idbCommand, this.Connection, this.Transaction,
                commandType,
                  commandText, this.Parameters);
                object returnValue = idbCommand.ExecuteScalar();
                idbCommand.Parameters.Clear();
                return returnValue;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public DataSet ExecuteDataSet(CommandType commandType, string
         commandText)
        {
            try
            {
                this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
                PrepareCommand(idbCommand, this.Connection, this.Transaction,
               commandType,
                  commandText, this.Parameters);
                IDbDataAdapter dataAdapter = DBManagerFactory.GetDataAdapter
                  (this.ProviderType);
                dataAdapter.SelectCommand = idbCommand;
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                idbCommand.Parameters.Clear();
                return dataSet;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public DataSet SetDataSetForUpdate(CommandType commandType, string
      commandText)
        {
            try
            {
                this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
                PrepareCommand(idbCommand, this.Connection, this.Transaction,
               commandType,
                  commandText, this.Parameters);
                IDbDataAdapter dataAdapter = DBManagerFactory.GetDataAdapter
                  (this.ProviderType);
                dataAdapter.SelectCommand = idbCommand;
                UpdateDataSet = new DataSet();
                dataAdapter.Fill(UpdateDataSet);
                idbCommand.Parameters.Clear();
                return UpdateDataSet;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void BulkUpdateByDataSet(CommandType commandType, string commandText, DataTable ds, CommandBuilderType commandBuilderType = CommandBuilderType.NONE)
        {
            try
            {
                IDbDataAdapter dataAdapter = CreateCommandAndDataAdapter();
                GetTheCommandBuilder(dataAdapter);
                dataAdapter.SelectCommand = this.idbCommand;
                PrepareCommand(idbCommand, this.Connection, this.Transaction, commandType, commandText, this.Parameters, commandBuilderType);
                BuildInsertCommandForCommandBuilder();
                dataAdapter.SelectCommand = idbCommand;
                DataSet dataSet = new DataSet();
                dataSet.Tables.Add(ds.Copy());
                dataAdapter.Update(dataSet);
                idbCommand.Parameters.Clear();
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void BuildInsertCommandForCommandBuilder()
        {
            try
            {
                this.iDbCommandBuilder.GetInsertCommand();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void GetTheCommandBuilder(IDbDataAdapter dataAdapter)
        {
            this.iDbCommandBuilder = DBManagerFactory.GetCommandBuilder(this.providerType, dataAdapter);
        }

        private IDbDataAdapter CreateCommandAndDataAdapter()
        {
            //Command and Adapter creation 
            this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
            IDbDataAdapter dataAdapter = DBManagerFactory.GetDataAdapter
              (this.ProviderType);
            return dataAdapter;
        }
        public DataTable ExecuteDataTable(CommandType commandType, string
       commandText)
        {
            try
            {
                this.idbCommand = DBManagerFactory.GetCommand(this.ProviderType);
                PrepareCommand(idbCommand, this.Connection, this.Transaction,
               commandType,
                  commandText, this.Parameters);
                IDbDataAdapter dataAdapter = DBManagerFactory.GetDataAdapter
                  (this.ProviderType);
                dataAdapter.SelectCommand = idbCommand;
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                idbCommand.Parameters.Clear();
                if (dataSet.Tables.Count > 0)
                {
                    return dataSet.Tables[0];

                }

                return null;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void RollBack()
        {
            if (this.idbTransaction != null)
                this.idbTransaction.Rollback();
            idbTransaction = null;
        }


    }
}
