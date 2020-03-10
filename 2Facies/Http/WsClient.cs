using System;
using System.Collections.Generic;
using System.Windows;
using WebSocketSharp;

namespace _2Facies
{
    public class WsClient
    {
        public static Packet.Room Room { get; private set; }

        private static WebSocket socket = new WebSocket(Request.SocketURL);
        private List<Action<MessageEventArgs>> events;
        public WsClient(Action<Packet.ErrorCode> ErrorHandler)
        {
            events = new List<Action<MessageEventArgs>>();
            socket.Connect();

            On("error", (e) =>
            {
                int val;
                string codeStr = e.Data.Split('@')[1];
                var isInt = int.TryParse(codeStr, out val);
                var isDefined = Enum.IsDefined(typeof(Packet.ErrorCode), val);

                if (!isDefined || !isInt)
                {
                    ErrorHandler(Packet.ErrorCode.WrongCode);
                    return;
                }
                var code = (Packet.ErrorCode)val;
                switch (code)
                {
                    case Packet.ErrorCode.RoomJoin:
                        Room = null;
                        break;
                    case Packet.ErrorCode.RoomLeave:
                        Room = null;
                        break;
                }
                //exception handling
                ErrorHandler(code);
            });
        }
        public void On(string eventName, Action<MessageEventArgs> action)
        {
            if (events.Contains(action))
                return;
            socket.OnMessage += (sender, e) =>
            {
                if (e.Data.Split('@')[0] == eventName)
                    action(e);
            };
            events.Add(action);
        }
        public void Emit(string eventName, string message)
        {
            socket.Send($"{eventName}@{message}");
        }
        public void Emit(string eventName, string room, string message)
        {
            socket.Send($"{eventName} {room}@{message}");
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
