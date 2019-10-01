using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Web;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;

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
        HttpListener listener;
        Hashtable hashtable;
        FileStream file;
        StreamReader fileReader;
        StreamWriter fileWriter;
        /// <summary>
        /// Added 2019/10/1
        /// initialize the ip end point and DB
        /// </summary>
        public Server()
        {
            Console.WriteLine("Server Start!");
            file = File.Open("save.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fileReader = new StreamReader(file);
            fileWriter = new StreamWriter(file);
            hashtable = new Hashtable();
            listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8000/");
            listener.Start();
        }
        /// <summary>
        /// Added 2019/10/1
        /// Build the server
        /// </summary>
        /// <returns></returns>
        public async Task BuildServer()
        {
            await LoadFromFileAsync();
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
                    Save2FileAsync();
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
            await fileWriter.WriteAsync(JsonConvert.SerializeObject(hashtable));
            await fileWriter.FlushAsync();
        }
        public async ValueTask LoadFromFileAsync()
        {
            var h = JsonConvert.DeserializeObject<Hashtable>(await fileReader.ReadToEndAsync()); 
            if (h!=null)
            {
                hashtable =h;
            }
        }
    }
}
