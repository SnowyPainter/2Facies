using System.Collections.Generic;
using System.Windows;

namespace _2Facies
{
    public interface IPacket
    {
        Dictionary<string, string> Tuple();
    }
    public interface IPublicData
    {
        string Id { get; set; }
    }
    public static class Packet
    {
        public static Dictionary<string, int> MaxLength = new Dictionary<string, int>()
        {
            {"id", 20 }, {"password", 25}, {"name", 20}, {"email", 320}
        };
        public static Dictionary<string, string> CookiesName = new Dictionary<string, string>()
        {
            {"login","signin" }, {"register", "signup"}
        };

        public class Login : IPacket
        {
            public Login() { }
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
                    {"id", Id } , {"password", Password},
                };

                return tuple;
            }
        }
        public class Register : IPacket
        {
            public Register() { }
            public Register(string id, string password, string name, string email, int age)
            {
                Id = id; Password = password;
                Name = name; Email = email; Age = age;
            }

            public string Id { get; set; }
            public string Password { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public int Age { get; set; }

            public Dictionary<string, string> Tuple()
            {
                var tuple = new Dictionary<string, string>()
                {
                    {"id", Id } , {"password", Password},
                    {"name", Name }, {"email", Email}, {"age", Age.ToString()}
                };

                return tuple;
            }
            public void Bind(Dictionary<string, string> data)
            {
                Id = data["id"];
                Password = data["password"];
                Name = data["name"];
                Email = data["email"];
                Age = int.Parse(data["age"]);
            }
        }

        public class DataPublic : IPacket, IPublicData
        {
            public DataPublic() { }
            public DataPublic(string id)
            {
                Id = id;
            }
            public DataPublic(string id, string name, string email):this(id)
            {
                Name = name; Email = email;
            }

            public string Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public int Age { get; set; }

            public Dictionary<string,string> Tuple()
            {
                return new Dictionary<string, string>()
                {
                    {"userId", Id }, {"name", Name}, {"email", Email}, {"age", Age.ToString()}
                };
            }
            public void Bind(Dictionary<string, string> jsonData)
            {
                Id = jsonData["userId"];
                Name = jsonData["name"];
                Email = jsonData["email"];
                Age = int.Parse(jsonData["age"]);
            }
        }

    }
}
