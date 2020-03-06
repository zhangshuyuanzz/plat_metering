using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Base.kit
{
    public class DataItem
    {
        public string GroupName { get; set; }
        public string TagName { get; set; }
        public string OpcTagName { get; set; }

        public int TagId { get; set; }
        public byte DataType { get; set; }
        public object Value { get; set; }
        public string DataTime { set; get; }
        public ushort Quality { set; get; }
        public bool Active { set; get; }
        public UInt32 TagHandle { set; get; }
        public string Tagstr { set; get; }
        public string TagIP { set; get; }

        public DataItem()
        {
            this.GroupName = null;
            this.TagName = "unKown name";
            this.OpcTagName = null;
            this.DataType = 1;
            this.Quality = 192;
            this.Active = false;
            this.Value = 0;
            this.DataTime = DateTime.Now.ToString(); 
        }

        public DataItem(string name,int id,byte type,object value,string time,string inip)
        {
            this.TagName = name;
            this.TagId = id;
            this.DataType = type;
            this.Value = value;
            this.DataTime = time;// DateTime.Now.ToString();
            this.Quality = 192;
            this.Active = true;
            this.TagIP = inip;
        }
    }
    public class FlowDev
    {
        public int devid ;
        public string devname;
        public string devdescription;
        private bool devprefix;
        List<DataItem> FlowTagList = new List<DataItem>();

        public FlowDev(string name, int id,string prefix,string desc)
        {
            this.devid = id;
            this.devname = name;
            this.devdescription = desc;
            if (prefix.Contains("no") == true)
            {
                devprefix = false;
            }
            else {
                devprefix = true ;
            }
        }

        public List<DataItem> GetTagList()
        {
            return FlowTagList;
        }
        public void AddTagList(DataItem TagBuf)
        {
            if (TagBuf == null)
            {
                return;
            }
            if (devprefix == true)
            {
                TagBuf.OpcTagName = "DefinitionRecords.IP_AnalogDef.\"" + TagBuf.TagName + "\"";
            }
            else {
                TagBuf.OpcTagName = TagBuf.TagName;
            }
            FlowTagList.Add(TagBuf);
        }

        public string GetTagName(string name)
        {
            string retName = name;
            if (devprefix == true)
            {
                retName = "DefinitionRecords.IP_AnalogDef.\"" + name + "\"";
            }
            
            return retName;
        }

        public int GetDevId()
        {
            return this.devid;
        }

        public string GetDevDesc()
        {
            return this.devdescription;
        }
    }
    public class EMDev 
    {
        public int devid;
        public string devname;
        public string devdescription;
        private bool devprefix;

        List<DataItem> EmTagList = new List<DataItem>();

        public EMDev(string name, int id, string prefix, string desc)
        {
            this.devid = id;
            this.devname = name;
            this.devdescription = desc;
            if (prefix.Contains("no") == true)
            {
                devprefix = false;
            }
            else
            {
                devprefix = true;
            }
        }
        public List<DataItem> GetTagList()
        {
            return EmTagList;
        }
        public void AddTagList(DataItem TagBuf)
        {
            if (TagBuf == null) {
                return;
            }
            if (devprefix == true)
            {
                TagBuf.OpcTagName = "DefinitionRecords.IP_AnalogDef.\"" + TagBuf.TagName + "\"";
            }
            else
            {
                TagBuf.OpcTagName = TagBuf.TagName;
            }
            EmTagList.Add(TagBuf);
        }
        public string GetTagName(string name)
        {
            string retName = name;
            if (devprefix == true)
            {
                retName = "DefinitionRecords.IP_AnalogDef.\"" + name + "\"";
            }
            return retName;
        }


        public int GetDevId()
        {
            return this.devid;
        }

        public string GetDevDesc()
        {
            return this.devdescription;
        }
    }
    public class NMDev 
    {
        public string devdescription;

        public DataItem taginfo;
        public bool devprefix;
        public string devuint;
        public string devfac;
        public int devtype;

        public NMDev()
        {

        }
        public void setTagLable()
        {

            if (taginfo.TagName == null)
            {
            }
            if (devprefix == true)
            {
                taginfo.OpcTagName = "DefinitionRecords.IP_AnalogDef.\"" + taginfo.TagName + "\"";
            }
            else
            {
                taginfo.OpcTagName = taginfo.TagName;
            }
        }
    }

    public enum DATATYPE
    { 
        FLOAT = 1,
        LONG  = 2
    }
}
