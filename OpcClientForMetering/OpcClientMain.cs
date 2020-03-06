using Base.kit;
using Common.log;
using SkKit.kit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UcAsp.Opc;

namespace OpcClientForMetering
{
    class OpcClientMain
    {
        readonly NLOG logger = new NLOG("OpcClientMain");
        public OpcClient OClient = null;
        public TagEventHandler OpcSetTagChanged;
        public OpcClientMain(string ClientHandle)
        {
            if (ClientHandle == null) {
                return;
            }
            logger.Debug("opc name [{}]", ClientHandle);
            OClient = new OpcClient(new Uri(ClientHandle));
            if (OClient.Connect == OpcStatus.Connected) {
                logger.Debug("Connected success");
            }
            else {
                logger.Debug("Connected fail!!");
                OClient = null;
            }
        }
        public OpcClientMain(string ip,string name)
        {
            if (name == null)
            {
                return;
            }
            logger.Debug("ip [{}]name[{}]", ip,name);
            OClient = new OpcClient(ip,name);

            if (OClient.Connect == OpcStatus.Connected)
            {
                logger.Debug("Connected success");
            }
            else
            {
                logger.Debug("Connected fail!!");
                OClient = null;
            }
        }
        public void OpcClientMainReadOneTag(ref DataItem ReadOTag)
        {
            if (OClient == null) {
                return;
            }
            logger.Debug("OpcClientMainReadOneTag-TagName[{}]", ReadOTag.TagName);
            OpcItemValue data = this.OClient.ReadOneTag<string>(ReadOTag.TagName);
            logger.Debug("ItemId[{}]Value[{}]Timestamp[{}]", data.ItemId, data.Value, data.Timestamp);
            ReadOTag.Value = data.Value;
            ReadOTag.DataTime = data.Timestamp.ToString();
            ReadOTag.Active = true;
        }
        public void OpcClientMainRead(ref ConcurrentDictionary<string, NMDev> ReadDevList)
        {
            try
            {
                if (OClient == null)
                {
                    return;
                }
                foreach (KeyValuePair<string, NMDev> one in ReadDevList)
                {
                    logger.Debug("read opc-- dev name[{}]", one.Key);
                    OpcItemValue data = this.OClient.ReadOneTag<string>(one.Value.taginfo.OpcTagName);
                    one.Value.taginfo.Value = data.Value;
                    one.Value.taginfo.DataTime = data.Timestamp.ToString();
                    one.Value.taginfo.Active = true;
                    one.Value.taginfo.Quality = (ushort)(data.Quality == "good" ? 1 : 2);
                }
            }
            catch (Exception ex)
            {
                logger.Debug("error[{}]",ex.ToString());
            }


        }
        public void OpcClientMainSubscription(string[] Tagname,string groupname)
        {
            if (OClient == null)
            {
                return;
            }
            string msg;
            OpcGroup OpcSetSubGroup = this.OClient.AddGroup(groupname);
            logger.Debug("OpcClientMainSubscription---Length[{}]groupname[{}]", Tagname.Length, groupname);
            OpcSetSubGroup.DataChange += OpcSetGroup_DataChange;
            this.OClient.AddItems(groupname, Tagname, out msg);
        }
        private void OpcSetGroup_DataChange(object sender, ItemDataEventArgs e)
        {
            List<DataItem> UPTagList = new List<DataItem>();
            logger.Debug("---subcription-------get----opc----server-----data------start-------------------------------");
            foreach (OpcItemValue o in e.Data)
            {
                DataItem one = new DataItem();
                logger.Debug("base--GroupName[{}]ItemId[{}]Value[{}]Timestamp[{}]", o.GroupName, o.ItemId,o.Value,o.Timestamp);
                one.GroupName = o.GroupName;
                one.TagName = o.ItemId;
                one.Value = o.Value;
                one.DataTime = o.Timestamp.ToString();
                UPTagList.Add(one);
            }
            OpcSetTagChanged?.Invoke(UPTagList);
        }
    }
}
