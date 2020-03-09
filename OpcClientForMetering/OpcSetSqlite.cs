using Base.kit;
using Common.log;
using SkKit.kit;
using SQLite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpcClientForMetering
{
    class OpcSetSqlite
    {
        readonly NLOG logger = new NLOG("OpcSetSqlite");
        private SQLiteHelper OpcSetDBHandle = null;
        private string OpcSetDBPath = @".\DB\OpcSetClient.db";
        public OpcSetSqlite()
        {
            if (System.IO.File.Exists(OpcSetDBPath))
            {
                logger.Debug("file exist!!");
            }
            else
            {
                logger.Debug("file is not exist!!");
                return;
            }
            this.OpcSetDBHandle = new SQLiteHelper(OpcSetDBPath);
            if (this.OpcSetDBHandle.Open() == false)
            {
                logger.Debug("open error !!!");
                return;
            }
        }
        public void OpcSetWriteDev(List<NMDev> devlist)
        {
            if (OpcSetDBHandle == null || devlist == null || devlist.Count == 0)
            {
                return ;
            }
            this.OpcSetDBHandle.SaveDev("ADevTb", devlist);
        }
        public bool OpcSetWriteTag(string Table, List<NMDev> taglist)
        {
            if (OpcSetDBHandle == null || taglist == null || taglist.Count == 0)
            {
                return false;
            }
            this.OpcSetDBHandle.SaveTag(Table, taglist);

            return true;
        }
        public bool OpcSetUpdateTag(string Table, List<DataItem> taglist)
        {
            if (OpcSetDBHandle == null || taglist == null || taglist.Count == 0)
            {
                return false;
            }
            this.OpcSetDBHandle.UpdateTag(Table, taglist);

            return true;
        }
        public void OpcSetDelAll()
        {
            logger.Debug("dell all");
            if (OpcSetDBHandle == null)
            {
                return ;
            }
            List<string> Delstr = new List<string>();
            Delstr.Add("ADevTb");
            Delstr.Add("RealTimeOpc");
            Delstr.Add("BannerOpc");
            this.OpcSetDBHandle.DelAllMeter(Delstr);

            return ;
        }

        public bool OpcSetDataGet(List<string> ips)
        {
            if (OpcSetDBHandle == null)
            {
                return false;
            }
            string insql = "select * from opc_tag where devip in ( {0} )";
            List<object> list = this.OpcSetDBHandle.ExecuteRow(insql, ips);
            logger.Debug("OpcServer Get--Count[{}]", list.Count);

            List<DataItem> alltag = new List<DataItem>();
            foreach (object o in list)
            {
                Dictionary<string, object> d = (Dictionary<string, object>)o;
                logger.Debug("-----------------------------------------------sssssssssssss---------------------------------------");
                DataItem onetag = new DataItem();
                foreach (KeyValuePair<string, object> s in d)
                {
                    logger.Debug("key[{}]Value[{}]", s.Key, s.Value);
                    switch (s.Key)
                    {
                        case "tagid":
                            onetag.TagId = Convert.ToInt32(s.Value);
                            break;
                        case "name":
                            onetag.TagName = Convert.ToString(s.Value);
                            break;
                        case "value":
                            onetag.Value = s.Value;
                            break;
                        case "time":
                            onetag.DataTime = (DateTime)s.Value;
                            break;
                        case "type":
                            onetag.DataType = Convert.ToByte(s.Value);
                            break;
                        default:
                            break;
                    }
                }
                alltag.Add(onetag);
            }
            logger.Debug("-----------------------------------------------eeeeeeeeeee---------------------------------------");
            return true;
        }
    }
}
