 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixPlatC;
//using ShiJueHangKong.ClassLib;
namespace System
{
    public delegate void ActionBool(bool res);
    public delegate void ActionToolBack(object sender, params float[] data);
}
namespace TurntableCtrl.classlib
{

    public class ControlData
    {
        public ControlData() {
            mConfigXML = new ConfigXML();
        }
        public  float[] SpeedInT = new float[] { 0.005f, 0.01f, 0.05f, 0.1f, 0.5f, 1.0f, 3.0f, 5.0f, 10.0f, 15.0f, 20.0f, 40.0f, 60.0f, 80.0f, 100.0f, 130.0f, 150.0f, 200.0f, 250.0f, 300.0f, 400.0f, 500.0f, 600.0f, 700.0f, 800.0f };
        public int[] StepLength = new int[] { 1, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 120, 150, 170, 250, 280, 300, 400, 500, 600, 700, 900, 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 10000, 30000,50000,80000,100000,300000,500000 };
        public SixPlatform mSixPlaneF = null;
        public ConfigXML mConfigXML = null;
        public string textString = "";
        public float MaxAngele = 6;
        public float MinAngele = -6;
        /// <summary>
        /// 单个平台控制类
        /// </summary>
        public SinglePlatFormControl mSinglePlatFormControl = null;
        public static ControlData mControlData = null;
        public List<string> RunningMessage=new List<string> ();
        public static ControlData ShareInstance()
        {
            if (mControlData == null)
            {
                mControlData = new ControlData();
            }
            return mControlData;
        }
        /// <summary>
        /// 有没有第七轴，这个是八轴平台的可以不用管
        /// </summary>
        public bool UseSeveAix = false;
        #region 平台在复位时候用的数据，建议不要该，否则复位时容易跑过，或者算出来的脉冲不准
        /// <summary>
        /// 复位时的速度
        /// </summary>
        public float FuweiSpeed = 3;
        /// <summary>
        /// 复位时的加速度
        /// </summary>
        public float FuweiAcc = 3;
        /// <summary>
        /// 复位时的初速度
        /// </summary>
        public float FuweistarSpeed = 0.0f;
        /// <summary>
        /// 飞行速度
        /// </summary>
        public float limitMaxspeed = 50;
 
#if Test
        public Form1.ShowString mshw = null;
#endif 
#endregion
        /// <summary>
        /// 显示信息到控制台，只有调试模式可用
        /// </summary>
        /// <param name="msg"></param>
        public void ShowConsoleLabel(string msg)
        {
#if Test
            if (mshw != null)
            {
                mshw(msg);
            }
#endif
        }
        public Action ToSaveData = null;
        public void WriteSettings() {
            if (ToSaveData != null) {
                ToSaveData();
            }
        }
    }
    /// <summary>
    /// 配置
    /// </summary>
    public class ConfigXML
    {
        public ConfigXML() { }
        public int USEDEBUG = 0;
        public float P_ShangLong = 300;
        public float P_ShangShort = 100;
        public float P_XiaLong = 300;
        public float P_XiaShort = 100;
        public float P_XingC = 200;
        public float P_Height = 300;
        public string COMName = "COM1";
        public float comTime = 20;
        public float FU_speed = 10;
        public float FU_Acc = 1;
        public float FU_dist = 20000000;
        public float P2PSpeed = 1;
        public float P2Plength = 1000;
        public float WorkSpeed = 60;
        public int[] DeltalPos = new int[] {0,0,0,0,0,0};
        public float[] ZhouMin = new float[] { 0, 0, 0, 0, 0, 0 };
        public float[] ZhouMax = new float[] { 255, 255, 255, 255, 255, 255 };
        public CardType mCardType = CardType.iMC3xx2E;
        public string CardTypeString = CardType.iMC3xx2E+"/"+CardType.iMC4xxE_A;
    }
    public class Movdata {
        public Movdata() { }
        public int Model = 0;
        public float StartAngle = 0;
        public float TargetAngle = 0;
        public float NeedTime = 0;
        public float StartTime = 0;
        public float EndTime = 0;
        public float cury_K = 0;
        /// <summary>
        /// 计算时间
        /// </summary>
        /// <param name="starttime"></param>
        /// <returns></returns>
        public float  Caculator(float starttime) {
            StartTime = starttime;
            EndTime = StartTime + NeedTime;
            if (Model == 3) {
                return EndTime;
            }
            if (NeedTime == 0) {
                return EndTime;
            }
            cury_K = (TargetAngle - StartAngle) / NeedTime;
            return EndTime;
        }
        public float GetAngle(float nowtime) {
            float detime = nowtime - StartTime;
            return StartAngle + cury_K * detime;
        }
    }
    public class Movement {
        public Movement() { }
        public List<Movdata> mlist = new List<Movdata>();

    }
    //public class MoveController {
    //    public DateTime StartTime=DateTime .Now;
    //    public bool BegionMove = false;
    //}
    public delegate void ShowInt(int pid);
    public delegate void OnMoveBack(Movdata mdata);
    public delegate void ONZITAI(float rx,float ry,float rz);

}

