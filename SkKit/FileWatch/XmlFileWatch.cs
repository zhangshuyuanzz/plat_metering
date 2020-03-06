using Base.kit;
using Common.log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SkKit.FileWatch
{
    public class XmlFileWatch
    {
        private static readonly NLOG logger = new NLOG("XmlFileWatch");
        public delegate void xmlFileSystemEventHandler(XmlFileWatch inhandle);

        public event xmlFileSystemEventHandler OnChanged;
        private FileSystemWatcher xmlwatcher;
        public Queue<string> ChangFileList = new Queue<string>();

        private LittleTools lthandle = new LittleTools();
        private System.Timers.Timer TriggerFileInfoTimer = new System.Timers.Timer(3000);

        public XmlFileWatch(string xmlpath)
        {
            xmlwatcher = new FileSystemWatcher();
            xmlwatcher.NotifyFilter = NotifyFilters.Size;
            xmlwatcher.Path = xmlpath;
            xmlwatcher.EnableRaisingEvents = true; //启动监控
            xmlwatcher.Filter = "*.xml";
            xmlwatcher.Changed += new FileSystemEventHandler(ChangedInfo);
            xmlwatcher.IncludeSubdirectories = false; //设置监控C盘目录下的所有子目录
            TriggerFileInfoTimer.Elapsed += new System.Timers.ElapsedEventHandler(timerInfoed);
            TriggerFileInfoTimer.AutoReset = false;
        }
        static void TimerAction(object obj)
        {
            Console.WriteLine("System.Threading.Timer {0:T}", DateTime.Now);
        }
        public  void ChangedInfo(object sender, FileSystemEventArgs e)
        {
            logger.Debug("------------------FullPath[{}]--name[{}]changetype[{}]", e.FullPath,e.Name,e.ChangeType);
            XmlKit testxml = new XmlKit();
            string Ip_str = e.Name;
            string xmpath = e.FullPath;
            if (testxml.isxml(xmpath) == false) {
                return;
            }
            string ip = Ip_str.Remove(Ip_str.Length - 4, 4);
            logger.Debug("ip-------------------[{}]-----------",ip);
            if (lthandle.LTJudgeIsIpv4(ip) == true) {
                lock (this)
                {
                    if (ChangFileList.Contains(ip) == false)
                    {
                        ChangFileList.Enqueue(ip);
                    }
                    stoptimer(TriggerFileInfoTimer);

                    starttimer(TriggerFileInfoTimer);
                }
            }

        }
        private void timerInfoed(object sender, System.Timers.ElapsedEventArgs e)
        {
            logger.Debug("---------------------timerInfoed---");
         //   stoptimer(TriggerFileInfoTimer);

                logger.Debug("now----invoke!!   Count[{}]", ChangFileList.Count);
                foreach (string str in ChangFileList)
                {
                    logger.Debug("str[{}]", str);
                }
                OnChanged?.Invoke(this);
        }
        public string GetandDeltQueue()
        {
            string oneip = null;
            lock (this)
            {
                if(ChangFileList.Count != 0)
                   oneip = ChangFileList.Dequeue();
            }
            return oneip;
        }
        private bool timerstatus = false;   //默认状态为停止--false
        private void stoptimer(System.Timers.Timer buffertimer)
        {
            buffertimer.Stop();
            timerstatus = false;
        }
        private void starttimer(System.Timers.Timer buffertimer)
        {
            buffertimer.Start();
            timerstatus = true;
        }
        private void resettimer(System.Timers.Timer buffertimer)
        {
            buffertimer.Stop();
            buffertimer.Start();
            timerstatus = true;
        }
        private bool gettimerstatus(System.Timers.Timer buffertimer)
        {
            return timerstatus;
        }
    }
}
