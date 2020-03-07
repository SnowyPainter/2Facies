using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace _2Facies.Resource
{
    /// <summary>
    /// ChatMessage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChatMessage : UserControl
    {
        public ChatMessage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string Message
        {
            get { return MessageBlock.Text; }
            set { MessageBlock.Text = value; }
        }

        public Brush BackColor
        {
            get { return ChatBorder.BorderBrush; }
            set { ChatBorder.Background = value; }
        }

        public TextAlignment TextAlign
        {
            get { return MessageBlock.TextAlignment; }
            set { MessageBlock.TextAlignment = value; }
        }
    }
}
