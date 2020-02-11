using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2Facies
{
    public interface IPacket
    {
        Dictionary<string, string> Tuple();
    }
    public static class Packet
    {
        public static Dictionary<string, int> MaxLength = new Dictionary<string, int>()
        {
            {"Id", 20 }, {"Password", 25}, {"Name", 20}, {"Email", 320}
        };
        public static Dictionary<string, string> CookiesName = new Dictionary<string, string>()
        {
            {"login","signin" }, {"register", "signup"}
        };

        public class Login:IPacket
        {
            public Login(string id, string password)
            {
                Id = id; Password = password;
            }

            public string Id { get; set; }
            public string Password { get; set; }

            public Dictionary<string, string> Tuple()
            {
                var tuple = new Dictionary<string, string>()
                {
                    {"Id", Id } , {"Password", Password},
                };

                return tuple;
            }
        }
        public class Register:IPacket
        {
            public Register(string id, string password, string name, string email)
            {
                Id = id; Password = password;
                Name = name; Email = email;
            }

            public string Id { get; set; }
            public string Password { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }

            public Dictionary<string, string> Tuple()
            {
                var tuple = new Dictionary<string,string>()
                {
                    {"Id", Id } , {"Password", Password},
                    {"Name", Name }, {"Email", Email}
                };

                return tuple;
            }
        }

    }
}
