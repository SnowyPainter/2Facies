using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _2Facies
{
    /// <summary>
    /// LoadingWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoadingWindow : Window
    {
        private bool IsDone = false;
        public LoadingWindow(string message)
        {
            InitializeComponent();
            LoadingMessage.Text = message;
        }
        public void LoadingDone()
        {
            IsDone = true;
            this.Close();
        }
        public void SetLoadingMessage(string message)
        {
            LoadingMessage.Text = message;
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !IsDone;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
