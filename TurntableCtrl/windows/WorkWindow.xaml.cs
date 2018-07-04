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
using TurntableCtrl.classlib;
using System.Threading;
using System.Windows.Threading;
using Visifire.Charts;

namespace TurntableCtrl.windows
{
    /// <summary>
    /// WorkWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WorkWindow : Window
    {
        private string Configfolder = System.AppDomain.CurrentDomain.BaseDirectory;
        private string Filepathd = "";
        private string ConfigPath = "";
        private List<Movdata> Mlist = new List<Movdata>();
        private ControlData m_ControlData = ControlData.ShareInstance();
        // 发送工作线程
        private Thread SendThread = null;
        private bool isRunning = false;
        private Action workInThread = null;

        private bool BeginMove = false;
        private DateTime StartTime = DateTime.Now;
        private int NowIndex = 0;

        // 
        private AddActionCommand actionCmdWindow = null;
               
        public WorkWindow()
        {
            InitializeComponent();
            if (FileXmlOperate.ExistFolderPath(Configfolder) == false)
            {

                FileXmlOperate.CreateFoldPath(Configfolder);
            }
            Filepathd = Configfolder + "\\" + "movedata.mathb";
            ConfigPath = Configfolder + "\\" + "DebugConfig.txt";

            Mlist.Clear();

            if (FileXmlOperate.Exist(Filepathd))
            {

                Movement tmp_Movement = new Movement();
                tmp_Movement = (Movement)FileXmlOperate.ReadXmlSerializer(Filepathd, tmp_Movement);
                Mlist = tmp_Movement.mlist;
                UpdateListbox();
            }
            EnabledBeginEnd(false);
            //Enabled_Readybutton(true);
            isRunning = true;
            SendThread = new Thread(SendMethod);
            SendThread.Start();
            Intlized();

            timer2.Interval = new System.TimeSpan(0, 0, 1);
            timer2.Tick += new EventHandler(Timer2_Tick);
            timer1.Tick += Timer1_Tick;

            /// serial chart line
            /// 
             ////
            //Chart chart = new Chart();
            Title title = new Title();
            title.Text = "角度曲线";
            title.Padding = new Thickness(0, 10, 5, 0);
            // 向图标添加标题 
            _chart.Titles.Add(title);
            //////////////////////////////////////////////////////////////////////////
            _chart.Series.Clear();
            DataSeries dataSeriesx = new DataSeries();
            dataSeriesx.RenderAs = RenderAs.QuickLine;
            dataSeriesx.Name = "rx";
            _chart.Series.Add(dataSeriesx);
            DataSeries dataSeriesy = new DataSeries();
            dataSeriesy.RenderAs = RenderAs.QuickLine;
            dataSeriesy.Name = "ry";
            _chart.Series.Add(dataSeriesy);
            DataSeries dataSeriesz = new DataSeries();
            dataSeriesz.RenderAs = RenderAs.QuickLine;
            dataSeriesz.Name = "rz";
            _chart.Series.Add(dataSeriesz);
            
            _chart.Series[0].DataPoints = new DataPointCollection();
            _chart.Series[1].DataPoints = new DataPointCollection();
            _chart.Series[2].DataPoints = new DataPointCollection();
        }

        protected override void OnClosed(EventArgs e)
        {
            isRunning = false;
            SendThread.Abort();
            base.OnClosed(e);
        }

        private static int i = 0;
        private void Timer2_Tick(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            LightDataPoint ldx = new LightDataPoint();
            LightDataPoint ldy = new LightDataPoint();
            LightDataPoint ldz = new LightDataPoint();
            ldx.XValue = talsec;
            ldy.XValue = talsec;
            ldz.XValue = talsec;
            //ld.YValue = Math.Sin(i * 0.1);
            //_chart.Series[0].DataPoints.Add(ld);
            if (BeginMove)
            {                
                commandListBox.SelectedIndex = NowIndex;
                RunTime.Text = "时间:" + Math.Round(talsec, 2) + "秒";
                RunState.Text = "俯仰:" + rx + "\n偏航:" + ry + "\n滚转:" + rz;

                RunType.Text = nowModel;
                RunSpeed.Text = nowSpeed.ToString() + "度/秒";
                /// @Todo: 增加保存rx，ry，rz，以绘制曲线
                ldx.YValue = rx;
                ldy.YValue = ry;
                ldz.YValue = rz;
                _chart.Series[0].DataPoints.Add(ldx);
                _chart.Series[1].DataPoints.Add(ldy);
                _chart.Series[2].DataPoints.Add(ldz);
            }
            else
            {
                rx = 0;
                ry = 0;
                rz = 0;
                float[] abc = m_ControlData.mSixPlaneF.rounds(0, 0, 0);
                float[] Le = m_ControlData.mSixPlaneF.Six_Platform_RoundF(0, 45, 0, abc[0], abc[1], abc[2]);
                int[] Spos = m_ControlData.mSinglePlatFormControl.thisplatform.Conver_LengthTopluse2(Le);
                for (int i = 0; i < 6; i++)
                {
                    Spos[i] = Spos[i] + m_ControlData.mConfigXML.DeltalPos[i];
                }
                
                SinglePlatFormControl.SixSame_ManualStart(m_ControlData.mSinglePlatFormControl.thisplatform.g_handle, Spos, 10, 10 * 0.01f, 0);

                RunTime.Text = "时间:" + Math.Round(talsec, 2) + "秒";
                RunState.Text = "俯仰:" + rx + "\n偏航:" + ry + "\n滚转:" + rz;
                RunType.Text = "N/A";
                RunSpeed.Text = nowSpeed.ToString() + "度/秒";

                EnabledBeginEnd(true);
                timer2.Stop();
            }
        }

        // 线程运行内容
        private void SendMethod()
        {
            while (isRunning)
            {
                Thread.Sleep(2);
                if (workInThread != null)
                {
                    workInThread();
                }
            }
            return;
        }
        private void Intlized()
        {
            m_ControlData.mSinglePlatFormControl = new SinglePlatFormControl();
            m_ControlData.mSixPlaneF = new SixPlatC.SixPlatform();
            m_ControlData.mSixPlaneF.CreateSixPlatForm(m_ControlData.mConfigXML.P_ShangLong, m_ControlData.mConfigXML.P_ShangShort, m_ControlData.mConfigXML.P_XiaLong, m_ControlData.mConfigXML.P_XiaShort, m_ControlData.mConfigXML.P_Height, m_ControlData.mConfigXML.P_XingC);
            m_ControlData.mSixPlaneF.CreateJie(45);
        }

        #region 指令操作区域：指令添加、修改、删除、清空、保存、导入、导出等
        /////////////////////////////////////////////////////////////////////////////////

        private float globalTotalTime = 0;    
        /// <summary>
        /// 更新指令显示区
        /// </summary>
        private void UpdateListbox()
        {
            commandListBox.Items.Clear();            
            float totaltime = 0;
            CaculatorCurey();
            /// 绘制曲线
            //_chart.Series[0].DataPoints.Clear();
            for (int i = 0; i < Mlist.Count; i++)
            {
                totaltime = Mlist[i].Caculator(totaltime);
                commandListBox.Items.Add(i + "--" + GetMovString(Mlist[i], totaltime));

            }
            globalTotalTime = totaltime;
            /// 绘制曲线
            //_chart.Series[0].DataPoints.Clear();

        }
        /// <summary>
        /// 重新计算指令集合起止角度、终止角度和总时间
        /// </summary>
        private void CaculatorCurey()
        {
            float totaltime = 0;
            float Erx = 0;
            float Ery = 0;
            float Erz = 0;
            for (int i = 0; i < Mlist.Count; i++)
            {
                switch (Mlist[i].Model)
                {
                    case 0:
                        Mlist[i].StartAngle = Erx;
                        Erx = Clamp_Angle(Mlist[i].TargetAngle);
                        break;
                    case 1:
                        Mlist[i].StartAngle = Ery;
                        Ery = Clamp_Angle(Mlist[i].TargetAngle);
                        break;
                    case 2:
                        Mlist[i].StartAngle = Erz;
                        Erz = Clamp_Angle(Mlist[i].TargetAngle);
                        break;
                    case 3:
                        break;
                    default:
                        break;
                }
                totaltime = Mlist[i].Caculator(totaltime);
            }
        }
        /// <summary>
        /// 控制角度范围
        /// </summary>
        /// <param name="nowangle"></param>
        /// <returns></returns>
        private float Clamp_Angle(float nowangle)
        {
            if (nowangle < m_ControlData.MinAngele)
            {
                nowangle = m_ControlData.MinAngele;
            }
            if (nowangle > m_ControlData.MaxAngele)
            {
                nowangle = m_ControlData.MaxAngele;
            }
            return nowangle;
        }
        private string GetMovString(Movdata movdata, float timer)
        {
            string resd = "";
            switch (movdata.Model)
            {
                case 0:
                    resd = "(" + timer + "s" + ")" + "\t偏航" + "--起始角度:" + movdata.StartAngle + "--目标角度:" + movdata.TargetAngle + "--时间:" + movdata.NeedTime;
                    break;
                case 1:
                    resd = "(" + timer + "s" + ")" + "\t滚转" + "--起始角度:" + movdata.StartAngle + "--目标角度:" + movdata.TargetAngle + "--时间:" + movdata.NeedTime;
                    break;
                case 2:
                    resd = "(" + timer + "s" + ")" + "\t俯仰" + "--起始角度:" + movdata.StartAngle + "--目标角度:" + movdata.TargetAngle + "--时间:" + movdata.NeedTime;
                    break;
                case 3:
                    resd = "(" + timer + "s" + ")" + "\t延迟" + "--时间:" + movdata.NeedTime;
                    break;
                default:
                    resd = "错误指令";
                    break;
            }
            return resd;
        }

        private void G_Over()
        {
            BeginMove = false;
            workInThread = null;
        }

        
        private float rx = 0;
        private float ry = 0;
        private float rz = 0;
        private float xx = 0;
        private float yy = 45;
        private float zz = 0;
        private double talsec = 0;

        private String nowModel = "N/A";
        private float nowSpeed = 0;
        private void CaculatorTosend()
        {
            if (BeginMove)
            {
                TimeSpan tmpd = DateTime.Now - StartTime;
                talsec = tmpd.TotalSeconds;
                float starttime = Mlist[NowIndex].StartTime;
                float edtime = Mlist[NowIndex].EndTime;
                int model = Mlist[NowIndex].Model;
                nowSpeed = Mlist[NowIndex].cury_K;               
                switch (model)
                {
                    case 0:
                        rz = Mlist[NowIndex].GetAngle((float)talsec);
                        nowModel = "俯仰";
                        break;
                    case 1:
                        ry = Mlist[NowIndex].GetAngle((float)talsec);
                        nowModel = "偏航";
                        break;
                    case 2:
                        rx = Mlist[NowIndex].GetAngle((float)talsec);
                        nowModel = "滚转";
                        break;
                    case 3:
                        nowModel = "延迟";
                        break;
                    default:
                        nowModel = "N/A";
                        break;
                }
                if (m_ControlData.mSinglePlatFormControl.HaveCompeleteFuwei)
                {
                    float[] abc = m_ControlData.mSixPlaneF.rounds(rx, ry, rz);
                    float[] Le = m_ControlData.mSixPlaneF.Six_Platform_RoundF(xx, yy, zz, abc[0], abc[1], abc[2]);
                    int[] Spos = m_ControlData.mSinglePlatFormControl.thisplatform.Conver_LengthTopluse2(Le);
                    for (int i = 0; i < 6; i++)
                    {
                        Spos[i] = Spos[i] + m_ControlData.mConfigXML.DeltalPos[i];
                    }
                    SinglePlatFormControl.SixSame_ManualStart(m_ControlData.mSinglePlatFormControl.thisplatform.g_handle, Spos, m_ControlData.mConfigXML.WorkSpeed, m_ControlData.mConfigXML.WorkSpeed * 0.01f, 0);
                }
                if (talsec > edtime)
                {
                    if (NowIndex + 1 < Mlist.Count)
                    {
                        NowIndex++;
                    }
                    else
                    {
                        G_Over();
                    }
                }
            }
            else
            {
                EnabledBeginEnd(true);
            }
        }

        /// <summary>
        /// 添加指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddCmd_Click(object sender, RoutedEventArgs e)
        {
            if(actionCmdWindow !=null)
            {
                actionCmdWindow.Close();
                actionCmdWindow = null;
            }
            actionCmdWindow = new AddActionCommand();
            if (actionCmdWindow.ShowDialog() == true)
            {
                Mlist.Add(actionCmdWindow.movdata);
                UpdateListbox();
            } 
        }

        /// <summary>
        /// 保存到指令文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveCmd_Click(object sender, RoutedEventArgs e)
        {
            CaculatorCurey();
            Movement tmp_Movement = new Movement();
            tmp_Movement.mlist = this.Mlist;
            FileXmlOperate.WriteXmelSerilalzizer(Filepathd, tmp_Movement);
            MessageBox.Show("保存完成");
            //this.ShowTips("保存完成");
        }
        /// <summary>
        /// 修改指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditCmd_Click(object sender, RoutedEventArgs e)
        {
            if (commandListBox.SelectedIndex < 0 || commandListBox.SelectedIndex >= Mlist.Count)
            {
                return;
            }

            if (actionCmdWindow != null)
            {
                actionCmdWindow.Close();
                actionCmdWindow = null;
            }
            actionCmdWindow = new AddActionCommand(Mlist[commandListBox.SelectedIndex]);
            if (actionCmdWindow.ShowDialog() == true)
            {
                Mlist[commandListBox.SelectedIndex] = actionCmdWindow.movdata;
                UpdateListbox();
            }           
        }
        /// <summary>
        /// 删除指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteCmd_Click(object sender, RoutedEventArgs e)
        {
            if (commandListBox.SelectedIndex < 0 || commandListBox.SelectedIndex >= Mlist.Count)
            {
                return;
            }
            Mlist.RemoveAt(commandListBox.SelectedIndex);
            UpdateListbox();
        }
        /// <summary>
        /// 清空指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearCmd_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("确认要删除所有指令","清空警告", 
                MessageBoxButton.OKCancel,MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                Mlist.Clear();
                UpdateListbox();
            }
        }

        private void PackageCmd_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog1.Filter = "指令文件|*.mathb";
            saveFileDialog1.DefaultExt = "mathb";//缺省默认后缀名
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string pathd = saveFileDialog1.FileName;
                CaculatorCurey();
                Movement tmp_Movement = new Movement();
                tmp_Movement.mlist = this.Mlist;
                FileXmlOperate.WriteXmelSerilalzizer(pathd, tmp_Movement);
                MessageBox.Show("打包完成");
            }
        }
        /// <summary>
        /// 导入指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadCmd_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Filter = "指令文件|*.mathb";
            openFileDialog1.DefaultExt = "mathb";//缺省默认后缀名
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string pat = openFileDialog1.FileName;
                if (FileXmlOperate.Exist(pat))
                {
                    Movement tmp_Movement = new Movement();
                    tmp_Movement = (Movement)FileXmlOperate.ReadXmlSerializer(pat, tmp_Movement);
                    for (int i = 0; i < tmp_Movement.mlist.Count; i++)
                    {
                        Mlist.Add(tmp_Movement.mlist[i]);
                    }
                    UpdateListbox();
                }
            }
        }
        // 双击事件
        private void commandListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox listbox = sender as ListBox;
            if(listbox.SelectedItem == null)
            {
                AddCmd_Click(sender, e);
            }
            else
            {
                EditCmd_Click(sender, e);
            }
        }
