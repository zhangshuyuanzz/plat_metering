using Common.log;
using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Text;
using Base.kit;

namespace OpcClientForMetering
{
    class OpcSetOracle
    {
        readonly NLOG logger = new NLOG("OpcSetOracle");
        OracleConnection oledbConnection;
        string OracleConnStr = "Data Source=(DESCRIPTION = (ADDRESS_LIST=" +
                                            "(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT = 1521)))" +
                                            " (CONNECT_DATA =(SERVICE_NAME={1})) );User Id ={2}; Password={3};";
        public OpcSetOracle(string ip,string srcname, string UsrId, string UsrPw)
        {
            logger.Debug("OpcSetOracle--ip[{}]--[{}][{}][{}]---", ip, srcname, UsrId, UsrPw);
            string dash = string.Format(OracleConnStr, ip, srcname, UsrId, UsrPw);
            logger.Debug("dash[{}]", dash);

            oledbConnection = new OracleConnection(dash);
            oledbConnection.Open();
            logger.Debug("oracle db conn---[{}]",oledbConnection.State.ToString());
        }
        void isconned()
        {
            if (oledbConnection.State == System.Data.ConnectionState.Closed) {
                oledbConnection.Open();
            }
        }
        public int OpcSetOracleInsertData(string tblNm,DataItem indata)
        {
            string OpcSetSQLString = "insert into " + tblNm + "(NAME,VALUE,TIME) values ('{0}','{1}','{2}')" ;
            OpcSetSQLString = string.Format(OpcSetSQLString, indata.TagName, indata.Value, indata.DataTime);
            logger.Debug("OpcSetSQLString[{}]", OpcSetSQLString);
            using (OracleCommand cmd = new OracleCommand(OpcSetSQLString, this.oledbConnection))
            {
                int rows = 0;
                try
                {
                    isconned();
                    rows = cmd.ExecuteNonQuery();
                }
                catch (OracleException E)
                {
                    logger.Debug("error[{}]", E.ToString());
                }
                return rows;
            }
        }
        string OpcOracleString = "insert into {0}" +
                                            "( TAG_NAME,TAG_DESCRI,TAG_OWNER,TAG_VALUE,TAG_UNIT,TAG_QUALITY,TIME)"+
                                             " values ('{1}','{2}','{3}',{4},'{5}',{6},'{7}')";
        public int OracleInsertListData(string tblNm, List<NMDev> devList)
        {
            int rows = devList.Count;
            logger.Debug("rows[{}]", rows);
            string OpcTString;
            try
            {
                isconned();
                object tagValue ;
                OracleCommand cmd = this.oledbConnection.CreateCommand();

                foreach (NMDev item in devList)
                {
                    tagValue = item.taginfo.Value; //Convert.ToSingle(item.taginfo.Value);// (float)item.taginfo.Value;
                    logger.Debug("TagName[{}]value[{}]DataTime[{}]", item.taginfo.TagName, tagValue, item.taginfo.DataTime);
                    OpcTString = string.Format(OpcOracleString, tblNm, 
                                                                item.taginfo.TagName, item.devdescription, item.devfac,
                                                                tagValue, item.devuint, item.taginfo.Quality, item.taginfo.DataTime);
                    logger.Debug("OpcSQLString[{}]", OpcTString);
                    cmd.CommandText = OpcTString;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception E)
            {
                logger.Debug("error[{}]", E.ToString());
            }
            return rows;
        }
    }
}
