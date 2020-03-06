using Base.kit;
using Common.log;
using SkKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpcClientForMetering
{
    class OpcSetSocketSver
    {
        readonly NLOG logger = new NLOG("OpcSetSocketSver");
        public SkServer OpcSetServerHandle;
        private Dictionary<string, DevInfo> DevConnList = new Dictionary<string, DevInfo>();
        OpcSetConfig OpcSKConfig;
        public OpcSetSocketSver(OpcSetConfig Opcconfig)
        {
            OpcSKConfig = Opcconfig;
            logger.Debug("port[{}]", Opcconfig.SocketPort);
            OpcSetServerHandle = new SkServer(Opcconfig.SocketPort);
            OpcSetServerHandle.SkStartListen();
            OpcSetServerHandle.Server_get_handle += new SkServer.ServerDataEventHandler(OpcSetGetData);
            OpcSetServerHandle.Server_conn_handle += new SkServer.ServerConnEventHandler(ConnedNotification);
            OpcSetServerHandle.Server_disconn_handle += new SkServer.ServerDisconnEventHandler(DisconnedNotification);
        }
        public void OpcSetGetData(List<DevTagIndo> tagslist, string UDip)
        {
            logger.Debug("updateDevInfo---ip[{}]", UDip);
            try
            {
                SkSendTagList(UDip);
            }
            catch (Exception ex)
            {
                logger.Error("error[{}]", ex.ToString());
            }
            logger.Debug("end !!OpcSetGetData  UDip[{ }]", UDip);

        }
        public void ConnedNotification(object handle, string ip)
        {
            logger.Debug("ConnedNotification--ip[{}]", ip);
            DevInfo onedev;

            if (DevConnList.ContainsKey(ip) == false)
            {
                onedev = new DevInfo(handle);
                DevConnList.Add(ip, onedev);
            }
            else
            {
                logger.Debug("has this ip");
                onedev = DevConnList[ip];
            }
            logger.Debug("ConnedNotification end");
        }
        public void DisconnedNotification(object handle, string ip)
        {
            logger.Debug("DisconnedNotification--ip[{}]", ip);

            if (DevConnList.ContainsKey(ip) == true)
            {
                DevInfo buffer = DevConnList[ip];
                buffer.TagList.Clear();
                DevConnList.Remove(ip);
                foreach (string s in DevConnList.Keys)
                {
                    logger.Debug("sssss[{}]", s);
                }
            }
        }
        private void SkSendTagList(string SendIp )
        {
            logger.Debug("SkSendTagList!!ip[{}]", SendIp);
            DevInfo onedev = DevConnList[SendIp];
            SkServer ServerHandle = (SkServer)onedev.SkClientHandle;
            byte[] medata = OpcSetServerJointFrame();
            if (medata != null) {
                ServerHandle.CoSendByte(SendIp, medata);
            }
        }
        private byte[] OpcSetServerJointFrame()
        {
            try
            {
                int counttag = OpcSKConfig.DevListAll.Count;
                int CountOracle = OpcSKConfig.DevBannerList.Count;
                int TotalCount = counttag + CountOracle;
                if ((TotalCount) <= 255)
                {
                    byte[] medata = new byte[4 + 6 * (TotalCount)];
                    medata[0] = medata[1] = 0xFE;
                    medata[2] = Convert.ToByte(TotalCount);
                    medata[medata.Length - 1] = 0xFE;

                    for (int count = 0; count < counttag; count++)
                    {
                        DataItem element = null;//OpcSKConfig.TagListAll.ElementAt(count).Value;
                        if (element.Active == false) {
                            continue;
                        }
                        logger.Debug("TagName[{}]TagId[{}]Value[{}]", element.TagName, element.TagId, element.Value);
                        UInt16 tagid = Convert.ToUInt16(element.TagId);
                        float tagvalue = Convert.ToSingle(element.Value);
                        
                        byte[] byteid = BitConverter.GetBytes(tagid);
                        byte[] bytevalue = BitConverter.GetBytes(tagvalue);
                        Buffer.BlockCopy(byteid, 0, medata, 3 + count * 6, 2);
                        Buffer.BlockCopy(bytevalue, 0, medata, 5 + count * 6, 4);
                    }

                    for (int count = counttag; count < TotalCount; count++)
                    {
                        DataItem element = null;//OpcSKConfig.evBannerList.ElementAt(count- counttag).Value;
                        logger.Debug("oracel--TagName[{}]TagId[{}]Value[{}]", element.TagName, element.TagId, element.Value);
                        if (element.Active == false)
                        {
                            continue;
                        }
                        UInt16 tagid = Convert.ToUInt16(element.TagId);
                        float tagvalue = Convert.ToSingle(element.Value);
                        byte[] byteid = BitConverter.GetBytes(tagid);
                        byte[] bytevalue = BitConverter.GetBytes(tagvalue);
                        Buffer.BlockCopy(byteid, 0, medata, 3 + count * 6, 2);
                        Buffer.BlockCopy(bytevalue, 0, medata, 5 + count * 6, 4);
                    }
                    //
                    logger.Debug(" send frame[{}]", LittleTools.ByteToHexStr(medata));
                    return medata;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Debug("error [{}]",ex.ToString());
                return null;
            }
        }
    }
}
