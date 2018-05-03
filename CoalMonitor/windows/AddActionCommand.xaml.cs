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
using CoalMonitor.classlib;

namespace CoalMonitor.windows
{
    /// <summary>
    /// AddActionCommand.xaml 的交互逻辑
    /// </summary>
    public partial class AddActionCommand : Window
    {
        private ControlData mControlData = ControlData.ShareInstance();
        public Movdata movdata { get; set; }
        public AddActionCommand()
        {
            InitializeComponent();
        }


        private float Clamp_Angle(float nowangle)
        {
            if (nowangle < mControlData.MinAngele)
            {
                nowangle = mControlData.MinAngele;
            }
            if (nowangle > mControlData.MaxAngele)
            {
                nowangle = mControlData.MaxAngele;
            }
            return nowangle;
        }
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int mod = commandBox.SelectedIndex;
                float Targetangle = float.Parse(commandAngle.Text);
                float Needtime = float.Parse(commandTime.Text);
                movdata = new Movdata();
                movdata.Model = mod;
                movdata.TargetAngle = Clamp_Angle(Targetangle);
                movdata.NeedTime = Needtime;
                this.DialogResult = true;
                this.Close();
            }
            catch
            {
                //MessageBox.Show("输入格式有误");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
