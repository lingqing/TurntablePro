using System;
using System.Collections.Generic;
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

namespace CoalMonitor.windows
{
    /// <summary>
    /// Welcom.xaml 的交互逻辑
    /// </summary>
    public partial class Welcom : Window
    {
        public Welcom()
        {
            InitializeComponent();
        }

        private void WorkWindowBtn_Click(object sender, RoutedEventArgs e)
        {
            WorkWindow ww = new WorkWindow();
            ww.Show();
            this.Close();
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DebugWindowBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
