using System;
using System.Net.Http;
using System.Threading.Tasks;

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
                var query = Console.ReadLine();
                var re = await client.PostAsync(uri, query);
                Console.WriteLine(re);
            }
        }
    }
    /// <summary>
    /// added 2019/10/3
    /// </summary>
    public class Client
    {
        public Client()
        {
            HttpClient = new HttpClient();
        }
        public HttpClient HttpClient { get; }
        public async Task<string> PostAsync(string uri, string msg)
        {
            var c = new StringContent(msg);
            var response = await HttpClient.PostAsync(uri, c);
            Console.WriteLine("Posted!");
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetAsync(string uri)
        {
            return await HttpClient.GetStringAsync(uri);
        }
    }
}
