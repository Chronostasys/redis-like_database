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
            string uri = "http://49.234.6.167:8000";
            var client = new Client();
            while (true)
            {
                var query = Console.ReadLine();
                var re = await client.PostAsync(uri, query);
                Console.WriteLine(re);
            }
        }
    }
}
