using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using WebSocketSharp;

namespace _2Facies
{
    public class WsClient
    {
        private readonly byte[] Alpha, Space;

        public static Packet.Room Room { get; private set; }
        [ThreadStatic]
        private static WebSocket socket = new WebSocket(Http.Request.SocketURL);
        private static Dictionary<string, EventHandler<MessageEventArgs>> events = new Dictionary<string, EventHandler<MessageEventArgs>>();
        public WsClient(Action<Packet.ErrorCode> ErrorHandler)
        {
            Alpha = Encoding.UTF8.GetBytes("@");
            Space = Encoding.UTF8.GetBytes(" ");

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

        public void Emit(Packet.Headers header, string room, byte[] message)
        {
            if (message == null)
            {
                message = Encoding.UTF8.GetBytes("");
            }
            var headerBytes = Encoding.UTF8.GetBytes(((int)header).ToString());
            var roomBytes = Encoding.UTF8.GetBytes(room);
            socket.Send(headerBytes.Combine(Space).Combine(roomBytes).Combine(Alpha).Combine(message));
        }
        public void Create(string roomTitle, int maxParticipants, Action<MessageEventArgs> dataReturn)
        {
            socket.Send($"{((int)Packet.Headers.Create).ToString()}@{roomTitle} {maxParticipants}");

            On("created", dataReturn);
            
        }
        public void Join(string roomName)
        {
            socket.Send($"{((int)Packet.Headers.Join).ToString()} {roomName}@");
            Room = new Packet.Room(roomName);
        }
        public void Leave(string roomName)
        {
            socket.Send($"{((int)Packet.Headers.Leave).ToString()} {roomName}@");
            Room = null;
        }
    }
}
