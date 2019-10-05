using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClientShared
{
    /// <summary>
    /// added 2019/10/3
    /// </summary>
    public class Client
    {
        string uri;
        public void SetUri(string _uri)
        {
            uri = _uri;
        }
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
        public async Task<string> PostAsync(string msg)
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
