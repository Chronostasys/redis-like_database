using System;
using System.Net.Http;
using System.Threading.Tasks;
using ClientShared;

namespace DatabaseClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string uri = "http://127.0.0.1:8000";
            var client = new Client();
            while (true)
            {
                var s = Console.ReadLine();
                s = s.Replace("\\n", "\n");
                s = s.Replace("\\r", "\r");
                var re = await client.PostAsync(uri, s);
                Console.WriteLine(re);
            }
        }
    }
}
