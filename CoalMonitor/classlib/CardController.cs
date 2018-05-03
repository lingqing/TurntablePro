 //#define card
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
//using System.Windows.Forms;
using System.Windows;
using CoalMonitor.classlib;
using CoalMonitor.classlib.PK4;
namespace CoalMonitor.classlib
{
#if card
    public class CardController
    {
        public int BrakeAxie01 = 17;
        public int BrakeAxie02 = 18;
        public int BrakeAxie03 = 19;
        public int BrakeAxie04 = 20;
        public int BrakeAxie05 = 21;
        public int BrakeAxie06 = 22;
        public CardController()
        {
            HaveCompeleteFuwei = false;
        }
        public List<OnePlatform> M_platformlist = new List<OnePlatform>();
        public List<OnePlatform> M_platErrorMist = new List<OnePlatform>();
        public System.Action mDocheck = null;
        public System.Action FuweiOkDo = null;
        public System.Action SevenOkDo = null;
        ControlData mControlData = ControlData.ShareInstance();
        public bool IsInOpen
        {
            get
            {
                if (M_platformlist.Count == 0)
                {
                    return false;
                }
                else
                {
                    bool res = false;
                    for (int i = 0; i < M_platformlist.Count; i++)
                    {
                        if (M_platformlist[i].isOpen())
                        {
                            res = true;
                            break;
                        }
                    }
                    return res;
                }
            }
        }
        public bool IsInFuwei = false;
        public bool ConnectToMachine()
        {
            if (M_platformlist.Count > 0)
            {
                M_platformlist.Clear();
            }
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    try
                    {
                        IntPtr tmphand = IMC_Pkg.PKG_IMC_Open(i, j);
                        if (tmphand != IntPtr.Zero)
                        {
                            OnePlatform tmpOne = new OnePlatform();
                            tmpOne.g_handle = tmphand;
                            tmpOne.PlatIDnumb = j;
                            DoConfigCard(ref tmpOne);
                            M_platformlist.Add(tmpOne);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return true;

        }
        bool IstoUp = false;
        public bool HaveCompeleteFuwei = false;
        #region 复位模块
        public void Set_FuWei_data(float acc, float stav, float targetv, int dist)
        {
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                M_platformlist[i].FuweiAcc = acc;
                M_platformlist[i].Fuweistarv = stav;
                M_platformlist[i].FuweiTargetv = targetv;
                M_platformlist[i].FuweiDistance = dist;
            }
        }
        public bool CheckLimitSwitch_UP()
        {
            bool res = true;
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                int[] tmp = GetLimitSwitch(i);
                for (int j = 0; j < M_platformlist[i].g_naxis; j++)
                {
                    if (j < 7 && tmp[j] == 1)
                    {
                        res = false;
                        break;
                    }
                }
            }
            return res;
        }
        public bool CheckLimitSwitch_Down()
        {
            bool res = true;
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                int[] tmp = GetLimitSwitch(i);
                for (int j = M_platformlist[i].g_naxis; j < tmp.Length; j++)
                {
                    if (j < tmp.Length - 1 && tmp[j] == 1)
                    {
                        res = true;
                        break;
                    }
                }
            }
            return res;
        }
        /// <summary>
        /// 准备复位
        /// </summary>
        /// <returns></returns>
        public void PrePareResetPos_Up()
        {


            if (IsInOpen == false)
            {
                throw new Exception("控制卡尚未打开");
            }
            if (IsInFuwei)
            {
                MessageBox.Show("正在复位中");
                return;
            }
            IstoUp = true;
            IsInFuwei = true;
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                M_platformlist[i].ResetToUp = false;
            }

