using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace _2Facies
{
    public static class ServerClient
    {
        private static readonly HttpClient client = new HttpClient();

        private static readonly string url_login = $"{Http.Request.Domain}/{Http.Request.LoginRequestURL}";
        private static readonly string url_register = $"{Http.Request.Domain}/{Http.Request.RegisterRequestURL}";


        public static async Task<bool> ServerConnectionCheck()
        {
            try
            {
                var response = ServerClient.RequestGet($"{Http.Request.Domain}/api/client/version");
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
        public static async Task<List<Packet.Room>> RoomList(int limit)
        {
            if (limit <= 0) return null;
            var raw = await (await ServerClient.RequestGet($"{Http.Request.Domain}/{Http.Request.RoomListURL}/{limit}")).ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Packet.Room>>(raw);
        }
        public static async Task<List<Packet.Room>> ConnectableRoomList()
        {
            var url = $"{Http.Request.Domain}/{Http.Request.ConnectableRoomListURL}";
            var rawlist = await (await ServerClient.RequestGet(url)).ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Packet.Room>>(rawlist);
        }
    }
}
