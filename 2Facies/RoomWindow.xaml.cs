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
            if(!microphoneMute)
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
            InitializeComponent();
            client = new WsClient(errorSocketHandler);
            client.Join(id, UserWindow.UserData.Id);

            InitSocketEvents();

            //variable init
            InitVariables();

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
        private void InitVariables()
        {
            logger = new Logger(new FileInfo($@"{FileResources.LogFile}"), loggerHandler);
            
            rta = new SharpRTA();
            rta.Start(sampleHandler);

            microphoneMute = true;

            brushConverter = new BrushConverter();

            selfColor = (SolidColorBrush)brushConverter.ConvertFrom("#9951b8");
            otherColor = (SolidColorBrush)brushConverter.ConvertFrom("#bdbdbd");
        }
        private void InitSocketEvents()
        {
            client.On(Headers.Broadcast.ToStringValue(), (packet) =>
            {
                var message = packet.Body;
                var writer = packet.Caster;
                //Debug.WriteLine($"ID:{packet.Caster}, BODY:{packet.Body}");
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    var msg = new ChatMessage
                    {
                        Message = $"[{writer}] {message}",
                        BackColor = otherColor,
                        TextAlign = TextAlignment.Left,
                        HorizontalAlign = HorizontalAlignment.Left
                    };
                    Console.WriteLine(packet.Body);
                    ChatPanel.Children.Add(msg);

                }));
            });

            //********************FIXED MUST**
            client.On(Headers.BroadcastAudio.ToStringValue(), (packet) =>
            {
                var data = packet.Body;
                Debug.WriteLine(data);
                Stream ms = new MemoryStream(Convert.FromBase64String(data));

                rta.Keep(ms.ToByteArray());

            });
            //update participants
            client.On(Headers.Participants.ToStringValue(), (packet) =>
            {
                var participants = int.Parse(packet.Body);
                currentParticipants = participants;
                Debug.WriteLine($"Participatns Caster:{packet.Caster}, BODY:{packet.Body}");
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
            if(rta.Active)
            {
                rta.Stop();
            }
            if (UserWindow.UserData != null && WsClient.Room != null)
            {
                var roomId = WsClient.Room.Id;
                
                client.Leave(roomId, UserWindow.UserData.Id);
                client.Emit(new SocketPacket.Participants(roomId, UserWindow.UserData.Name));
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

        private int currentParticipants;
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
                client.Emit(new SocketPacket.Broadcast(WsClient.Room.Id, UserWindow.UserData.Name, text));
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
        //Record Voicechat / NONE COMPLETE / -> WEB RTC - ing
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

        private void Window_Initialized(object sender, EventArgs e)
        {
            /*
            var pc = new PeerConnection();
            var config = new PeerConnectionConfiguration
            {
                IceServers = new List<IceServer> {
                    new IceServer{ Urls = { "stun:stun.l.google.com:19302" } 
                   }
                }
            };
            await pc.InitializeAsync(config);
            await pc.AddLocalAudioTrackAsync();
            */
        }

        public RoomWindow() //testing
        {
            InitializeComponent();

            InitVariables();

            UserWindow.UserData = new Packet.DataPublic("12", "James", "S@g");
            client = new WsClient(errorSocketHandler);
            client.Create("Title", 3, (msge) =>
            {
                Debug.WriteLine(msge.Body);
            });

            client.Join("0", UserWindow.UserData.Id);
            InitSocketEvents();

            client.Emit(new SocketPacket.Participants(WsClient.Room.Id, UserWindow.UserData.Name));
        }
    }
}
