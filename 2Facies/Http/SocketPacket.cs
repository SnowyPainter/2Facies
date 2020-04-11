using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2Facies.Socket
{
    public interface ISendPacket
    {
        Headers Header { get; }
        string RoomId { get; }
        string UserId { get; }
        byte[] ToPacket();
    }
    public interface IReceivePacket
    {
        string Caster { get; }
        string Body { get; }
    }
    public enum Headers
    {
        Error = 0,
        Join = 11,
        Leave = 12,
        Create = 13,
        Broadcast = 21,
        BroadcastAudio = 22,
        Participants = 23,
    }
    public static class SocketPacket
    {
        public class Receive: IReceivePacket
        {
            public string Caster { get; private set; }
            public string Body { get; private set; }
            public Receive(string caster, string body)
            {
                Caster = caster;
                Body = body;
            }
        }

        public class Broadcast : ISendPacket
        {
            public string RoomId { get; private set; }      
            public string UserId { get; private set; }
            public Headers Header { get; private set; }
            public string Body { get; private set; }
            public Broadcast(string roomId, string userId,string message)
            {
                if(message == null)
                    message = "";
                if(roomId == null)
                    throw new Exception("RoomID Must be not to be NULL");

                Header = Headers.Broadcast;
                RoomId = roomId;
                UserId = userId;
                Body = message;
            }
            
            public byte[] ToPacket()
            {
                var packet = $"{Header.ToStringValue()} {RoomId}@{UserId}@{Body}";
                return Encoding.UTF8.GetBytes(packet);
            }
        }
        public class BroadcastAudio : ISendPacket
        {
            public string RoomId { get; private set; }
            public string UserId { get; private set; }
            public Headers Header { get; private set; }
            public byte[] Audio { get; private set; }
            public BroadcastAudio(string targetRoomId,string userId, byte[] audio)
            {
                if (audio == null)
                {
                    throw new Exception("Audio Must be not to be NULL");
                }
                if (targetRoomId == null)
                {
                    throw new Exception("RoomID Must be not to be NULL");
                }
                Header = Headers.BroadcastAudio;
                RoomId = targetRoomId;
                UserId = userId;
                Audio = audio;
            }

            public byte[] ToPacket()
            {
                var packet = $"{Header.ToStringValue()} {RoomId}@{UserId}@{Convert.ToBase64String(Audio)}";
                return Encoding.UTF8.GetBytes(packet);
            }
        }
        public class Participants : ISendPacket
        {
            public string RoomId { get; private set; }
            public string UserId { get; private set; }
            public Headers Header { get; private set; }
            public Participants(string targetRoomId, string userId)
            {
                if (targetRoomId == null)
                {
                    throw new Exception("RoomID Must be not to be NULL");
                }
                Header = Headers.Participants;
                RoomId = targetRoomId;
                UserId = userId;
            }

            public byte[] ToPacket()
            {
                var packet = $"{Header.ToStringValue()} {RoomId}@{UserId}@";
                return Encoding.UTF8.GetBytes(packet);
            }
        }
        public class CreateRoom
        {
            public string Title { get; private set; }
            public int MaxParticipants { get; private set; }
            public Headers Header { get; private set; }
            public CreateRoom(string title, int maxParticipants)
            {
                if (title == null)
                {
                    title = "";
                }
                Header = Headers.Create;
                Title = title;
                MaxParticipants = maxParticipants;
            }

            public byte[] ToPacket()
            {
                var packet = $"{Header.ToStringValue()}@@{Title} {MaxParticipants}";
                return Encoding.UTF8.GetBytes(packet);
            }
        }
        public class Join : ISendPacket
        {
            public Headers Header { get; private set; }
            public string RoomId { get; private set; }
            public string UserId { get; private set; }
            public Join(string room, string joiner)
            {
                Header = Headers.Join;
                RoomId = room;
                UserId = joiner;
            }
            public byte[] ToPacket()
            {
                var packet = $"{Header.ToStringValue()} {RoomId}@{UserId}@";
                return Encoding.UTF8.GetBytes(packet);
            }
        }
        public class Leave : ISendPacket
        {
            public Headers Header { get; private set; }
            public string RoomId { get; private set; }
            public string UserId { get; private set; }
            public Leave(string room, string leaver)
            {
                Header = Headers.Leave;
                RoomId = room;
                UserId = leaver;
            }
            public byte[] ToPacket()
            {
                var packet = $"{Header.ToStringValue()} {RoomId}@{UserId}@";
                return Encoding.UTF8.GetBytes(packet);
            }
        }

        public class Literal:ISendPacket
        {
            public Headers Header { get; private set; }
            public string UserId { get; private set; }
            public string RoomId { get; private set; }
            public List<string> Content { get; private set; }
            public Literal(Headers header)
            {
                Header = header;
                Content = new List<string>();
                RoomId = "";
            }
            public void SetRoom(string roomId)
            {
                RoomId = roomId;
            }
            public void AddContent(string content)
            {
                Content.Add(content);
            }

            public byte[] ToPacket()
            {
                string contents = " ";
                foreach(var c in Content)
                {
                    contents += $"{c} ";
                }
                contents = contents.Substring(0, contents.Length - 1);

                string constant = $"{Header.ToStringValue()}{(RoomId != "" ? " " + RoomId : "")}@@{contents}";
                return Encoding.UTF8.GetBytes(constant);
            }
        }
    }
}
