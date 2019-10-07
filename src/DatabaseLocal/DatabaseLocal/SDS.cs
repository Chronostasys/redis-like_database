using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseLocal
{
    class SDS
    {
        sds mysds;
        struct sds
        {
            public char[] s;
            public int len;
            public int free;
            public sds(int _s, int _len)
            {
                int _free = _s - _len;
                s = new char[_s];
                len = _len;
                free = _free;
            }
        }
        public int Length
        {
            get { return mysds.len; }
        }
        public int Free
        {
            get { return mysds.free; }
        }
        public SDS(string s):this(0, s)
        {

        }
        /// <summary>
        /// 初始化SDS
        /// </summary>
        /// <param name="len">总分配空间长</param>
        /// <param name="s">字符串</param>
        public SDS(int len, string s)
        {
            if (len>s.Length)
            {
                mysds = new sds(len, s.Length);
            }
            else
            {
                mysds = new sds(s.Length * 2, s.Length);
            }
            Set(s);
        }
        /// <summary>
        /// 设置string值
        /// </summary>
        /// <param name="s"></param>
        public void Set(string s)
        {
            if (mysds.s.Length<s.Length)
            {
                mysds.s = new char[s.Length * 2];
            }
            mysds.len = s.Length;
            mysds.free = mysds.s.Length - s.Length;
            for (int i = 0; i < s.Length; i++)
            {
                mysds.s[i] = s[i];
            }
        }
        /// <summary>
        /// 转换为标准string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new string(mysds.s);
        }
        public void Append(string s)
        {
            if (mysds.free < s.Length)
            {
                var copy = mysds.s;
                mysds.s = new char[(s.Length+mysds.len) * 2];
                for (int i = 0; i < copy.Length; i++)
                {
                    mysds.s[i] = copy[i];
                }
            }
            var len = mysds.len;
            mysds.len = s.Length + mysds.len;
            mysds.free = mysds.s.Length - mysds.len;
            for (int i = 0; i < s.Length; i++)
            {
                mysds.s[len + i] = s[i];
            }
        }
    }
}
