using _2Facies.Resource;
using _2Facies.RTC;
using _2Facies.Socket;
using MaterialDesignThemes.Wpf;
using Microsoft.MixedReality.WebRTC;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace _2Facies
{
    /// <summary>
    /// RoomWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RoomWindow : Window
    {
        private void errorSocketHandler(Packet.ErrorCode code)
        {
            switch (code)
            {
                case Packet.ErrorCode.IncorrectTypeError:
                    MessageBox.Show("올바르지못한 종류의 통신입니다.");
                    logger.Log("Error type of message is incorrect", true);
                    break;
                case Packet.ErrorCode.ChatConnect:
                    MessageBox.Show("채팅 연결이 불안정합니다.");
                    logger.Log("Error connecting on SocketRoom", true);
                    break;
                case Packet.ErrorCode.ChatSend:
                    MessageBox.Show("채팅을 보낼수 없습니다.");
                    logger.Log("Error Sending message on SocketRoom", true);
                    break;
                case Packet.ErrorCode.ChatRecv:
                    MessageBox.Show("채팅을 주고 받는데에 문제가 있습니다.");
                    logger.Log("Error Receiving Other messages on SocketRoom", true);
                    break;
                case Packet.ErrorCode.RoomFull:
                    logger.Log("Error Room which connecting was filled with people", true);
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        this.Close();
                    }));
                    break;
                case Packet.ErrorCode.RoomNotFound:
                    MessageBox.Show("방이 존재하지 않습니다.");
                    logger.Log("Error SocketRoom Not Exist", true);
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        this.Close();
                    }));
                    break;

            }
        }
        private void sampleHandler(SampleRTA sample)
        {
            if (!microphoneMute)
                client.Emit(new SocketPacket.BroadcastAudio(WsClient.Room.Id, UserWindow.UserData.Id, sample.Sample));
        }
        private void loggerHandler(string logMessage)
        {
            LeastLog.Text = logMessage;
        }
        //--------------------------------------------------------
        //------------------Control, Object Init-----------------
        //--------------------------------------------------------
        public RoomWindow(string id)
        {
            client = new WsClient(errorSocketHandler);

            InitializeComponent();

            client.Join(id, UserWindow.UserData.Id);
            client.Emit(new SocketPacket.Participants(WsClient.Room.Id, UserWindow.UserData.Name));
        }
        public RoomWindow(Packet.Room data) : this(data.Id)
        {
            if (data != null)
            {
                Title_Text.Text = $"2Facies {data.Title} / [{data.Max}]";
            }
        }
        //-------------------------------------------------------
        //------------------------Initalize Objects--------------
        //-------------------------------------------------------

        private void Window_Initilized(object sender, EventArgs e)
        {
            InitSocketEvents();
            InitVariables();
        }

        private void InitVariables()
        {
            //participants[UserWindow.UserData.Id] = UserWindow.UserData;
            logger = new Logger(new FileInfo($@"{FileResources.LogFile}"), loggerHandler);
            rta = new SharpRTA();
            brushConverter = new BrushConverter();

            rta.Start(sampleHandler);
            microphoneMute = true;

            selfColor = (SolidColorBrush)brushConverter.ConvertFrom("#9951b8");
            otherColor = (SolidColorBrush)brushConverter.ConvertFrom("#bdbdbd");

        }
        private void InitSocketEvents()
        {
            client.On(Headers.Broadcast.ToStringValue(), async (packet) =>
            {
                if (!participants.ContainsKey(packet.Caster))
                    participants.Add(packet.Caster,await ServerClient.GetPublicUserData(UserWindow.UserData.Id));
                    

                string id = packet.Caster;
                string message = packet.Body;
                string writer = participants[id].Name;

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    var msg = new ChatMessage
                    {
                        Message = $"[{writer}] {message}",
                        BackColor = otherColor,
                        TextAlign = TextAlignment.Left,
                        HorizontalAlign = HorizontalAlignment.Left
                    };
                    ChatPanel.Children.Add(msg);

                }));
            });

            //********************FIXED MUST**
            client.On(Headers.BroadcastAudio.ToStringValue(), (packet) =>
            {
                var data = packet.Body;
                Stream ms = new MemoryStream(Convert.FromBase64String(data));

                rta.Keep(ms.ToByteArray());

            });
            //update participants
            client.On(Headers.Participants.ToStringValue(), (packet) =>
            {
                var participants = int.Parse(packet.Body);
                //Debug.WriteLine($"Participatns Caster:{packet.Caster}, BODY:{packet.Body}");
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ParticipantsText.Text = $"{participants}명 접속중";
                }));
            });
        }
        //-------------------------------------------------------
        //-------------------------Window Interactive------------
        //-------------------------------------------------------
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (rta.Active)
            {
                rta.Stop();
            }
            if (UserWindow.UserData != null && WsClient.Room != null)
            {
                var roomId = WsClient.Room.Id;
                var userData = UserWindow.UserData;

                if (participants.ContainsKey(userData.Id)) participants.Remove(userData.Id);

                client.Emit(new SocketPacket.Broadcast(WsClient.Room.Id, userData.Id, $"{userData.Name} left this room"));
                client.Leave(roomId, userData.Id);
                client.Emit(new SocketPacket.Participants(roomId, userData.Name));
            }

        }
        private void WindowClose_Btn_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void WindowMinimize_Btn_Clicked(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void NavigationBar_DragMove(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        //--------------------------------------------------------
        //--------------------------------------------------------
        //--------------------------------------------------------

        //-------------------Variables Declare----------------------------

        private Logger logger;
        private WsClient client;
        private SharpRTA rta;

        private BrushConverter brushConverter;
        private SolidColorBrush selfColor, otherColor;

        private int currentParticipants { get { return participants.Count; } }
        private Dictionary<string, Packet.DataPublic> participants = new Dictionary<string, Packet.DataPublic>();
        private bool microphoneMute;

        private Point currentPoint;
        private SolidColorBrush currentForeColor = Brushes.Yellow;
        private double currentStrokeThickness = 5;
        private Polyline polyLine;
        //-------------------/Variables----------------------------

        private void ChatTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var text = ChatTextBox.Text;
            if (text != "" && e.Key == Key.Enter)
            {
                client.Emit(new SocketPacket.Broadcast(WsClient.Room.Id, UserWindow.UserData.Id, text));
                var msg = new ChatMessage
                {
                    Message = text,
                    BackColor = selfColor,
                    TextAlign = TextAlignment.Right,
                    HorizontalAlign = HorizontalAlignment.Right
                };
                ChatPanel.Children.Add(msg);

                ChatTextBox.Text = "";
                ChatScrollViewer.ScrollToBottom();
            }
        }
        //***********************************
        //Record Voicechat / NONE COMPLETE / -> WEB RTC - ing -> RTA
        //***********************************
        private void VoiceChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (microphoneMute)
            {
                VoiceChatButton.Background = (SolidColorBrush)brushConverter.ConvertFromString("#FFC10F0F");
                VoiceChatIcon.Kind = PackIconKind.Record;
                microphoneMute = false;
            }
            else
            {
                VoiceChatButton.Background = (SolidColorBrush)brushConverter.ConvertFromString("#FF673AB7");
                VoiceChatIcon.Kind = PackIconKind.Microphone;
                microphoneMute = true;
            }
        }
        //***********************************

        private void CanvasDraw_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                currentPoint = e.GetPosition(DrawCanvas);

                polyLine = new Polyline();
                polyLine.Stroke = currentForeColor;
                polyLine.StrokeThickness = currentStrokeThickness;

                DrawCanvas.Children.Add(polyLine);
            }
        }
        private void CanvasDraw_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                polyLine = (Polyline)DrawCanvas.Children[DrawCanvas.Children.Count - 1];
                polyLine.Points.Add(e.GetPosition(DrawCanvas));
            }
        }
        private void CanvasDraw_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Released)
            {

            }
        }
        public RoomWindow() //testing
        {
            UserWindow.UserData = new Packet.DataPublic("A", "닉네임123", "S@g", 13, true);
            client = new WsClient(errorSocketHandler);
            client.Create("Title", 3, (msge) =>
            {
                //Debug.WriteLine(msge.Body);
            });

            client.Join("0", UserWindow.UserData.Id);

            InitializeComponent();

            var data = UserWindow.UserData;
            client.Emit(new SocketPacket.Broadcast(WsClient.Room.Id, data.Id, $"{data.Name} Joined"));
            client.Emit(new SocketPacket.Participants(WsClient.Room.Id, data.Id));

        }
    }
}
