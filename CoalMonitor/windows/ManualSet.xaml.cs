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
using System.Windows.Threading;
using System.Collections;

namespace CoalMonitor.windows
{
    /// <summary>
    /// ManualSet.xaml 的交互逻辑
    /// </summary>
    public partial class ManualSet : Window
    {
        private List<int> SpeedListIndex= new List<int>() { 1, 1, 1, 1, 1, 1 };
        private List<int> StepListIndex = new List<int>() { 1, 1, 1, 1, 1, 1 };
        private List<float> localSpeedList = new List<float>() { 1, 1, 1, 1, 1, 1 };
        private List<int> localStepList = new List<int>() { 1, 1, 1, 1, 1, 1 };

        private List<AixParameter> listAixPara = new List<AixParameter>();
        
        public ManualSet()
        {
            InitializeComponent();
            
            timer1.Tick += Timer1_Tick;
            UseLastBtn.IsEnabled = false;
            ToMiddle.IsEnabled = true;
            //AixListBox.IsEnabled = false;
            listAixPara.Add(new AixParameter("1轴"));
            listAixPara.Add(new AixParameter("2轴"));
            listAixPara.Add(new AixParameter("3轴"));
            listAixPara.Add(new AixParameter("4轴"));
            listAixPara.Add(new AixParameter("5轴"));
            listAixPara.Add(new AixParameter("6轴"));
            AixListBox.ItemsSource = listAixPara;

        }

        private void AixsToMove(int aix, bool toup)
        {
            if (aix < mControlData.mSinglePlatFormControl.thisplatform.g_naxis)
            {
                int dir = toup == true ? 1 : -1;
                int[] mbck = mControlData.mSinglePlatFormControl.GetCommendBack();

                float speedT = localSpeedList[aix-1];
                int delt = localStepList[aix-1] * dir;

                mbck[aix] += delt;
                mControlData.mSinglePlatFormControl.P2P_runing_start(aix, speedT * 0.5f, speedT * 0.5f, speedT, mbck[aix]);
            }
        }
        private List<T> FindVisualChildList<T>(DependencyObject obj, string childName) where T : DependencyObject
        {
            List<T> childList = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T && child.GetValue(NameProperty).ToString() == childName)
                {
                    childList.Add((T)child);
                }
                else
                {
                    List<T> t2 = FindVisualChildList<T>(child, childName);
                    if (t2 != null && t2.Count > 0)
                    {
                        foreach (T t in t2)
                        {
                            childList.Add(t);
                        }
                    }
                }
            }

            return childList;
        }
        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            AixParameter para = (AixParameter)btn.DataContext;
            if (para.TitleName.Length < 1) return;
            int aixIndex = Convert.ToInt32(para.TitleName.Substring(0, 1));
            //ComboBox box = FindVisualChildList<ComboBox>(AixListBox, "SpeedList")[aixIndex - 1];
            //localSpeedList[aixIndex - 1] = Convert.ToSingle(box.SelectedItem);
            AixsToMove(aixIndex, true);
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            AixParameter para = (AixParameter)btn.DataContext;
            if (para.TitleName.Length < 1) return;
            int aixIndex = Convert.ToInt32(para.TitleName.Substring(0, 1));
            AixsToMove(aixIndex, false);

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
        /// <summary>
        /// 居中平台
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        public event Action BeginToMiddle = null;
        private void ToMiddle_Click(object sender, RoutedEventArgs e)
        {
            ToMiddle.IsEnabled = false;            
            BeginToMiddle?.Invoke();
        }

        private DispatcherTimer timer1 = new DispatcherTimer();
        private ControlData mControlData = ControlData.ShareInstance();
        private int[] MiddlePulse = new int[] { 0, 0, 0, 0, 0, 0 };
        

        public void ToMiddleCom()
        {
            ToMiddle.IsEnabled = false;
            AixListBox.IsEnabled = true;
            timer1.Start(); 
            int[] mbck = mControlData.mSinglePlatFormControl.GetCommendBack();
            for (int i = 0; i < 6; i++)
            {
                MiddlePulse[i] = mbck[i];
            }
            UseLastBtn.IsEnabled = true;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (mControlData.mSinglePlatFormControl.thisplatform == null)
            {
                return;
            }
            if (mControlData.mSinglePlatFormControl.thisplatform.isOpen())
            {
                int[] mbck = mControlData.mSinglePlatFormControl.GetCommendBack();
                ushort[,] mswitch = mControlData.mSinglePlatFormControl.GetLimitSwitch();
                //ArrayList aixParaList = this.Resources["AixesList"] as ArrayList;
                for (int i = 0; i < 6; i++)
                {     
                    listAixPara[i].Position = mbck[i] + "";
                    /// @Todo:
                    //Poslist[i].Text = mbck[i] + "";
                    //LimitS[i].Text = GetLimitText(mswitch[i, 0]);
                    //LimitX[i].Text = GetLimitText(mswitch[i, 1]);
                }
                AixListBox.Items.Refresh();
            }
        }

        private void SpeedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            AixParameter para = (AixParameter)box.DataContext;
            if (para.TitleName.Length < 1) return;
            int selectIndex = Convert.ToInt32(para.TitleName.Substring(0, 1));
            if (SpeedListIndex.Count < selectIndex) { MessageBox.Show("轴数目不匹配"); return; }

            SpeedListIndex[selectIndex - 1] = box.SelectedIndex;
            localSpeedList[selectIndex - 1] = Convert.ToSingle(box.SelectedItem);
        }

        private void StepListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            AixParameter para = (AixParameter)box.DataContext;
            if (para.TitleName.Length < 1) return;
            int selectIndex = Convert.ToInt32(para.TitleName.Substring(0, 1));
            if (StepListIndex.Count < selectIndex) { MessageBox.Show("轴数目不匹配"); return; }
            StepListIndex[selectIndex - 1] = box.SelectedIndex;
            localStepList[selectIndex - 1] = Convert.ToInt32(box.SelectedItem);
        }

        public event Action OnComplete = null;
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            timer1.Stop();
            string lastdelt = "";
            int[] mbck = mControlData.mSinglePlatFormControl.GetCommendBack();
            for (int i = 0; i < 6; i++)
            {
                mControlData.mConfigXML.DeltalPos[i] = mbck[i] - MiddlePulse[i];
                lastdelt += "--" + (i + 1) + "--" + mControlData.mConfigXML.DeltalPos[i];
            }
            //   this.ShowTips(lastdelt);
            if (OnComplete != null)
            {
                OnComplete();
            }
            this.Close();
        }

        private void UseLastBtn_Click(object sender, RoutedEventArgs e)
        {
            if (OnComplete != null)
            {
                OnComplete();
            }
            this.Close();
        }
    }
}