            SetOrigPosAll();
            Set_Limitswitch(1);///开启限位功能
            mDocheck = null;
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                mDocheck += M_platformlist[i].FuweiMethod;
            }
            mDocheck += DoToCheckCompelete;
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                M_platformlist[i].BegionToUp();
            }

        }
        public void PrePareResetPos_Down()
        {
            IstoUp = false;
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                M_platformlist[i].Fu_GotoDown();
            }
        }
        /// <summary>
        /// 复位时需要将这个函数放到一个线程里
        /// </summary>
        public void FuweiCheck_method()
        {
            if (mDocheck != null)
            {
                mDocheck();
            }
        }
        void DoToCheckCompelete()
        {
            if (IstoUp)
            {
                bool res = true;
                for (int i = 0; i < M_platformlist.Count; i++)
                {
                    if (M_platformlist[i].ResetToUp == false)
                    {
                        res = false;
                        break;
                    }
                }
                //所有平台开始向下复位
                if (res)
                {
                    WaitForTime wait = new WaitForTime(1.0f, AllToDown);
                    IstoUp = false;
                }
            }
            else
            {
                bool res = true;
                for (int i = 0; i < M_platformlist.Count; i++)
                {
                    if (M_platformlist[i].HaveResetPos == false)
                    {
                        res = false;
                        break;
                    }
                }
                if (res)
                {
                    mDocheck = null;
                    AllPlatformCompelete();
                }
            }
        }
        void AllToDown()
        {
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                PrePareResetPos_Down();

            }
        }
        void AllPlatformCompelete()
        {
            IsInFuwei = false;
            HaveCompeleteFuwei = true;
            if (FuweiOkDo != null)
            {
                FuweiOkDo();
            }
        }
        #endregion
        int DoConfigCard(ref OnePlatform Tempone)
        {
            try
            {
                if (Tempone.isOpen())
                {
                    int st = 0;
                    string err = "";
                    IMC_Pkg.PKG_IMC_InitCfg(Tempone.g_handle);
                    Tempone.g_naxis = IMC_Pkg.PKG_IMC_GetNaxis(Tempone.g_handle);//获得控制卡最大轴数
                    st = IMC_Pkg.PKG_IMC_ClearIMC(Tempone.g_handle);
                    st = IMC_Pkg.PKG_IMC_Emstop(Tempone.g_handle, 0);
                    for (int i = 0; i < Tempone.g_naxis; i++)
                    {
                        st = IMC_Pkg.PKG_IMC_ClearError(Tempone.g_handle, i);
                        if (st == 0)
                            break;
                        st = IMC_Pkg.PKG_IMC_ClearAxis(Tempone.g_handle, i);
                        if (st == 0)
                            break;
                        st = IMC_Pkg.PKG_IMC_SetStopfilt(Tempone.g_handle, 1, i);
                        if (st == 0)
                            break;
                        st = IMC_Pkg.PKG_IMC_SetExitfilt(Tempone.g_handle, 0, i);
                        if (st == 0)
                            break;
                        st = IMC_Pkg.PKG_IMC_SetPulWidth(Tempone.g_handle, Tempone.m_cfg[i].steptime, i);
                        if (st == 0)
                            break;
                        st = IMC_Pkg.PKG_IMC_SetPulPolar(Tempone.g_handle, Tempone.m_cfg[i].pulpolar, Tempone.m_cfg[i].dirpolar, i);
                        if (st == 0)
                            break;
                        st = IMC_Pkg.PKG_IMC_SetEncpEna(Tempone.g_handle, Tempone.m_cfg[i].encpena, i);
                        if (st == 0)
                            break;
                        st = IMC_Pkg.PKG_IMC_SetEncpMode(Tempone.g_handle, Tempone.m_cfg[i].encpmode, Tempone.m_cfg[i].encpdir, i);
                        if (st == 0)
                            break;
                        st = IMC_Pkg.PKG_IMC_SetEncpRate(Tempone.g_handle, Tempone.m_cfg[i].encpfactor, i);
                        if (st == 0)
                            break;
                        st = IMC_Pkg.PKG_IMC_SetVelAccLim(Tempone.g_handle, Tempone.m_cfg[i].vellim, Tempone.m_cfg[i].acclim, i);
                        if (st == 0)
                            break;
                        st = IMC_Pkg.PKG_IMC_Setlimit(Tempone.g_handle, Tempone.m_cfg[i].plimitena, Tempone.m_cfg[i].plimitpolar, Tempone.m_cfg[i].nlimitena, Tempone.m_cfg[i].nlimitpolar, i);
                        if (st == 0)
                            break;
                        st = IMC_Pkg.PKG_IMC_SetAlm(Tempone.g_handle, Tempone.m_cfg[i].almena, Tempone.m_cfg[i].almpolar, i);
                        if (st == 0)
                            break;
                        st = IMC_Pkg.PKG_IMC_SetSmooth(Tempone.g_handle, Tempone.m_cfg[i].Smooth, i);
                        if (st == 0)
                            break;
                        st = IMC_Pkg.PKG_IMC_SetEna(Tempone.g_handle, Tempone.m_cfg[i].ena, i);//使能驱动器放在最后
                        if (st == 0)
                            break;
                    }
                    if (st == 0)
                    {
                        err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                        throw new Exception(err);
                    }

                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// 关闭使能
        /// </summary>
        public void RemoveSon(int PID)
        {
            int st = 0;
            string err;
            for (int i = 0; i < M_platformlist[PID].g_naxis; i++)
            {
                st = IMC_Pkg.PKG_IMC_SetEna(M_platformlist[PID].g_handle, 0, i);
                if (st == 0)
                {
                    err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                }
            }
        }
        /// <summary>
        /// 脉冲清零
        /// </summary>
        /// <param name="Lid">控制卡在列表中的编号</param>
        /// <param name="aix">轴号</param>
        public void SetOrigPos(int PID, int aix)
        {
            if (M_platformlist[PID].isOpen())
            {
                int st;
                string err;
                st = IMC_Pkg.PKG_IMC_SetPos(M_platformlist[PID].g_handle, 0, aix);
                if (st == 0)
                {
                    err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                    throw new Exception(err);
                }
            }
        }
        /// <summary>
        /// 点对点运动
        /// </summary>
        /// <param name="PID"></param>
        /// <param name="m_axis"></param>
        /// <param name="m_acc"></param>
        /// <param name="m_startvel"></param>
        /// <param name="m_tgvel"></param>
        /// <param name="m_pos"></param>
        public void P2P_runing_start(int PID, int m_axis, double m_acc, double m_startvel, double m_tgvel, int m_pos)
        {
            if (M_platformlist[PID].isOpen())
            {
                int st;
                string err;
                st = IMC_Pkg.PKG_IMC_SetAccel(M_platformlist[PID].g_handle, m_acc, m_acc, m_axis);
                if (st != 0)
                {
                    st = IMC_Pkg.PKG_IMC_MoveAbs(M_platformlist[PID].g_handle, m_pos, m_startvel, m_tgvel, 0, m_axis);
                }
                if (st == 0)
                {
                    err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                    throw new Exception(err);
                }
            }
        }
        public static void StopChabu(IntPtr PID)
        {
            IMC_Pkg.PKG_IMC_PFIFOstop(PID, (int)IMC_Pkg.FIFO_SEL.SEL_PFIFO1);
        }
        public static void SetSmoothdata(IntPtr PID, short smth) {
            for (int i = 0; i < 6; i++)
            {
                IMC_Pkg.PKG_IMC_SetSmooth(PID, smth, i);
            }

        }
        public static void StopP2P(IntPtr PID)
        {
            for (int i = 0; i < 6; i++)
            {
                IMC_Pkg.PKG_IMC_P2Pstop(PID, i);
            }
        }
        public static void P2P_runing_start(IntPtr PID, int m_axis, double m_acc, double m_startvel, double m_tgvel, int m_pos)
        {
            int st;
            string err;
            st = IMC_Pkg.PKG_IMC_SetAccel(PID, m_acc, m_acc, m_axis);
            if (st != 0)
            {
                st = IMC_Pkg.PKG_IMC_MoveAbs(PID, m_pos, m_startvel, m_tgvel, 0, m_axis);
            }
            if (st == 0)
            {
                err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                throw new Exception(err);
            }

        }
        public static void P2P_runing_start(int wait, IntPtr PID, int m_axis, double m_acc, double m_startvel, double m_tgvel, int m_pos)
        {
            int st;
            string err;
            st = IMC_Pkg.PKG_IMC_SetAccel(PID, m_acc, m_acc, m_axis);
            if (st != 0)
            {
                st = IMC_Pkg.PKG_IMC_MoveAbs(PID, m_pos, m_startvel, m_tgvel, wait, m_axis);
            }
            if (st == 0)
            {
                err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                throw new Exception(err);
            }

        }
        public static void SixSame_ManualStart(IntPtr PID, int[] Posd, double targetv, double acc, int wait)
        {

            double m_acc, m_tgvel, m_endvel, m_rate;
            int m_segnum, m_axisCnt, m_wait;
            int[] m_axis = new int[6];
            int[] m_pos = new int[6];
            bool m_isAbs = true;

            m_axisCnt = 0;
            for (int j = 0; j < 6; j++)
            {//获得直线的位置
                m_axis[m_axisCnt] = j;
                m_pos[m_axisCnt] = Posd[j];
                m_axisCnt++;
            }
            m_segnum = 1;
            m_isAbs = true;
            m_tgvel = targetv;
            m_acc = acc;
            m_rate = 1;
            m_endvel = 0;
            m_wait = wait;
            //需要等待圆弧插补运动完成，这会阻塞主线程使程序看起来像无响应状态
            //因此需要使用线程

            int st, i, n, fifo;
            string err;
            int[] segpos;
            segpos = new int[m_segnum * m_axisCnt];
            for (i = 0; i < m_segnum; i++)
            {
                for (n = 0; n < m_axisCnt; n++)
                    segpos[i * m_axisCnt + n] = m_pos[n];
            }
            fifo = (int)IMC_Pkg.FIFO_SEL.SEL_PFIFO1;//使用插补空间 1
            st = IMC_Pkg.PKG_IMC_PFIFOclear(PID, fifo); //清空PFIFO
            if (st != 0)
            {
                st = SetPFIFO(PID, m_acc, m_rate, fifo);
            }
            if (st != 0)
            {
                st = IMC_Pkg.PKG_IMC_AxisMap(PID, m_axis, m_axisCnt, fifo);//映射轴
            }
            if (st != 0)
            {
                if (m_isAbs)//使用绝对坐标
                {
                    if (m_segnum > 1)
                        st = IMC_Pkg.PKG_IMC_MulLine_Pos(PID, segpos, m_axisCnt, m_segnum, m_tgvel, m_endvel, m_wait, fifo);
                    else
                        st = IMC_Pkg.PKG_IMC_Line_Pos(PID, m_pos, m_axisCnt, m_tgvel, m_endvel, m_wait, fifo);
                }
                else
                {//使用相对坐标
                    if (m_segnum > 1)
                        st = IMC_Pkg.PKG_IMC_MulLine_Dist(PID, segpos, m_axisCnt, m_segnum, m_tgvel, m_endvel, m_wait, fifo);
                    else
                        st = IMC_Pkg.PKG_IMC_Line_Dist(PID, m_pos, m_axisCnt, m_tgvel, m_endvel, m_wait, fifo);
                }
            }
            if (st == 0)
            {
                err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                throw new Exception(err);
            }
        }
        static int SetPFIFO(IntPtr handle, double acc, double rate, int fifo)
        {
            int st;
            st = IMC_Pkg.PKG_IMC_PFIFOrun(handle, fifo);			//启用插补空间
            if (st == 0)
                return st;
            st = IMC_Pkg.PKG_IMC_SetPFIFOaccel(handle, acc, fifo);	//设置加速度
            if (st == 0)
                return st;
            st = IMC_Pkg.PKG_IMC_SetFeedrate(handle, rate, fifo);	//设置进给倍率
            return st;
        }
        /// <summary>
        /// 获取返回的脉冲
        /// </summary>
        /// <returns></returns>
        public int[] GetCommendBack(int PID)
        {
            int i = 0;
            if (!M_platformlist[PID].isOpen())
            {
                throw new Exception("控制卡尚未打开");
            }
            if (IMC_Pkg.PKG_IMC_GetEncp(M_platformlist[PID].g_handle, M_platformlist[PID].m_encp, M_platformlist[PID].g_naxis) != 0)//获得反馈编码器位置
            {
                for (i = 0; i < M_platformlist[PID].g_naxis; i++)
                {
                    if (M_platformlist[PID].m_encp[i] != M_platformlist[PID].m_encp_t[i])
                    {
                        M_platformlist[PID].m_encp_t[i] = M_platformlist[PID].m_encp[i];
                    }
                }
            }
            return M_platformlist[PID].m_encp;

        }
        public void Open_out(int PID, int OutNumb, int open)
        {
            if (!M_platformlist[PID].isOpen())
            {
                throw new Exception("控制卡尚未打开");
            }
            int outdata;
            int st;
            string err;
            outdata = open;
            st = IMC_Pkg.PKG_IMC_SetOut(M_platformlist[PID].g_handle, OutNumb, outdata, (int)IMC_Pkg.FIFO_SEL.SEL_IFIFO);
            if (st == 0)
            {
                err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                throw new Exception(err);
            }
        }
        public ushort[] GetIO_out(int PID)
        {
            if (!M_platformlist[PID].isOpen())
            {
                throw new Exception("控制卡尚未打开");
            }
            int i;
            if (IMC_Pkg.PKG_IMC_GetGout(M_platformlist[PID].g_handle, M_platformlist[PID].m_gout) != 0)
            {
                for (i = 0; i < 48; i++)
                {
                    if (M_platformlist[PID].m_gout[i] != M_platformlist[PID].m_gout_s[i])
                    {
                        M_platformlist[PID].m_gout_s[i] = M_platformlist[PID].m_gout[i];
                    }
                }
            }

            return M_platformlist[PID].m_gout;

        }
        public ushort[] GetIO_In(int PID)
        {


            if (!M_platformlist[PID].isOpen())
            {
                throw new Exception("控制卡尚未打开");
            }
            int i = 0;
            if (IMC_Pkg.PKG_IMC_GetGin(M_platformlist[PID].g_handle, M_platformlist[PID].m_gin) != 0)
            {
                for (i = 0; i < 32; i++)
                {
                    if (M_platformlist[PID].m_gin[i] != M_platformlist[PID].m_gin_t[i])
                    {
                        M_platformlist[PID].m_gin_t[i] = M_platformlist[PID].m_gin[i];
                    }
                }
            }

            return M_platformlist[PID].m_gin_t;

        }
        public int[] GetLimitSwitch(int PID)
        {


            if (!M_platformlist[PID].isOpen())
            {
                throw new Exception("控制卡尚未打开");
            }
            if (IMC_Pkg.PKG_IMC_GetAin(M_platformlist[PID].g_handle, M_platformlist[PID].m_aio, M_platformlist[PID].g_naxis) != 0)
            {
                for (int i = 0; i < M_platformlist[PID].g_naxis; i++)
                {
                    for (int k = 0; k < M_platformlist[PID].g_naxis; k++)
                    {
                        if (M_platformlist[PID].m_aio[i, k] != M_platformlist[PID].m_aio_s[i, k])
                        {
                            M_platformlist[PID].m_aio_s[i, k] = M_platformlist[PID].m_aio[i, k];
                        }
                    }
                }

            }
            int[] tmps = new int[2 * M_platformlist[PID].g_naxis];
            for (int i = 0; i < 2; i++)
            {
                for (int k = 0; k < M_platformlist[PID].g_naxis; k++)
                {
                    tmps[i * M_platformlist[PID].g_naxis + k] = M_platformlist[PID].m_aio_s[k, i];
                }
            }
            return tmps;
        }
        /// <summary>
        /// 所有轴清零
        /// </summary>
        public void SetOrigPosAll()
        {
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    SetOrigPos(i, j);
                }
            }
        }
        public void Set_Limitswitch(int Open)
        {
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                if (!M_platformlist[i].isOpen())
                {
                    throw new Exception("控制卡尚未打开");
                }
                int st;
                string err;
                for (int j = 0; j < M_platformlist[i].g_naxis; j++)
                {
                    st = IMC_Pkg.PKG_IMC_Setlimit(M_platformlist[i].g_handle, Open, 0, Open, 0, j);
                    if (st == 0)
                    {
                        err = IMC_Pkg.PKG_IMC_GetFunErrStr();

                        throw new Exception(err);
                    }
                }

            }
        }
        public static void P2P_runing_StopOne(IntPtr hanled, int Axis)
        {

            int st = IMC_Pkg.PKG_IMC_P2Pstop(hanled, Axis);
            st = IMC_Pkg.PKG_IMC_P2Pvel(hanled, 0, Axis);
            if (st == 0)
            {
                string err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                throw new Exception(err);
            }
        }
        public void Controller_Pause()
        {
            for (int j = 0; j < M_platformlist.Count; j++)
            {
                int st;
                string err;
                for (int i = 0; i < M_platformlist[j].g_naxis; i++)
                {
                    st = IMC_Pkg.PKG_IMC_SetAccel(M_platformlist[j].g_handle, 1, 1, i);
                    if (st == 0)
                    {
                        err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                        throw new Exception(err);
                    }
                }
                st = IMC_Pkg.PKG_IMC_Pause(M_platformlist[j].g_handle, 1);
                if (st == 0)
                {
                    err = IMC_Pkg.PKG_IMC_GetFunErrStr();

                    throw new Exception(err);
                }
            }

        }
        public void Controller_PauseRemove()
        {
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                int st;
                string err;
                st = IMC_Pkg.PKG_IMC_Pause(M_platformlist[i].g_handle, 0);
                if (st == 0)
                {
                    err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                    throw new Exception(err);
                }
            }
        }
        /// <summary>
        /// 关闭抱闸
        /// </summary>
        public void Closed_Aixs_brake()
        {
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                Open_out(i, BrakeAxie01, 0);
                Open_out(i, BrakeAxie02, 0);
                Open_out(i, BrakeAxie03, 0);
                Open_out(i, BrakeAxie04, 0);
                Open_out(i, BrakeAxie05, 0);
                Open_out(i, BrakeAxie06, 0);
            }

        }
        public void Open_Aixs_brake()
        {

            for (int i = 0; i < M_platformlist.Count; i++)
            {
                Open_out(i, BrakeAxie01, 1);
                Open_out(i, BrakeAxie02, 1);
                Open_out(i, BrakeAxie03, 1);
                Open_out(i, BrakeAxie04, 1);
                Open_out(i, BrakeAxie05, 1);
                Open_out(i, BrakeAxie06, 1);
            }
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void ClosedControllercard()
        {
            Controller_EM_Stop();
            RemoveEnabled();
            Closed_card();
        }
        /// <summary>
        /// 解除使能
        /// </summary>
        public void RemoveEnabled()
        {
            for (int j = 0; j < M_platformlist.Count; j++)
            {
                RemoveSon(j);
            }
        }
        /// <summary>
        /// 紧急停止
        /// </summary>
        public void Controller_EM_Stop()
        {
            for (int j = 0; j < M_platformlist.Count; j++)
            {
                int st;
                string err;
                for (int i = 0; i < M_platformlist[j].g_naxis; i++)
                {
                    st = IMC_Pkg.PKG_IMC_SetExitfilt(M_platformlist[j].g_handle, 1, i);
                    if (st == 0)
                    {
                        err = IMC_Pkg.PKG_IMC_GetFunErrStr();

                        throw new Exception(err);
                    }
                }
                st = IMC_Pkg.PKG_IMC_Emstop(M_platformlist[j].g_handle, 1);
                if (st == 0)
                {
                    err = IMC_Pkg.PKG_IMC_GetFunErrStr();

                    throw new Exception(err);
                }
            }
        }
        void EStopPlat(IntPtr hand)
        {
            int st;
            string err;

            st = IMC_Pkg.PKG_IMC_Emstop(hand, 1);
            if (st == 0)
            {
                err = IMC_Pkg.PKG_IMC_GetFunErrStr();

                throw new Exception(err);
            }

        }
        public void Closed_card()
        {
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                IMC_Pkg.PKG_IMC_Close(M_platformlist[i].g_handle);
                M_platformlist[i].g_handle = IntPtr.Zero;
            }
        }
        /// <summary>
        /// 获取平滑度
        /// </summary>
        /// <returns></returns>
        public double[] Getsmooth(int PID)
        {
            //   GetCardconfig();
            double[] tmpspeed = new double[M_platformlist[PID].g_naxis];
            for (int i = 0; i < M_platformlist[PID].g_naxis; i++)
            {
                tmpspeed[i] = M_platformlist[PID].m_cfg[i].Smooth;
            }
            return tmpspeed;
        }
        public void SetSmoothData(int PID, short smoothdu)
        {
            if (M_platformlist[PID].isOpen() == false)
            {
                return;
            }
            for (int i = 0; i < M_platformlist[PID].g_naxis; i++)
            {
                M_platformlist[PID].m_cfg[i].Smooth = smoothdu;
                IMC_Pkg.PKG_IMC_SetSmooth(M_platformlist[PID].g_handle, smoothdu, i);
            }
        }
        public bool getError()
        {
            if (IsInOpen)
            {
                bool res = true;
                for (int i = 0; i < M_platformlist.Count; i++)
                {
                    if (M_platformlist[i].GetFucEror() == false)
                    {
                        res = false;
                        EStopPlat(M_platformlist[i].g_handle);
                        M_platErrorMist.Add(M_platformlist[i]);
                        M_platformlist.RemoveAt(i);
                    }
                }
                return res;
            }
            else
            {
                return true;
            }

        }
        public void EPauseStopPlatAll()
        {
            for (int j = 0; j < M_platformlist.Count; j++)
            {
                int st;
                string err;
                for (int i = 0; i < M_platformlist[j].g_naxis; i++)
                {
                    st = IMC_Pkg.PKG_IMC_SetAccel(M_platformlist[j].g_handle, 1, 1, i);
                    if (st == 0)
                    {
                        err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                        throw new Exception(err);
                    }
                }
                st = IMC_Pkg.PKG_IMC_Pause(M_platformlist[j].g_handle, 1);
                if (st == 0)
                {
                    err = IMC_Pkg.PKG_IMC_GetFunErrStr();

                    throw new Exception(err);
                }
            }
        }
        public void RemoveEstopPlatAll()
        {
            for (int i = 0; i < M_platformlist.Count; i++)
            {
                int st;
                string err;
                st = IMC_Pkg.PKG_IMC_Pause(M_platformlist[i].g_handle, 0);
                if (st == 0)
                {
                    err = IMC_Pkg.PKG_IMC_GetFunErrStr();

                    throw new Exception(err);
                }
            }
        }
    }
