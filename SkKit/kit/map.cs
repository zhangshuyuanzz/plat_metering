using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Base.kit
{
    public class R : R1<string, object>
    {
        public int ErrorCode { get { return (int)this["error_code"]; } }
        public R(int eCode, string msg)
        {
            this.Add("error_code", eCode);
            this.Add("message", msg);
        }
        public R(int eCode) : this(eCode, "")
        {
        }
        public R()
        {
        }
    }

    public class Map<K, T> : Dictionary<K, T>
    {
        public T Get(K name)
        {
            if (ContainsKey(name))
            {
                return this[name];
            }

            return default(T);
        }

        public void Add0(K k , T t)
        {
            this[k] = t;
        }
        public T Remove0(K name)
        {
            T result = Get(name);
            Remove(name);
            return result;
        }
    }

    public class R1<K, T> : Dictionary<K, T>
    {
        public void Add0(K k, T t)
        {
            this[k] = t;
        }

        public T Get(K name, T def = default(T))
        {
            if (ContainsKey(name))
            {
                return this[name];
            }

            return def;
        }
        public int GetInt(K name)
        {
            return Int32.Parse(GetStr(name).ToString());
        }

        public string GetStr(K name)
        {
            T str = Get(name);
            if (str == null)
            {
                return null;
            }

            return str.ToString();
        }
    }

    public class M
    {
        public string Name { get; set; }
        
        public R1<string, string> Attribute { get; set; }

        public R1<string, List<M>> Children { get; set; }
    }

    public class ListR1<K, V> : R1<K, List<V>>
    {
        public List<V> Get(K name)
        {
            if (ContainsKey(name))
            {
                return this[name];
            }
            List<V> res = new List<V>();
            this[name] = res;
            return res;
        }
    }
}
