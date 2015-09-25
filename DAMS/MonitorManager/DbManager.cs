using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DAMS.MonitorManager
{
    class DbManager
    {
        //Add media file's management info into T_MEDIA table
        public void InsertSystemInfo(Dictionary<string, string> alValues)
        {
            StringBuilder strSqlDel = new StringBuilder();
            strSqlDel.Append("delete from T_DASTATION ");
            strSqlDel.Append(" where DAS_NUMBER=@DAS_NUMBER");
            MySqlParameter[] delParameters = {
					new MySqlParameter("@DAS_NUMBER", alValues["@DAS_NUMBER"])
			};

            String strSqlAdd= "insert into T_DASTATION ("
                       + "UUID, DAS_NUMBER, DAS_SERIAL_NUMBER, DAS_NAME, UNIT_UUID, IP"
                       + ", AUTHORITY_STATUS, AUTHORITY_PERIOD, AUTHORITY_PERIOD_UNIT, DT_AUTHORITY, AUTHORITY_OP"
                       + ", ITEM_1, ITEM_2, ITEM_3, ITEM_4, ITEM_5, ITEM_6, ITEM_7, ITEM_8, ITEM_9, ITEM_10"
                       + ", STATUS_1, STATUS_2, STATUS_3, STATUS_4, STATUS_5, STATUS_6, STATUS_7, STATUS_8, STATUS_9, STATUS_10"
                       + ", IS_ACTIVE, CREATE_DT, CREATE_OP, CREATE_PG, UPDATE_DT, UPDATE_OP, UPDATE_PG"
                       + ")values("
                       + "getuid, @DAS_NUMBER, @DAS_SERIAL_NUMBER, @DAS_NAME, @UNIT_UUID, @IP"
                       + ", @AUTHORITY_STATUS, @AUTHORITY_PERIOD, @AUTHORITY_PERIOD_UNIT, @DT_AUTHORITY, @AUTHORITY_OP"
                       + ", 'network', 'cpu', 'memory', 'hdd1', 'hdd2', 'hdd3', '', '', '', ''"
                       + ", @STATUS_1, @STATUS_2, @STATUS_3, @STATUS_4, @STATUS_5, @STATUS_6, '', '', '', ''"
                       + ", '1', systimestamp, 'Administrator', 'DAMS', systimestamp, 'Administrator', 'DAMS'"
                       + ")";
            try
            {
                LogConfig.info("Administrator", "    SQL : " + strSqlDel.ToString());
                int rows = MySqlHelper.ExecuteNonQuery(System.Data.CommandType.Text, strSqlDel.ToString(), delParameters);

                MySqlParameter[] addParameters = ParseSQL(alValues);
                LogConfig.info("Administrator", "    SQL : " + strSqlAdd);
                
                rows = MySqlHelper.ExecuteNonQuery(System.Data.CommandType.Text, strSqlDel.ToString(), addParameters);
                if (rows < 0)
                {
                    LogConfig.error("Administrator","保存失败");
                }
            }
            catch (Exception ex)
            {
                //Log4NetWorker.Fatal("!!!! DB Exception : " + ex.Message);
                throw ex;
            }
        }

        private MySqlParameter[] ParseSQL(Dictionary<string, string> alValues)
        {
            List<MySqlParameter> addParameters = new List<MySqlParameter>();
            foreach (KeyValuePair<string, string> value in alValues)
            {
                addParameters.Add(new MySqlParameter(value.Key, value.Value));
            }
            return addParameters.ToArray();
        }
    }
}
