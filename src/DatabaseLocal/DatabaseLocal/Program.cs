using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace DatabaseLocal
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Server server = new Server();
            while (true)
            {
                await server.BuildServer();
            }
        }
    }
    /// <summary>
    /// 2019/10/1 Added
    /// </summary>
    class Server
    {
        BinaryFormatter binaryFormatter;
        HttpListener listener;
        Hashtable hashtable;
        //FileStream file;
        //StreamReader fileReader;
        //StreamWriter fileWriter;
        string path = "save.txt";
        /// <summary>
        /// Added 2019/10/1
        /// initialize the ip end point and DB
        /// </summary>
        public Server()
        {
            binaryFormatter = new BinaryFormatter();
            Console.WriteLine("Server Start!");
            //file = File.Open("save.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //fileReader = new StreamReader(file);
            //fileWriter = new StreamWriter(file);
            hashtable = new Hashtable();
            listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8000/");
            listener.Start();
            LoadFromFileAsync();
        }
        /// <summary>
        /// Added 2019/10/1
        /// Build the server
        /// </summary>
        /// <returns></returns>
        public async Task BuildServer()
        {
            var context = await listener.GetContextAsync();
            var inStream = context.Request.InputStream;
            var outStream = context.Response.OutputStream;
            var reader = new StreamReader(inStream);
            var s = reader.ReadToEnd();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"receive:{s}");
            var strs = s.Split(' ');
            if (strs[0] == "set")
            {
                if (strs.Length != 3)
                {
                    ResponseAsync(outStream, " Illegal Input");
                }
                else
                {
                    hashtable.Add(strs[1], strs[2]);
                    ResponseAsync(outStream, "ok!");
                    await Save2FileAsync();
                }
            }
            else if (strs.Length != 2)
            {
                ResponseAsync(outStream, " Illegal Input");
            }
            else
            {
                if (strs[0] == "delete")
                {
                    hashtable.Remove(strs[1]);
                    ResponseAsync(outStream, "ok!");
                    Save2FileAsync();
                }
                else if (strs[0] == "get")
                {
                    var re = hashtable[strs[1]];
                    try
                    {
                        await ResponseAsync(outStream, (string)re);
                    }
                    catch (Exception)
                    {

                        await ResponseAsync(outStream, "Null!");
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// Added 2019/10/1
        /// Response the requests
        /// </summary>
        /// <param name="stream">The response stream</param>
        /// <param name="s">Response message</param>
        /// <returns></returns>
        public async Task ResponseAsync(Stream stream, string s)
        {
            var writer = new StreamWriter(stream);
            await writer.WriteLineAsync(s);
            await writer.FlushAsync();
            stream.DisposeAsync();
        }
        public async ValueTask Save2FileAsync()
        {
            lock (this)
            {
                File.WriteAllText(path, hashtable.GetString());
            }
        }
        public async ValueTask LoadFromFileAsync()
        {
            //var h = JsonConvert.DeserializeObject<Hashtable>(await fileReader.ReadToEndAsync());
            try
            {
                lock (this)
                {
                    hashtable.FromString(File.ReadAllText(path));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
    [Serializable]
    public class Hashtable
    {
        [Serializable]
        private struct bucket
        {
            public Object key;
            public Object val;
            public int hash_coll;
        }
        private bucket[] buckets; //存储哈希表数据的数组（数据桶）
        private int count; //元素个数
        private int loadsize; //当前允许存储的元素个数
        private float loadFactor; //填充因子

        public Hashtable() : this(0, 1.0f) { }


        /// <summary>
        /// 抄袭微软的默认设定，最大容量设0.72
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="loadFactor"></param>
        public Hashtable(int capacity, float loadFactor)
        {
            if (!(loadFactor >= 0.1f && loadFactor <= 1.0f))
                throw new ArgumentOutOfRangeException(
                "填充因子必须在0.1～1之间");
            this.loadFactor = loadFactor > 0.72f ? 0.72f : loadFactor;
            double rawsize = capacity / this.loadFactor;
            int hashsize = (rawsize > 11) ? HashHelpers.GetPrime((int)rawsize) : 11;
            buckets = new bucket[hashsize];
            loadsize = (int)(this.loadFactor * hashsize);
        }
        public virtual void Add(Object key, Object value)
        {
            Insert(key, value, true);
        }
        /// <summary>
        /// 这个方法中，公式抄微软的h(x,i)=h1(x)+i*h2(x)
        /// 其中h2与hashsize互质，且要求hashsize为质数，以保证每次能遍历哈希表
        /// 所以hashhelper中记录了很多质数，用来提供hashsize
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashsize"></param>
        /// <param name="seed"></param>
        /// <param name="incr"></param>
        /// <returns></returns>
        private uint InitHash(Object key, int hashsize, out uint seed, out uint incr)
        {
            uint hashcode = (uint)GetHash(key) & 0x7FFFFFFF;
            seed = (uint)hashcode; //h1
            incr = (uint)(1 + (((seed >> 5) + 1) % ((uint)hashsize - 1)));
            return hashcode;
        }
        public virtual Object this[Object key]
        {
            get
            {
                uint seed; //h1
                uint incr; //h2
                uint hashcode = InitHash(key, buckets.Length,
   out seed, out incr);
                int ntry = 0;
                bucket b;
                int bn = (int)(seed % (uint)buckets.Length);
                do
                {
                    b = buckets[bn];
                    if (b.key == null)
                    {
                        return null;
                    }
                    if (((b.hash_coll & 0x7FFFFFFF) == hashcode) &&
                    KeyEquals(b.key, key))
                    {
                        return b.val;
                    }
                    bn = (int)(((long)bn + incr) %
                    (uint)buckets.Length);
                } while (b.hash_coll < 0 && ++ntry < buckets.Length);
                return null;
            }
            set
            {
                Insert(key, value, false);
            }
        }
        /// <summary>
        /// 长度不够时，扩张为一个近似两倍的质数
        /// </summary>
        private void Expand()
        {
            int rawsize = HashHelpers.GetPrime(buckets.Length * 2);
            Rehash(rawsize);
        }
        /// <summary>
        /// 扩张时hash会变，所以重新录入一遍
        /// </summary>
        /// <param name="newsize"></param>
        private void Rehash(int newsize)
        {
            bucket[] newBuckets = new bucket[newsize];
            for (int nb = 0; nb < buckets.Length; nb++)
            {
                bucket oldb = buckets[nb];
                if ((oldb.key != null) && (oldb.key != buckets))
                {
                    PutEntry(newBuckets, oldb.key, oldb.val,
                    oldb.hash_coll & 0x7FFFFFFF);
                }
            }
            buckets = newBuckets;
            loadsize = (int)(loadFactor * newsize);
            return;
        }
        /// <summary>
        /// 在Rehash里用的
        /// </summary>
        /// <param name="newBuckets"></param>
        /// <param name="key"></param>
        /// <param name="nvalue"></param>
        /// <param name="hashcode"></param>
        private void PutEntry(bucket[] newBuckets, Object key,Object nvalue, int hashcode)
        {
            uint seed = (uint)hashcode; //h1
            uint incr = (uint)(1 + (((seed >> 5) + 1) %((uint)newBuckets.Length - 1)));
            int bn = (int)(seed % (uint)newBuckets.Length);
            do
            {
                if ((newBuckets[bn].key == null) || (newBuckets[bn].key == buckets))
                {
                    newBuckets[bn].val = nvalue;
                    newBuckets[bn].key = key;
                    newBuckets[bn].hash_coll |= hashcode;
                    return;
                }

                if (newBuckets[bn].hash_coll >= 0)
                {
                    newBuckets[bn].hash_coll |=
                        unchecked((int)0x80000000);
                }
                //多加一个h2
                bn = (int)(((long)bn + incr) % (uint)newBuckets.Length);
            } while (true);
        }
        /// <summary>
        /// 就是封装一下
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual int GetHash(Object key)
        {
            return key.GetHashCode();
        }
        /// <summary>
        /// 判断key是否相等
        /// 在Remove、Add里用到
        /// </summary>
        /// <param name="item"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual bool KeyEquals(Object item, Object key)
        {
            return item == null ? false : item.Equals(key);
        }

        /// <summary>
        /// 当add为true时用作添加元素，当add为false时用作修改元素值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="nvalue"></param>
        /// <param name="add"></param>
        private void Insert(Object key, Object nvalue, bool add)
        {   
            //如果超过允许存放元素个数的上限则扩容
            if (count >= loadsize)
            {
                Expand();
            }
            uint seed; //h1
            uint incr; //h2
            uint hashcode = InitHash(key, buckets.Length, out seed, out incr);
            int ntry = 0;
            int emptySlotNumber = -1;
            int bn = (int)(seed % (uint)buckets.Length);

            do
            {   
                
                //出现冲突
                if (emptySlotNumber == -1 && (buckets[bn].key == buckets) &&
                        (buckets[bn].hash_coll < 0))
                {
                    emptySlotNumber = bn;
                }
                if (buckets[bn].key == null)
                {
                    if (emptySlotNumber != -1)
                        bn = emptySlotNumber;
                    buckets[bn].val = nvalue;
                    buckets[bn].key = key;
                    buckets[bn].hash_coll |= (int)hashcode;
                    count++;
                    return;
                }
                //找到重复键
                if (((buckets[bn].hash_coll & 0x7FFFFFFF) == hashcode) &&
                    KeyEquals(buckets[bn].key, key))
                {
                    if (add)
                    {
                        throw new ArgumentException("添加了重复的键值！");
                    }
                    buckets[bn].val = nvalue;
                    return;
                }
                //存在冲突则置hash_coll的最高位为1
                if (emptySlotNumber == -1)
                {
                    if (buckets[bn].hash_coll >= 0)
                    {
                        buckets[bn].hash_coll |= unchecked((int)0x80000000);
                    }
                }
                //再加一个h2
                bn = (int)(((long)bn + incr) % (uint)buckets.Length);
            } while (++ntry < buckets.Length);
            throw new InvalidOperationException("添加失败！");
        }
        /// <summary>
        /// 移除元素
        /// </summary>
        /// <param name="key"></param>
        public virtual void Remove(Object key)
        {
            uint seed; //h1
            uint incr; //h2
            uint hashcode = InitHash(key, buckets.Length, out seed, out incr);
            int ntry = 0;
            bucket b;
            int bn = (int)(seed % (uint)buckets.Length);
            do
            {
                b = buckets[bn];
                if (((b.hash_coll & 0x7FFFFFFF) == hashcode) && KeyEquals(b.key, key)) //如果找到相应的键值
                {   
                    //保留最高位，其余清0
                    buckets[bn].hash_coll &= unchecked((int)0x80000000);
                    //如果原来存在冲突
                    if (buckets[bn].hash_coll != 0)
                    {   
                        //使key指向buckets
                        //个人理解：因为使用者不可能添加等于buckets的key
                        buckets[bn].key = buckets;
                    }
                    else
                    {
                        //原来不存在冲突
                        //置key为空
                        buckets[bn].key = null;
                    }
                    //设val为空
                    buckets[bn].val = null;
                    count--;
                    return;
                }
                //再加一个h2
                bn = (int)(((long)bn + incr) % (uint)buckets.Length);
            } while (b.hash_coll < 0 && ++ntry < buckets.Length);
        }
        public override string ToString()
        {
            string s = string.Empty;
            for (int i = 0; i < buckets.Length; i++)
            {
                if (buckets[i].key != null && buckets[i].key != buckets)
                {   
                    //不为空位时打印索引、键、值、hash_coll
                    s += string.Format("{0,-5}{1,-8}{2,-8}{3,-8}rn",
                        i.ToString(), buckets[i].key.ToString(),
                        buckets[i].val.ToString(),
                        buckets[i].hash_coll.ToString());
                }
                else
                {   
                    //是空位时则打印索引和hash_coll
                    s += string.Format("{0,-21}{1,-8}rn", i.ToString(),
                                        buckets[i].hash_coll.ToString());
                }
            }
            return s;
        }
        /// <summary>
        /// 序列化（仅支持key和val为string类型）
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            string a = "";
            foreach (var item in buckets)
            {
                if (item.val!=null)
                {
                    a += item.key.ToString();
                    a += " ";
                    a += item.val.ToString();
                    a += " ";
                }
            }
            return a;
        }
        /// <summary>
        /// 反序列化（仅支持key和val为值类型）
        /// </summary>
        /// <param name="origin"></param>
        public void FromString(string origin)
        {
            
            var s = origin.Split(" ");
            for (int i = 0; i < s.Length/2; i++)
            {
                this.Add(s[2 * i], s[2 * i + 1]);
            }
        }
        public virtual int Count
        {   
            //获取元素个数
            get { return count; }
        }
    }
}
