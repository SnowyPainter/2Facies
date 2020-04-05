using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using WebSocketSharp;

namespace _2Facies
{
    public class WsClient
    {
        public static Packet.Room Room { get; private set; }
        [ThreadStatic]
        private static WebSocket socket = new WebSocket(Http.Request.SocketURL);
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

        public void Emit(IRoomSockPacket packet)
        {
            socket.Send(packet.ToPacket());
        }

        public void Create(string roomTitle, int maxParticipants, Action<MessageEventArgs> createRoomResultHandler)
        {
            var crp = new SocketPacket.CreateRoom(roomTitle, maxParticipants);
            socket.Send(crp.ToPacket());

            On(Packet.Headers.Create.ToStringValue(), createRoomResultHandler);
            
        }
        public void Join(string roomName)
        {
            SocketPacket.Literal l = new SocketPacket.Literal(Packet.Headers.Join);
            l.SetRoom(roomName);
            
            socket.Send(l.ToPacket());
            Room = new Packet.Room(roomName);
        }
        public void Leave(string roomName)
        {
            SocketPacket.Literal l = new SocketPacket.Literal(Packet.Headers.Leave);
            l.SetRoom(roomName);

            socket.Send(l.ToPacket());
            Room = null;
        }
    }
}
