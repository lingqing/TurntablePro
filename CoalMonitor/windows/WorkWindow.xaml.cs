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
using System.Threading;

namespace CoalMonitor.windows
{
    /// <summary>
    /// WorkWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WorkWindow : Window
    {
        private string Configfolder = System.AppDomain.CurrentDomain.BaseDirectory;
        private string Filepathd = "";
        private List<Movdata> Mlist = new List<Movdata>();
        private ControlData m_ControlData = ControlData.ShareInstance();
        // 发送工作线程
        private Thread SendThread = null;
        private bool isRunning = false;
        private Action workInThread = null;

        private bool BegionMove = false;
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
            Mlist.Clear();

            if (FileXmlOperate.Exist(Filepathd))
            {

                Movement tmp_Movement = new Movement();
                tmp_Movement = (Movement)FileXmlOperate.ReadXmlSerializer(Filepathd, tmp_Movement);
                Mlist = tmp_Movement.mlist;
                UpdateListbox();
            }
            //Enabled_begionEnd(false);
            //Enabled_Readybutton(true);
            //isRunning = true;
            //SendThread = new Thread(SendMethod);
            //SendThread.Start();
            Intlized();
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
        private void UpdateListbox()
        {
            commandListBox.Items.Clear();            
            float totaltime = 0;
            CaculatorCurey();
            for (int i = 0; i < Mlist.Count; i++)
            {
                totaltime = Mlist[i].Caculator(totaltime);
                commandListBox.Items.Add(i + "--" + GetMovString(Mlist[i], totaltime));
            }
        }

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
                    resd = "(" + timer + "s" + ")" + "偏航" + "--起始角度:" + movdata.StartAngle + "--目标角度:" + movdata.TargetAngle + "--时间:" + movdata.NeedTime;
                    break;
                case 1:
                    resd = "(" + timer + "s" + ")" + "滚转" + "--起始角度:" + movdata.StartAngle + "--目标角度:" + movdata.TargetAngle + "--时间:" + movdata.NeedTime;
                    break;
                case 2:
                    resd = "(" + timer + "s" + ")" + "俯仰" + "--起始角度:" + movdata.StartAngle + "--目标角度:" + movdata.TargetAngle + "--时间:" + movdata.NeedTime;
                    break;
                case 3:
                    resd = "(" + timer + "s" + ")" + "延迟" + "--时间:" + movdata.NeedTime;
                    break;
                default:
                    resd = "错误指令";
                    break;

            }
            return resd;
        }

        private void G_Over()
        {
            BegionMove = false;
            workInThread = null;
        }

        //private override 

        private float rx = 0;
        private float ry = 0;
        private float rz = 0;
        private float xx = 0;
        private float yy = 45;
        private float zz = 0;
        private double talsec = 0;
        private void CaculatorTosend()
        {
            if (BegionMove)
            {
                TimeSpan tmpd = DateTime.Now - StartTime;
                talsec = tmpd.TotalSeconds;
                float starttime = Mlist[NowIndex].StartTime;
                float edtime = Mlist[NowIndex].EndTime;
                int model = Mlist[NowIndex].Model;
                switch (model)
                {
                    case 0:
                        rz = Mlist[NowIndex].GetAngle((float)talsec);
                        break;
                    case 1:
                        ry = Mlist[NowIndex].GetAngle((float)talsec);
                        break;
                    case 2:
                        rx = Mlist[NowIndex].GetAngle((float)talsec);
                        break;
                    case 3:
                        break;
                    default:
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

        /// <summary>
        /// 添加指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddCmd_Click(object sender, RoutedEventArgs e)
        {
            //if (mMovtionform != null)
            //{
            //    mMovtionform.Close();
            //    mMovtionform = null;
            //}
            //mMovtionform = new Movtionform();
            //mMovtionform.OnBack_ += OnCallBack;
            //mMovtionform.OnChange_ += OnChangeBack;
            //mMovtionform.UIModel = 0;
            //mMovtionform.Owner = this;
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
            
            //mMovtionform.OnShowD(0, 0, 0, 0, Mlist.Count);
            //mMovtionform.ShowDialog();
        }
    }
}
