using Base.kit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkKit.kit
{
    public interface INormalDev
    {
        List<DataItem> GetTagList();
        void AddTagList(DataItem TagBuf);
        string GetDevName();
        int GetDevId();
        string GetDevDesc();

        string GetTagName(string name);
    }
}
