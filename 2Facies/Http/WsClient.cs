using _2Facies.Socket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using WebSocketSharp;

namespace _2Facies
{
    public class WsClient
    {
        public static Packet.Room Room { get; private set; }
        //[ThreadStatic]
        private static WebSocket socket = new WebSocket(Http.Request.SocketURL);
        private static Dictionary<string, EventHandler<MessageEventArgs>> events = new Dictionary<string, EventHandler<MessageEventArgs>>();
        public WsClient(Action<Packet.ErrorCode> ErrorHandler)
        {

            socket.Connect();

            On(Headers.Error.ToStringValue(), (packet) =>
            {
                int codeValue;

                //Debug.WriteLine("Error Packet.Body : "+packet.Body);

                if (!int.TryParse(packet.Body, out codeValue) || !Enum.IsDefined(typeof(Packet.ErrorCode), codeValue))
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
        public void On(string eventName, Action<IReceivePacket> receiver)
        {
            if(events.ContainsKey(eventName))
            {
                socket.OnMessage -= events[eventName];
                events.Remove(eventName);
            }

            events.Add(eventName, (sender, e) =>
            {
                var splited = e.Data.Split('@');
                //Debug.WriteLine($"Header: {splited[0]} == {eventName} : {splited[0] == eventName}");
                if (splited[0] == eventName)
                {
                    var receive = new SocketPacket.Receive(splited[1], splited[2]);

                    receiver(receive);
                }
            });

            socket.OnMessage += events[eventName];
            
        }

        public void Emit(ISendPacket packet)
        {
            socket.Send(packet.ToPacket());
        }

        public void Create(string roomTitle, int maxParticipants, Action<IReceivePacket> createRoomResultHandler)
        {
            var crp = new SocketPacket.CreateRoom(roomTitle, maxParticipants);
            socket.Send(crp.ToPacket());

            On(Headers.Create.ToStringValue(), createRoomResultHandler);
            
        }
        public void Join(string roomName, string joiner)
        {
            socket.Send(new SocketPacket.Join(roomName, joiner).ToPacket());
            Room = new Packet.Room(roomName);
        }
        public void Leave(string roomName, string leaver)
        {
            socket.Send(new SocketPacket.Leave(roomName, leaver).ToPacket());
            Room = null;
        }
    }
}
