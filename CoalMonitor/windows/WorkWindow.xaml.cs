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
    /// WorkWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WorkWindow : Window
    {
        public WorkWindow()
        {
            InitializeComponent();
            
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {            
            this.Close();
        }

        /// <summary>
        /// 手动准备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManualReady_Click(object sender, RoutedEventArgs e)
        {
            ManualSet ms = new ManualSet();
            ms.Show();
            this.Close();
        }
    }
}
