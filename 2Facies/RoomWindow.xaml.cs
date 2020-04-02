using MaterialDesignThemes.Wpf;
using NAudio.Wave;
using System;
using System.IO;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace _2Facies
{
    /// <summary>
    /// RoomWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RoomWindow : Window
    {
        public void ChatHandler(Packet.ErrorCode code)
        {
            switch (code)
            {
                case Packet.ErrorCode.IncorrectTypeError:
                    MessageBox.Show("올바르지못한 종류의 통신입니다.");
                    break;
                case Packet.ErrorCode.ChatConnect:
                    MessageBox.Show("채팅 연결이 불안정합니다.");
                    break;
                case Packet.ErrorCode.ChatSend:
                    MessageBox.Show("채팅을 보낼수 없습니다.");
                    break;
                case Packet.ErrorCode.ChatRecv:
                    MessageBox.Show("채팅을 주고 받는데에 문제가 있습니다.");
                    break;
                case Packet.ErrorCode.RoomFull:
                    MessageBox.Show("방이 꽉 찼습니다.");
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.Close();
                    }));
                    break;
                case Packet.ErrorCode.RoomNotFound:
                    MessageBox.Show("방이 존재하지 않습니다.");
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        this.Close();
                    }));
                    break;

            }
        }
        private void audioHandler(float sample)
        {
            float converted = 100 * sample;
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                VoiceAudioScaleBar.Value = converted;
            }));
        }
        //--------------------------------------------------------
        //------------------Control, Object Init-----------------
        //--------------------------------------------------------
        public RoomWindow(string id)
        {
            InitializeComponent();
            client = new WsClient(ChatHandler);
            client.Join(id);

            InitSocketEvents();

            //variable init
            InitVariables();

            client.Emit(Packet.Headers.Participants, WsClient.Room.Id, null);
        }
        public RoomWindow(Packet.Room data) : this(data.Id)
        {
            InitUIElements(data);
        }
        private void InitVariables()
        {
            voiceChatMaxTime = TimeSpan.FromSeconds(2);
            voiceChatTimerInterval = 0.01f; // 0.01 second

            ButtonProgressAssist.SetMaximum(VoiceChatButton, voiceChatMaxTime.Seconds);

            audioRec = new AudioRecord(0, 8000, 1, audioHandler);
            audioRec.SetTimer(voiceChatTimerInterval, RecordButtonTimer_Tick);

            var brushconv = new BrushConverter();

            selfColor = (SolidColorBrush)brushconv.ConvertFrom("#9951b8");
            otherColor = (SolidColorBrush)brushconv.ConvertFrom("#bdbdbd");
        }
        private void InitUIElements(Packet.Room info)
        {
            if (info != null)
            {
                Title_Text.Text = $"2Facies {info.Title} / [{info.Max}]";
            }
        }
        private void InitSocketEvents()
        {
            client.On(Packet.Headers.Broadcast.ToStringValue(), (ev) =>
            {
                var data = ev.Data.Split('@')[1];

                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    var msg = new Resource.ChatMessage
                    {
                        Message = data,
                        BackColor = otherColor,
                        TextAlign = TextAlignment.Left,
                        HorizontalAlign = HorizontalAlignment.Left
                    };
                    ChatPanel.Children.Add(msg);

                }));
            });
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
                var participants = ev.Data.Split('@')[1];

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ParticipantsText.Text = $"{participants}명 접속중";
                }));
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (UserWindow.userData != null && WsClient.Room != null)
            {
                var r = WsClient.Room.Id;
                client.Leave(WsClient.Room.Id);
                client.Emit(Packet.Headers.Participants, r, null);
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
        //------------------Control, Object Init-----------------
        //--------------------------------------------------------

        //-------------------Variables Declare----------------------------

        private WsClient client;
        private AudioRecord audioRec;

        private SolidColorBrush selfColor, otherColor;
        private bool recording = false;
        
        private TimeSpan voiceChatMaxTime;
        private float voiceChatTimerInterval;

        //-------------------Variables----------------------------
        private void SetVoiceChatState(bool rec)
        {
            var brushConv = new BrushConverter();
            VoiceAudioScaleBar.Value = 0.0f;
            if (rec)
            {
                VoiceChatButton.Background = (SolidColorBrush)brushConv.ConvertFromString("#FFC10F0F");
                VoiceChatIcon.Kind = PackIconKind.Record;
            }
            else
            {
                VoiceChatButton.Background = (SolidColorBrush)brushConv.ConvertFromString("#FF673AB7");
                VoiceChatIcon.Kind = PackIconKind.Microphone;
                ButtonProgressAssist.SetValue(VoiceChatButton, 0);
            }
        }
        //Chat
        private void ChatTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var text = ChatTextBox.Text;
            if (text != "" && e.Key == Key.Enter)
            {
                client.Emit(Packet.Headers.Broadcast, WsClient.Room.Id, Encoding.UTF8.GetBytes(text));
                var msg = new Resource.ChatMessage
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

        //Record Voicechat
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
                    
                    var audio = Encoding.UTF8.GetBytes(Convert.ToBase64String(pcmAudio.ConvertWavToMp3()));
                    Console.WriteLine($"sending base64 Len : {audio.Length}");
                    if (audio.Length < 30000)
                    {
                        client.Emit(Packet.Headers.BroadcastAudio, WsClient.Room.Id, audio);
                    }
                    else
                    {
                        Console.WriteLine("Too Much Audio Sounds");
                    }
                }
            }

            SetVoiceChatState(recording);
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
        public RoomWindow() //testing
        {
            InitializeComponent();

            InitVariables();

            client = new WsClient(ChatHandler);
            client.Create("Title", 3, (msge) =>
            {

            });

            client.Join("0");
            InitSocketEvents();

            client.Emit(Packet.Headers.Participants, WsClient.Room.Id, null);
        }

    }
}
