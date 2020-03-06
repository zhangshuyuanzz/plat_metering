using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SQLite;
using Common.log;
using Base.kit;
using System.Data.Common;
using SkKit.kit;

namespace SQLite
{

    /// <summary>   
    /// SQLite数据库操作帮助类  
    /// 提供一系列方便的调用:  
    /// Execute,Save,Update,Delete...  
    /// </summary>  
    public class SQLiteHelper
    {
        static private NLOG logger = new NLOG("SQLiteHelper");
        private bool _showSql = true;

        /// <summary>  
        /// 是否输出生成的SQL语句  
        /// </summary>  
        public bool ShowSql
        {
            get
            {
                return this._showSql;
            }
            set
            {
                this._showSql = value;
            }
        }

        private readonly string DbFilePath;

        private SQLiteConnection _conn;

        public SQLiteHelper(string dataFilePath)
        {
            if (dataFilePath == null)
                throw new ArgumentNullException("dataFile=null");
            this.DbFilePath = dataFilePath;
        }

        /// <summary>  
        /// <para>打开SQLiteManager使用的数据库连接</para>  
        /// </summary>  
        public bool Open()
        {
            this._conn = OpenConnection(this.DbFilePath);
            return this._conn == null ? false : true;
        }

        public void Close()
        {
            if (this._conn != null)
            {
                this._conn.Close();
            }
        }

        /// <summary>  
        /// <para>安静地关闭连接,保存不抛出任何异常</para>  
        /// </summary>  
        public void CloseQuietly()
        {
            if (this._conn != null)
            {
                try
                {
                    this._conn.Close();
                }
                catch { }
            }
        }

        /// <para>创建一个连接到指定数据文件的SQLiteConnection,并Open</para>  
        public static SQLiteConnection OpenConnection(string dataFile)
        {
            try
            {
            logger.Debug("OpenConnection[{}]", dataFile);
            if (dataFile == null)
               return null;

            if (!File.Exists(dataFile))
            {
                logger.Debug("file is not exist!!!");
                //return null;
                SQLiteConnection.CreateFile(dataFile);
            }
            SQLiteConnection conn = new SQLiteConnection("data source =" + dataFile);
            conn.Open();
            return conn;
            }
            catch (Exception e)
            {
                logger.Debug("error[{}]",e.ToString());
                return null;
            }
        }

        /// <summary>  
        /// <para>读取或设置SQLiteManager使用的数据库连接</para>  
        /// </summary>  
        public SQLiteConnection Connection
        {
            get
            {
                return this._conn;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                this._conn = value;
            }
        }

        protected void EnsureConnection()
        {
            if (this._conn == null)
            {
                throw new Exception("SQLiteManager.Connection=null");
            }
        }

        public string GetDataFile()
        {
            return this.DbFilePath;
        }
        public bool TableExists(string table)
        {
            if (table == null)
                throw new ArgumentNullException("table=null");
            this.EnsureConnection();
            SQLiteCommand cmd = new SQLiteCommand("SELECT count(*) as c FROM sqlite_master WHERE type='table' AND name=@tableName ");
            cmd.Connection = this.Connection;
            cmd.Parameters.Add(new SQLiteParameter("tableName", table));
            SQLiteDataReader reader = cmd.ExecuteReader();
            reader.Read();
            int c = reader.GetInt32(0);
            reader.Close();
            reader.Dispose();
            cmd.Dispose();
            //return false;  
            return c == 1;
        }

        public int ExecuteNonQuery(string sql, SQLiteParameter[] paramArr)
        {
            if (sql == null )
            {
                throw new ArgumentNullException("sql=null || paramArr == null");
            }
            this.EnsureConnection();

            if (this.ShowSql)
            {
                Console.WriteLine("SQL: " + sql);
            }

            SQLiteCommand cmd = new SQLiteCommand();
            cmd.CommandText = sql;
            if (paramArr != null)
            {
                cmd.Parameters.AddRange(paramArr);
            }
            cmd.Connection = this.Connection;
            int c = cmd.ExecuteNonQuery();
            return c;
        }

        public SQLiteDataReader ExecuteReader(string sql, SQLiteParameter[] paramArr)
        {
            return (SQLiteDataReader)ExecuteReader(sql, paramArr, (ReaderWrapper)null);
        }
        public object ExecuteReader(string sql, SQLiteParameter[] paramArr, ReaderWrapper readerWrapper)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql=null");
            }
            this.EnsureConnection();

