﻿using System.Net.Http;
using System.Threading.Tasks;

namespace _2Facies
{
    public static class ServerClient
    {
        private static readonly HttpClient client = new HttpClient();

        private static readonly string url_login = $"{Request.Domain}/{Request.LoginRequestURL}";
        private static readonly string url_register = $"{Request.Domain}/{Request.RegisterRequestURL}";


        public static async Task<bool> ServerConnectionCheck()
        {
            try
            {
                var response = ServerClient.RequestGet($"{Request.Domain}/api/client/version");
                return await response != null;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<HttpContent> RequestPost(IPacket value, string postUrl)
        {
            var content = new FormUrlEncodedContent(value.Tuple());
            var response = await client.PostAsync(postUrl, content);

            return response.Content;
        }
        /*private static async Task<HttpResponseMessage> RequestPost(Dictionary<string, string> data, string postUrl, HttpClient client)
        {
            var content = new FormUrlEncodedContent(data);
            var response = await client.PostAsync(postUrl, content);
            return response;
        }*/
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
        public static async Task<string> Register(Packet.Register user)
        {
            string response = (await RequestPost(user, url_register)).ReadAsStringAsync().Result;
            return response;
        }
    }
}