#endregion
        ////////////////////////////////////////////////////////////////////////////////////
        /// 按钮操作响应程序
        /// /////////////////////////////////////////////////////////////////////////////
        private ManualSet mManualSet = null;
        // 退出按钮
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //public event Action WriteConfig = null;
        private void OnCompleteL()
        {
            EnabledBeginEnd(true);

            float[] abc = m_ControlData.mSixPlaneF.rounds(0, 0, 0);
            float[] Le = m_ControlData.mSixPlaneF.Six_Platform_RoundF(0, 45, 0, abc[0], abc[1], abc[2]);
            int[] Spos = m_ControlData.mSinglePlatFormControl.thisplatform.Conver_LengthTopluse2(Le);
            for (int i = 0; i < 6; i++)
            {
                Spos[i] = Spos[i] + m_ControlData.mConfigXML.DeltalPos[i];
            }
            SinglePlatFormControl.SixSame_ManualStart(m_ControlData.mSinglePlatFormControl.thisplatform.g_handle, Spos, 10, 10 * 0.01f, 0);

            //if (WriteConfig != null)
            {
                WriteConfig();
            }
        }

        private void WriteConfig()
        {
            FileXmlOperate.WriteXmelSerilalzizer(ConfigPath, m_ControlData.mConfigXML);
        }
        // 手动准备按钮，弹出手动操作窗口
        private void ManualReady_Click(object sender, RoutedEventArgs e)
        {
            mManualSet = new ManualSet();
            mManualSet.BeginToMiddle += ConnectAndPrepare; // 居中操作回掉
            mManualSet.OnComplete += OnCompleteL;       // 完成回掉操作
            usemanual = true;
            if (mManualSet.ShowDialog() != true)
            {
                EnabledReadybutton(true);
            }
            
            //this.Close();
        }
        // 自动准备按钮
        private void AutoReady_Click(object sender, RoutedEventArgs e)
        {
            //EnabledReadybutton(false);
            //usemanual = false;
            //GlobalClass.NowCarType = CardType.iMC4xxE_A;
            //if (IsPreparing) return;
            //IsPreparing = true;
            //if (m_ControlData.mSinglePlatFormControl.ConnectToMachine() == false)
            //{
            //    MessageBox.Show("控制卡连接失败，请检查接线");
            //    IsPreparing = false;
            //    return;
            //}            
            //WaitForTime tmpw = new WaitForTime(1.0f, Config);
            usemanual = false;
            ConnectAndPrepare();
        }

        // 是否准备中
        private bool IsPreparing = false;
        private bool usemanual = false;
        /// <summary>
        /// 手动准备运行主程序
        /// </summary>
        [Obsolete("This Funtion is obsolete; use ConnectAndPrepare instead")]
        private void ManualParepare()
        {
            EnabledReadybutton(false);
            usemanual = true;
            GlobalClass.NowCarType = CardType.iMC4xxE_A;
            if (IsPreparing) return;          
            IsPreparing = true;
            if (m_ControlData.mSinglePlatFormControl.ConnectToMachine() == false)
            {
                MessageBox.Show("控制卡连接失败，请检查接线");
                IsPreparing = false;
                return;
            }
            WaitForTime tmpw = new WaitForTime(1.0f, Config);
        }
        private void ConnectAndPrepare()
        {
            EnabledReadybutton(false);
            //usemanual = true;
            GlobalClass.NowCarType = CardType.iMC4xxE_A;
            if (IsPreparing) return;
            IsPreparing = true;
            if (m_ControlData.mSinglePlatFormControl.ConnectToMachine() == false)
            {
                MessageBox.Show("控制卡连接失败，请检查接线");
                IsPreparing = false;
                return;
            }
            WaitForTime tmpw = new WaitForTime(1.0f, Config);
        }
        private void Config()
        {
            int res = m_ControlData.mSinglePlatFormControl.thisplatform.DoConfigCard();
            WaitForTime tmpw = new WaitForTime(1.0f, /*prepareres*/BeginRes);
        }
        //[Obsolete("This class is obsolete; use class B instead")]
        //private void prepareres()
        //{
        //    this.Invoke(new Action(BeginRes));
        //}
        /// <summary>
        /// 
        /// </summary>
        private void BeginRes()
        {
            if (m_ControlData.mSinglePlatFormControl.HaveCompeleteFuwei)
            {
                MessageBox.Show("已经完成复位");
                return;
            }
            if (m_ControlData.mSinglePlatFormControl.thisplatform.isOpen())
            {
                m_ControlData.mSinglePlatFormControl.BegionToResetPlanel(CompleteFuWei, m_ControlData.mConfigXML.FU_Acc, m_ControlData.mConfigXML.FU_speed, (int)m_ControlData.mConfigXML.FU_dist);
            }
            else
            {
                MessageBox.Show("正在复位中");
            }
        }

        private void CompleteFuWei()
        {

            for (int i = 0; i < 6; i++)
            {
                SinglePlatFormControl.P2P_runing_StopOne(m_ControlData.mSinglePlatFormControl.thisplatform.g_handle, i);
            }
            WaitForTime tm = new WaitForTime(1.0f, TCaculator);
        }

        private void TCaculator()
        {
            m_ControlData.mSinglePlatFormControl.thisplatform.CaculatorBili(m_ControlData.mConfigXML.P_XingC);
            m_ControlData.mSinglePlatFormControl.thisplatform.SetOrigPosAll();
            for (int i = 0; i < 6; i++)
            {
                SinglePlatFormControl.P2P_runing_start(m_ControlData.mSinglePlatFormControl.thisplatform.g_handle, i, m_ControlData.mConfigXML.FU_Acc, m_ControlData.mConfigXML.FU_speed, m_ControlData.mConfigXML.FU_speed, 0);
            }
            WaitForTime tm = new WaitForTime(1.0f, ToClear);
            //   m_ControlData.mSinglePlatFormControl.Set_Limitswitch(0);
        }
        private void ToClear()
        {

            for (int i = 0; i < 6; i++)
            {
                SinglePlatFormControl.P2P_runing_start(m_ControlData.mSinglePlatFormControl.thisplatform.g_handle, i, m_ControlData.mConfigXML.FU_Acc, m_ControlData.mConfigXML.FU_speed, m_ControlData.mConfigXML.FU_speed, 0);
            }
            WaitForTime tm = new WaitForTime(1.0f, /*ToReady*/Showms);

        }
        //private void ToReady()
        //{
        //    this.Invoke(new Action(showms));
        //}
        private Action mDocheck = null;
        private DispatcherTimer timer1 = new DispatcherTimer();
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (mDocheck != null)
            {
                mDocheck();
            }
        }
        private void Showms()
        {
            m_ControlData.mSinglePlatFormControl.Set_Limitswitch(0);
            timer1.Start();
            //timer1.Enabled = true;
            //timer1.Tick += Timer1_Tick;
            ///回中完成后会调用ToMiddleComp（）函数
            CalutorToMiddle(ToMiddleComp);
        }
        private void EnabledReadybutton(bool res)
        {
            ManualReady.IsEnabled = res;
            AutoReady.IsEnabled = res;
        }
        private void EnabledBeginEnd(bool res)
        {
            StartWork.IsEnabled = res;
            StopWork.IsEnabled = res;
        }
        private void ToMiddleComp()
        {
            if (usemanual == false)
            {
                EnabledReadybutton(false);
                EnabledBeginEnd(true);
            }
            else if(mManualSet != null)
            {
                mManualSet.ToMiddleCom();
            }
        }
        private int[] TargetPos = new int[6];
        private Action ReachAndCallBack = null;
        private void CalutorToMiddle(Action callback)
        {
            if (usemanual)
            {
                ///计算中间位置每个缸的长度
                ///
                float[] abc = m_ControlData.mSixPlaneF.rounds(0, 0, 0);
                float[] Zitaidata = m_ControlData.mSixPlaneF.Six_Platform_RoundF(0, 45, 0, abc[0], abc[1], abc[2]);

                ///将缸的长度转换成脉冲
                int[] Spos = m_ControlData.mSinglePlatFormControl.thisplatform.Conver_LengthTopluse2(Zitaidata);
                //for (int i = 0; i < 6; i++)
                //{
                //    Spos[i] = Spos[i] + m_ControlData.mConfigXML.DeltalPos[i];
                //}
                TargetPos = Spos;
                for (int j = 0; j < 6; j++)
                {
                    SinglePlatFormControl.P2P_runing_start(m_ControlData.mSinglePlatFormControl.thisplatform.g_handle, j, 1, m_ControlData.mConfigXML.FU_speed, m_ControlData.mConfigXML.FU_speed, Spos[j]);
                }
            }
            else
            {
                ///计算中间位置每个缸的长度
                ///
                float[] abc = m_ControlData.mSixPlaneF.rounds(0, 0, 0);
                float[] Zitaidata = m_ControlData.mSixPlaneF.Six_Platform_RoundF(0, 45, 0, abc[0], abc[1], abc[2]);

                ///将缸的长度转换成脉冲
                int[] Spos = m_ControlData.mSinglePlatFormControl.thisplatform.Conver_LengthTopluse2(Zitaidata);
                for (int i = 0; i < 6; i++)
                {
                    Spos[i] = Spos[i] + m_ControlData.mConfigXML.DeltalPos[i];
                }
                TargetPos = Spos;
                for (int j = 0; j < 6; j++)
                {
                    SinglePlatFormControl.P2P_runing_start(m_ControlData.mSinglePlatFormControl.thisplatform.g_handle, j, 1, m_ControlData.mConfigXML.FU_speed, m_ControlData.mConfigXML.FU_speed, Spos[j]);
                }

            }

            //回中后回调赋值给ReachAndCallBack
            ReachAndCallBack = callback;
            //将CheckToDoCallback检查回中是否完成放到mthread1线程
            mDocheck = null;
            mDocheck = CheckToDoCallback;
        }
        private void CheckToDoCallback()
        {
            ///回去控制卡的脉冲返回
            int[] mback = m_ControlData.mSinglePlatFormControl.GetCommendBack();

            int[] mback2 = new int[6];
            for (int j = 0; j < mback2.Length; j++)
            {
                mback2[j] = mback[j];
            }
            //回中完成
            if (CheckPulseresult(TargetPos, mback2))
            {
                mDocheck = null;
                if (ReachAndCallBack != null)
                {
                    //调用回中完成的委托也就是ToMiddleComp（）
                    ReachAndCallBack();
                }
            }

        }
        public bool CheckPulseresult(int[] targetpos, int[] nowpos)
        {
            if (targetpos.Length != nowpos.Length)
            {
                return false;
            }
            bool resd = true;
            for (int i = 0; i < targetpos.Length; i++)
            {
                if (Math.Abs(targetpos[i] - nowpos[i]) > 5)
                {
                    resd = false;
                    break;
                }
            }
            return resd;
        }
        private DispatcherTimer timer2 = new DispatcherTimer(); 
        private void StartWork_Click(object sender, RoutedEventArgs e)
        {
            rx = 0;
            ry = 0;
            rz = 0;
            NowIndex = 0;
            StartTime = DateTime.Now;
            BeginMove = true;

            timer2.Start();
            workInThread = CaculatorTosend;
            ////

            _chart.Series[0].DataPoints.Clear();
            _chart.Series[1].DataPoints.Clear();
            _chart.Series[2].DataPoints.Clear();
            // 初始化一个新的Axis 
            Axis xAxis = new Axis();
            // 设置axis的属性 
            xAxis.Enabled = true;
            xAxis.StartFromZero = true;
            xAxis.AxisMinimum = 100;
            //图表的X轴坐标按什么来分类，如时分秒 
            xAxis.Interval = 10;
            //图表中的X轴坐标间隔如2，3，20等，单位为xAxis.IntervalType设置的时分秒。 
            xAxis.IntervalType = IntervalTypes.Number;
            //给图标添加Axis 
            // AxisLabels xLabel = new AxisLabels();            

            _chart.AxesX.Clear();
            _chart.AxesX.Add(xAxis);

            StartWork.IsEnabled = false;
        }

        private void StopWork_Click(object sender, RoutedEventArgs e)
        {
            G_Over();

            timer2.Stop();
        }
    }
}