            SQLiteCommand cmd = new SQLiteCommand(sql, this.Connection);
            if (paramArr != null)
            {
                foreach (SQLiteParameter p in paramArr)
                {
                    cmd.Parameters.Add(p);
                }
            }
            SQLiteDataReader reader = cmd.ExecuteReader();
            object result = null;
            if (readerWrapper != null)
            {
                result = readerWrapper(reader);
            }
            else
            {
                result = reader;
            }
            reader.Close();
            reader.Dispose();
            cmd.Dispose();
            return result;
        }

        public List<object> ExecuteRow(string sql, List<string> paramArr, RowWrapper rowWrapper=null)
        {
            if (sql == null)
            {
                throw new ArgumentNullException("sql=null");
            }
            this.EnsureConnection();

            if (paramArr != null && paramArr.Count != 0)
            {
                string ippara = "\"0\"";
                foreach (string p in paramArr)
                {
                    ippara = ippara + ", \"" + p  + "\"" ; 
                }
                logger.Debug("ippara[{}]", ippara);
                sql = string.Format(sql, ippara);
            }
            else {
                return null;
            }
            logger.Debug("--end-sql[{}]", sql);

            SQLiteCommand cmd = new SQLiteCommand(sql, this.Connection);

            if (rowWrapper == null)
            {
                rowWrapper = new RowWrapper(SQLiteHelper.WrapRowToDictionary);
            }

            SQLiteDataReader reader = cmd.ExecuteReader();
            List<object> result = new List<object>();
            if (reader.HasRows)
            {
                int rowNum = 0;
                while (reader.Read())
                {
                    object row = rowWrapper(rowNum, reader);
                    result.Add(row);
                    rowNum++;
                }
            }
            reader.Close();
            reader.Dispose();
            cmd.Dispose();
            return result;
        }

        public static object WrapRowToDictionary(int rowNum, SQLiteDataReader reader)
        {
            int fc = reader.FieldCount;
            Dictionary<string, object> row = new Dictionary<string, object>();
            for (int i = 0; i < fc; i++)
            {
                string fieldName = reader.GetName(i);
                object value = reader.GetValue(i);
                row.Add(fieldName, value);
            }
            return row;
        }

        private static string BuildInsert(string table)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("REPLACE INTO ").Append(table).Append(" ( ID,DevName,Decription) values ( @ID,@DevName,@Decription)");
            return buf.ToString();
        }
        private static SQLiteParameter[] IinsertBuildParamArray(NMDev devdata)
        {
            List<SQLiteParameter> list = new List<SQLiteParameter>();
            list.Add(new SQLiteParameter("@ID", devdata.taginfo.TagId));
            list.Add(new SQLiteParameter("@DevName", devdata.taginfo.TagName));
            list.Add(new SQLiteParameter("@Decription", devdata.devdescription));

            if (list.Count == 0)
                return null;
            return list.ToArray();
        }
        public int SaveDev(string table, List<NMDev> entity)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table=null");
            }
            string sql = BuildInsert(table);
            this.EnsureConnection();  //no useless
            logger.Debug("sql[{}]", sql);

            DbTransaction dbTrans = SqlhelpBeginDTS(this.Connection);  //this.Connection.BeginTransaction();
            int count = 0;
            foreach (NMDev s in entity)
            {
                SQLiteParameter[] arr = IinsertBuildParamArray(s);
                count += ExecuteNonQuery(sql, arr);
            }
            SqlhelpCommitDTS(dbTrans);
            return count;
        }
        private static SQLiteParameter[] SaveBuildTagArray(NMDev devdata)
        {
            List<SQLiteParameter> list = new List<SQLiteParameter>();
            list.Add(new SQLiteParameter("@tagId", devdata.taginfo.TagId));
            list.Add(new SQLiteParameter("@OpcItemName", devdata.taginfo.OpcTagName));
            list.Add(new SQLiteParameter("@TagName", devdata.taginfo.TagName));
            list.Add(new SQLiteParameter("@value", devdata.taginfo.Value));
            list.Add(new SQLiteParameter("@Unit", devdata.devuint));
            list.Add(new SQLiteParameter("@Time", devdata.taginfo.DataTime));
            list.Add(new SQLiteParameter("@TagType", devdata.devtype));
            list.Add(new SQLiteParameter("@tagDesc", devdata.devdescription));
            list.Add(new SQLiteParameter("@owner", devdata.devfac));
            list.Add(new SQLiteParameter("@tagQuality", devdata.taginfo.Quality));

            logger.Debug("----TagId[{}]", devdata.taginfo.TagId);
            logger.Debug("----OpcTagName[{}]", devdata.taginfo.OpcTagName);
            logger.Debug("----TagName[{}]", devdata.taginfo.TagName);
            logger.Debug("----Value[{}]", devdata.taginfo.Value);
            logger.Debug("----devuint[{}]", devdata.devuint);
            logger.Debug("----DataTime[{}]", devdata.taginfo.DataTime);
            logger.Debug("----devtype[{}]", devdata.devtype);
            logger.Debug("----devdescription[{}]", devdata.devdescription);
            logger.Debug("----devfac[{}]", devdata.devfac);
            logger.Debug("----Quality[{}]", devdata.taginfo.Quality);

            if (list.Count == 0)
                return null;
            return list.ToArray();
        }
        private static string BuildSaveTag(string table)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("REPLACE INTO ").Append(table).Append(" ( tagId,OpcItemName,TagName,value,Unit,Time,TagType,tagDesc,owner,tagQuality) values ( @tagId,@OpcItemName,@TagName,@value,@Unit,@Time,@TagType,@tagDesc,@owner,@tagQuality)");
            return buf.ToString();
        }

        private static SQLiteParameter[] UpdateBuildTagArray(DataItem devdata)
        {
            List<SQLiteParameter> list = new List<SQLiteParameter>();
            list.Add(new SQLiteParameter("@value", devdata.Value));
            list.Add(new SQLiteParameter("@Time", devdata.DataTime));
            list.Add(new SQLiteParameter("@tagQuality", devdata.Quality));
            list.Add(new SQLiteParameter("@OpcItemName", devdata.OpcTagName));

            logger.Debug("Value[{}]DataTime[{}]OpcItemName[{}]Quality[{}]", devdata.Value, devdata.DataTime, devdata.OpcTagName, devdata.Quality);
            if (list.Count == 0)
                return null;
            return list.ToArray();
        }
        public int UpdateTag(string table, List<DataItem> entity)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table=null");
            }
            string sql = "UPDATE " + table + " SET value=@value,time=@time,tagQuality=@tagQuality WHERE OpcItemName=@OpcItemName"; 
            this.EnsureConnection();  //no useless
            logger.Debug("sql[{}]", sql);

            DbTransaction dbTrans = SqlhelpBeginDTS(this.Connection);  //this.Connection.BeginTransaction();
            int count = 0;
            foreach (DataItem s in entity)
            {
                SQLiteParameter[] arr = UpdateBuildTagArray( s);
                count += ExecuteNonQuery(sql, arr);
            }
            SqlhelpCommitDTS(dbTrans);
            return count;
        }
        public int SaveTag(string table, List<NMDev> entity)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table=null");
            }
            string sql = BuildSaveTag(table);
            this.EnsureConnection();  //no useless
            logger.Debug("sql[{}]", sql);

            DbTransaction dbTrans = SqlhelpBeginDTS(this.Connection);  //this.Connection.BeginTransaction();
            int count = 0;
            foreach (NMDev s in entity)
            {
                SQLiteParameter[] arr = SaveBuildTagArray(s);
                count += ExecuteNonQuery(sql, arr);
            }
            SqlhelpCommitDTS(dbTrans);
            return count;
        }
        public void DelAllMeter(List<string> Tables)
        {
            try
            {
                string DelTab = "delete from {0}";
                DbTransaction dbTrans = SqlhelpBeginDTS(this.Connection);  //this.Connection.BeginTransaction();

                this.EnsureConnection();
                foreach (string ss in Tables) {
                    string sqlstr = string.Format(DelTab,ss);
                    int ret = this.ExecuteNonQuery(sqlstr, null);
                }
                SqlhelpCommitDTS(dbTrans);
            }
            catch (Exception ex)
            {
                logger.Debug("error [{}]", ex.ToString());
            }

            return ;
        }
        private static SQLiteParameter[] BuildParamArray(DataItem tagidata)
        {
            List<SQLiteParameter> list = new List<SQLiteParameter>();
            DataItem s = tagidata;
            list.Add(new SQLiteParameter("@tagid", s.TagId));
            list.Add(new SQLiteParameter("@name", s.TagName));
            list.Add(new SQLiteParameter("@type", s.DataType));
            list.Add(new SQLiteParameter("@value", s.Value));
            list.Add(new SQLiteParameter("@time", s.DataTime));
            list.Add(new SQLiteParameter("@devip", s.TagIP));

            if (list.Count == 0)
                return null;
            return list.ToArray();
        }

        private static string BuildReplace(string table)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("REPLACE INTO ").Append(table).Append(" ( tagid,name,type,value,time,devip) values ( @tagid,@name,@type,@value,@time ,@devip)");
            logger.Debug(" after buf[{}]", buf);
            return buf.ToString();
        }
        public int Replace(string table, List<DataItem> entity)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table=null");
            }
            string sql = BuildReplace(table);
            this.EnsureConnection();  //no useless
            logger.Debug("sql[{}]", sql);

            DbTransaction dbTrans = SqlhelpBeginDTS(this.Connection);  //this.Connection.BeginTransaction();
            int count = 0;
            foreach (DataItem s in entity) {
                SQLiteParameter[] arr = BuildParamArray(s);
                count += ExecuteNonQuery(sql, arr);
            }
            SqlhelpCommitDTS(dbTrans);
            return count;
        }

        public int DeleteAll(string table, string Delp)
        {
            if (table == null || Delp == null)
            {
                throw new ArgumentNullException("table=null");
            }
            this.EnsureConnection();

            DbTransaction dbTrans = SqlhelpBeginDTS(this.Connection);  //this.Connection.BeginTransaction();

            string sql = "delete from " + table + " where devip = @delip" ;
            SQLiteParameter[] wpara = new SQLiteParameter[1];
            wpara[0] = new SQLiteParameter("@delip", Delp);
            int ret = this.ExecuteNonQuery(sql, wpara);
            SqlhelpCommitDTS(dbTrans);

            return ret;
        }
        private static SQLiteParameter[] BuildUpdateParamArray(DataItem tagidata)
        {
            List<SQLiteParameter> Uplist = new List<SQLiteParameter>();
            DataItem s = tagidata;
            Uplist.Add(new SQLiteParameter("@value", s.Value));
            Uplist.Add(new SQLiteParameter("@time", s.DataTime));
            Uplist.Add(new SQLiteParameter("@name", s.TagName));

            if (Uplist.Count == 0)
                return null;
            return Uplist.ToArray();

        }
        public int Update(string table, List<DataItem> entity)
        {
            int total = 0;
            StringBuilder UpdateBuf = new StringBuilder();

            DbTransaction dbTrans = SqlhelpBeginDTS(this.Connection); //this.Connection.BeginTransaction();
            UpdateBuf.Append("UPDATE ").Append(table).Append(" SET value=@value,time=@time WHERE name=@name");
            string updatesql = UpdateBuf.ToString();
            logger.Debug("updatesql[{}]", updatesql);
            foreach (DataItem s in entity)
            {
                total += ExecuteNonQuery(updatesql, BuildUpdateParamArray(s));
            }
            SqlhelpCommitDTS(dbTrans);
            return total;
        }
        private DbTransaction SqlhelpBeginDTS(SQLiteConnection DbConn)
        {
            DbTransaction dbTrans = null;
            try
            {
                dbTrans = DbConn.BeginTransaction();
            }
            catch (Exception ex)
            {
                logger.Debug("begin Dts error: [{}]",ex.ToString());
                dbTrans.Rollback();
                dbTrans = DbConn.BeginTransaction();
            }
            return dbTrans;
        }
        private void SqlhelpCommitDTS(DbTransaction DTSHandle)
        {
            if (DTSHandle == null) {
                return;
            }
            try
            {
                DTSHandle.Commit(); ;
            }
            catch (Exception ex)
            {
                logger.Debug("begin Dts error: [{}]", ex.ToString());
            }
        }
    }

    /// <summary>  
    /// 在SQLiteManager.Execute方法中回调,将SQLiteDataReader包装成object   
    /// </summary>  
    /// <param name="reader"></param>  
    /// <returns></returns>  
    public delegate object ReaderWrapper(SQLiteDataReader reader);

    /// 将SQLiteDataReader的行包装成object  

    public delegate object RowWrapper(int rowNum, SQLiteDataReader reader);

}