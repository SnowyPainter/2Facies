using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace _2Facies
{
    public static class ServerClient
    {
        private static readonly HttpClient client = new HttpClient();

        private static readonly string url_login = $"{RequestingUrls.Domain}/{RequestingUrls.LoginRequestURL}";

        

        public static async Task<HttpContent> RequestPost(IPacket value, string postUrl)
        {
            var content = new FormUrlEncodedContent(value.Tuple());
            var response = await client.PostAsync(postUrl, content);
            
            return response.Content;
        }
        private static async Task<HttpResponseMessage> RequestPost(Dictionary<string,string> data, string postUrl, HttpClient client)
        {
            var content = new FormUrlEncodedContent(data);
            var response = await client.PostAsync(postUrl, content);
            return response;
        }
        public static async Task<HttpContent> RequestGet(string getUrl)
        {
            var response = await client.GetAsync(getUrl);
            response.EnsureSuccessStatusCode();
            return response.Content;
        }
        public static async Task<HttpContent> RequestGet(string getUrl, HttpClient customClient)
        {
            var response = await customClient.GetAsync(getUrl);
            response.EnsureSuccessStatusCode();
            return response.Content;
        }

        public static async Task<string> Login(Packet.Login user)
        {
            string token = (await RequestPost(user, url_login)).ReadAsStringAsync().Result;
            return token;
        }

    }
}
