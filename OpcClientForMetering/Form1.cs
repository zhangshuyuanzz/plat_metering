using Base.kit;
using Common.log;
using SkKit;
using SkKit.kit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace OpcClientForMetering
{
    public partial class Form1 : Form
    {
        static public readonly NLOG logger = new NLOG("Form1");
        OpcSetConfig OpcSetCfg ;
        OpcClientMain OpcSetClientH = null;
        //OpcSetSqlite OpcSetSqlite;
         OpcSetOracle OpcSetOracleH;
        public Form1()
        {
            InitializeComponent();
            this.tagName.Width = this.listView1.Width * 7 / 16-10;
            this.tagValue.Width = this.listView1.Width * 4 / 16-10;
            this.tagTime.Width  = this.listView1.Width * 5 / 16-2;
            OpcSetCfg = new OpcSetConfig();
            OpcSetCfg.OpcSetConfigParseXml();
            OpcSetOracleH = new OpcSetOracle(OpcSetCfg.oracleIp, OpcSetCfg.oracleSerNm, OpcSetCfg.oracleUsr, OpcSetCfg.oraclePw);
            this.trueversion.Text = "V." + Application.ProductVersion;
            logger.Debug("Form1---finish!!");
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            logger.Debug("Form1_Load--OpcSetCfg.OpcIP[{}]", OpcSetCfg.OpcIP);
            OpcSetClientH = new OpcClientMain(OpcSetCfg.OpcIP, OpcSetCfg.OpcName);

            Thread scanThread = new Thread(new ThreadStart(OnlyOnceThread));
            scanThread.IsBackground = true;
            scanThread.Start();
        }
        void OnlyOnceThread()
        {
            this.OpcSetClientH.OpcClientMainRead(ref OpcSetCfg.DevListAll);

            var result3 = from v in OpcSetCfg.DevListAll orderby v.Key select v;

            List<string> AllTagList = new List<string>();//实时 订阅用
            List<string> AllBannerTagList = new List<string>();  //班量 订阅用

            this.listView1.BeginUpdate();
            foreach (KeyValuePair<string, NMDev> d in result3)
            {
                AllTagList.Add(d.Value.taginfo.OpcTagName);
                OpcClientInsetData(d.Value.taginfo);
            }
            List<NMDev>cutimelist =  OpcSetCfg.DevListAll.Values.ToList();
            OpcSetOracleH.OracleInsertListData(OpcSetCfg.cuTmTbl, cutimelist); //实时设备的tag 写到数据库

              this.OpcSetClientH.OpcClientMainRead(ref OpcSetCfg.DevBannerList);
            foreach (KeyValuePair<string, NMDev> h in OpcSetCfg.DevBannerList)
            {
                AllBannerTagList.Add(h.Value.taginfo.OpcTagName);
            }
            List<NMDev> bantimelist = OpcSetCfg.DevBannerList.Values.ToList();
            OpcSetOracleH.OracleInsertListData(OpcSetCfg.bannerTbl, bantimelist); //班量的tag 写到数据库

            this.listView1.EndUpdate();

            string[] mmmsf = AllTagList.ToArray();
            string[] bannerzhu = AllBannerTagList.ToArray();
            OpcSetClientH.OpcClientMainSubscription(mmmsf,"realtime");
            OpcSetClientH.OpcClientMainSubscription(bannerzhu, "banner");

            this.OpcSetClientH.OpcSetTagChanged += new SkKit.kit.TagEventHandler(OpcClientChangeTagNew);
        }
        void OpcClientChangeTagNew(List<DataItem> Taglist)
        {
            logger.Debug("get opc server Taglist---count[{}]", Taglist.Count);
            List<DataItem> RealList = new List<DataItem>();
            List<DataItem> BanList = new List<DataItem>();

            foreach (DataItem tl in Taglist)
            {
                logger.Debug("OpcClientChangeTagNew--GroupName[{}]TagName[{}]", tl.GroupName, tl.TagName);
                if (tl.GroupName == "realtime") {
                    DataItem newtag = new DataItem();
                    newtag.OpcTagName = tl.TagName;
                    newtag.Value = tl.Value;
                    newtag.DataTime = tl.DataTime;
                    newtag.Quality = tl.Quality;
                    OpcSetUpdateTag(newtag);
                    RealList.Add(newtag);
                }
                else if (tl.GroupName == "banner")
                {
                    DataItem newtag = new DataItem();
                    newtag.OpcTagName = tl.TagName;
                    newtag.Value = tl.Value;
                    newtag.DataTime = tl.DataTime;
                    newtag.Quality = tl.Quality;
                    BanList.Add(newtag);
                }
            }
          //  this.OpcSetSqlite.OpcSetUpdateTag("RealTimeOpc", RealList);
           // this.OpcSetSqlite.OpcSetUpdateTag("BannerOpc", BanList);

        }
        void OpcSetUpdateTag(DataItem OTag)
        {
            ListViewItem listview = this.listView1.FindItemWithText(OTag.OpcTagName);
            if (listview != null)
            {
                this.listView1.Items[listview.Index].SubItems[2].Text = OTag.Value.ToString();
                this.listView1.Items[listview.Index].SubItems[3].Text = OTag.DataTime;
                logger.Debug("update win-Indexe[{}]opc client name[{}]value[{}] time [{}]", listview.Index, OTag.OpcTagName, OTag.Value, OTag.DataTime);
            }
            else
            {
                logger.Debug("listview find none");
            }
        }
        private void Form1Closed(object sender, EventArgs e)
        {
            logger.Debug("win_closed!!");
            System.Environment.Exit(System.Environment.ExitCode);
            this.Dispose();
            this.Close();
        }
        void OpcClientInsetData(DataItem onett)
        {
            ListViewItem li = new ListViewItem();
            li.Text = onett.OpcTagName;
            li.SubItems.Add(onett.TagName);
            li.SubItems.Add(onett.Value.ToString());
            li.SubItems.Add(onett.DataTime);

            this.listView1.Items.Add(li);
            logger.Debug("OpcClient  display window---name[{}]Value[{}]", onett.TagName, onett.Value);
        }
        private void AddItem_Click(object sender, EventArgs e)
        {
            logger.Debug("AddItem_Click  [{}]",this.inputitem.Text);
            //  testinsertdata();
        }

        private void Inputitem_TextChanged(object sender, EventArgs e)
        {

        }
        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            logger.Debug("ListView1_MouseDoubleClick");
            if (this.listView1.SelectedIndices.Count == 0)
            {
                logger.Debug("none select item");
                return;
            }
            try
            {
                ListViewItem sectag =  this.listView1.SelectedItems[0];
                logger.Debug("-index[{}]--Text[{}]", this.listView1.SelectedIndices[0], sectag.Text);
                this.DelInputItem.Text = sectag.Text;
            }
            catch (Exception ex)
            {
                logger.Debug("error [{}]", ex.ToString());
            }
        }
        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedIndices.Count == 0) {
                logger.Debug("none select item");
                return;
            }
            try
            {
                logger.Debug("-index[{}]--", this.listView1.SelectedIndices[0]);
            }
            catch (Exception ex)
            {
                logger.Debug("error [{}]",ex.ToString());
            }

        }
        void OpcSetFindTagIndex(string tagname)
        {
            logger.Debug("OpcSetFindTagIndex--[{}]",tagname);
            ListViewItem onetag = this.listView1.FindItemWithText(tagname);
            if (onetag == null) {
                MessageBox.Show("无此条目，请再次检查！！","WARN", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                return;
            }
            logger.Debug("Index--[{}]", onetag.Index);
            if (onetag.Index >= 1) {
                this.listView1.Items.RemoveAt(onetag.Index);
            }
        }
        private void DelitemBtn_Click(object sender, EventArgs e)
        {
            try
            {
                logger.Debug("DelitemBtn_Click");
                if (this.DelInputItem.Text.Length == 0 | this.DelInputItem.Text == null)
                {
                    return;
                }
                OpcSetFindTagIndex(this.DelInputItem.Text);
            }
            catch (Exception ex)
            {
                logger.Debug("error insert [{}]",ex.ToString());
            }
        }

        private void ListView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }
    }
}
