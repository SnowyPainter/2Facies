using _2Facies.Resource;
using MaterialDesignThemes.Wpf;
using Microsoft.MixedReality.WebRTC;
using NAudio.Wave;
using System;
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
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.Close();
                    }));
                    break;
                case Packet.ErrorCode.RoomNotFound:
                    MessageBox.Show("방이 존재하지 않습니다.");
                    logger.Log("Error SocketRoom Not Exist", true);
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.Close();
                    }));
                    break;

            }
        }
        private void audioRecordHandler(byte[] buffer, float maxSample)
        {
            float converted = 100 * maxSample;
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                VoiceAudioScaleBar.Value = converted;
            }));
        }
        private void loggerHandler(string logMessage)
        {
            LeastLog.Text = logMessage;
        }
        private void RecordButtonTimer_Tick(object sender, int tickCount)
        {

            if (TimeSpan.FromSeconds(voiceChatTimerInterval * tickCount) >= voiceChatMaxTime)
            {
                Console.WriteLine("Interrupt");
                audioRec.InterruptRecording();
                (sender as DispatcherTimer).Stop();
                return;
            }
            ButtonProgressAssist.SetValue(VoiceChatButton, voiceChatTimerInterval * tickCount);
        }
        private void SetAudioUIState(bool record)
        {
            if (record)
            {
                VoiceChatButton.Background = (SolidColorBrush)brushConverter.ConvertFromString("#FFC10F0F");
                VoiceChatIcon.Kind = PackIconKind.Record;
            }
            else
            {
                VoiceChatButton.Background = (SolidColorBrush)brushConverter.ConvertFromString("#FF673AB7");
                VoiceChatIcon.Kind = PackIconKind.Microphone;
                ButtonProgressAssist.SetValue(VoiceChatButton, 0);
            }
            VoiceAudioScaleBar.Value = 0f;
        }
        //--------------------------------------------------------
        //------------------Control, Object Init-----------------
        //--------------------------------------------------------
        public RoomWindow(string id)
        {
            InitializeComponent();
            client = new WsClient(errorSocketHandler);
            client.Join(id);

            InitSocketEvents();

            //variable init
            InitVariables();

            client.Emit(new SocketPacket.Participants(WsClient.Room.Id, UserWindow.userData.Name));
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
            brushConverter = new BrushConverter();
            audioRec = new AudioRecord(0, 8000, 1, audioRecordHandler);

            audioRec.SetTimer(voiceChatTimerInterval, RecordButtonTimer_Tick);
            ButtonProgressAssist.SetMaximum(VoiceChatButton, voiceChatMaxTime.Seconds);

            voiceChatMaxTime = TimeSpan.FromSeconds(2);
            voiceChatTimerInterval = 0.05f;

            selfColor = (SolidColorBrush)brushConverter.ConvertFrom("#9951b8");
            otherColor = (SolidColorBrush)brushConverter.ConvertFrom("#bdbdbd");
        }
        private void InitSocketEvents()
        {
            client.On(Packet.Headers.Broadcast.ToStringValue(), (ev) =>
            {
                var splited = ev.Data.Split('@');
                var message = splited[2];
                var writer = splited[1];

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    var msg = new ChatMessage
                    {
                        Message = $"[{writer}] {message}",
                        BackColor = otherColor,
                        TextAlign = TextAlignment.Left,
                        HorizontalAlign = HorizontalAlignment.Left
                    };
                    Console.WriteLine(ev.Data);
                    ChatPanel.Children.Add(msg);

                }));
            });

            //********************FIXED MUST**
            client.On(Packet.Headers.BroadcastAudio.ToStringValue(), (ev) =>
            {
                var data = ev.Data.Split('@')[1];
                Console.WriteLine(data.Length);
                Stream ms = new MemoryStream(Convert.FromBase64String(data));

                var mp3Reader = new Mp3FileReader(ms);
                var waveOut = new WaveOutEvent();
                waveOut.Init(mp3Reader);
                waveOut.Play();

            });
            //update participants
            client.On(Packet.Headers.Participants.ToStringValue(), (ev) =>
            {
                var participants = int.Parse(ev.Data.Split('@')[1]);
                currentParticipants = participants;

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
            if (UserWindow.userData != null && WsClient.Room != null)
            {
                var roomId = WsClient.Room.Id;
                client.Leave(roomId);
                client.Emit(new SocketPacket.Participants(roomId, UserWindow.userData.Name));
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
        private AudioRecord audioRec;

        private BrushConverter brushConverter;
        private SolidColorBrush selfColor, otherColor;

        private int currentParticipants;

        private bool recording = false;
        private TimeSpan voiceChatMaxTime;
        private float voiceChatTimerInterval;

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
                client.Emit(new SocketPacket.Broadcast(WsClient.Room.Id, UserWindow.userData.Name, text));
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
            recording = !recording;

            if (recording)
            {
                audioRec.Start();
            }
            else
            {
                var stream = audioRec.Stop();

                if (stream != null)
                {
                    var pcmAudio = stream.ToByteArray();
                    if(pcmAudio.Length < 3000)
                        logger.Log($"MP3 Convert Failed(Too Short PCM).AudioLen:{pcmAudio.Length}", true);
                    else
                    {
                        var audio = Encoding.UTF8.GetBytes(Convert.ToBase64String(pcmAudio.ToMP3()));
                        if (audio.Length < 10000)
                            client.Emit(new SocketPacket.BroadcastAudio(WsClient.Room.Id, UserWindow.userData.Name, audio));
                        else
                            logger.Log($"Too Large to Send.AudioLen:{audio.Length}", true);
                    }
                    
                }
            }

            SetAudioUIState(recording);
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

        private async void Window_Initialized(object sender, EventArgs e)
        {
            var deviceList = await PeerConnection.GetVideoCaptureDevicesAsync();

            // For example, print them to the standard output
            foreach (var device in deviceList)
            {
                MessageBox.Show($"Found webcam {device.name} (id: {device.id})");
            }
        }

        public RoomWindow() //testing
        {
            InitializeComponent();

            InitVariables();

            UserWindow.userData = new Packet.DataPublic("12", "James", "S@g");
            client = new WsClient(errorSocketHandler);
            client.Create("Title", 3, (msge) =>
            {
                //Console.WriteLine(msge.Data);
            });

            client.Join("0");
            InitSocketEvents();

            client.Emit(new SocketPacket.Participants(WsClient.Room.Id, UserWindow.userData.Name));
        }
    }
}
