using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Common
{
    public class SafeDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dict;
        public int Count
        {
            get
            {
                int ct;
                try
                {
                    ct = this._dict.Count;
                }
                finally
                {
                }
                return ct;
            }
        }
        public TValue this[TKey key]
        {
            get
            {
                TValue Item;
                TValue local2;
                try
                {
                    bool flag = !this._dict.ContainsKey(key);
                    if (flag)
                    {
                        Item = default(TValue);
                        return Item;
                    }
                    local2 = this._dict[key];
                }
                catch (KeyNotFoundException ex)
                {
                    local2 = default(TValue);
                }
                finally
                {
                }
                Item = local2;
                return Item;
            }
            set
            {
                try
                {
                    this._dict[key] = value;
                }
                finally
                {
                }
            }
        }
        public List<TKey> Keys
        {
            get
            {
                try
                {
                    var list = new List<TKey>();
                    Dictionary<TKey, TValue>.Enumerator enumerator = this._dict.GetEnumerator();
                    foreach (var pair in this._dict)
                        list.Add(pair.Key);
                    return list;
                }
                finally
                {
                }
            }
        }
        public List<TValue> Values
        {
            get
            {
                try
                {
                    var list = new List<TValue>();
                    foreach (var pair in this._dict)
                        list.Add(pair.Value);
                    return list;
                }
                finally
                {
                }
            }
        }
        public SafeDictionary()
        {
            this._dict = new Dictionary<TKey, TValue>();
        }
        public void Add(TKey key, TValue value)
        {
            try
            {
                this._dict.Add(key, value);
            }
            finally
            {
            }
        }
        public void Clear()
        {
            try
            {
                this._dict.Clear();
            }
            finally
            {
            }
        }
        public bool ContainsKey(TKey key)
        {
            bool flag;
            try
            {
                flag = this._dict.ContainsKey(key);
            }
            finally
            {
            }
            return flag;
        }
        public bool Remove(TKey key)
        {
            bool flag;
            try
            {
                flag = this._dict.Remove(key);
            }
            finally
            {
            }
            return flag;
        }
    }
}