#endif 
    public class SinglePlatFormControl {
        ControlData mControlData = ControlData.ShareInstance();
        private Thread mResetThread = null;
        private bool mResetThreadIsRunning = false;
        private Action DoInThread = null;
        private Action FuWeiComplete = null;
        private float F_AccD = 0;
        private float F_Speed = 0;
        private int F_Dist = 0;
        private bool IsInFuWei = false;
        private bool IsToUp = false;
        public  bool HaveCompeleteFuwei = false;

        public OnePlatform thisplatform = null;
        public bool ConnectToMachine()
        {
            bool res = false;
            for (int i = 0; i < 5; i++)
            {
                if (res)
                {
                    break;
                }
                for (int j = 0; j < 10; j++)
                {

                    if (ConnectToCard(i, j))
                    {
                        res = true;
                        break;
                    }
                }
            }

            return res;

        }
        #region 可切换
        public void P2P_runing_start(int m_axis, double m_acc, double m_startvel, double m_tgvel, int m_pos)
        {
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                int st;
                string err;
                st = IMC_Pkg.PKG_IMC_SetAccel(thisplatform.g_handle, m_acc, m_acc, m_axis);
                if (st != 0)
                {
                    st = IMC_Pkg.PKG_IMC_MoveAbs(thisplatform.g_handle, m_pos, m_startvel, m_tgvel, 0, m_axis);
                }
                if (st == 0)
                {
                    err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                    throw new Exception(err);
                }
            }
            else
            {
                int st;
                string err;
                st = IMC_Pkg4.PKG_IMC_SetAccel(thisplatform.g_handle, m_acc, m_acc, m_axis);
                if (st != 0)
                {
                    st = IMC_Pkg4.PKG_IMC_MoveAbs(thisplatform.g_handle, m_pos, m_startvel, m_tgvel, m_tgvel, 0, m_axis);
                }
                if (st == 0)
                {
                    err = IMC_Pkg4.GetFunErrStr();
                    throw new Exception(err);
                }

            }

        }
        public ushort[,] GetLimitSwitch()
        {
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                int st = IMC_Pkg.PKG_IMC_GetAin(thisplatform.g_handle, thisplatform.m_aio, thisplatform.g_naxis);
                if (st == 0)
                {
                    string err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                    throw new Exception(err);

                }
                return thisplatform.m_aio;
            }
            else
            {

                int st = IMC_Pkg4.PKG_IMC_GetAin(thisplatform.g_handle, thisplatform.m_aio, thisplatform.g_naxis);
                if (st == 0)
                {
                    string err = IMC_Pkg4.GetFunErrStr();
                    throw new Exception(err);

                }
                return thisplatform.m_aio;
            }
        }
        public string[] GetControlNetCard()
        {
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                byte[] info = new byte[16 * 256];
                int num = 0, i;
                List<string> str = new List<string>();

                //程序初始化时搜索PC中的网卡
                if (IMC_Pkg.PKG_IMC_FindNetCard(info, ref num) != 0)
                {
                    for (i = 0; i < num; i++)
                    {
                        string ty = System.Text.Encoding.Default.GetString(info, i * 256, 256);
                        str.Add(ty);
                    }

                }
                return str.ToArray();
            }
            else
            {

                byte[] info = new byte[16 * 256];
                int num = 0, i;
                List<string> str = new List<string>();

                //程序初始化时搜索PC中的网卡
                if (IMC_Pkg4.PKG_IMC_FindNetCard(info, ref num) != 0)
                {
                    for (i = 0; i < num; i++)
                    {
                        string ty = System.Text.Encoding.Default.GetString(info, i * 256, 256);
                        str.Add(ty);
                    }

                }
                return str.ToArray();
            }
        }
        public bool ConnectToCard(int netid, int cardid)
        {
            bool res = false;
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                try
                {
                    IntPtr tmphand = IMC_Pkg.PKG_IMC_Open(netid, cardid);
                    if (tmphand != IntPtr.Zero)
                    {
                        thisplatform = new OnePlatform();
                        thisplatform.g_handle = tmphand;
                        thisplatform.PlatIDnumb = cardid;
                        thisplatform.g_naxis = IMC_Pkg.PKG_IMC_GetNaxis(tmphand);//获得控制卡最大轴数
                        res = true;
                    }
                    else
                    {


                    }
                }
                catch
                {
                }
            }
            else
            {
                try
                {
                    IntPtr tmphand = IMC_Pkg4.PKG_IMC_Open(netid, cardid);
                    if (tmphand != IntPtr.Zero)
                    {
                        thisplatform = new OnePlatform();
                        thisplatform.g_handle = tmphand;
                        thisplatform.PlatIDnumb = cardid;
                        thisplatform.g_naxis = IMC_Pkg4.PKG_IMC_GetNaxis(tmphand);//获得控制卡最大轴数
                        res = true;
                    }
                    else
                    {


                    }
                }
                catch
                {
                }


            }
            return res;
        }
        public int[] GetCommendBack()
        {
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                int i = 0;
                if (!thisplatform.isOpen())
                {
                    throw new Exception("控制卡尚未打开");
                }
                if (IMC_Pkg.PKG_IMC_GetEncp(thisplatform.g_handle, thisplatform.m_encp, thisplatform.g_naxis) != 0)//获得反馈编码器位置
                {
                    for (i = 0; i < thisplatform.g_naxis; i++)
                    {
                        if (thisplatform.m_encp[i] != thisplatform.m_encp_t[i])
                        {
                            thisplatform.m_encp_t[i] = thisplatform.m_encp[i];
                        }
                    }
                }
                return thisplatform.m_encp;
            }
            else
            {
                int i = 0;
                if (!thisplatform.isOpen())
                {
                    throw new Exception("控制卡尚未打开");
                }
                if (IMC_Pkg4.PKG_IMC_GetEncp(thisplatform.g_handle, thisplatform.m_encp, thisplatform.g_naxis) != 0)//获得反馈编码器位置
                {
                    for (i = 0; i < thisplatform.g_naxis; i++)
                    {
                        if (thisplatform.m_encp[i] != thisplatform.m_encp_t[i])
                        {
                            thisplatform.m_encp_t[i] = thisplatform.m_encp[i];
                        }
                    }
                }
                return thisplatform.m_encp;
            }

        }
        public void BegionToResetPlanel(Action FuWeiOk, float Acc, float Speed, int Dist)
        {
            if (IsInFuWei)
            {
                return;
            }
            F_AccD = Acc;
            F_Speed = Speed;
            F_Dist = Dist;
            FuWeiComplete = null;
            FuWeiComplete = FuWeiOk;

            thisplatform.FuweiAcc = Acc;
            thisplatform.FuweiTargetv = Speed;
            thisplatform.FuweiDistance = Dist;

            mResetThreadIsRunning = true;
            mResetThread = new Thread(ResetMethod);
            mResetThread.Start();
            F_ToUp();
        }
        private void F_ToUp()
        {

            if (thisplatform.isOpen() == false)
            {
                throw new Exception("控制卡尚未打开");
            }
            if (IsInFuWei)
            {
                MessageBox.Show("正在复位中");
                return;
            }
            IsInFuWei = true;
            IsToUp = true;
            thisplatform.ResetToUp = false;

            SetOrigPosAll();
            Set_Limitswitch(1);///开启限位功能
            DoInThread = null;
            DoInThread += thisplatform.FuweiMethod;
            DoInThread += DoToCheckCompelete;
            thisplatform.BegionToUp();

        }
        void ResetMethod()
        {
            while (mResetThreadIsRunning)
            {
                Thread.Sleep(10);
                if (DoInThread != null)
                {
                    DoInThread();
                }

            }

            try
            {
                mResetThread.Abort();
                mResetThread = null;
            }
            catch
            {

            }

        }
        void DoToCheckCompelete()
        {
            if (IsToUp)
            {
                //所有平台开始向下复位
                if (thisplatform.ResetToUp)
                {
                    IsToUp = false;
                    WaitForTime wait = new WaitForTime(1.0f, AllToDown);
                   
                }
            }
            else
            {
                
                if (thisplatform.HaveResetPos)
                {
                    DoInThread = null;
                    AllPlatformCompelete();
                }
            }
        }
        void AllPlatformCompelete()
        {
          
            mResetThreadIsRunning = false;
            IsInFuWei = false;
            HaveCompeleteFuwei = true;
             
            if (FuWeiComplete != null)
            {
                FuWeiComplete();
            }
        }
        void AllToDown()
        {
            PrePareResetPos_Down();
        }
        public void PrePareResetPos_Down()
        {
            IsToUp = false;
            thisplatform.Fu_GotoDown();
        }
        public void Set_Limitswitch(int Open)
        {
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                if (!thisplatform.isOpen())
                {
                    throw new Exception("控制卡尚未打开");
                }
                int st;
                string err;
                for (int j = 0; j < thisplatform.g_naxis; j++)
                {
                    st = IMC_Pkg.PKG_IMC_Setlimit(thisplatform.g_handle, Open, 0, Open, 0, j);
                    if (st == 0)
                    {
                        err = IMC_Pkg.PKG_IMC_GetFunErrStr();

                        throw new Exception(err);
                    }
                }
            }
            else
            {
                if (!thisplatform.isOpen())
                {
                    throw new Exception("控制卡尚未打开");
                }
                int st;
                string err;
                for (int j = 0; j < thisplatform.g_naxis; j++)
                {
                    st = IMC_Pkg4.PKG_IMC_Setlimit(thisplatform.g_handle, Open, 0, Open, 0, j);
                    if (st == 0)
                    {
                        err = IMC_Pkg4.GetFunErrStr();

                        throw new Exception(err);
                    }
                }
            }
        }
        public void SetOrigPosAll()
        {
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                if (thisplatform.isOpen())
                {
                    int st;
                    string err;
                    for (int i = 0; i < thisplatform.g_naxis; i++)
                    {
                        st = IMC_Pkg.PKG_IMC_SetPos(thisplatform.g_handle, 0, i);
                        if (st == 0)
                        {
                            err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                            throw new Exception(err);
                        }
                    }

                }
            }
            else
            {
                if (thisplatform.isOpen())
                {
                    int st;
                    string err;
                    for (int i = 0; i < thisplatform.g_naxis; i++)
                    {
                        st = IMC_Pkg4.PKG_IMC_SetPos(thisplatform.g_handle, 0, i);
                        if (st == 0)
                        {
                            err = IMC_Pkg4.GetFunErrStr();
                            throw new Exception(err);
                        }
                    }

                }

            }
        }
       
        public static void P2P_runing_start(IntPtr PID, int m_axis, double m_acc, double m_startvel, double m_tgvel, int m_pos)
        {
        
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                int st;
                string err;
                st = IMC_Pkg.PKG_IMC_SetAccel(PID, m_acc, m_acc, m_axis);
                if (st != 0)
                {
                    st = IMC_Pkg.PKG_IMC_MoveAbs(PID, m_pos, m_startvel, m_tgvel, 0, m_axis);
                }
                if (st == 0)
                {
                    err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                    throw new Exception(err);
                }
            }
            else {
                int st;
                string err;
                st = IMC_Pkg4.PKG_IMC_SetAccel(PID, m_acc, m_acc, m_axis);
                if (st != 0)
                {
                    st = IMC_Pkg4.PKG_IMC_MoveAbs(PID, m_pos, m_startvel, m_tgvel, m_tgvel, 0, m_axis);
                }
                if (st == 0)
                {
                    err = IMC_Pkg4.GetFunErrStr();
                    throw new Exception(err);
                }

            }
   
        }
        public static void P2P_runing_StopOne(IntPtr hanled, int Axis)
        {
            int st = 0;
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                
                st = IMC_Pkg.PKG_IMC_P2Pstop(hanled, Axis);
                st = IMC_Pkg.PKG_IMC_P2Pvel(hanled, 0, Axis);
                if (st == 0)
                {
                    string err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                    throw new Exception(err);
                }
            }
            else {
                 
                 st = IMC_Pkg4.PKG_IMC_P2Pstop(hanled, Axis);
                st = IMC_Pkg4.PKG_IMC_P2Pvel(hanled, 0, Axis);
                if (st == 0)
                {
                    string err = IMC_Pkg4.GetFunErrStr();
                    throw new Exception(err);
                }
            }
        }
        public static void SixSame_ManualStart(IntPtr PID, int[] Posd, double targetv, double acc, int wait)
        {

                double m_acc, m_tgvel, m_endvel, m_rate;
                int m_segnum, m_axisCnt, m_wait;
                int[] m_axis = new int[6];
                int[] m_pos = new int[6];
                bool m_isAbs = true;

                m_axisCnt = 0;
                for (int j = 0; j < 6; j++)
                {//获得直线的位置
                    m_axis[m_axisCnt] = j;
                    m_pos[m_axisCnt] = Posd[j];
                    m_axisCnt++;
                }
                m_segnum = 1;
                m_isAbs = true;
                m_tgvel = targetv;
                m_acc = acc;
                m_rate = 1;
                m_endvel = targetv;
                m_wait = wait;
                //需要等待圆弧插补运动完成，这会阻塞主线程使程序看起来像无响应状态
                //因此需要使用线程

                int st, i, n, fifo;
                string err;
                int[] segpos;
                segpos = new int[m_segnum * m_axisCnt];
                for (i = 0; i < m_segnum; i++)
                {
                    for (n = 0; n < m_axisCnt; n++)
                        segpos[i * m_axisCnt + n] = m_pos[n];
                }
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                fifo = (int)IMC_Pkg.FIFO_SEL.SEL_PFIFO1;//使用插补空间 1
                st = IMC_Pkg.PKG_IMC_PFIFOclear(PID, fifo); //清空PFIFO
                if (st != 0)
                {
                    st = SetPFIFO(PID, m_acc, m_rate, fifo);
                }
                if (st != 0)
                {
                    st = IMC_Pkg.PKG_IMC_AxisMap(PID, m_axis, m_axisCnt, fifo);//映射轴
                }
                if (st != 0)
                {
                    if (m_isAbs)//使用绝对坐标
                    {
                        if (m_segnum > 1)
                            st = IMC_Pkg.PKG_IMC_MulLine_Pos(PID, segpos, m_axisCnt, m_segnum, m_tgvel, m_endvel, m_wait, fifo);
                        else
                            st = IMC_Pkg.PKG_IMC_Line_Pos(PID, m_pos, m_axisCnt, m_tgvel, m_endvel, m_wait, fifo);
                    }
                    else
                    {//使用相对坐标
                        if (m_segnum > 1)
                            st = IMC_Pkg.PKG_IMC_MulLine_Dist(PID, segpos, m_axisCnt, m_segnum, m_tgvel, m_endvel, m_wait, fifo);
                        else
                            st = IMC_Pkg.PKG_IMC_Line_Dist(PID, m_pos, m_axisCnt, m_tgvel, m_endvel, m_wait, fifo);
                    }
                }
                if (st == 0)
                {
                    err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                    throw new Exception(err);
                }
            }
            else {
                fifo = (int)FIFO_SEL.SEL_PFIFO1;//使用插补空间 1
                st = IMC_Pkg4.PKG_IMC_PFIFOclear(PID, fifo); //清空PFIFO
                if (st != 0)
                {
                    st = SetPFIFO(PID, m_acc, m_rate, fifo);
                }
                if (st != 0)
                {
                    st = IMC_Pkg4.PKG_IMC_AxisMap(PID, m_axis, m_axisCnt, fifo);//映射轴
                }
                if (st != 0)
                {
                    if (m_isAbs)//使用绝对坐标
                    {
                        if (m_segnum > 1)
                            st = IMC_Pkg4.PKG_IMC_MulLine_Pos(PID, segpos, m_axisCnt, m_segnum, m_tgvel, m_endvel, m_wait, fifo);
                        else
                            st = IMC_Pkg4.PKG_IMC_Line_Pos(PID, m_pos, m_axisCnt, m_tgvel, m_endvel, m_wait, fifo);
                    }
                    else
                    {//使用相对坐标
                        if (m_segnum > 1)
                            st = IMC_Pkg4.PKG_IMC_MulLine_Dist(PID, segpos, m_axisCnt, m_segnum, m_tgvel, m_endvel, m_wait, fifo);
                        else
                            st = IMC_Pkg4.PKG_IMC_Line_Dist(PID, m_pos, m_axisCnt, m_tgvel, m_endvel, m_wait, fifo);
                    }
                }
                if (st == 0)
                {
                    err = IMC_Pkg4.GetFunErrStr();
                    throw new Exception(err);
                }
            }
        }
        static int SetPFIFO(IntPtr handle, double acc, double rate, int fifo)
        {
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                int st;
                st = IMC_Pkg.PKG_IMC_PFIFOrun(handle, fifo);            //启用插补空间
                if (st == 0)
                    return st;
                st = IMC_Pkg.PKG_IMC_SetPFIFOaccel(handle, acc, fifo);  //设置加速度
                if (st == 0)
                    return st;
                st = IMC_Pkg.PKG_IMC_SetFeedrate(handle, rate, fifo);   //设置进给倍率
                return st;
            }
            else {
                int st;
                st = IMC_Pkg4.PKG_IMC_PFIFOrun(handle, fifo);            //启用插补空间
                if (st == 0)
                    return st;
                st = IMC_Pkg4.PKG_IMC_SetPFIFOaccel(handle, acc, fifo);  //设置加速度
                if (st == 0)
                    return st;
                st = IMC_Pkg4.PKG_IMC_SetFeedrate(handle, rate);   //设置进给倍率
                return st;
            }
        }
        #endregion
        public SinglePlatFormControl() { }

    }
    public class OnePlatform
    {
        public const int MAX_NAXIS = 16;

        public CFG_INFO[] m_cfg = new CFG_INFO[MAX_NAXIS];
        public int[] m_encp = new int[MAX_NAXIS];
        public int[] m_curpos = new int[MAX_NAXIS];
        public ushort[] m_error = new ushort[MAX_NAXIS];
        public int[] m_encp_t = new int[MAX_NAXIS];
        public int[] m_curpos_t = new int[MAX_NAXIS];
        public ushort[] m_error_t = new ushort[MAX_NAXIS];
        public ushort[] m_gout = new ushort[48];
        public ushort[] m_gout_s = new ushort[48];
        public ushort[] m_gin = new ushort[32];
        public ushort[] m_gin_t = new ushort[32];
        public ushort[,] m_aio = new ushort[MAX_NAXIS, 6];
        public ushort[,] m_aio_s = new ushort[MAX_NAXIS, 6];

        public int[] TargetPos;
        public int TargetSeve;

        public IntPtr g_handle;
        public int g_naxis;
        public int UsedAixe = 6;
        /// <summary>
        /// 有没有滑台
        /// </summary>
        public bool UsedHuatai = false;
        /// <summary>
        /// 平台编号
        /// </summary>
        public int PlatIDnumb = 0;

        public bool HaveResetPos = false;
        System.Action MfuweiMehod = null;
        ControlData mControlData = ControlData.ShareInstance();
        public float FuweiAcc = 1.0f;
        public float Fuweistarv = 0.5f;
        public float FuweiTargetv = 1.5f;
        public int FuweiDistance = 2000000;

        bool[] Fuweirest = new bool[MAX_NAXIS];
        public bool ResetToUp = false;

        private bool HaveCacuLater = false;
        public float[] MaichongBili;
        public float[] ZongPulses;
        public bool sevenup = false;
        public bool sevendown = false;

        #region  可以切换
        public OnePlatform()
        {
            g_handle = IntPtr.Zero;
            g_naxis = 0;

            for (int i = 0; i < m_cfg.Length; i++)
            {
                m_cfg[i].ena = 1;
                m_cfg[i].steptime = 1000;
                m_cfg[i].pulpolar = 1;
                m_cfg[i].dirpolar = 1;
                m_cfg[i].vellim = 10000;
                m_cfg[i].acclim = 1100;
                m_cfg[i].plimitena = 1;
                m_cfg[i].plimitpolar = 0;
                m_cfg[i].nlimitena = 1;
                m_cfg[i].nlimitpolar = 0;
                m_cfg[i].almena = 0;
                m_cfg[i].almpolar = 0;
                m_cfg[i].INPena = 0;
                m_cfg[i].INPpolar = 0;
                m_cfg[i].encpfactor = 1;
                m_cfg[i].encpena = 0;
                m_cfg[i].encpmode = 0;
                m_cfg[i].encpdir = 1;
                m_cfg[i].Smooth = 192;
            }
        }
        public int DoConfigCard(CFG_INFO[] m_cfgFF)
        {
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                this.m_cfg = m_cfgFF;
                try
                {
                    if (this.isOpen())
                    {
                        int st = 0;
                        string err = "";
                        IMC_Pkg.PKG_IMC_InitCfg(this.g_handle);
                        this.g_naxis = IMC_Pkg.PKG_IMC_GetNaxis(this.g_handle);//获得控制卡最大轴数
                        st = IMC_Pkg.PKG_IMC_ClearIMC(this.g_handle);
                        st = IMC_Pkg.PKG_IMC_Emstop(this.g_handle, 0);
                        for (int i = 0; i < this.g_naxis; i++)
                        {
                            st = IMC_Pkg.PKG_IMC_ClearError(this.g_handle, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_ClearAxis(this.g_handle, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetStopfilt(this.g_handle, 1, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetExitfilt(this.g_handle, 0, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetPulWidth(this.g_handle, m_cfgFF[i].steptime, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetPulPolar(this.g_handle, m_cfgFF[i].pulpolar, m_cfgFF[i].dirpolar, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetEncpEna(this.g_handle, m_cfgFF[i].encpena, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetEncpMode(this.g_handle, m_cfgFF[i].encpmode, m_cfgFF[i].encpdir, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetEncpRate(this.g_handle, m_cfgFF[i].encpfactor, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetVelAccLim(this.g_handle, m_cfgFF[i].vellim, m_cfgFF[i].acclim, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_Setlimit(this.g_handle, m_cfgFF[i].plimitena, m_cfgFF[i].plimitpolar, m_cfgFF[i].nlimitena, m_cfgFF[i].nlimitpolar, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetAlm(this.g_handle, m_cfgFF[i].almena, m_cfgFF[i].almpolar, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetSmooth(this.g_handle, m_cfgFF[i].Smooth, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetEna(this.g_handle, m_cfgFF[i].ena, i);//使能驱动器放在最后
                            if (st == 0)
                                break;
                        }
                        if (st == 0)
                        {
                            err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                            throw new Exception(err);
                        }

                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch
                {
                    return 0;
                }
            }
            else {

                this.m_cfg = m_cfgFF;
                try
                {
                    if (this.isOpen())
                    {
                        int i, st = 0;
                        string err;
              
                        st = IMC_Pkg4.PKG_IMC_ClearIMC(this.g_handle);
                        for (i = 0; i < this.g_naxis; i++)
                        {
                            st = IMC_Pkg4.PKG_IMC_ClearAxis(this.g_handle, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetStopfilt(this.g_handle, 1, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetExitfilt(this.g_handle, 0, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetPulWidth(this.g_handle, m_cfg[i].steptime, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetPulPolar(this.g_handle, m_cfg[i].pulpolar, m_cfg[i].dirpolar, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetEncpEna(this.g_handle, m_cfg[i].encpena, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetEncpMode(this.g_handle, m_cfg[i].encpmode, m_cfg[i].encpdir, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetVelAccLim(this.g_handle, m_cfg[i].vellim, m_cfg[i].acclim, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_Setlimit(this.g_handle, m_cfg[i].plimitena, m_cfg[i].plimitpolar, m_cfg[i].nlimitena, m_cfg[i].nlimitpolar, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetAlm(this.g_handle, m_cfg[i].almena, m_cfg[i].almpolar, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetEna(this.g_handle, m_cfg[i].ena, i);//使能驱动器放在最后
                            if (st == 0)
                                break;
                        }
                        if (st == 0)
                        {
                            err = IMC_Pkg4.GetFunErrStr();
                            throw new Exception(err);
                        }

                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch
                {
                    return 0;
                }

            }
        }
        public int DoConfigCard()
        {
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
   
                try
                {
                    if (this.isOpen())
                    {
                        int st = 0;
                        string err = "";
                        IMC_Pkg.PKG_IMC_InitCfg(this.g_handle);
                        this.g_naxis = IMC_Pkg.PKG_IMC_GetNaxis(this.g_handle);//获得控制卡最大轴数
                        st = IMC_Pkg.PKG_IMC_ClearIMC(this.g_handle);
                        st = IMC_Pkg.PKG_IMC_Emstop(this.g_handle, 0);
                        for (int i = 0; i < this.g_naxis; i++)
                        {
                            st = IMC_Pkg.PKG_IMC_ClearError(this.g_handle, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_ClearAxis(this.g_handle, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetStopfilt(this.g_handle, 1, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetExitfilt(this.g_handle, 0, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetPulWidth(this.g_handle, m_cfg[i].steptime, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetPulPolar(this.g_handle, m_cfg[i].pulpolar, m_cfg[i].dirpolar, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetEncpEna(this.g_handle, m_cfg[i].encpena, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetEncpMode(this.g_handle, m_cfg[i].encpmode, m_cfg[i].encpdir, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetEncpRate(this.g_handle, m_cfg[i].encpfactor, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetVelAccLim(this.g_handle, m_cfg[i].vellim, m_cfg[i].acclim, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_Setlimit(this.g_handle, m_cfg[i].plimitena, m_cfg[i].plimitpolar, m_cfg[i].nlimitena, m_cfg[i].nlimitpolar, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetAlm(this.g_handle, m_cfg[i].almena, m_cfg[i].almpolar, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetSmooth(this.g_handle, m_cfg[i].Smooth, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg.PKG_IMC_SetEna(this.g_handle, m_cfg[i].ena, i);//使能驱动器放在最后
                            if (st == 0)
                                break;
                        }
                        if (st == 0)
                        {
                            err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                            throw new Exception(err);
                        }

                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch
                {
                    return 0;
                }
            }
            else
            {

                try
                {
                    if (this.isOpen())
                    {
                        int i, st = 0;
                        string err;

                        st = IMC_Pkg4.PKG_IMC_ClearIMC(this.g_handle);
                        for (i = 0; i < this.g_naxis; i++)
                        {
                            st = IMC_Pkg4.PKG_IMC_ClearAxis(this.g_handle, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetStopfilt(this.g_handle, 1, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetExitfilt(this.g_handle, 0, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetPulWidth(this.g_handle, m_cfg[i].steptime, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetPulPolar(this.g_handle, m_cfg[i].pulpolar, m_cfg[i].dirpolar, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetEncpEna(this.g_handle, m_cfg[i].encpena, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetEncpMode(this.g_handle, m_cfg[i].encpmode, m_cfg[i].encpdir, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetVelAccLim(this.g_handle, m_cfg[i].vellim, m_cfg[i].acclim, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_Setlimit(this.g_handle, m_cfg[i].plimitena, m_cfg[i].plimitpolar, m_cfg[i].nlimitena, m_cfg[i].nlimitpolar, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetAlm(this.g_handle, m_cfg[i].almena, m_cfg[i].almpolar, i);
                            if (st == 0)
                                break;
                            st = IMC_Pkg4.PKG_IMC_SetEna(this.g_handle, m_cfg[i].ena, i);//使能驱动器放在最后
                            if (st == 0)
                                break;
                        }
                        if (st == 0)
                        {
                            err = IMC_Pkg4.GetFunErrStr();
                            throw new Exception(err);
                        }

                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch
                {
                    return 0;
                }

            }
        }
        public bool isOpen()
        {
            return g_handle != IntPtr.Zero;
        }
        public void FuweiMethod()
        {
            if (g_handle == IntPtr.Zero)
            {
                throw new Exception("控制卡尚未打开");
            }
 
            if (MfuweiMehod != null)
            {
                MfuweiMehod();
            }
     

        }
        public void SetOrigPosAll()
        {
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                for (int i = 0; i < g_naxis; i++)
                {
                    int st;
                    string err;
                    st = IMC_Pkg.PKG_IMC_SetPos(g_handle, 0, i);
                    if (st == 0)
                    {
                        err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                        throw new Exception(err);
                    }
                }
            }
            else {
                for (int i = 0; i < g_naxis; i++)
                {
                    int st;
                    string err;
                    st = IMC_Pkg4.PKG_IMC_SetPos(g_handle, 0, i);
                    if (st == 0)
                    {
                        err = IMC_Pkg4.GetFunErrStr();
                        throw new Exception(err);
                    }
                }

            }
        }
        void SetOrigPosAix(int i)
        {
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                int st;
                string err;
                st = IMC_Pkg.PKG_IMC_SetPos(g_handle, 0, i);
                if (st == 0)
                {
                    err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                    throw new Exception(err);
                }
            }
            else {
                int st;
                string err;
                st = IMC_Pkg4.PKG_IMC_SetPos(g_handle, 0, i);
                if (st == 0)
                {
                    err = IMC_Pkg4.GetFunErrStr();
                    throw new Exception(err);
                }
            }
        }
        public void BegionToUp()
        {
            MaichongBili = new float[g_naxis];
            ZongPulses = new float[g_naxis];
            HaveCacuLater = false;

            HaveResetPos = false;
            ResetFuweiBool();
            MfuweiMehod = null;
            SetOrigPosAll();
            for (int i = 0; i < 6; i++)
            {
                SinglePlatFormControl.P2P_runing_start(g_handle, i, FuweiAcc, Fuweistarv, FuweiTargetv, FuweiDistance);
            }
            MfuweiMehod = CheckUpd;
        }
        public void SevenToUp()
        {
            sevenup = false;
            MfuweiMehod = null;
            SetOrigPosAix(6);
            SinglePlatFormControl.P2P_runing_start(g_handle, 6, FuweiAcc, Fuweistarv, FuweiTargetv, FuweiDistance);
            MfuweiMehod = CheckUpSeven;
        }
        void CheckUpSeven()
        {
            checkInPutIo();

            if (!sevenup)
            {
                if (m_aio[6, 0] == 0)
                {
                    SinglePlatFormControl.P2P_runing_StopOne(g_handle, 6);
                    sevenup = true;
                }
            }
            if (sevenup)
            {

                MfuweiMehod = null;

            }


        }
        public void SevenToDown()
        {
            sevendown = false;
            MfuweiMehod = null;
            SetOrigPosAix(6);
            SinglePlatFormControl.P2P_runing_start(g_handle, 6, FuweiAcc, Fuweistarv, FuweiTargetv, -FuweiDistance);
            MfuweiMehod = CheckDownSeven;

        }
        void checkInPutIo() {
           
            if (GlobalClass.NowCarType == CardType.iMC3xx2E)
            {
                int st = IMC_Pkg.PKG_IMC_GetAin(g_handle, m_aio, g_naxis);
                if (st == 0)
                {
                    string err = IMC_Pkg.PKG_IMC_GetFunErrStr();
                    throw new Exception(err);

                }
            }
            else
            {
                int st = IMC_Pkg4.PKG_IMC_GetAin(g_handle, m_aio, g_naxis);
                if (st == 0)
                {
                    string err = IMC_Pkg4.GetFunErrStr();
                    throw new Exception(err);
                }
            }
        }
        void CheckDownSeven()
        {
            checkInPutIo();
            if (!sevendown)
            {
                if (m_aio[6, 1] == 0)
                {
                    SinglePlatFormControl.P2P_runing_StopOne(g_handle, 6);
                    sevendown = true;
                }
            }
            if (sevenup)
            {
                MfuweiMehod = null;
            }

        }
        void ResetFuweiBool()
        {
            Fuweirest = new bool[6];
            for (int i = 0; i < Fuweirest.Length; i++)
            {
                Fuweirest[i] = false;
            }
        }
        void CheckUpd()
        {
            checkInPutIo();

            for (int i = 0; i < 6; i++)
            {
                if (!Fuweirest[i])
                {
                    if (m_aio[i, 0] == 0)
                    {
                        Fuweirest[i] = true;
                    }
                }

            }
            bool resd = true;

            for (int i = 0; i < 6; i++)
            {
                if (Fuweirest[i] == false)
                {
                    resd = false;
                    break;
                }
            }

            if (resd)
            {
                MfuweiMehod = null;
                for (int i = 0; i < 6; i++)
                {
                    SinglePlatFormControl.P2P_runing_StopOne(this.g_handle, i);
                }
                ResetToUp = true;
            }

        }
        public void Fu_GotoDown()
        {
            SetOrigPosAll();
            ResetFuweiBool();
            for (int i = 0; i < 6; i++)
            {
                SinglePlatFormControl.P2P_runing_start(g_handle, i, FuweiAcc, Fuweistarv, FuweiTargetv, -FuweiDistance);
            }
       
            MfuweiMehod = CheckDowns;
          
        }
        public void P2PToTargetPos(int i, float v, int Pos)
        {
            SinglePlatFormControl.P2P_runing_start(g_handle, i, 1, 0, v, Pos);
        }
        void CheckDowns()
        {
            checkInPutIo();
            for (int i = 0; i < 6; i++)
            {
                if (!Fuweirest[i])
                {
                    if (m_aio[i, 1] == 0)
                    {
                        Fuweirest[i] = true;
                    }
                }

            }
            bool resd = true;

            for (int i = 0; i < 6; i++)
            {
                if (Fuweirest[i] == false)
                {
                    resd = false;
                    break;
                }
            }
            if (resd)
            {
                MfuweiMehod = null;
                HaveResetPos = true;
            }
        }
        public void CaculatorBili(float xingcheg)
        {
            if (HaveCacuLater == false)
            {
                if (GlobalClass.NowCarType == CardType.iMC3xx2E)
                {
                    int i = 0;
                    if (!this.isOpen())
                    {
                        throw new Exception("控制卡尚未打开");
                    }
                    if (IMC_Pkg.PKG_IMC_GetEncp(this.g_handle, this.m_encp, this.g_naxis) != 0)//获得反馈编码器位置
                    {
                        for (i = 0; i < this.g_naxis; i++)
                        {
                            if (this.m_encp[i] != this.m_encp_t[i])
                            {
                                this.m_encp_t[i] = this.m_encp[i];
                            }
                        }
                    }
 
                }
                else
                {
                    int i = 0;
                    if (!this.isOpen())
                    {
                        throw new Exception("控制卡尚未打开");
                    }
                    if (IMC_Pkg4.PKG_IMC_GetEncp(this.g_handle, this.m_encp, this.g_naxis) != 0)//获得反馈编码器位置
                    {
                        for (i = 0; i < this.g_naxis; i++)
                        {
                            if (this.m_encp[i] != this.m_encp_t[i])
                            {
                                this.m_encp_t[i] = this.m_encp[i];
                            }
                        }
                    }
         
                }
                for (int i = 0; i < 6; i++)
                {
                    MaichongBili[i] = Math.Abs(m_encp[i]) / xingcheg * 1.0f;
                    ZongPulses[i] = Math.Abs(m_encp[i]);
                }
                HaveCacuLater = true;
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    MaichongBili[i] = ZongPulses[i] / xingcheg * 1.0f;
                }
            }

        }
        public void CaculatorBili(float[] xingcheg)
        {
            if (HaveCacuLater)
            {
                for (int i = 0; i < 6; i++)
                {
                    MaichongBili[i] = ZongPulses[i] / xingcheg[i] * 1.0f;
                }
            }
        }
        public float[] getAllPulse()
        {

            return ZongPulses;
        }
        public void FindOriposComplete()
        {
            SetOrigPosAll();
        }
        public int[] Conver_LengthTopluse(float[] tmps2)
        {
            int[] Pos = new int[6];
            for (int i = 0; i < 6; i++)
            {
                if (tmps2[i] < 0 || tmps2[i] > 450)
                {

                    throw new Exception("缸体数据超出行程");
                }
                Pos[i] = (int)(MaichongBili[i] * tmps2[i]);
            }
            return Pos;
        }
        public int[] Conver_LengthTopluse2(float[] tmps2)
        {
            int[] Pos = new int[6];
            for (int i = 0; i < 6; i++)
            {
                Pos[i] = (int)(MaichongBili[i] * tmps2[i]);
            }
            return Pos;
        }
        public int[] Conver_LengthTopluse2(int[] tmps2)
        {
            int[] Pos = new int[6];
            for (int i = 0; i < 6; i++)
            {
                Pos[i] = (int)(MaichongBili[i] * tmps2[i]);
            }
            return Pos;
        }
        public int GetSeverPos(float LL)
        {
            return (int)(MaichongBili[6] * LL);
        }
        #endregion









        public bool GetFucEror()
        {
            bool res = true;
            ushort[] tmp = new ushort[g_naxis];
            IMC_Pkg.PKG_IMC_GetErrorReg(g_handle, tmp, g_naxis);
            if (mControlData.UseSeveAix)
            {
                for (int i = 0; i < 7; i++)
                {
                    byte mb = (byte)tmp[i];
                    if ((mb & 0x40) != 0x00)
                    {
                        res = false;
                        break;
                    }
                }
            }
            else
            {

                for (int i = 0; i < 6; i++)
                {
                    byte mb = (byte)tmp[i];
                    if ((mb & 0x40) != 0x00)
                    {
                        res = false;
                        break;
                    }
                }
            }
            return res;
        }
 
    }
    public struct CFG_INFO
    {
        /// <summary>
        /// 使能驱动器
        /// </summary>
        public int ena;
        /// <summary>
        /// 脉冲宽度   
        /// </summary>
        public UInt32 steptime;
        /// <summary>
        /// 脉冲电平
        /// </summary>
        public int pulpolar;
        /// <summary>
        /// 方向电平
        /// </summary>
        public int dirpolar;
        /// <summary>
        /// 速度极限
        /// </summary>
        public double vellim;
        /// <summary>
        /// 加速度极限
        /// </summary>
        public double acclim;
        /// <summary>
        /// 硬件限位
        /// </summary>
        public int plimitena;
        /// <summary>
        /// 限位电平
        /// </summary>
        public int plimitpolar;
        /// <summary>
        /// 硬件限位
        /// </summary>
        public int nlimitena;
        /// <summary>
        /// 限位电平
        /// </summary>
        public int nlimitpolar;
        /// <summary>
        /// 伺服报警使能
        /// </summary>
        public int almena;
        /// <summary>
        /// 伺服报警电平
        /// </summary>
        public int almpolar;
        /// <summary>
        /// 伺服到位使能
        /// </summary>
        public int INPena;
        /// <summary>
        /// 伺服到位电平
        /// </summary>
        public int INPpolar;
        /// <summary>
        /// 反馈倍率
        /// </summary>
        public double encpfactor;
        /// <summary>
        /// 反馈使能,使用编码器反馈
        /// </summary>
        public int encpena;
        /// <summary>
        /// 反馈计数模式
        /// </summary>
        public int encpmode;
        /// <summary>
        /// 反馈计数方向
        /// </summary>
        public int encpdir;
        /// <summary>
        /// 保留
        /// </summary>
        int res;
        /// <summary>
        /// 平滑度
        /// </summary>
        public short Smooth;
    }
    public class IMC_Pkg
    {       //FIFO编号
        public enum FIFO_SEL
        {
            SEL_IFIFO,
            SEL_QFIFO,
            SEL_PFIFO1,
            SEL_PFIFO2,
            SEL_CFIFO,
        }
        //WR_MUL_DES, *pWR_MUL_DES 定义
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct WR_MUL_DES
        {
            // WORD->UInt16
            public Int16 addr;

            // WORD->UInt16
            public UInt16 axis;

            // WORD->UInt16
            public UInt16 len;

            // WORD[4]
            public UInt16 data_0;
            public Int16 data_1;
            public Int16 data_2;
            public Int16 data_3;
        }

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct EventInfo
        {
            //操作码，即枚举类型IMC_EVENT_CMD中的值
            public short EventCMD;

            //执行类型，即枚举类型IMC_EventType中的值
            public short EventType;

            //指向操作数1的参数地址
            public short Src1_loc;

            //操作数1所属的轴号
            public short Src1_axis;

            //指向操作数2的参数地址
            public short Src2_loc;

            //操作数2所属的轴号
            public short Src2_axis;

            //保留
            public int reserve1;

            //指向存储目标的参数地址
            public short dest_loc;

            //目标参数所属的轴号
            public short dest_axis;

            //保留
            public int reserve2;
        }


        //事件类型定义
        public enum IMC_EventType
        {
            IMC_Allways,        //“无条件执行”

            IMC_Edge_Zero,      //“边沿型条件执行”——变为0时
            IMC_Edge_NotZero,   //“边沿型条件执行”——变为非0时
            IMC_Edge_Great,     //“边沿型条件执行”——变为大于时
            IMC_Edge_GreatEqu,  //“边沿型条件执行”——变为大于等于时
            IMC_Edge_Little,    //“边沿型条件执行”——变为小于时
            IMC_Edge_Carry,     //“边沿型条件执行”——变为溢出时
            IMC_Edge_NotCarry,  //“边沿型条件执行”——变为无溢出时

            IMC_IF_Zero,        //“电平型条件执行”——若为0
            IMC_IF_NotZero,     //“电平型条件执行”——若为非0
            IMC_IF_Great,       //“电平型条件执行”——若大于
            IMC_IF_GreatEqu,    //“电平型条件执行”——若大于等于
            IMC_IF_Little,      //“电平型条件执行”——若小于
            IMC_IF_Carry,       //“电平型条件执行”——若溢出
            IMC_IF_NotCarry     //“电平型条件执行”——若无溢出
        }
        [DllImport(("IMC_Pkg.dll"), EntryPoint = "PKG_IMC_FindNetCard")]
        //static extern int DllRegisterServer();
        public static extern int
            PKG_IMC_FindNetCard(byte[] info,        //返回找到的网卡名称
                                ref int num);		//返回找到的网卡数量
        //打开控制卡设备，与设备建立通信连接
        [DllImport("IMC_Pkg.dll")]
        public static extern IntPtr
            PKG_IMC_Open(int netcardsel,            //网卡索引，由搜索网卡函数返回的结果决定
                                int imcid);		//IMC控制卡的id，由控制卡上的拨码开关设置决定
        //打开控制卡设备，与设备建立通信连接
        [DllImport("IMC_Pkg.dll")]
        public static extern IntPtr
            PKG_IMC_OpenX(int netcardsel,           //网卡索引，由搜索网卡函数返回的结果决定
                                int imcid,          //IMC控制卡的id，由控制卡上的拨码开关设置决定
                                int timeout,        //通信超时时间，单位毫秒
                                int openMode);		//打开模式；1：混杂模式， 0：非混杂模式
        //使用密码打开控制卡设备，与设备建立通信连接
        [DllImport("IMC_Pkg.dll")]
        public static extern IntPtr
            PKG_IMC_OpenUsePassword(int netcardsel,     //网卡索引，由搜索网卡函数返回的结果决定
                                int imcid,  			//IMC控制卡的id，由控制卡上的拨码开关设置决定
                                ref char password);		//密码字符串
        //关闭打开的设备。
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_Close(IntPtr Handle);

        //配置函数
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_IMC_InitCfg(IntPtr handle);
        //清空控制卡中所有的FIFO
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_ClearIMC(IntPtr handle); 	    //设备句柄
        //清空指定轴的所有状态
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_ClearAxis(IntPtr handle,        //设备句柄，
                            int axis);				//轴号			
        //设置指定轴的有效电平的脉冲宽度
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetPulWidth(IntPtr handle,      //设备句柄，
                            UInt32 ns,              //脉冲宽度，单位为纳秒
                            int axis);				//需要设置脉冲宽度的轴号
        //设置指定轴的脉冲和方向的有效电平
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetPulPolar(IntPtr handle,      //设备句柄，
                            int pul,                //脉冲信号的有效电平。非零：高电平有效； 零：低电平有效。
                            int dir,                //方向信号的有效电平。非零：高电平有效； 零：低电平有效。
                            int axis);				//需要设置有效电平的轴号。
        //使能/禁止控制卡接收编码器反馈
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetEncpEna(IntPtr handle,       //设备句柄，
                            int ena,                //使能标志。非零：使能； 零：不使能。
                            int axis);				//需要能/禁止控制卡接收编码器反馈的轴号。
        //设置控制卡接收编码器反馈的计数模式和计数方向
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetEncpMode(IntPtr handle,      //设备句柄，
                            int mode,               //编码器的计数模式。零：正交信号模式； 非零：脉冲+方向模式
                            int dir,                //编码器的计数方向。
                            int axis);				//需要设置的轴号。
        //设置编码器反馈倍率
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetEncpRate(IntPtr handle,      //设备句柄，
                            double rate,            //倍率
                            int axis); 				//需要设置的轴号。
        //设置指定轴的速度和加速度限制
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetVelAccLim(IntPtr handle,     //设备句柄，
                            double vellim,          //速度极限，单位为脉冲每毫秒。
                            double acclim,          //加速度极限，单位为脉冲每平方毫秒。
                            int axis);				//需要设置速度和加速度极限的轴号
        //使能/禁止指定轴的驱动器
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetEna(IntPtr handle,           //设备句柄，
                            int ena,                //使能标志。非零：使能； 零：不使能。
                            int axis);				//需要使能/禁止驱动器的轴号。
        //使能/禁止硬件限位输入端口和设置其有效极性。
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_Setlimit(IntPtr handle, 	    //设备句柄，
                            int plimEna, 			//是否使能硬件正限位功能。非零：使能； 零：不使能。
                            int plimPolar, 			//正限位极性；非零：有效； 零：低电平有效。
                            int nlimEna, 			//是否使能硬件负限位功能。非零：使能； 零：不使能。
                            int nlimPolar,          //负限位极性；非零：有效； 零：低电平有效。
                            int axis);				//轴号。
        //使能伺服报警输入和设置其有效极性
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetAlm(IntPtr handle,           //设备句柄，
                            int ena,                //是否使能伺服报警输入功能。非零：使能； 零：不使能。
                            int polar,              //极性；非零：高电平有效； 零：低电平有效。
                            int axis);				//轴号。
        //使能伺服到位输入和设置其有效极性
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetINP(IntPtr handle,           //设备句柄，
                            int ena,                //是否使能伺服到位输入功能。非零：使能； 零：不使能。
                            int polar,              //极性；非零：高电平有效； 零：低电平有效。
                            int axis);				//轴号。
        //设置急停输入端的有效极性
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetEmstopPolar(IntPtr handle,   //设备句柄，
                            int polar,              //极性；非零：高电平有效； 零：低电平有效。
                            int axis);				//轴号。
        //设置通用输入端的有效极性
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetInPolar(IntPtr handle,       //设备句柄，
                            int polar, 				//极性；非零：高电平有效； 零：低电平有效。
                            int inPort);			//输入端口，范围1 - 32。
        //设置发生错误时，电机是否停止运动
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetStopfilt(IntPtr handle,      //设备句柄，
                            int stop,               //出错时是否停止运行；非零：停止； 零：不停止。
                            int axis);				//轴号。
        //设置发生错误时，电机是否退出运动
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetExitfilt(IntPtr handle,      //设备句柄，
                            int exit,               //出错时是否退出运行；非零：退出； 零：不退出。
                            int axis);				//轴号。
        //设置静态补偿的范围
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetRecoupRange(IntPtr handle,   //设备句柄，
                            int range,              //误差补偿值；取值范围0 - 32767。
                            int axis);				//轴号。

        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_GetConfig(IntPtr handle,  		//设备句柄，
                            ref UInt32 steptime, 	//脉冲宽度，单位为纳秒
                            ref int pulpolar, 		//脉冲的有效电平；零：低电平有效； 非零：高电平有效
                            ref int dirpolar, 		//方向的有效电平；零：低电平有效； 非零：高电平有效
                            ref int encpena, 		//是否使用编码器反馈；零：禁用； 非零：使用
                            ref int encpmode, 		//编码器计数模式
                            ref int encpdir, 		//编码器计数方向
                            ref double encpfactor, 	//编码器倍率
                            ref double vellim, 		//速度极限，单位为脉冲/毫秒
                            ref double acclim, 		//加速度极限，单位为脉冲/平方毫秒
                            ref int drvena, 		//是否使能驱动器；零：不使能； 非零：使能
                            ref int plimitena, 		//是否使用硬件正限位；零：禁用； 非零：使用
                            ref int plimitpolar, 	//硬件正限位有效电平；零：低电平有效； 非零：高电平有效
                            ref int nlimitena, 		//是否使用硬件负限位；零：禁用； 非零：使用
                            ref int nlimitpolar, 	//硬件负限位有效电平；零：低电平有效； 非零：高电平有效
                            ref int almena, 		//是否使用伺服报警；零：禁用； 非零：使用
                            ref int almpolar, 		//伺服报警有效电平；零：低电平有效； 非零：高电平有效
                            ref int INPena, 		//是否使用伺服到位；零：禁用； 非零：使用
                            ref int INPpolar,       //伺服到位有效电平；零：低电平有效； 非零：高电平有效
                            int axis);				//需要获取信息的轴号
        //点到点运动函数
        //使轴从当前位置移动到指定的目标位置
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_MoveAbs(IntPtr handle,          //设备句柄，
                            int pos,                //目标位置，单位为脉冲；
                            double startvel,        //起始速度，单位为脉冲每毫秒；
                            double tgvel,           //目标速度，单位为脉冲每毫秒；
                            int wait,               //是否等待运动完成后，函数才返回。非零：等待运动完成；零：不等待。
                            int axis); 				//指定轴号
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_MoveAbs_P(IntPtr handle,            //设备句柄，
                            int pos,                //目标位置，单位为脉冲；
                            double startvel,        //起始速度，单位为脉冲每毫秒；
                            double tgvel,           //目标速度，单位为脉冲每毫秒；
                            int wait,               //是否等待运动完成后，函数才返回。非零：等待运动完成；零：不等待。
                            int axis); 				//指定轴号

        //使轴从当前位置移动到指定的距离
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_MoveDist(IntPtr handle,         //设备句柄，
                            int dist,               //移动距离，单位为脉冲；
                            double startvel,        //起始速度，单位为脉冲每毫秒；
                            double tgvel,           //目标速度，单位为脉冲每毫秒；
                            int wait,               //是否等待运动完成后，函数才返回。非零：等待运动完成；零：不等待。
                            int axis); 				//指定轴号；
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_MoveDist_P(IntPtr handle,       //设备句柄，
                            int dist,               //移动距离，单位为脉冲；
                            double startvel,        //起始速度，单位为脉冲每毫秒；
                            double tgvel,           //目标速度，单位为脉冲每毫秒；
                            int wait,               //是否等待运动完成后，函数才返回。非零：等待运动完成；零：不等待。
                            int axis); 				//指定轴号；
        //立即改变当前正在执行的点到点运动的运动速度
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_P2Pvel(IntPtr handle,           //设备句柄，
                            double tgvel,           //目标速度，单位为脉冲每毫秒；
                            int axis);				//轴号；
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_P2Pvel_P(IntPtr handle,         //设备句柄，
                            double tgvel,           //目标速度，单位为脉冲每毫秒；
                            int axis);				//轴号；
        //设置当前点到点运动的加速度和减速度
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetAccel(IntPtr handle,         //设备句柄，
                            double accel,           //加速度，单位为脉冲每平方毫秒；
                            double decel,           //减速度；
                            int axis);				//轴号；
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetAccel_P(IntPtr handle,       //设备句柄，
                            double accel,           //加速度，单位为脉冲每平方毫秒；
                            double decel,           //减速度；
                            int axis);				//轴号；
        //设置点到点运动模式
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_P2Pmode(IntPtr handle,          //设备句柄，
                            int mode,               //运动模式；零：普通模式； 非零：跟踪模式
                            int axis);				//轴号。
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_P2Pmode_P(IntPtr handle,        //设备句柄，
                            int mode,               //运动模式；零：普通模式； 非零：跟踪模式
                            int axis);				//轴号。
        //改变点到点运动的目标位置
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_P2PnewPos(IntPtr handle,        //设备句柄，
                            int tgpos,              //新的目标位置，单位为脉冲；
                            int axis);				//轴号。
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_P2PnewPos_P(IntPtr handle,          //设备句柄，
                            int tgpos,              //新的目标位置，单位为脉冲；
                            int axis);				//轴号。
        //减速停止点到点运动
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_P2Pstop(IntPtr handle,          //设备句柄，
                            int axis);				//轴号。
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_P2Pstop_P(IntPtr handle,            //设备句柄，
                            int axis);				//轴号。
        //使轴立即按指定的速度一直运动，直到速度被改变为止
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_MoveVel(IntPtr handle,          //设备句柄，
                            double startvel,        //起始速度，单位为脉冲每平方毫秒； 
                            double tgvel,           //指定轴的运动速度，单位为脉冲每平方毫秒；
                            int axis);				//轴号。
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_MoveVel_P(IntPtr handle,        //设备句柄，
                            double startvel,        //起始速度，单位为脉冲每平方毫秒； 
                            double tgvel,           //指定轴的运动速度，单位为脉冲每平方毫秒；
                            int axis);				//轴号。
        //插补函数
        //立即将参与插补运动的轴号映射到X、Y、Z、A、B、…、等对应的标识上
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_AxisMap(IntPtr handle,          //设备句柄，
                            int[] axis,             //需要映射的轴号的数组
                            int num,                //需要映射的轴的数量
                            int fifo);				//对哪个插补空间进行映射，可选SEL_PFIFO1和SEL_PFIFO2。
        //立即启用插补空间
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_PFIFOrun(IntPtr handle,         //设备句柄，
                            int fifo);				//启用哪个插补空间，可选SEL_PFIFO1或SEL_PFIFO2。
        //立即改变插补的加速度
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetPFIFOaccel(IntPtr handle,   //设备句柄，
                            double accel,           //插补的加速度，单位为脉冲每平方毫秒； 
                            int fifo);				//设置哪个插补空间的插补的加速度，可选SEL_PFIFO1或SEL_PFIFO2。
        //立即改变插补的加速度
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetPFIFOvelMode(IntPtr handle,   //设备句柄，
                            int mode, 			    //速度规划模式 
                            int fifo);				//设置哪个插补空间的速度规划模式，可选SEL_PFIFO1或SEL_PFIFO2。
        //单段直线插补（绝对运动）
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_Line_Pos(IntPtr handle,         //设备句柄，
                            int[] pos,              //参与插补运动的轴的位置，单位为脉冲；
                            int axisNum,            //参与插补运动的轴数；
                            double tgvel,           //插补运动的速度，单位为脉冲每平方毫秒；
                            double endvel,          //插补运动的末端速度，单位为脉冲每平方毫秒；
                            int wait,               //是否等待插补运动完成，函数才返回。非零：等待运动完成；零：不等待。
                            int fifo);				//指定将此运动指令发送到哪个FIFO中执行，可选SEL_PFIFO1或SEL_PFIFO2。
        //单段直线插补（相对运动）
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_Line_Dist(IntPtr handle,        //设备句柄，
                            int[] dist,             //参与插补运动的轴的移动距离，单位为脉冲；
                            int axisNum,            //参与插补运动的轴数；
                            double tgvel,           //插补运动的速度，单位为脉冲每平方毫秒；
                            double endvel,          //插补运动的末端速度，单位为脉冲每平方毫秒；
                            int wait,               //是否等待插补运动完成，函数才返回。非零：等待运动完成；零：不等待。
                            int fifo);				//指定将此运动指令发送到哪个FIFO中执行，可选SEL_PFIFO1或SEL_PFIFO2。
        //多段连续直线插补（绝对运动）
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_MulLine_Pos(IntPtr handle,     //设备句柄，
                            int[] pos,          //多段参与插补运动的轴的位置，单位为脉冲；
                            int axisNum,            //参与插补运动的轴数；
                            int segNum,             //插补运动的段数；
                            double tgvel,           //插补运动的速度，单位为脉冲每平方毫秒；
                            double endvel,          //插补运动的最后一段的结束速度，单位为脉冲每平方毫秒；
                            int wait,               //是否等待插补运动完成，函数才返回。非零：等待运动完成；零：不等待。
                            int fifo);				//指定将此运动指令发送到哪个FIFO中执行，可选SEL_PFIFO1或SEL_PFIFO2。
        //多段连续直线插补（相对运动）
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_MulLine_Dist(IntPtr handle,    //设备句柄，
                            int[] dist,             //多段参与插补运动的轴的移动距离，单位为脉冲； 
                            int axisNum,            //参与插补运动的轴数；
                            int segNum,             //插补运动的段数；
                            double tgvel,           //插补运动的速度，单位为脉冲每平方毫秒；
                            double endvel,          //插补运动的最后一段的结束速度，单位为脉冲每平方毫秒；
                            int wait,               //是否等待插补运动完成，函数才返回。非零：等待运动完成；零：不等待。
                            int fifo);				//指定将此运动指令发送到哪个FIFO中执行，可选SEL_PFIFO1或SEL_PFIFO2。
        //圆弧插补（绝对运动）
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_Arc_Pos(IntPtr handle,          //设备句柄，
                            int endx,               //参与圆弧插补的X轴的终点的位置，单位为脉冲；
                            int endy,               //参与圆弧插补的Y轴的终点的位置，单位为脉冲；
                            int cx,                 //参与圆弧插补的X轴的圆心，单位为脉冲；
                            int cy,                 //参与圆弧插补的Y轴的圆心，单位为脉冲； 
                            int dir,                //圆弧运动的方向
                            double tgvel,           //插补运动的速度，单位为脉冲每平方毫秒；
                            double endvel,          //插补运动的结束速度，单位为脉冲每平方毫秒；
                            int wait,               //是否等待插补运动完成，函数才返回。非零：等待运动完成；零：不等待。
                            int fifo);				//指定将此运动指令发送到哪个FIFO中执行，可选SEL_PFIFO1或SEL_PFIFO2。
        //圆弧插补（相对运动）
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_Arc_Dist(IntPtr handle,         //设备句柄，
                            int dist_x,             //参与圆弧插补的X轴的终点相对于起点的距离，单位为脉冲；
                            int dist_y,             //参与圆弧插补的Y轴的终点相对于起点的距离，单位为脉冲；
                            int dist_cx,            //参与圆弧插补的X轴的圆心相对于起点的距离，单位为脉冲；
                            int dist_cy,            //参与圆弧插补的Y轴的圆心相对于起点的距离，单位为脉冲；
                            int dir,                //圆弧运动的方向
                            double tgvel,           //插补运动的速度，单位为脉冲每平方毫秒；
                            double endvel,          //插补运动的结束速度，单位为脉冲每平方毫秒；
                            int wait,               //是否等待插补运动完成，函数才返回。非零：等待运动完成；零：不等待。
                            int fifo);				//指定将此运动指令发送到哪个FIFO中执行，可选SEL_PFIFO1或SEL_PFIFO2。
        //圆弧直线插补（绝对运动）
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_ArcLine_Pos(IntPtr handle,      //设备句柄，
                            int endx,               //参与圆弧插补的X轴的终点的位置，单位为脉冲；
                            int endy,               //参与圆弧插补的Y轴的终点的位置，单位为脉冲；
                            int cx,                 //参与圆弧插补的X轴的圆心，单位为脉冲；
                            int cy,                 //参与圆弧插补的Y轴的圆心，单位为脉冲；
                            int dir,                //圆弧运动的方向； 
                            int[] pos,              //跟随圆弧插补运动的轴的位置，单位为脉冲； 
                            int axisNum,            //跟随圆弧插补运动的轴数；
                            double tgvel,           //插补运动的速度，单位为脉冲每平方毫秒；
                            double endvel,          //插补运动的结束速度，单位为脉冲每平方毫秒；
                            int wait,               //是否等待插补运动完成，函数才返回。非零：等待运动完成；零：不等待。
                            int fifo);				//指定将此运动指令发送到哪个FIFO中执行，可选SEL_PFIFO1或SEL_PFIFO2。
        //圆弧直线插补（相对运动）
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_ArcLine_Dist(IntPtr handle,     //设备句柄，
                            int dist_x,             //参与圆弧插补的X轴的终点相对于起点的距离，单位为脉冲；
                            int dist_y,             //参与圆弧插补的Y轴的终点相对于起点的距离，单位为脉冲；
                            int dist_cx,            //参与圆弧插补的X轴的圆心相对于起点的距离，单位为脉冲；
                            int dist_cy,            //参与圆弧插补的Y轴的圆心相对于起点的距离，单位为脉冲；
                            int dir,                //圆弧运动的方向； 
                            int[] dist,             //跟随圆弧插补运动的轴的移动距离，单位为脉冲；
                            int axisNum,            //跟随圆弧插补运动的轴数；
                            double tgvel,           //插补运动的速度，单位为脉冲每平方毫秒；
                            double endvel,          //插补运动的结束速度，单位为脉冲每平方毫秒；
                            int wait,               //是否等待插补运动完成，函数才返回。非零：等待运动完成；零：不等待。
                            int fifo);				//指定将此运动指令发送到哪个FIFO中执行，可选SEL_PFIFO1或SEL_PFIFO2。
        //立即改变插补的进给倍率。当进给倍率设为0时，可实现插补的暂停，再次设为非零则解除暂停
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetFeedrate(IntPtr handle,     //设备句柄，
                            double rate,            //进给倍率； 
                            int fifo);				//设置哪个插补空间的插补的进给倍率，可选SEL_PFIFO1或SEL_PFIFO2。
        //立即停止插补运动
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_PFIFOstop(IntPtr handle,        //设备句柄，
                            int fifo);              //停止哪个插补空间的插补，可选SEL_PFIFO1或SEL_PFIFO2。
                                                    //判断插补运动是否停止
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_isPstop(IntPtr handle,      //设备句柄，
                            int fifo);             //哪个插补空间的插补停止，可选SEL_PFIFO1或SEL_PFIFO2。
        //立即清空发到插补空间中未被执行的所有指令
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_PFIFOclear(IntPtr handle,       //设备句柄，
                            int fifo);				//清空哪个插补空间的指令，可选SEL_PFIFO1或SEL_PFIFO2。

        //齿轮函数
        //设置指定轴跟随电子手轮运动
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_HandWheel(IntPtr Handle,        //设备句柄，
                            double rate,            //电子手轮倍率；
                            int axis);				//跟随手轮运动的轴号；
        //设置从动轴跟随主动轴运动
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_Gear1(IntPtr Handle,            //设备句柄，
                            double rate,            //齿轮倍率；
                            int master,             //主动轴号
                            int axis);				//从动轴的轴号。
        //设置从动轴跟随主动轴运动
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_Gear2(IntPtr Handle,            //设备句柄，
                            double rate,            //齿轮倍率；
                            int master,             //主动轴号
                            int axis);				//从动轴的轴号。
        //立即脱离电子手轮或齿轮的啮合
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_ExitGear(IntPtr Handle,         //设备句柄，
                            int axis);				//从动轴的轴号。


        //IO设置函数
        //对输出端口进行控制
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetOut(IntPtr handle,           //设备句柄，
                            int outPort,            //输出端口；范围是1 – 48；
                            int data,               //控制输出端口的状态； 零：断开输出端口； 非零：连通输出端口。
                            int fifo);              //指定将此指令发送到哪个FIFO中执行。

        //搜零函数
        //设置当前搜零过程中使用的高速度和低速度
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetHomeVel(IntPtr handle,       //设备句柄，
                            double hight,           //搜零过程中使用的高速度，单位为脉冲每毫秒；
                            double low,             //搜零过程中使用的低速度，单位为脉冲每毫秒；
                            int axis);              //轴号；
                                                    //设置编码器索引信号的极性
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetHomeIndexPolar(IntPtr handle, //设备句柄，
                            int polar,              //索引信号的极性， 非零：上升沿有效， 0：下降沿有效
                            int axis);				//轴号；
        //使用零点开关搜索零点
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_HomeSwitch1(IntPtr Handle,     //设备句柄，
                            int dir,                //搜零方向。零：正方向搜零；非零：负方向搜零；
                            int riseEdge,           //指定检测原点开关的边沿；零：下降沿； 非零：上升沿
                            int pos,                //设置零点时刻零点开关的位置值；
                            int stpos,              //搜零结束时，电机停止的位置；
                            double movVel,          //设置零点后移动到停止位置时的速度
                            int wait,               //是否等待搜零结束
                            int axis);				//轴号。
        //使用零点开关搜索零点
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_HomeSwitch2(IntPtr Handle,     //设备句柄，
                            int dir,                //搜零方向。零：正方向搜零；非零：负方向搜零；
                            int riseEdge,           //指定检测原点开关的边沿；零：下降沿； 非零：上升沿
                            int pos,                //设置零点时刻零点开关的位置值；
                            int stpos,              //搜零结束时，电机停止的位置；
                            double movVel,          //设置零点后移动到停止位置时的速度
                            int wait,               //是否等待搜零结束
                            int axis);				//轴号。
        //使用零点开关搜索零点
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_HomeSwitch3(IntPtr Handle,     //设备句柄，
                            int dir,                //搜零方向。零：正方向搜零；非零：负方向搜零；
                            int riseEdge,           //指定检测原点开关的边沿；零：下降沿； 非零：上升沿
                            int pos,                //设置零点时刻零点开关的位置值；
                            int stpos,              //搜零结束时，电机停止的位置；
                            double movVel,          //设置零点后移动到停止位置时的速度
                            int wait,               //是否等待搜零结束
                            int axis);				//轴号。
        //使用零点开关和索引信号搜索零点
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_HomeSwitchIndex1(IntPtr Handle,    //设备句柄，
                            int dir,                    //搜零方向。零：正方向搜零；非零：负方向搜零；
                            int riseEdge,               //指定检测原点开关的边沿；零：下降沿； 非零：上升沿
                            int pos,                    //设置零点时刻零点开关的位置值；
                            int stpos,                  //搜零结束时，电机停止的位置；
                            double movVel,              //设置零点后移动到停止位置时的速度
                            int wait,                   //是否等待搜零结束
                            int axis);					//轴号。
        //使用零点开关和索引信号搜索零点
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_HomeSwitchIndex2(IntPtr Handle, //设备句柄，
                            int dir,                //搜零方向。零：正方向搜零；非零：负方向搜零；
                            int riseEdge,           //指定检测原点开关的边沿；零：下降沿； 非零：上升沿
                            int pos,                //设置零点时刻零点开关的位置值；
                            int stpos,              //搜零结束时，电机停止的位置；
                            double movVel,          //设置零点后移动到停止位置时的速度
                            int wait,               //是否等待搜零结束
                            int axis);				//轴号。
        //使用零点开关和索引信号搜索零点
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_HomeSwitchIndex3(IntPtr Handle,     //设备句柄，
                            int dir,                //搜零方向。零：正方向搜零；非零：负方向搜零；
                            int riseEdge,           //指定检测原点开关的边沿；零：下降沿； 非零：上升沿
                            int pos,                //设置零点时刻零点开关的位置值；
                            int stpos,              //搜零结束时，电机停止的位置；
                            double movVel,          //设置零点后移动到停止位置时的速度
                            int wait,               //是否等待搜零结束
                            int axis);				//轴号。
        //使用索引信号搜索零点
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_HomeIndex(IntPtr Handle,        //设备句柄，
                            int dir,                //搜零方向。零：正方向搜零；非零：负方向搜零；
                            int pos,                //设置零点时刻零点开关的位置值；
                            int stpos,              //搜零结束时，电机停止的位置；
                            double movVel,          //设置零点后移动到停止位置时的速度
                            int wait,               //是否等待搜零结束
                            int axis);				//轴号。
        //把该轴的当前位置设定为指定值
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetPos(IntPtr Handle,           //设备句柄，
                            int pos,                //设置的指定值，单位为脉冲；
                            int axis);				//轴号。
        //立即停止搜零运动
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_HomeStop(IntPtr Handle,         //设备句柄，
                            int axis);				//需要停止搜零运动的轴号。

        //获取状态函数
        //获取轴数
        [DllImport("IMC_Pkg.dll")]
        public static extern int
         PKG_IMC_GetNaxis(IntPtr Handle);           //设备句柄，

        //获得所有轴的机械位置。
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_GetEncp(IntPtr Handle,      //设备句柄，
                           int[] pos,               //用于保存机械位置的数组；单位为脉冲；
                           int axisnum);            //控制卡的轴数。
                                                    //获得所有轴的指令位置
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_GetCurpos(IntPtr Handle,    //设备句柄，
                           int[] pos,               //用于保存指令位置的数组；单位为脉冲；
                           int axisnum);            //控制卡的轴数。
                                                    //获得所有轴的错误状态
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_GetErrorReg(IntPtr Handle,     //设备句柄，
                           UInt16[] err,            //用于保存所有轴的错误状态；有错误则相应的位会置1。
                           int axisnum);            //控制卡的轴数。
                                                    //获得所有轴的运动状态
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_GetMoving(IntPtr Handle,        //设备句柄，
                           UInt16[] moving,        //用于保存所有轴的运动状态，零：已停止运动； 非零：正在运动中
                           int axisnum);            //控制卡的轴数。
                                                    //获得所有轴的功能输入端口状态
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_GetAin(IntPtr Handle,       //设备句柄，
                           UInt16[,] ain,           //用于保存所有轴功能输入端的状态
                           int axisnum);            //控制卡的轴数。
                                                    //获得所有通用输入端口的状态
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_GetGin(IntPtr Handle,           //设备句柄，
                           UInt16[] gin);          //用于保存所有通用输入端的状态。
                                                   //获得所有输出端口的状态
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_GetGout(IntPtr Handle,      //设备句柄，
                           UInt16[] gout);         //用于保存所有输出端的状态

        [DllImport("IMC_Pkg.dll")]
        public static extern int
             PKG_IMC_ClearError(IntPtr Handle,      //设备句柄，
                            int axis);              //需要清除错误的轴号
                                                    //其他功能函数
                                                    //所有轴立即急停或解除急停状态
        [DllImport("IMC_Pkg.dll")]
        public static extern int
           PKG_IMC_Emstop(IntPtr Handle,            //设备句柄，
                           int isStop);			//急停还是解除急停；非零：急停； 零：解除急停。
        //对所有轴立即暂停或解除暂停状态
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_Pause(IntPtr Handle,            //设备句柄，
                            int pause);				//暂停还是解除暂停状态；非零：暂停； 零：解除暂停。
        //设置每个轴的平滑度
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetSmooth(IntPtr handle,        //设备句柄，
                                short smooth,       //平滑度，值越大则越平滑，但运动轨迹的误差就越大；
                                int aixs); 			//轴号；	

        //退出等待运动完成
        [DllImport("IMC_Pkg.dll")]
        public static extern void
            PKG_IMC_ExitWait();
        //当函数返回错误是，使用此函数获得错误提示
        [DllImport("IMC_Pkg.dll")]
        public static extern string
            PKG_IMC_GetFunErrStr();
        //获得错误寄存器是字符串说明
        [DllImport("IMC_Pkg.dll")]
        public static extern string
            PKG_IMC_GetRegErrorStr(UInt16 err);

        //ADD事件
        //将某个寄存器的值与另一个寄存器的值进行相加，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_ADD32(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis, 		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定数值进行相加，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_ADD32i(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            int data, 							//32位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与另一个寄存器的值进行相加，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_ADD48(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中 
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定数值进行相加，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_ADD48i(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            Int64 data, 						//64位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号

        //CMP事件
        //将某个寄存器的值与另一个寄存器的值进行相减，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_CMP32(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定数值进行相减，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_CMP32i(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            int data, 							//32位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与另一个寄存器的值进行相减，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_CMP48(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定数值进行相减，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_CMP48i(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            Int64 data, 						//64位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号

        //SCA事件
        //将某个寄存器的值乘以另一个寄存器指定的倍率，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_SCA32(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值乘以指定的倍率，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_SCA32i(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            int data, 							//32位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值乘以另一个寄存器指定的倍率，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_SCA48(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值乘以乘以指定的倍率，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_SCA48i(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            Int64 data, 						//64位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //MUL事件
        //
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_MUL32L(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_MUL32iL(ref EventInfo info,//事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            int data, 							//32位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_MUL32A(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_MUL32iA(ref EventInfo info,//事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            int data, 							//32位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号

        //COP事件
        //将某个16位寄存器的值赋值给目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_COP16(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个32位寄存器的值赋值给目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_COP32(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个48位寄存器的值赋值给目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_COP48(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号

        //SET事件
        //将指定的数值赋值给16位目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_SET16(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short data, 						//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将指定的数值赋值给32位目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_SET32(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            int data, 							//32位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将指定的数值赋值给48位目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_SET48(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            Int64 data, 						//64位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号


        //OR事件
        //将某个寄存器的值与另一个寄存器的值进行或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_OR16(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与另一个寄存器的值进行或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_OR16B(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定的数值进行或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_OR16i(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,        //寄存器1的地址和其对应的轴号
                            short data, 						//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定的数值进行或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_OR16iB(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,        //寄存器1的地址和其对应的轴号
                            short data, 						//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号

        //AND事件
        //将某个寄存器的值与另一个寄存器的值进行与运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_AND16(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与另一个寄存器的值进行与运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_AND16B(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定的数值进行与运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_AND16i(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,        //寄存器1的地址和其对应的轴号
                            short data, 						//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定的数值进行与运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_AND16iB(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,        //寄存器1的地址和其对应的轴号
                            short data, 						//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号


        //XOR事件
        //将某个寄存器的值与另一个寄存器的值进行异或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_XOR16(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与另一个寄存器的值进行异或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_XOR16B(ref EventInfo info,    //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定的数值进行异或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_XOR16i(ref EventInfo info,    //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,        //寄存器1的地址和其对应的轴号
                            short data, 						//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定的数值进行异或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg.dll")]
        public static extern int PKG_Fill_XOR16iB(ref EventInfo info,//事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,        //寄存器1的地址和其对应的轴号
                            short data,							//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号


        //底层函数封装
        //设置多个寄存器为指定值
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetMulParam(IntPtr Handle,      //设备句柄，
                                WR_MUL_DES[] pdes,  //WR_MUL_DES结构体数组；
                                int ArrNum,         //pdes数组中有效数据的个数；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //设置16位寄存器为指定值
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetParam16(IntPtr Handle,       //设备句柄，
                                short paramloc,     //寄存器地址；
                                short data,         //16位整型数据
                                int axis,           //轴号；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //设置32位寄存器为指定值
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetParam32(IntPtr Handle,       //设备句柄，
                                short paramloc,     //寄存器地址；
                                int data,           //32位整型数据
                                int axis,           //轴号；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //设置48位寄存器为指定值
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetParam48(IntPtr Handle,       //设备句柄，
                                short paramloc,     //寄存器地址；
                                Int64 data,         //64位整型数据
                                int axis,           //轴号；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //将寄存器某个位的值设为指定值（1或者0）
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_SetParamBit(IntPtr Handle,      //设备句柄，
                                short paramloc,     //寄存器地址；
                                short bit,          //寄存器的某个位，范围 0 – 15；
                                short val,          //指定的值, 1或者0；
                                int axis,           //轴号；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //将寄存器指定的位的值由1变为0或者由0变为1
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_TurnParamBit(IntPtr Handle,     //设备句柄，
                            short paramloc,         //寄存器地址；
                            short bit,              //寄存器的某个位，范围 0 – 15；
                            int axis,               //轴号；
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //置位或清零寄存器的某些位
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_ORXORParam(IntPtr Handle,       //设备句柄，
                            short paramloc,         //寄存器地址；
                            short ORdata,           //与寄存器进行相或的值；
                            short XORdata,          //与寄存器进行相异或的值；
                            int axis,               //轴号；
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //阻塞FIFO执行后续指令，直到寄存器的某个位变为指定值或超时为止
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_WaitParamBit(IntPtr Handle,     //设备句柄，
                            short paramloc,         //寄存器地址；
                            short bit,              //寄存器的某个位，范围 0 – 15；
                            short val,              //指定值，1或0；
                            int timeout,            //超时时间，单位为毫秒；
                            int axis,               //轴号；
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //阻塞FIFO执行后续指令，直到超过设定的时间为止
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_WaitTime(IntPtr Handle,         //设备句柄，
                            int time,               //等待时间，单位为毫秒
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //阻塞FIFO执行后续指令，直到寄存器的值变为指定值或超时为止
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_WaitParam(IntPtr Handle,        //设备句柄，
                            short paramloc,         //寄存器地址；
                            short data,             //指定的值；
                            int timeout,            //超时时间，单位为毫秒；
                            int axis,               //轴号；
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //阻塞FIFO执行后续指令，直到寄存器的值与mask进行相与后的值与data值相等或超时为止
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_WaitParamMask(IntPtr Handle,    //设备句柄，
                            short paramloc,         //寄存器地址；
                            short mask,             //与寄存器进行相与的值
                            short data,             //用于比较的值；
                            int timeout,            //超时时间，单位为毫秒；
                            int axis,               //轴号；
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //读取多个寄存器的值
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_GetMulParam(IntPtr Handle,      //设备句柄，
                            WR_MUL_DES[] pdes,      //WR_MUL_DES结构体数组；
                            int ArrNum);			//WR_MUL_DES结构体数组的有效成员个数。
        //读取16位寄存器的值
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_GetParam16(IntPtr Handle,       //设备句柄，
                            short paramloc, 		//寄存器地址；
                            ref short data,         //16位整型变量的地址，用于保存16位寄存器的值；
                            int axis);				//轴号；
        //读取32位寄存器的值
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_GetParam32(IntPtr Handle,       //设备句柄，
                            short paramloc, 		//寄存器地址；
                            ref int data,           //32位整型变量的地址，用于保存32位寄存器的值；
                            int axis);				//轴号；
        //读取48位寄存器的值
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_GetParam48(IntPtr Handle,       //设备句柄，
                            short paramloc,         //寄存器地址；
                            ref Int64 data,         //64位整型变量的地址，用于保存48位寄存器的值；
                            int axis);				//轴号；

        //将设置的事件安装到控制卡中
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_InstallEvent(IntPtr Handle,     //设备句柄，
                            EventInfo[] pEvent,     //事件结构体数组，由事件填充函数填充；
                            UInt16 EventNum         //事件指令的数量；
                            );
        //停止安装的事件运行
        [DllImport("IMC_Pkg.dll")]
        public static extern int
            PKG_IMC_StopEvent(IntPtr Handle); //设备句柄，
    }
}
