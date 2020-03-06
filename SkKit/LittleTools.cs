using Common.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkKit
{
    public class LittleTools
    {
        static private NLOG logger = new NLOG("LittleTools");
        public bool LTJudgeIsIpv4(string IPstr)
        {
            string ipreg = @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}";
            if (System.Text.RegularExpressions.Regex.IsMatch(IPstr, ipreg))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string ByteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr +=  "0x" +bytes[i].ToString("X2") + ",";
                }
            }
            return returnStr;
        }
    }
    public class LTMap<K, T> : Dictionary<K, T>
    {
        public bool LtContains(K name)
        {
            lock (this)
            {
                if (ContainsKey(name) == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
