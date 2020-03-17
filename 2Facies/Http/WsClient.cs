using System;
using System.Collections.Generic;
using System.Windows;
using WebSocketSharp;

namespace _2Facies
{
    public class WsClient
    {
        public static Packet.Room Room { get; private set; }
        [ThreadStatic]
        private static WebSocket socket = new WebSocket(Request.SocketURL);
        private static Dictionary<string, EventHandler<MessageEventArgs>> events = new Dictionary<string, EventHandler<MessageEventArgs>>();
        public WsClient(Action<Packet.ErrorCode> ErrorHandler)
        {

            socket.Connect();

            On("error", (e) =>
            {
                int codeValue;

                if (!int.TryParse(e.Data.Split('@')[1], out codeValue) || !Enum.IsDefined(typeof(Packet.ErrorCode), codeValue))
                {
                    ErrorHandler(Packet.ErrorCode.WrongCode);
                    return;
                }

                Packet.ErrorCode code = (Packet.ErrorCode)codeValue;

                switch (code)
                {
                    case Packet.ErrorCode.RoomJoin:
                        Room = null;
                        break;
                    case Packet.ErrorCode.RoomLeave:
                        Room = null;
                        break;
                }
                //Pass error handler
                ErrorHandler(code);
            });
        }
        public void On(string eventName, Action<MessageEventArgs> action)
        {
            if(events.ContainsKey(eventName))
            {
                socket.OnMessage -= events[eventName];
                events.Remove(eventName);
            }

            events.Add(eventName, (sender, e) =>
            {
                if (e.Data.Split('@')[0] == eventName)
                    action(e);
            });

            socket.OnMessage += events[eventName];
            
        }
        public void Emit(string eventName, string message)
        {
            socket.Send($"{eventName}@{message}");
        }
        public void Emit(string eventName, string room, string message)
        {
            socket.Send($"{eventName} {room}@{message}");
        }
        public void Create(string roomTitle, int maxParticipants, Action<MessageEventArgs> dataReturn)
        {
            socket.Send($"create@{roomTitle} {maxParticipants}");

            On("created", dataReturn);
            
        }
        public void Join(string roomName)
        {
            socket.Send($"join {roomName}@");
            Room = new Packet.Room(roomName);
        }
        public void Leave(string roomName)
        {
            socket.Send($"leave {roomName}@");
            Room = null;
        }
    }
}
