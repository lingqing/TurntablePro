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
using CoalMonitor.classlib;

namespace CoalMonitor.windows
{
    /// <summary>
    /// ManualSet.xaml 的交互逻辑
    /// </summary>
    public partial class ManualSet : Window
    {
        public ManualSet()
        {
            InitializeComponent();
            //List<String> myList = new List<String>{ "1", "2", "3", "4" };
            //listBox.ItemsSource = myList;
        }

        private void listBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            AixParameter para = (AixParameter)btn.DataContext;
            switch (para.TitleName)
            {
                case "1轴":
                    MessageBox.Show("1轴");
                    break;
                case "2轴":
                    MessageBox.Show("2轴");
                    break;
                default:
                    break;
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 返回主页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
