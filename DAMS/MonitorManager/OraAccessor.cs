using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Data;
using System.Collections;
using System.Reflection;

namespace DAMS.MonitorManager
{
    class OraAccessor
    {
        //protected OracleConnection connection;
        private OleDbConnection connection;
        private String connectionString;

        #region Constructor
        /// <summary>
        /// Constructor with connection string as a parameter
        /// </summary>
        public OraAccessor(String connStr)
        {
            this.connectionString = connStr;
            try
            {
                connection = new OleDbConnection(connectionString);
                LogConfig.info("Administrator", " -- DB connecting string is as : " + connectionString);
            }
            catch (Exception ex)
            {
                //Log4NetWorker.Fatal("!!!! DB Exception: " + ex.Message);
                LogConfig.error("Administrator", "         Function: OraAccessor.OraAccessor");
                throw ex;
            }
        }
        #endregion

        #region Open the DB connection
        public void OpenConn()
        {
            //Log4NetWorker.Debug(" -------- OraAccessor.OpenConn");
            try
            {
                if (this.connection == null)
                    connection = new OleDbConnection(connectionString);

                if (this.connection.State != System.Data.ConnectionState.Open)
                    this.connection.Open();
            }
            catch (Exception ex)
            {
                this.connection = null;
                LogConfig.error("Administrator", "!!!! DB Exception: " + ex.Message);
                LogConfig.error("Administrator", "         Function: OraAccessor.OpenConn");
                throw ex;
            }
        }

        public OleDbConnection GetConn()
        {
            //Log4NetWorker.Debug(" -------- OraAccessor.GetConn");
            OpenConn();
            return this.connection;
        }

        public void CloseConn()
        {
            //Log4NetWorker.Debug(" -------- OraAccessor.CloseConn");
            try
            {
                if (this.connection.State == System.Data.ConnectionState.Open)
                    this.connection.Close();

                this.connection = null;
            }
            catch (Exception ex)
            {
                //Log4NetWorker.Fatal("!!!! DB Exception: " + ex.Message);
                LogConfig.error("Administrator", "         Function: OraAccessor.CloseConn");
                throw ex;
            }
        }
        #endregion

        #region Qury a SQL and return results as a DataTable
        public DataTable QueryDB(string sql)
        {
            //Log4NetWorker.Debug(" -------- OraAccessor.QueryDB");
            DataTable dt = null;
            try
            {
                //Get db connection
                using (OleDbConnection con = this.GetConn())
                {
                    //Execute the SQL command
                    OleDbCommand cmd = new OleDbCommand(sql, con);
                    dt = ExecuteDataTable(cmd);
                }
            }
            catch (Exception ex)
            {
                //Log4NetWorker.Fatal("!!!! DB Exception: " + ex.Message);
                LogConfig.error("Administrator", "         Function: OraAccessor.QueryDB");
                throw ex;
            }
            finally
            {
                //Close the connection
                this.CloseConn();
            }
            return dt;
        }

        public int UpdateTable(String sql)
        {
            //Log4NetWorker.Debug(" -------- OraAccessor.InsertTable");
            int result = 0;

            try
            {
                //Get db connection
                using (OleDbConnection con = this.GetConn())
                {
                    //Execute the SQL command
                    OleDbCommand cmd = new OleDbCommand(sql, con);

                    //OracleParameter para1 = new OracleParameter(":name", "abc");
                    //cmd.Parameters.Add(para1);
                    result = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                LogConfig.error("Administrator", "!!!! DB Exception: " + ex.Message);
                LogConfig.error("Administrator", "         Function: OraAccessor.InsertTable");
                //throw ex;
            }
            finally
            {
                //Close the connection
                this.CloseConn();
            }
            return result;
        }


        public DataTable ExecuteDataTable(OleDbCommand cmd)
        {
            DataSet dtSet = new DataSet();
            using (OleDbDataAdapter dtAdpter = new OleDbDataAdapter(cmd))
            {
                try
                {
                    dtAdpter.Fill(dtSet);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            if (dtSet.Tables.Count > 0)
            {
                dtSet.Tables[0].DefaultView.RowStateFilter = DataViewRowState.Unchanged |
                                                          DataViewRowState.Added |
                                                          DataViewRowState.ModifiedCurrent |
                                                          DataViewRowState.Deleted;
                return dtSet.Tables[0];
            }
            else
                return null;
        }
        #endregion

        #region Update a date set
        public int UpdateDataSet(DataTable dt, OleDbCommand insertCmd, OleDbCommand updateCmd, OleDbCommand deleteCmd)
        {
            if ((insertCmd == null) && (updateCmd == null) && (deleteCmd == null))
                return 0;

            using (OleDbDataAdapter dtAdpter = new OleDbDataAdapter())
            {
                if (insertCmd != null)
                {
                    dtAdpter.InsertCommand = insertCmd;
                    dtAdpter.InsertCommand.UpdatedRowSource = UpdateRowSource.None;
                }
                if (updateCmd != null)
                {
                    dtAdpter.UpdateCommand = updateCmd;
                    dtAdpter.UpdateCommand.UpdatedRowSource = UpdateRowSource.None;
                }
                if (deleteCmd != null)
                {
                    dtAdpter.DeleteCommand = deleteCmd;
                    dtAdpter.DeleteCommand.UpdatedRowSource = UpdateRowSource.None;
                }

                //UpdateBatchSize: Number of commands; 0: No limited; 1: Prohibit batch update 2+:
                //dtAdpter.UpdateBatchSize = 0; 
                try
                {
                    int row = dtAdpter.Update(dt);
                    return row;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion
    }
}
