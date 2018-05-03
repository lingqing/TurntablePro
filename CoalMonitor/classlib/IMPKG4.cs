using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace CoalMonitor.classlib.PK4
{
    public class IMPKG4
    {
        double[] m_ad = new double[8];
        /// <summary>
        /// 使能AD口
        /// </summary>
        /// <param name="handle"></param>
        public void AD_intlize(IntPtr handle) {
            //使能全部AD采样
            for (int i = 0; i < 8; i++)
            {
                IMC_Pkg4.PKG_IMC_SetADena(handle, 1, i);
            }
        }
        /// <summary>
        /// 获得输入的模拟量 电压
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public double[] AD_GetAdOut_double(IntPtr handle) {
            double ad;
            int st, i, data;
            string err;

            ad = 0;
            for (i = 0; i < 8; i++)
            {
                st = IMC_Pkg4.PKG_IMC_GetADdata(handle, ref ad, i);
                if (st == 0)
                {
                    err = IMC_Pkg4.GetFunErrStr();
                }
                else
                {
                    if (m_ad[i] != ad)
                    {
                        m_ad[i] = ad;
                        data = (int)(m_ad[i] / 10 * 4096 + 0.5);
                        //ListView1.Items[i].SubItems[1].Text = data.ToString();
                        //ListView1.Items[i].SubItems[2].Text = m_ad[i].ToString();
                    }
                }
            }
            return m_ad;

        }
        /// <summary>
        /// 获得输入的模拟量 转换值
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public int[] AD_GetAdOut_int(IntPtr handle)
        {
            double ad;
            int st, i, data;
            string err;
            List<int> Tdata = new List<int>();
            ad = 0;
            for (i = 0; i < 8; i++)
            {
                st = IMC_Pkg4.PKG_IMC_GetADdata(handle, ref ad, i);
                if (st == 0)
                {
                    err = IMC_Pkg4.GetFunErrStr();
                }
                else
                {
                    if (m_ad[i] != ad)
                    {
                        m_ad[i] = ad;
                        data = (int)(m_ad[i] / 10 * 4096 + 0.5);
                        //ListView1.Items[i].SubItems[1].Text = data.ToString();
                        //ListView1.Items[i].SubItems[2].Text = m_ad[i].ToString();
                        Tdata.Add(data);
                    }
                }
            }
            return Tdata.ToArray ();

        }
        /// <summary>
        /// 禁用模拟量输入
        /// </summary>
        /// <param name="handle"></param>
        public void AD_Disable_AD(IntPtr handle) {

            int st, id;
            string err;
            id = 0;  //模块ID， 应与设置时相同
            st = IMC_Pkg4.PKG_IMC_DisADCtrl(handle, id);
            if (st == 0)
            {
                err = IMC_Pkg4.GetFunErrStr();
            }
        }
        /// <summary>
        /// 设置模拟量输入
        /// </summary>
        /// <param name="handle">控制卡句柄</param>
        /// <param name="ADstart">控制范围起点</param>
        /// <param name="ADend">控制范围终点</param>
        /// <param name="ch">AD通道</param>
        /// <param name="tgstart">模拟量电压输入起点</param>
        /// <param name="tgend">模拟量电压输入终点</param>
        /// <param name="sel">控制目标</param>
        public void AD_setAD(IntPtr handle,double ADstart,double ADend,int ch,double tgstart,double tgend,int sel) {
            int st;
            string err;
            //控制卡中总共有g_naxis个控制模块，每个模块都是相互独立的
            //在这里全部使用1个控制模块，即模块0
            int id = 0;

            st = IMC_Pkg4.PKG_IMC_SetADCtrlEX(handle, ADstart, ADend, ch, tgstart, tgend, sel, id);
            if (sel > 0)
            {
                if (st > 0)
                    st = IMC_Pkg4.PKG_IMC_SetAccel(handle, 10, 10, sel - 1);
                if (st > 0)
                    st = IMC_Pkg4.PKG_IMC_P2Pvel(handle, 10.0, sel - 1);
            }
            if (st > 0)
                st = IMC_Pkg4.PKG_IMC_SetDAena(handle, 1, ch);
            if (st == 0)
            {
                err = IMC_Pkg4.GetFunErrStr();
            }

        }
    }
    //***********************************************************************************************************/
    //       此头文件为iMC参数地址的宏定义                                                                      */
    //       格式：                                                                                             */
    //       //描述                                                                                             */
    //       public const Int16   参数名Loc =      地址     // 轴或全局参数  数据格式                           */
    //                                                                                                          */
    //       其中“参数名”是指在iMC的参数名，其后加Loc表示该参数的地址                                         */
    //                                                                                                          */
    //       字母A表示轴参数，字母G表示全局参数                                                                 */
    //                                                                                                          */
    //       iMC中共有以下几种数据格式的类型：                                                                  */
    //       S32：32位带符号位的整数，即小数部分的位数为0，参数值的范围为：[-2147483648,2147483647]。           */
    //       U32：32位无符号位的整数，即小数部分的位数为0，参数值的范围为：[0, 4294967295]。                    */
    //       S16：16位带符号位的整数，即小数部分的位数为0，参数值的范围为：[-32768,32767]。                     */
    //       U16：16位无符号位的整数，即小数部分的位数为0，参数值的范围为：[0, 65535]。                         */
    //       F16：16位标志参数，仅取两个值：0或FFFFh。                                                          */
    //       R16：16位寄存器，各位域具有具体的意义，部分位域需设置。                                            */
    //       S16.32：48位带符号位，高16位为整数部分，低32位为小数部分，参数值的范围：[-32768.0,32767.999999999767] */
    //       U16.32：48位无符号位，高16位为整数部分，低32位为小数部分，参数值的范围：[0.0,65535.999999999767]   */
    //       S16.16：32位带符号位，高16位为整数部分，低16位为小数部分，参数值的范围：[-32768.0,32767.999984741211]  */
    //       U16.16：32位无符号位，高16位为整数部分，低32位为小数部分，参数值的范围：[0.0,65535.999984741211]。 */
    //       S0.16：16位有符号位，16位为小数部分。                                                              */
    //       因此，iMC中的参数值所表示的实际值为：                                                              */
    //       实际值=参数值/2^n                                                                                  */
    //       其中n为小数部分的位数。                                                                            */
    //       例如，对于一个S32格式的数，“00018000h”（h表示十六进制表示）表示的十进制的值为98304/2^0 = 98304； */
    //       对于一个S16.16格式的数，“00018000h”表示的十进制的值为98304/2^16 = 1.5。                          */
    //***********************************************************************************************************/
    public class ParamDef
    {
        /*********************************************************************************************************/
        #region 点到点运动参数
        /*********************************************************************************************************/
        //连续速度运动的目标速度（
        public const Int16 mcstgvelLoc = 6;         //A		S16.16

        //主坐标系是否处于速度斜升过程中（加速或减速）。FFFFh：处于速度斜升过程，0：等于目标速度运行。
        //另：电子齿轮运动时，可以判断从动轴是否已达到传动速度，即是否达到主动轴速度乘以传动比率的值。0：达到，FFFFh：未达到
        public const Int16 mcsslopeLoc = 8;         //A		F16

        //点到点运动的指令位置（目标位置）
        public const Int16 mcstgposLoc = 12;        //A		S32	

        //点到点运动的起始速度
        public const Int16 mcsstartvelLoc = 14;         //A		S16.16	

        //点到点运动的最大规划速度
        public const Int16 mcsmaxvelLoc = 16;           //A		S16.16	

        //点到点运动的加速度
        public const Int16 mcsaccelLoc = 18;            //A		S16.16

        //点到点运动的减速度
        public const Int16 mcsdecelLoc = 20;            //A		S16.16 

        //点到点运动的模式，0：普通模式，FFFFh：跟踪模式
        public const Int16 mcstrackLoc = 22;            //A		F16			

        //写入非零启动点到点运动，写入零，停止当前的点到点运动
        public const Int16 mcsgoLoc = 23;           //A		F16	

        //点到点运动的终点速度
        public const Int16 mcsendvelLoc = 26;           //A	

        //目标移动距离，写入的mcsdis必须不为0
        public const Int16 mcsdistLoc = 56;         //A		S32	

        #endregion
        /*********************************************************************************************************/
        #region 增量式点到点运动
        /*********************************************************************************************************/
        //增量式点到点运动，增量速度的来源轴号
        public const Int16 incmoveaxisLoc = 194;            //A		S16				

        //若非零，使能某速度值增量运动，如写入encsvel地址时是该轴跟随手轮运动
        public const Int16 incmoveptrLoc = 195;         //A		F16			

        //增量运动倍率
        public const Int16 incmoverateLoc = 196;            //A		S32			 

        #endregion
        /*********************************************************************************************************/
        #region 反向间隙误差补偿的参数
        /*********************************************************************************************************/
        //间隙补偿和线性补偿起始速度。
        public const Int16 cpstartvelLoc = 41;          //A		S16.16

        //间隙补偿和线性补偿最高速度。
        public const Int16 cpmaxvelLoc = 43;            //A		S16.16

        //间隙补偿和线性补偿加速度。
        public const Int16 cpaccelLoc = 45;         //A		S16.16

        //间隙补偿和线性补偿减速度。
        public const Int16 cpdecelLoc = 47;         //A		S16.16

        //补偿使能。写入非零值即使能间隙补偿和线性补偿，清零则禁止。
        public const Int16 enacompenLoc = 49;           //A		F16

        //间隙补偿和线性补偿终止速度。
        public const Int16 cpendvelLoc = 53;            //A		S16.16

        //反向间隙误差大小，单位：脉冲。
        public const Int16 backlashLoc = 229;           //A		S32

        #endregion


        /*********************************************************************************************************/
        #region 主编码器相关参数
        /*********************************************************************************************************/
        //当前主编码器速度
        public const Int16 encpvelLoc = 75;         //A		S16.16 				

        //主编码器的计数寄存器
        public const Int16 encpLoc = 78;            //A		S32						

        //主编码器控制寄存器
        public const Int16 encpctrLoc = 539;            //A					

        #endregion
        /*********************************************************************************************************/
        #region 辅编码器相关参数
        /*********************************************************************************************************/
        //辅编码器的倍率因子
        public const Int16 encsfactorLoc = 343;         //G		S16.16 			

        //当前辅编码器速度
        public const Int16 encsvelLoc = 345;            //G		S16.16 				

        //辅编码器的计数寄存器
        public const Int16 encsLoc = 348;           //G		S32						

        //辅助编码器的控制寄存器
        public const Int16 encsctrLoc = 531;            //G		R16					

        //写操作清零encs
        public const Int16 clrencsLoc = 532;            //G		S16					

        #endregion
        /*********************************************************************************************************/
        #region 搜原点
        /*********************************************************************************************************/
        //搜索原点时设置原点的方向(间隙补偿方向,0:负方向走时补偿，FFFFh:正方向时补偿)
        public const Int16 homedirLoc = 235;            //A		

        //原点的偏移位置，设置机械原点时，该值被拷贝到指令位置寄存器curpos和编码器寄存器encp中。
        public const Int16 homeposLoc = 145;            //A		S32				

        //搜寻原点时低速段的速度
        public const Int16 lowvelLoc = 147;         //A		S16.16 			

        //搜寻原点时高速段的速度
        public const Int16 highvelLoc = 149;            //A		S16.16 			

        //搜寻原点的过程和方式控制寄存器
        public const Int16 homeseqLoc = 151;            //A		R16					

        //非零则开始执行搜寻原点，清零则停止
        public const Int16 gohomeLoc = 152;         //A		F16					

        //主机写入非零值则设当前位置为原点
        public const Int16 sethomeLoc = 153;            //A		F16					

        //非零表示已搜寻到原点
        public const Int16 homedLoc = 154;          //A		F16						

        //记录搜零时走过的距离
        public const Int16 homemovedistLoc = 163;           //A		S32				

        #endregion

        /*********************************************************************************************************/
        #region 其它轴参数	
        /*********************************************************************************************************/

        //写入非零运行该轴，0：停止运行该轴
        public const Int16 runLoc = 128;            //A		F16						

        //错误寄存器
        public const Int16 errorLoc = 130;          //A		R16						

        //设置最大允许位置误差
        public const Int16 poserrlimLoc = 131;          //A		S16					

        //平滑因子
        public const Int16 smoothLoc = 132;         //A		S16					

        //立刻暂停轴
        public const Int16 axispauseatonceLoc = 133;            //A		F16		

        //静止窗口
        public const Int16 settlewinLoc = 134;          //A		S16					

        //从停止运动到静止的时间（周期个数）
        public const Int16 settlenLoc = 135;        //A		S16					

        //错误时的停止过滤寄存器
        public const Int16 stopfiltLoc = 136;           //A		R16					

        //错误时立刻暂停该轴
        public const Int16 stopmodeLoc = 137;           //A		R16		

        //错误时的退出运行过滤寄存器
        public const Int16 exitfiltLoc = 138;           //A		R16				

        //正向软极限位置
        public const Int16 psoftlimLoc = 139;           //A		S32					

        //负向软极限位置
        public const Int16 nsoftlimLoc = 141;           //A		S32		

        //写入非零值对该轴实施清零操作。清零该轴的编码器计数、指令位置、目标位置等各种位置参数，
        //	以及各种运动状态标志：mcspos、mcstgpos、curpos、encp、pcspos、pcstgpos、status、
        //	error、emstop、hpause、events、encs、ticks、aiolat。
        public const Int16 clearLoc = 157;          //A		S16		

        //设置该轴的最大速度限制。无论何种运动模式，只要实际速度超出此极限值，将置位错误寄存器error中的位VELLIM域，
        //	此错误不可屏蔽，因此只要发生此错误，立刻退出该轴运行。注意：必须为正值。
        public const Int16 vellimLoc = 158;         //A		S16.16 			

        //设置该轴的最大加速度限制。无论何种运动模式，只要实际加速度超出此极限值，将置位错误寄存器error中的ACCLIM位域，
        //	此错误不可屏蔽，因此只要发生此错误，立刻退出该轴运行。注意：accellim必须为正值。
        public const Int16 accellimLoc = 160;           //A		S16.16 			

        //静止误差补偿速度
        public const Int16 fixvelLoc = 162;         //A		S0.16 				

        #endregion
        /*********************************************************************************************************/
        #region 电子齿轮相关的参数
        /*********************************************************************************************************/
        //电子齿轮运动模式中主动轴的轴号
        public const Int16 masterLoc = 169;         //A		S16				

        //指针，指向从动轴所跟随的主动轴的参数
        public const Int16 gearsrcLoc = 170;            //A		S16					

        //写入非零值开始接合，清零则脱离啮合
        public const Int16 engearLoc = 171;         //A		F16				

        //传动比率  
        public const Int16 gearratioLoc = 175;      //A		S16.32			
        #endregion
        /*********************************************************************************************************/
        #region 环形轴相关参数
        /*********************************************************************************************************/
        //设置环形轴的最大位置  
        public const Int16 cirposLoc = 184;         //A		S32			

        //设置该轴为线性轴或环形轴。若ciraxis为0，该轴为线性轴；若为FFFFh，该轴设置为环形轴。
        public const Int16 ciraxisLoc = 186;            //A		S16			

        //单向或双向环形标志。
        //若为0，为单向环形，位置范围为[0,cirpos)；若为非零，为双向环形，位置范围为(-cirpos,cirpos)。
        public const Int16 biciraxisLoc = 187;          //A		S16		 

        //记录循环次数，向上溢出加1；向下溢出减1
        public const Int16 cirswapLoc = 214;            //A		S16			
        #endregion
        /*********************************************************************************************************/
        #region 状态标志参数（只读参数）
        /*********************************************************************************************************/
        //逻辑位置
        public const Int16 logicposLoc = 225;           //A		S32

        //当前指令速度
        public const Int16 logicvelLoc = 227;           //A		S16.16

        //当前指令位置
        public const Int16 curposLoc = 59;          //A		S32

        //当前指令速度
        public const Int16 curvelLoc = 73;          //A		S16.16

        //标志是否规划运动  0：规划运动已停止，FFFFh：规划运动中（包括主坐标系，以及轮廓运动）
        public const Int16 profilingLoc = 215;          //A		F16			

        //标志是否正参与轮廓运动，FFFFh：轮廓运动中，0：轮廓运动已结束或CFIFO已空。
        public const Int16 contouringLoc = 217;         //A		F16			

        //标志是否规划运动以及运动平滑处理中，
        //0：停止规划运动且停止平滑处理，FFFFh：规划运动未完成或运动平滑处理进行中。
        public const Int16 movingLoc = 218;         //A		F16				

        //电机是否静止，0：规划运动已完成，且电机已静止；FFFFh：规划运动未完成，或虽已完成运动规划，但电机尚未静止。
        public const Int16 motionLoc = 219;         //A		F16				

        //位置误差越出静止窗口标志，若outsettle=FFFFh，表明当前位置误差poserr大于静止窗口参数settlewin。
        public const Int16 outsettleLoc = 220;          //A		F16				

        //当前位置误差值，指令位置与实际位置（反馈值）之差：poserr=curpos-encp。
        public const Int16 poserrLoc = 223;         //A		S16					
        #endregion
        /*********************************************************************************************************/
        #region 指令脉冲相关参数
        /*********************************************************************************************************/
        //脉冲输出模式及信号极性设置寄存器	
        public const Int16 stepmodLoc = 615;            //A		F16				

        //设置方向信号变化的延迟时间，单位是系统的时钟周期
        public const Int16 dirtimeLoc = 618;            //A		S16				

        //设定脉冲有效电平宽度，单位是系统的时钟周期
        public const Int16 steptimeLoc = 619;           //A		S16				
        #endregion
        /*********************************************************************************************************/
        #region 探针或index计数相关参数
        /*********************************************************************************************************/
        //探针或index的计数值
        public const Int16 counterLoc = 541;            //A		S16				

        //写操作则清零探针或index的计数值
        public const Int16 clrcounterLoc = 541;         //A		S16			
        #endregion
        /*********************************************************************************************************/
        #region 轮廓运动相关的参数
        /*********************************************************************************************************/
        //开启轮廓运动
        public const Int16 startgroupLoc = 256;         //G		F16				

        //参与轮廓运动的轴数
        public const Int16 groupnumLoc = 257;           //G		S16				

        //轮廓运动的轴组中，X轴对应的轴号
        public const Int16 group_xLoc = 258;            //G		S16				

        //轮廓运动的轴组中，Y轴对应的轴号
        public const Int16 group_yLoc = 259;            //G		S16				

        //轮廓运动的轴组中，Z轴对应的轴号
        public const Int16 group_zLoc = 260;            //G		S16				

        //轮廓运动的轴组中，A轴对应的轴号
        public const Int16 group_aLoc = 261;            //G		S16				

        //轮廓运动的轴组中，B轴对应的轴号
        public const Int16 group_bLoc = 262;            //G		S16				

        //轮廓运动的轴组中，C轴对应的轴号
        public const Int16 group_cLoc = 263;            //G		S16				

        //轮廓运动的轴组中，D轴对应的轴号
        public const Int16 group_dLoc = 264;            //G		S16				

        //轮廓运动的轴组中，E轴对应的轴号
        public const Int16 group_eLoc = 265;            //G		S16				

        //轮廓运动的轴组中，F轴对应的轴号
        public const Int16 group_fLoc = 266;            //G		S16				

        //轮廓运动的轴组中，G轴对应的轴号
        public const Int16 group_gLoc = 267;            //G		S16				

        //轮廓运动的轴组中，H轴对应的轴号
        public const Int16 group_hLoc = 268;            //G		S16				

        //轮廓运动的轴组中，I轴对应的轴号
        public const Int16 group_iLoc = 269;            //G		S16				

        //轮廓运动的轴组中，J轴对应的轴号
        public const Int16 group_jLoc = 270;            //G		S16				

        //轮廓运动的轴组中，K轴对应的轴号
        public const Int16 group_kLoc = 271;            //G		S16				

        //轮廓运动的轴组中，L轴对应的轴号
        public const Int16 group_lLoc = 272;            //G		S16				

        //轮廓运动的轴组中，M轴对应的轴号
        public const Int16 group_mLoc = 273;            //G		S16				

        //轮廓运动的平滑拟合时间，单位为控制周期
        public const Int16 groupsmoothLoc = 274;            //G		S16		  

        #endregion
        /*********************************************************************************************************/
        #region 轮廓运动专用CFIFO缓存器的相关参数
        /*********************************************************************************************************/
        //CFIFO中数据（WORD）的个数
        public const Int16 CFIFOcntLoc = 519;           //G		S16				

        //写操作清空CFIFO
        public const Int16 clrCFIFOLoc = 519;           //G		S16				

        #endregion
        /*********************************************************************************************************/
        #region  IFIFO/QFIFO缓存器相关参数
        /*********************************************************************************************************/
        //写操作清空IFIFO
        public const Int16 clrififoLoc = 513;           //G		S16				

        //IFIFO中数据（WORD）的个数
        public const Int16 ififocntLoc = 513;           //G		S16				

        //写操作清空QFIFO
        public const Int16 clrqfifoLoc = 521;           //G		S16				

        //QFIFO中数据（WORD）的个数
        public const Int16 qfifocntLoc = 521;           //G		S16				

        //QFIFO的等待指令的超时时间
        public const Int16 qwaittimeLoc = 492;          //G		S32			
        #endregion
        /*********************************************************************************************************/
        #region 插补运动相关参数
        /*********************************************************************************************************/
        //插补运动的轴参数

        //设置段的坐标数据以绝对值还是相对值表示。 0：段数据表示的是相对值，非零：段数据表示的是绝对值
        public const Int16 pathabsLoc = 205;            //A		S16				

        //当前执行段的终点
        public const Int16 segendLoc = 202;         //A		S32				

        //当前执行段的起始点
        public const Int16 segstartLoc = 200;           //A		S32				

        //该轴是否参与插补空间1的插补
        public const Int16 moveinpath1Loc = 204;            //A		F16			

        //该轴是否参与插补空间2的插补
        public const Int16 moveinpath2Loc = 165;            //A		F16			
        #endregion
        /*********************************************************************************************************/
        #region 插补空间1的参数
        /*********************************************************************************************************/
        //写入非零开始执行路径运动
        public const Int16 startpath1Loc = 352;         //G		F16			

        //标志是否正在执行插补
        public const Int16 pathmoving1Loc = 354;            //G		F16			

        //当前执行圆弧段的方向，0：顺时针，非零：逆时针
        public const Int16 arcdir1Loc = 355;            //G		S16			

        //插补路径规划速度方式
        //指定速度规划是否基于该段合成路径的长度，或某个轴在该段的移动距离。	
        //若asseglen为0，速度规划基于X、Y、Z三轴的合成路径长度，即pathvel是合成路径的速度。
        //当然，当pathaxisnum小于3时，则只有X轴或X、Y轴合成路径长度。
        //若asseglen非零，asseglen必须为1~pathaxisnum范围的一个值，表示采用segmap_x、segmap_y…所映射的轴的
        //移动距离进行速度规划，如1表示采用X轴的移动距离规划速度，因此pathvel即为X轴的速度。
        public const Int16 asseglen1Loc = 361;          //G		S16			

        //当前路径速度
        public const Int16 pathvel1Loc = 362;           //G		S16.16				

        //路径加速度
        public const Int16 pathacc1Loc = 366;           //G		S16.16				

        //参与路径运动的轴数
        public const Int16 pathaxisnum1Loc = 370;           //G		S16		 

        //映射为X轴的轴号
        public const Int16 segmap_x1Loc = 371;          //G		S16			

        //映射为Y轴的轴号
        public const Int16 segmap_y1Loc = 372;          //G		S16			

        //映射为Z轴的轴号
        public const Int16 segmap_z1Loc = 373;          //G		S16			

        //映射为A轴的轴号
        public const Int16 segmap_a1Loc = 374;          //G		S16			

        //映射为B轴的轴号
        public const Int16 segmap_b1Loc = 375;          //G		S16			

        //映射为C轴的轴号
        public const Int16 segmap_c1Loc = 376;          //G		S16			

        //映射为D轴的轴号
        public const Int16 segmap_d1Loc = 377;          //G		S16			

        //映射为E轴的轴号
        public const Int16 segmap_e1Loc = 378;          //G		S16			

        //映射为F轴的轴号
        public const Int16 segmap_f1Loc = 379;          //G		S16			

        //映射为G轴的轴号
        public const Int16 segmap_g1Loc = 380;          //G		S16			

        //映射为H轴的轴号
        public const Int16 segmap_h1Loc = 381;          //G		S16			

        //映射为I轴的轴号
        public const Int16 segmap_i1Loc = 382;          //G		S16			

        //映射为J轴的轴号
        public const Int16 segmap_j1Loc = 383;          //G		S16			

        //映射为K轴的轴号
        public const Int16 segmap_k1Loc = 384;          //G		S16			

        //映射为L轴的轴号
        public const Int16 segmap_l1Loc = 385;          //G		S16			

        //映射为M轴的轴号
        public const Int16 segmap_m1Loc = 386;          //G		S16			

        //段的目标运行速度
        public const Int16 segtgvel1Loc = 387;          //G		S16.16			

        //段的段末速度
        public const Int16 segendvel1Loc = 389;         //G		S16.16			

        //段的ID，用于标识正在执行第几段，每执行一段，该ID加1
        public const Int16 segID1Loc = 391;         //G		S32				

        //当前执行段的长度
        public const Int16 seglen1Loc = 393;            //G		S32				

        //当前执行圆弧段的半径
        public const Int16 radius1Loc = 395;            //G		S32				
        #endregion
        /*********************************************************************************************************/
        #region PFIFO1缓存器相关参数
        /*********************************************************************************************************/
        //PFIFO1中数据（WORD）个数
        public const Int16 PFIFOcnt1Loc = 565;          //G		S16			

        //写操作清空PFIFO1
        public const Int16 clrPFIFO1Loc = 565;          //G		S16			

        //PFIFO1等待指令超时时间
        public const Int16 pwaittime1Loc = 399;         //G		S16			
        #endregion
        /*********************************************************************************************************/
        #region 插补空间2的参数
        /*********************************************************************************************************/
        //写入非零开始执行路径运动
        public const Int16 startpath2Loc = 405;         //G		F16		

        //标志是否正在执行插补
        public const Int16 pathmoving2Loc = 407;            //G		F16			

        //当前执行圆弧段的方向，0：顺时针，非零：逆时针
        public const Int16 arcdir2Loc = 408;            //G		S16				

        //插补路径规划速度方式
        //指定速度规划是否基于该段合成路径的长度，或某个轴在该段的移动距离。	
        //若asseglen为0，速度规划基于X、Y、Z三轴的合成路径长度，即pathvel是合成路径的速度。
        //当然，当pathaxisnum小于3时，则只有X轴或X、Y轴合成路径长度。
        //若asseglen非零，asseglen必须为1~pathaxisnum范围的一个值，表示采用segmap_x、segmap_y…所映射的轴的
        //移动距离进行速度规划，如1表示采用X轴的移动距离规划速度，因此pathvel即为X轴的速度。
        public const Int16 asseglen2Loc = 414;          //G		S16			

        //当前路径速度
        public const Int16 pathvel2Loc = 415;           //G		S16.16				

        //路径加速度
        public const Int16 pathacc2Loc = 419;           //G		S16.16				

        //参与路径运动的轴数
        public const Int16 pathaxisnum2Loc = 423;           //G		S16			

        //映射为X轴的轴号
        public const Int16 segmap_x2Loc = 424;          //G		S16			

        //映射为Y轴的轴号
        public const Int16 segmap_y2Loc = 425;          //G		S16			

        //映射为Z轴的轴号
        public const Int16 segmap_z2Loc = 426;          //G		S16			

        //映射为A轴的轴号
        public const Int16 segmap_a2Loc = 427;          //G		S16			

        //映射为B轴的轴号
        public const Int16 segmap_b2Loc = 428;          //G		S16			

        //映射为C轴的轴号
        public const Int16 segmap_c2Loc = 429;          //G		S16			

        //映射为D轴的轴号
        public const Int16 segmap_d2Loc = 430;          //G		S16			

        //映射为E轴的轴号
        public const Int16 segmap_e2Loc = 431;          //G		S16			

        //映射为F轴的轴号
        public const Int16 segmap_f2Loc = 432;          //G		S16			

        //映射为G轴的轴号
        public const Int16 segmap_g2Loc = 433;          //G		S16			

        //映射为H轴的轴号
        public const Int16 segmap_h2Loc = 434;          //G		S16			

        //映射为I轴的轴号
        public const Int16 segmap_i2Loc = 435;          //G		S16			

        //映射为J轴的轴号
        public const Int16 segmap_j2Loc = 436;          //G		S16			

        //映射为K轴的轴号
        public const Int16 segmap_k2Loc = 437;          //G		S16			

        //映射为L轴的轴号
        public const Int16 segmap_l2Loc = 438;          //G		S16			

        //映射为M轴的轴号
        public const Int16 segmap_m2Loc = 439;          //G		S16			

        //段的目标运行速度
        public const Int16 segtgvel2Loc = 440;          //G		S16.16			

        //段的段末速度
        public const Int16 segendvel2Loc = 442;         //G		S16.16			

        //段的ID，用于标识正在执行第几段，每执行一段，该ID加1
        public const Int16 segID2Loc = 444;         //G		S32				

        //当前执行段的长度
        public const Int16 seglen2Loc = 446;            //G		S32				

        //当前执行圆弧段的半径
        public const Int16 radius2Loc = 448;            //G		S32				

        #endregion
        /*********************************************************************************************************/
        #region PFIFO2缓存器相关参数	 
        /*********************************************************************************************************/
        //PFIFO2中数据个数（WORD）
        public const Int16 PFIFOcnt2Loc = 685;          //G		S16		

        //写操作则清零PFIFO2
        public const Int16 clrPFIFO2Loc = 685;          //G		S16			

        //PFIFO2等待指令超时时间
        public const Int16 pwaittime2Loc = 452;         //G		S16			

        #endregion
        /*********************************************************************************************************/
        #region 输入输出（I/O）相关参数
        /*********************************************************************************************************/
        //脉冲输出及驱动器使能。写入FFFFh则使能脉冲输出及使能驱动器。注意：写入FFFFh，驱动器使能信号为低电平，
        //写入0则为高电平。若需将轴作为虚拟轴(运行正常，但不输出脉冲)，可清零ena。
        //另：可以用于判断脉冲输出和驱动器是否使能，0：脉冲输出和驱动器禁止，FFFFh：脉冲输出和驱动器使能。
        public const Int16 enaLoc = 550;            //A		F16				

        //轴IO电平状态寄存器
        //轴I/O数据寄存器。读出的是对应管脚的实时信号值；
        public const Int16 aioLoc = 683;            //A		R16				

        //轴IO设置寄存器
        public const Int16 aioctrLoc = 680;         //A		R16			

        //轴IO锁存寄存器
        public const Int16 aiolatLoc = 682;         //A		R16			

        //相应位域写入1则清零该位域
        public const Int16 clraiolatLoc = 682;          //A		R16		 

        //全局开关量输出gout1
        public const Int16 gout1Loc = 560;          //G		R16			

        //全局开关量输出gout2 
        public const Int16 gout2Loc = 561;          //G		R16			

        //全局开关量输出gout3
        public const Int16 gout3Loc = 555;          //G		R16			

        //全局开关量输入gin1
        public const Int16 gin1Loc = 706;           //G		R16				

        //全局开关量输入gin2
        public const Int16 gin2Loc = 707;           //G		R16				

        //锁存全局开关量输入gin1的有效边沿
        public const Int16 gin1latLoc = 612;            //G		R16			

        //锁存全局开关量输入gin2的有效边沿
        public const Int16 gin2latLoc = 613;            //G		R16			

        //伺服驱动器误差清零
        public const Int16 srstLoc = 551;           //A		R16				

        //读得到停止开关的状态，写则设置停止开关的有效极性
        public const Int16 stopinLoc = 563;         //G		R16			

        //输入开关量的数字滤波设置寄存器
        public const Int16 swfilterLoc = 548;           //G		S16			

        //使能srst作为伺服脉冲清除，并设置其脉冲宽度
        public const Int16 srstctrLoc = 552;            //A		R16		  

        #endregion
        /*********************************************************************************************************/
        #region 计时单元相关的参数
        /*********************************************************************************************************/
        //毫秒计时，写入毫秒数，每毫秒减1
        public const Int16 delaymsLoc = 704;            //G		S32	

        //delayms倒计时为0后该参数为0
        public const Int16 delayoutLoc = 704;           //G		S16			

        //倒计时计数器，写入非零开始每周期减1
        public const Int16 timerLoc = 481;          //G		S16			

        //控制周期计数器，每控制周期加1
        public const Int16 ticksLoc = 502;          //G		S32			

        #endregion
        /*********************************************************************************************************/
        #region 事件指令相关参数
        /*********************************************************************************************************/
        //事件指令数量，清零则禁止所有事件指令
        public const Int16 eventsLoc = 489;         //G		S16	
        #endregion

        /*********************************************************************************************************/
        #region 急停/暂停相关参数
        /*********************************************************************************************************/
        //某些位域若置位，则使所有轴的error寄存器相应位域置位
        public const Int16 emstopLoc = 500;         //G		R16		

        //非零则暂停
        public const Int16 hpauseLoc = 501;         //G		F16		
        #endregion

        /*********************************************************************************************************/
        #region  系统参数 Read only
        /*********************************************************************************************************/
        //控制分频
        public const Int16 clkdivLoc = 509;         //G		S16			

        //firmware 版本
        public const Int16 fwversionLoc = 511;          //G		S16			

        //系统基准时钟
        public const Int16 sysclkLoc = 628;         //G		S32				

        //该型号iMC支持的轴数
        public const Int16 naxisLoc = 634;          //G		S16				

        //硬件版本
        public const Int16 hwversionLoc = 635;          //G		S16			

        //清空所有FIFO和残留的等待指令等
        public const Int16 clearimcLoc = 494;           //G		S16		
        #endregion

        /*********************************************************************************************************/
        #region  预定义用户参数
        /*********************************************************************************************************/
        public const Int16 user16b0Loc = 307;           //G		S16		  	//16bit的预定义用户参数0
        public const Int16 user16b1Loc = 308;           //G		S16			//16bit的预定义用户参数1
        public const Int16 user16b2Loc = 309;           //G		S16			//16bit的预定义用户参数2
        public const Int16 user16b3Loc = 310;           //G		S16			//16bit的预定义用户参数3
        public const Int16 user16b4Loc = 311;           //G		S16			//16bit的预定义用户参数4
        public const Int16 user16b5Loc = 312;           //G		S16			//16bit的预定义用户参数5
        public const Int16 user16b6Loc = 313;           //G		S16			//16bit的预定义用户参数6
        public const Int16 user16b7Loc = 314;           //G		S16			//16bit的预定义用户参数7
        public const Int16 user16b8Loc = 315;           //G		S16			//16bit的预定义用户参数8
        public const Int16 user16b9Loc = 316;           //G		S16			//16bit的预定义用户参数9

        public const Int16 user32b0Loc = 317;           //G		S32			//32bit的预定义用户参数0
        public const Int16 user32b1Loc = 319;           //G		S32			//32bit的预定义用户参数1
        public const Int16 user32b2Loc = 321;           //G		S32			//32bit的预定义用户参数2
        public const Int16 user32b3Loc = 323;           //G		S32			//32bit的预定义用户参数3
        public const Int16 user32b4Loc = 325;           //G		S32			//32bit的预定义用户参数4
        public const Int16 user32b5Loc = 327;           //G		S32			//32bit的预定义用户参数5
        public const Int16 user32b6Loc = 329;           //G		S32			//32bit的预定义用户参数6
        public const Int16 user32b7Loc = 331;           //G		S32			//32bit的预定义用户参数7
        public const Int16 user32b8Loc = 333;           //G		S32			//32bit的预定义用户参数8
        public const Int16 user32b9Loc = 335;           //G		S32			//32bit的预定义用户参数9

        public const Int16 user48b0Loc = 337;           //G		S48			//48bit的预定义用户参数0
        public const Int16 user48b1Loc = 340;           //G		S48			//48bit的预定义用户参数1

        #endregion

        /*********************************************************************************************************/
        #region  位置捕获参数
        /*********************************************************************************************************/
        //写操作则清空位置捕获缓存器capfifo
        public const Int16 clrcapfifoLoc = 540;         //A		S16		

        //capfifo中已压入的位置数据个数
        public const Int16 capfifocntLoc = 540;         //A		S16			

        public const Int16 compdistLoc = 542;           //A		S32		
        #endregion

        /*********************************************************************************************************/
        #region  双龙门驱动
        /*********************************************************************************************************/
        //写入curvel的地址后，使能该轴进入龙门跟随主轴驱动
        public const Int16 gantrymainptrLoc = 188;          //A		S16		

        //龙门主轴轴号
        public const Int16 gantrymainaxisLoc = 189;         //A		S16				

        //龙门主从误差限值，即若 |从poserr-主poserr|>=gantryerrlim,置位主从error
        public const Int16 gantryerrlimLoc = 190;           //A		S16			 	

        //龙门误差补偿增益kp
        public const Int16 gantrykpLoc = 191;           //A		S16				

        //龙门驱动时，从动轴的错误哪些bit映射到主轴的错误寄存器
        public const Int16 gantryerrormapLoc = 192;         //A		S16				

        #endregion
        /*********************************************************************************************************/
        #region  tgpos proportion follow
        /*********************************************************************************************************/
        //跟随的参数
        public const Int16 propfollowptrLoc = 103;          //A		S16		

        //跟随的参数所在的轴号
        public const Int16 propfollowaxisLoc = 174;         //A		S16			

        //跟随的参数起始值
        public const Int16 propfollowfromLoc = 104;         //A		S32				

        //跟随的参数终结值
        public const Int16 propfollowtoLoc = 106;           //A		S32				

        //对应于跟随参数起始值时的输出值
        public const Int16 propstartLoc = 108;          //A		S32				

        //对应于跟随参数终结值时的输出值
        public const Int16 propendLoc = 110;            //A		S32				

        //跟随结果挂钩的参数
        public const Int16 prophookptrLoc = 172;            //A		S16			

        //跟随结果挂钩的参数所在的轴号
        public const Int16 prophookaxisLoc = 173;           //A		S16			


        public const Int16 propshiftLoc = 178;          //A		S32		

        #endregion

        /*********************************************************************************************************/
        #region  DA/PWM output proportion follow
        /*********************************************************************************************************/
        //跟随的参数
        public const Int16 PWMfollowptrLoc = 155;           //A		S16		

        //跟随的参数所在的轴号
        public const Int16 PWMfollowaxisLoc = 156;          //A		S16			

        //跟随的参数
        public const Int16 DAfollowptrLoc = 143;            //A		S16				

        //跟随的参数所在的轴号
        public const Int16 DAfollowaxisLoc = 144;           //A		S16			

        //跟随的参数起始值
        public const Int16 DAfollowfromLoc = 112;           //A		S32				

        //跟随的参数终结值
        public const Int16 DAfollowtoLoc = 114;         //A		S32				

        //对应于跟随参数起始值时的pwm值(频率或占空比)
        public const Int16 DAstartLoc = 116;            //A		S32				

        //对应于跟随参数终结值时的pwm值
        public const Int16 DAendLoc = 118;          //A		S32				

        //DA输出偏移值
        public const Int16 DAshiftLoc = 216;            //A		S16				

        //PWM输出偏移值
        public const Int16 PWMshiftLoc = 221;           //A		S32				

        //DA使能
        public const Int16 DAenaLoc = 700;          //A		S16				

        //PWM使能
        public const Int16 PWMctrLoc = 702;         //A		S16				

        //设定PWM的频率,pwmfreq = (f/1000)*65536，f为pwm的实际频率（脉冲/秒）。
        public const Int16 PWMfreqLoc = 708;            //A		S16.16		

        //PWM输出的占空比
        public const Int16 PWMpropLoc = 129;            //A		S16	

        #endregion

        /*********************************************************************************************************/
        #region Fix proportion follow
        /*********************************************************************************************************/
        //线性误差补偿跟随的参数
        public const Int16 fixfollowptrLoc = 166;           //A		S16		

        //线性误差补偿跟随的参数所在的轴号
        public const Int16 fixfollowaxisLoc = 167;          //A		S16			

        //线性误差补偿跟随的参数起始值
        public const Int16 fixfollowfromLoc = 120;          //A		S32				

        //跟随的参数终结值
        public const Int16 fixfollowtoLoc = 122;            //A		S32				

        //对应于跟随参数起始值时的补偿值
        public const Int16 fixstartLoc = 124;           //A		S32				

        //对应于跟随参数终结值时的补偿值
        public const Int16 fixendLoc = 126;         //A		S32				

        #endregion

        /*********************************************************************************************************/
        #region AD采样
        /*********************************************************************************************************/
        //AD通道使能，bit0~bit7中的bit为1,对应的通道使能
        public const Int16 ADchannelLoc = 614;          //G		S16			

        //16bit的AD数据，只读
        public const Int16 ADdataLoc = 696;         //A		S16			

        //32bit的AD数据，用于跟随等
        public const Int16 ADdata32Loc = 696;           //A		S32			

        #endregion


        /*********************************************************************************************************/
        #region 设置通信监测
        public const Int16 comdogLoc = 703;         //G		S16							

        #endregion
        //全局进给倍率	——需在内存中预设为 000010000h
        public const Int16 feedrateLoc = 350;           //G		S16.16



    }
    //FIFO编号
    public enum FIFO_SEL
    {
        SEL_IFIFO,
        SEL_QFIFO,
        SEL_PFIFO1,
        SEL_PFIFO2,
        SEL_CFIFO,
    }
    public class MyDef
    {
        //定义读写结构体中参数位宽
        public const Int16 IMC_REG_BIT_W16 = 1;
        public const Int16 IMC_REG_BIT_W32 = 2;
        public const Int16 IMC_REG_BIT_W48 = 3;
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
        IMC_Allways,		//“无条件执行”

        IMC_Edge_Zero,		//“边沿型条件执行”——变为0时
        IMC_Edge_NotZero,	//“边沿型条件执行”——变为非0时
        IMC_Edge_Great, 	//“边沿型条件执行”——变为大于时
        IMC_Edge_GreatEqu, 	//“边沿型条件执行”——变为大于等于时
        IMC_Edge_Little,	//“边沿型条件执行”——变为小于时
        IMC_Edge_Carry,		//“边沿型条件执行”——变为溢出时
        IMC_Edge_NotCarry, 	//“边沿型条件执行”——变为无溢出时

        IMC_IF_Zero,		//“电平型条件执行”——若为0
        IMC_IF_NotZero, 	//“电平型条件执行”——若为非0
        IMC_IF_Great,		//“电平型条件执行”——若大于
        IMC_IF_GreatEqu, 	//“电平型条件执行”——若大于等于
        IMC_IF_Little, 		//“电平型条件执行”——若小于
        IMC_IF_Carry,		//“电平型条件执行”——若溢出
        IMC_IF_NotCarry		//“电平型条件执行”——若无溢出
    }
    public class IMC_Pkg4
    {
        public static string GetFunErrStr()
        {
            string err;
            IntPtr errptr;
            // err = IMC_Pkg.PKG_IMC_GetFunErrStrA();
            errptr =PKG_IMC_GetFunErrStrW();
            err = System.Runtime.InteropServices.Marshal.PtrToStringUni(errptr);
            return err;
        }

        [DllImport(("IMC_Pkg4xxx.dll"), EntryPoint = "PKG_IMC_FindNetCard")]
        //static extern int DllRegisterServer();
        public static extern int
            PKG_IMC_FindNetCard(byte[] info,        //返回找到的网卡名称
                                ref int num);       //返回找到的网卡数量
                                                    //获得以太网卡的数量
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetNetCardNum();                //返回找到的网卡数量
                                                    //获得对应索引的网卡名称
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetNetCardName(int index,       //网卡索引	
                                    byte[] name);//返回对应索引的网卡名称
        //打开控制卡设备，与设备建立通信连接
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern IntPtr
            PKG_IMC_Open(int netcardsel,            //网卡索引，由搜索网卡函数返回的结果决定
                                int imcid);		//IMC控制卡的id，由控制卡上的拨码开关设置决定
        //打开控制卡设备，与设备建立通信连接
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern IntPtr
            PKG_IMC_OpenX(int netcardsel,           //网卡索引，由搜索网卡函数返回的结果决定
                                int imcid,          //IMC控制卡的id，由控制卡上的拨码开关设置决定
                                int timeout,        //通信超时时间，单位毫秒
                                int openMode);		//打开模式；1：混杂模式， 0：非混杂模式
        //使用密码打开控制卡设备，与设备建立通信连接
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern IntPtr
            PKG_IMC_OpenUsePassword(int netcardsel,     //网卡索引，由搜索网卡函数返回的结果决定
                                int imcid,  			//IMC控制卡的id，由控制卡上的拨码开关设置决定
                                ref sbyte password,		//密码字符串
                                int pwlen);		        //密码长度
        //关闭打开的设备。
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_Close(IntPtr Handle);
        //获取通信密码
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetPassword(IntPtr Handle, 	    //设备句柄
                            sbyte[] password, 		//密码
                            ref sbyte pwlen);		//密码长度
        //设置通信密码
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            IMC_SetPassword(IntPtr Handle, 	    	//设备句柄
                            ref sbyte oldpassword, 	//旧设备密码
                            sbyte pwolen, 			//旧设备密码长度
                            ref sbyte password, 		//新设备密码
                            sbyte pwnlen, 			//新设备密码长度
                            sbyte[] rPW, 			//通信密码
                            ref sbyte rpwlen);		//通信密码长度

        //配置函数
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_IMC_InitCfg(IntPtr handle);
        //清空控制卡中所有的FIFO
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_ClearIMC(IntPtr handle); 	    //设备句柄
        //清空指定轴的所有状态
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_ClearAxis(IntPtr handle,        //设备句柄，
                            int axis);				//轴号			
        //设置指定轴的有效电平的脉冲宽度
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetPulWidth(IntPtr handle,      //设备句柄，
                            UInt32 ns,              //脉冲宽度，单位为纳秒
                            int axis);				//需要设置脉冲宽度的轴号
        //设置指定轴的脉冲和方向的有效电平
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetPulPolar(IntPtr handle,      //设备句柄，
                            int pul,                //脉冲信号的有效电平。非零：高电平有效； 零：低电平有效。
                            int dir,                //方向信号的有效电平。非零：高电平有效； 零：低电平有效。
                            int axis);				//需要设置有效电平的轴号。
        //使能/禁止控制卡接收编码器反馈
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetEncpEna(IntPtr handle,       //设备句柄，
                            int ena,                //使能标志。非零：使能； 零：不使能。
                            int axis);				//需要能/禁止控制卡接收编码器反馈的轴号。
        //设置控制卡接收编码器反馈的计数模式和计数方向
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetEncpMode(IntPtr handle,      //设备句柄，
                            int mode,               //编码器的计数模式。零：正交信号模式； 非零：脉冲+方向模式
                            int dir,                //编码器的计数方向。
                            int axis);				//需要设置的轴号。
        //设置指定轴的速度和加速度限制
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetVelAccLim(IntPtr handle,     //设备句柄，
                            double vellim,          //速度极限，单位为脉冲每毫秒。
                            double acclim, 			//加速度极限，单位为脉冲每平方毫秒。
                            int axis);				//需要设置速度和加速度极限的轴号
        //设置每个轴的平滑度
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetSmooth(IntPtr handle, 	    //设备句柄，
                                short smooth, 		//平滑度，值越大则越平滑，但运动轨迹的误差就越大；
                                int aixs); 			//轴号；	
        //使能/禁止指定轴的驱动器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetEna(IntPtr handle,           //设备句柄，
                            int ena,                //使能标志。非零：使能； 零：不使能。
                            int axis);				//需要使能/禁止驱动器的轴号。
        //使能/禁止硬件限位输入端口和设置其有效极性。
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_Setlimit(IntPtr handle, 	    //设备句柄，
                            int plimEna, 			//是否使能硬件正限位功能。非零：使能； 零：不使能。
                            int plimPolar, 			//正限位极性；非零：有效； 零：低电平有效。
                            int nlimEna, 			//是否使能硬件负限位功能。非零：使能； 零：不使能。
                            int nlimPolar,          //负限位极性；非零：有效； 零：低电平有效。
                            int axis);				//轴号。
        //使能伺服报警输入和设置其有效极性
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetAlm(IntPtr handle,           //设备句柄，
                            int ena,                //是否使能伺服报警输入功能。非零：使能； 零：不使能。
                            int polar,              //极性；非零：高电平有效； 零：低电平有效。
                            int axis);				//轴号。
        //使能伺服到位输入和设置其有效极性
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetINP(IntPtr handle,           //设备句柄，
                            int ena,                //是否使能伺服到位输入功能。非零：使能； 零：不使能。
                            int polar,              //极性；非零：高电平有效； 零：低电平有效。
                            int axis);				//轴号。
        //设置急停输入端的有效极性
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetEmstopPolar(IntPtr handle,   //设备句柄，
                            int polar); 			//极性；非零：高电平有效； 零：低电平有效。
        //设置通用输入端的有效极性
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetInPolar(IntPtr handle,       //设备句柄，
                            int polar, 				//极性；非零：高电平有效； 零：低电平有效。
                            int inPort);			//输入端口，范围1 - 32。
        //设置发生错误时，电机是否停止运动
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetStopfilt(IntPtr handle,      //设备句柄，
                            int stop,               //出错时是否停止运行；非零：停止； 零：不停止。
                            int axis);				//轴号。
        //设置发生错误时，电机是否退出运动
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetExitfilt(IntPtr handle,      //设备句柄，
                            int exit,               //出错时是否退出运行；非零：退出； 零：不退出。
                            int axis);				//轴号。
        //设置静态补偿的范围
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetRecoupRange(IntPtr handle,   //设备句柄，
                            int range,              //误差补偿值；取值范围0 - 32767。
                            int axis);				//轴号。

        //设置通信看门狗。
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetComdog(IntPtr handle,        //设备句柄，
                            int ena,                //是否启用通信看门狗，零：禁用； 非零：启用
                            int time);				//喂狗时间，单位毫秒，取值范围是0 - 32767

        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetConfig(IntPtr handle,  		//设备句柄，
                            ref UInt32 steptime, 	//脉冲宽度，单位为纳秒
                            ref int pulpolar, 		//脉冲的有效电平；零：低电平有效； 非零：高电平有效
                            ref int dirpolar, 		//方向的有效电平；零：低电平有效； 非零：高电平有效
                            ref int encpena, 		//是否使用编码器反馈；零：禁用； 非零：使用
                            ref int encpmode, 		//编码器计数模式
                            ref int encpdir, 		//编码器计数方向
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
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_MoveAbs(IntPtr handle,          //设备句柄，
                            int pos, 				//目标位置，单位为脉冲；
                            double startvel, 		//起始速度，单位为脉冲每毫秒；
                            double endvel,          //终点速度，单位为脉冲每毫秒；
                            double tgvel,           //目标速度，单位为脉冲每毫秒；
                            int wait,               //是否等待运动完成后，函数才返回。非零：等待运动完成；零：不等待。
                            int axis); 				//指定轴号

        //使轴从当前位置移动到指定的距离
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_MoveDist(IntPtr handle,         //设备句柄，
                            int dist, 				//移动距离，单位为脉冲；
                            double startvel, 		//起始速度，单位为脉冲每毫秒；
                            double endvel,          //终点速度，单位为脉冲每毫秒；
                            double tgvel,           //目标速度，单位为脉冲每毫秒；
                            int wait,               //是否等待运动完成后，函数才返回。非零：等待运动完成；零：不等待。
                            int axis); 				//指定轴号；
        //立即改变当前正在执行的点到点运动的运动速度
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_P2Pvel(IntPtr handle,           //设备句柄，
                            double tgvel,           //目标速度，单位为脉冲每毫秒；
                            int axis);				//轴号；
        //设置当前点到点运动的加速度和减速度
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetAccel(IntPtr handle,         //设备句柄，
                            double accel,           //加速度，单位为脉冲每平方毫秒；
                            double decel,           //减速度；
                            int axis);				//轴号；
        //设置点到点运动模式
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_P2Pmode(IntPtr handle,          //设备句柄，
                            int mode,               //运动模式；零：普通模式； 非零：跟踪模式
                            int axis);				//轴号。
        //改变点到点运动的目标位置
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_P2PnewPos(IntPtr handle,        //设备句柄，
                            int tgpos,              //新的目标位置，单位为脉冲；
                            int axis);				//轴号。
        //减速停止点到点运动
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_P2Pstop(IntPtr handle,          //设备句柄，
                            int axis);				//轴号。
        //使轴立即按指定的速度一直运动，直到速度被改变为止
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_MoveVel(IntPtr handle,          //设备句柄，
                            double startvel,        //起始速度，单位为脉冲每平方毫秒； 
                            double tgvel,           //指定轴的运动速度，单位为脉冲每平方毫秒；
                            int axis);              //轴号。


        //插补函数
        //立即将参与插补运动的轴号映射到X、Y、Z、A、B、…、等对应的标识上
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_AxisMap(IntPtr handle,          //设备句柄，
                            int[] axis,             //需要映射的轴号的数组
                            int num,                //需要映射的轴的数量
                            int fifo);				//对哪个插补空间进行映射，可选SEL_PFIFO1和SEL_PFIFO2。
        //立即启用插补空间
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_PFIFOrun(IntPtr handle,         //设备句柄，
                            int fifo);				//启用哪个插补空间，可选SEL_PFIFO1或SEL_PFIFO2。
        //立即改变插补的加速度
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetPFIFOaccel(IntPtr handle,   //设备句柄，
                            double accel,           //插补的加速度，单位为脉冲每平方毫秒； 
                            int fifo);				//设置哪个插补空间的插补的加速度，可选SEL_PFIFO1或SEL_PFIFO2。
        //立即改变插补的加速度
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetPFIFOvelMode(IntPtr handle,   //设备句柄，
                            int mode, 			    //速度规划模式 
                            int fifo);				//设置哪个插补空间的速度规划模式，可选SEL_PFIFO1或SEL_PFIFO2。
        //单段直线插补（绝对运动）
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_Line_Pos(IntPtr handle,         //设备句柄，
                            int[] pos,              //参与插补运动的轴的位置，单位为脉冲；
                            int axisNum,            //参与插补运动的轴数；
                            double tgvel,           //插补运动的速度，单位为脉冲每平方毫秒；
                            double endvel,          //插补运动的末端速度，单位为脉冲每平方毫秒；
                            int wait,               //是否等待插补运动完成，函数才返回。非零：等待运动完成；零：不等待。
                            int fifo);				//指定将此运动指令发送到哪个FIFO中执行，可选SEL_PFIFO1或SEL_PFIFO2。
        //单段直线插补（相对运动）
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_Line_Dist(IntPtr handle,        //设备句柄，
                            int[] dist,             //参与插补运动的轴的移动距离，单位为脉冲；
                            int axisNum,            //参与插补运动的轴数；
                            double tgvel,           //插补运动的速度，单位为脉冲每平方毫秒；
                            double endvel,          //插补运动的末端速度，单位为脉冲每平方毫秒；
                            int wait,               //是否等待插补运动完成，函数才返回。非零：等待运动完成；零：不等待。
                            int fifo);				//指定将此运动指令发送到哪个FIFO中执行，可选SEL_PFIFO1或SEL_PFIFO2。
        //多段连续直线插补（绝对运动）
        [DllImport("IMC_Pkg4xxx.dll")]
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
        [DllImport("IMC_Pkg4xxx.dll")]
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
        [DllImport("IMC_Pkg4xxx.dll")]
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
        [DllImport("IMC_Pkg4xxx.dll")]
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
        [DllImport("IMC_Pkg4xxx.dll")]
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
        [DllImport("IMC_Pkg4xxx.dll")]
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
        //立即停止插补运动
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_PFIFOstop(IntPtr handle,        //设备句柄，
                            int fifo);              //停止哪个插补空间的插补，可选SEL_PFIFO1或SEL_PFIFO2。
                                                    //判断插补运动是否停止
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_isPstop(IntPtr handle,      //设备句柄，
                            int fifo);             //哪个插补空间的插补停止，可选SEL_PFIFO1或SEL_PFIFO2。
        //立即清空发到插补空间中未被执行的所有指令
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_PFIFOclear(IntPtr handle,       //设备句柄，
                            int fifo);				//清空哪个插补空间的指令，可选SEL_PFIFO1或SEL_PFIFO2。

        //齿轮函数
        //设置指定轴跟随电子手轮运动
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_HandWheel1(IntPtr Handle,       //设备句柄，
                            double rate, 			//电子手轮倍率；
                            int axis);				//跟随手轮运动的轴号；
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_HandWheel2(IntPtr Handle, 	    //设备句柄，
                            double rate, 			//电子手轮倍率；
                            int axis);				//跟随手轮运动的轴号；
        //退出由PKG_IMC_HandWheel2函数设置的电子手轮运动
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_ExitHandWheel2(IntPtr Handle,  //设备句柄，
                            int axis);				//跟随手轮运动的轴号；
        //设置从动轴跟随主动轴运动
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_Gear1(IntPtr Handle,            //设备句柄，
                            double rate,            //齿轮倍率；
                            int master,             //主动轴号
                            int axis);				//从动轴的轴号。
        //设置从动轴跟随主动轴运动
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_Gear2(IntPtr Handle,            //设备句柄，
                            double rate,            //齿轮倍率；
                            int master,             //主动轴号
                            int axis);				//从动轴的轴号。
        //立即脱离电子手轮或齿轮的啮合
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_ExitGear(IntPtr Handle,         //设备句柄，
                            int axis);				//从动轴的轴号。


        //IO设置函数
        //对输出端口进行控制
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetOut(IntPtr handle,           //设备句柄，
                            int outPort,            //输出端口；范围是1 – 48；
                            int data,               //控制输出端口的状态； 零：断开输出端口； 非零：连通输出端口。
                            int fifo);              //指定将此指令发送到哪个FIFO中执行。

        //搜零函数
        //设置当前搜零过程中使用的高速度和低速度
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetHomeVel(IntPtr handle,       //设备句柄，
                            double hight,           //搜零过程中使用的高速度，单位为脉冲每毫秒；
                            double low,             //搜零过程中使用的低速度，单位为脉冲每毫秒；
                            int axis);              //轴号；
                                                    //设置编码器索引信号的极性
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetHomeIndexPolar(IntPtr handle, //设备句柄，
                            int polar,              //索引信号的极性， 非零：上升沿有效， 0：下降沿有效
                            int axis);              //轴号；

        //使用零点开关搜索零点
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_HomeORG(IntPtr Handle,          //设备句柄，
                            int orgSW,                  //零点开关选择
                            int dir,                    //搜零方向。零：正方向搜零；非零：负方向搜零；
                            int stopmode,               //搜索到原点后的停止方式，零：立即停止在原点位置；非零：减速停止。
                            int riseEdge,               //指定原点位置的边沿；零：下降沿； 非零：上升沿
                            int edir,                   //从哪个移动方向来判断原点位置边沿；零：正方向移动；非零：负方向移动；
                            int reducer,				//减速开关选择
                            int pos,                    //设置零点时刻零点开关的位置值
                            double hightvel,            //搜零时使用的高速度
                            double lowvel,              //搜零时使用的低速度
                            int wait,                   //是否等待搜零结束后函数再返回
                            int axis);					//轴号
        //使用零点开关和索引信号搜索零点
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_HomeORGIndex(IntPtr Handle,     //设备句柄，
                            int orgSW,                  //零点开关选择
                            int dir,                    //搜零方向。零：正方向搜零；非零：负方向搜零；
                            int stopmode,               //搜索到原点后的停止方式，零：立即停止在原点位置；非零：减速停止。
                            int riseEdge,               //指定原点位置的边沿；零：下降沿； 非零：上升沿
                            int edir,                   //从哪个移动方向来判断原点位置边沿；零：正方向移动；非零：负方向移动；
                            int reducer,                //减速开关选择
                            int pos,                    //设置零点时刻索引信号所在的位置值
                            double hightvel,            //搜零时使用的高速度
                            double lowvel,              //搜零时使用的低速度
                            int wait,                   //是否等待搜零结束后函数再返回
                            int axis);					//轴号

        //
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetSReset(IntPtr Handle,        //设备句柄，
                            int ena,                //是否使能伺服复位输出，当使能后，急停和搜零时都会通过SRST端口输出一个脉冲信号
                            int steptime,           //输出的脉冲信号的宽度，单位为125微秒
                            int axis);				//轴号。

        //把该轴的当前位置设定为指定值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetPos(IntPtr Handle,           //设备句柄，
                            int pos,                //设置的指定值，单位为脉冲；
                            int axis);				//轴号。
        //立即停止搜零运动
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_HomeStop(IntPtr Handle,         //设备句柄，
                            int axis);              //需要停止搜零运动的轴号。

        //AD函数
        //设置AD采样功能。
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetADena(IntPtr handle,     //设备句柄；
                            int ena,                //使能还是禁止；
                            int ch);				//AD采样通道
        //获得指定通道的AD采样值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetADdata(IntPtr handle,    //设备句柄；
                            ref double ADdata,          //用于获取AD值，单位：伏
                            int ch);				//AD采样通道
        //设置某个目标根据AD输入的电压变化在指定区间变化
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetADCtrl(IntPtr handle,    //设备句柄
                            double ADstart,         //AD输入的起始值
                            double ADend,           //AD输入的终止值
                            int ch,                 //AD采样通道
                            int paramloc,           //跟随AD输入值变化的寄存器的地址；
                            int axis,               //跟随AD输入值变化的寄存器的轴号；
                            int paramStart,         //跟随AD输入值变化的寄存器的起始值
                            int paramEnd,           //跟随AD输入值变化的寄存器的终止值；
                            int id);				//控制功能模块ID，范围是0到(轴数 - 1)。
        //设置AD输入的电压变化控制某个目标在指定区间变化
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetADCtrlEX(IntPtr handle,  //设备句柄
                            double ADstart,         //AD输入的起始值
                            double ADend,           //AD输入的终止值
                            int ch,                 //AD采样通道
                            double tgStart,         //目标变化的起始值
                            double tgEnd,           //目标变化的终止值；
                            int tgid,               //目标ID
                            int id);				//控制功能模块ID，范围是0到(轴数 - 1)。
        //禁用AD控制功能。
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_DisADCtrl(IntPtr handle,    //设备句柄
                            int id);				//控制模块ID，范围是0到(轴数 - 1)。

        //DA函数
        //设置DA输出功能。
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetDAena(IntPtr handle,     //设备句柄
                            int ena,                //使能还是禁止；
                            int ch);				//DA输出通道
        //设置DA的基础输出值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetDAout(IntPtr handle,     //设备句柄
                            double da,              //DA的输出电压值,范围是-10.0V ~ +10.0V
                            int ch);				//DA输出通道
        //设置DA输出跟随指定的寄存器的变化而变化
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetDAFollow(IntPtr handle,  //设备句柄 
                            double DAstart,         //DA输出值变化区间的起始值；
                            double DAend,           //DA输出值变化区间的终止值
                            int ch,                 //DA输出通道；
                            int paramloc,           //DA输出值跟随控制卡中的哪个寄存器的值来变化,当它为0时，禁止此跟随输出功能
                            int axis,               //跟随的寄存器的轴号
                            int tgStart,            //跟随的寄存器的变化区间的起始值
                            int tgEnd);				//跟随的寄存器的变化区间的终止值。
        //设置DA输出跟随指定的寄存器的变化而变化
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetDAFollowEX(IntPtr handle,//设备句柄 
                            double DAstart,         //DA输出值变化区间的起始值；
                            double DAend,           //DA输出值变化区间的终止值
                            int ch,                 //DA输出通道；
                            double tgStart,         //目标变化区间的起始值
                            double tgEnd,           //目标变化区间的终止值。
                            int tgid);				//目标ID

        //PWM函数
        //设置PWM输出功能
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetPWMena(IntPtr handle,    //设备句柄
                            int ena,                //使能还是禁止；
                            int polar,              //PWM输出脉冲的有效极性
                            int ch);                //PWM输出通道
                                                    //设置PWM的基础输出值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetPWMprop(IntPtr handle,   //设备句柄
                            double pwm,             //pwm的占空比,范围是0 ~ 1.0
                            int ch);				//PWM输出通道
        //设置PWM输出功能
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetPWMfreq(IntPtr handle,   //设备句柄
                            double freq,            //PWM输出的频率，单位为脉冲/秒
                            int ch);                //PWM输出通道
                                                    //设置PWM输出的占空比跟随指定寄存器的变化而变化
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_PWMpropFollow(IntPtr handle,//设备句柄
                            int polar,              //PWM输出脉冲的有效电平。0：低电平有效； 非零：高电平有效
                            double freq,            //PWM输出的频率，单位为脉冲/秒
                            double PWMstart,        //PWM占空比跟随指定寄存器输出的变化区间的起始值
                            double PWMend,          //PWM占空比跟随指定寄存器输出的变化区间的终止值
                            double offset,          //PWM输出占空比的偏移值
                            int ch,                 //PWM输出通道
                            int paramloc,           //PWM占空比跟随变化的控制卡中的寄存器地址,当它为0时，禁止此跟随输出功能
                            int axis,               //跟随的寄存器的轴号
                            int paramStart,         //跟随的寄存器变化区间的起始值
                            int paramEnd);          //跟随的寄存器变化区间的终止值
                                                    //设置PWM输出的占空比跟随指定寄存器的变化而变化
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_PWMpropFollowEX(IntPtr handle,  //设备句柄
                            int polar,              //PWM输出脉冲的有效电平。0：低电平有效； 非零：高电平有效
                            double freq,            //PWM输出的频率，单位为脉冲/秒
                            double PWMstart,        //PWM占空比跟随指定目标输出的变化区间的起始值
                            double PWMend,          //PWM占空比跟随指定目标输出的变化区间的终止值
                            double offset,          //PWM输出占空比的偏移值
                            int ch,                 //PWM输出通道
                            double tgStart,         //跟随的目标变化区间的起始值
                            double tgEnd,           //跟随的目标变化区间的终止值
                            int tgid);              //目标ID
                                                    //设置PWM输出的频率跟随指定寄存器的变化而变化
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_PWMfreqFollow(IntPtr handle,//设备句柄
                            int polar,              //PWM输出脉冲的有效电平。0：低电平有效； 非零：高电平有效
                            double prop,            //PWM输出的占空比,范围是0 ~ 1.0
                            double freqStart,       //PWM输出频率跟随指定寄存器输出的变化区间的起始值，单位为脉冲/秒
                            double freqEnd,         //PWM输出频率跟随指定寄存器输出的变化区间的终止值，单位为脉冲/秒
                            double offset,          //PWM输出频率的偏移值
                            int ch,                 //PWM输出通道
                            int paramloc,           //PWM输出频率跟随变化的控制卡中的寄存器地址,当它为0时，禁止此跟随输出功能
                            int axis,               //跟随的寄存器的轴号
                            int paramStart,         //跟随的寄存器变化区间的起始值
                            int paramEnd);          //跟随的寄存器变化区间的终止值 
                                                    //设置PWM输出的频率跟随指定目标的变化而变化
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_PWMfreqFollowEX(IntPtr handle,  //设备句柄
                            int polar,              //PWM输出脉冲的有效电平。0：低电平有效； 非零：高电平有效
                            double prop,            //PWM输出的占空比,范围是0 ~ 1.0
                            double freqStart,       //PWM输出频率的变化区间的起始值，单位为脉冲/秒
                            double freqEnd,         //PWM输出频率的变化区间的终止值，单位为脉冲/秒
                            double offset,          //PWM输出频率的偏移值
                            int ch,                 //PWM输出通道
                            double tgStart,         //跟随的目标变化区间的起始值
                            double tgEnd,           //跟随的目标变化区间的终止值 
                            int tgid);              //目标ID
                                                    //取消PWM输出跟随功能。
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_DisPWMFollow(IntPtr handle,  //设备句柄
                            int ch);                //PWM输出通道
                                                    //使能补偿功能。
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_EnaCompen(IntPtr handle,    //设备句柄
                            int ena,                //非零：启用补偿功能； 零：禁用补偿功能。
                            int axis);				//轴号
        //设置补偿功能
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetCompenInfo(IntPtr handle,//设备句柄
                            double startvel,        //补偿的起始速度，单位为脉冲/毫秒。
                            double endvel,          //补偿终止时的速度，单位为脉冲/毫秒。
                            double tgvel,           //补偿的速度，单位为脉冲/毫秒。
                            double acc,             //补偿的加速度，单位为脉冲/平方毫秒。
                            double dec,             //补偿的减速度，单位为脉冲/平方毫秒。
                            int dist,               //补偿的间隙大小，单位为脉冲。
                            int axis);				//轴号

        //使能位置比较输出
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_EnaCMPOut(IntPtr handle,    //设备句柄
                            int ena,                //非零：启用比较输出功能； 零：禁用比较输出功能。
                            int mod,                //比较模式，零：比较距离。 非零：比较位置
                            int time,               //输出脉冲的宽度，单位为125uS，默认值为8
                            int axis);				//轴号
        //设置比较输出的位置或位移
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetCMPInfo(IntPtr handle,   //设备句柄
                            int dist,               //位置或位移，单位为脉冲。
                            int axis);				//轴号
        //设置位置捕获功能
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_EnaCapture(IntPtr handle,   //设备句柄
                            int ena,                //使能还是禁止探针捕获功能；零：禁止；非零：使能
                            int only,               //零：探针信号的每次输入都捕获；非零：只捕获一次探针输入
                            int axis);				//轴号
        //读取探针捕获的位置数据
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetCapData(IntPtr handle,   //设备句柄
                            int rdnum,              //将要读取的数据个数
                            ref int pdata, 			//用于保存读取的数据的缓存区
                            ref int dataNum, 			//实际读取到的数据个数
                            ref int lastNum,            //控制卡中剩余的数据个数
                            int axis);				//轴号

        //设置龙门驱动
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetGantry(IntPtr handle,    //设备句柄
                            double gantrykp,        //主从轴位置偏差修正系数，如果此参数为0，则禁止偏差修正。
                            ushort limit,   //主从轴位置偏差最大值。若主从轴位置偏差超过此值，则会出现误差超限错误。
                            int maxis,              //主动轴的轴号				
                            int axis);				//从动轴的轴号
        //取消龙门驱动
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_DisGantry(IntPtr handle,    //设备句柄
                            int axis);				//从动轴的轴号
        //设置源参数跟随目标参数的值做相应的变化
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_PropFollow(IntPtr handle,   //设备句柄，
                            int srcParam,           //源参数地址
                            int srcAxis,            //源参数轴号
                            int srcStart,           //源参数变化区间的起始值
                            int srcEnd,             //源参数变化区间的终止值
                            int srcOffset,          //源参数变化的偏移值
                            int tgParam,            //目标参数地址
                            int tgAxis,             //目标参数轴号
                            int tgStart,            //目标参数变化区间的起始值
                            int tgEnd,              //目标参数变化区间的终止值
                            int id);				//比例跟随模块ID
        //取消比例跟随功能
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_DisPropFollow(IntPtr handle,    //设备句柄，
                            int id);				//比例跟随模块ID


        //获取状态函数
        //获取轴数
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
         PKG_IMC_GetNaxis(IntPtr Handle);           //设备句柄，

        //获取控制卡型号
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern uint
           PKG_IMC_GetType(IntPtr Handle);  //设备句柄，
                                            //获取控制卡开关量输出端口的个数
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
           PKG_IMC_GetOutputNum(uint type);//控制卡型号
                                           //获取控制卡开关量输入端口的个数
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
           PKG_IMC_GetInputNum(uint type);  //控制卡型号
                                            //获取控制卡模拟量输入通道的个数
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
           PKG_IMC_GetADchNum(uint type);   //控制卡型号
                                            //获取控制卡模拟量输出通道的个数
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
           PKG_IMC_GetDAchNum(uint type);   //控制卡型号
                                            //获取控制卡PWM输出通道的个数
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
           PKG_IMC_GetPWMchNum(uint type);

        //获得所有轴的机械位置。
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetEncp(IntPtr Handle,      //设备句柄，
                           int[] pos,               //用于保存机械位置的数组；单位为脉冲；
                           int axisnum);            //控制卡的轴数。
                                                    //获得所有轴的指令位置
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetCurpos(IntPtr Handle,    //设备句柄，
                           int[] pos,               //用于保存指令位置的数组；单位为脉冲；
                           int axisnum);            //控制卡的轴数。
                                                    //读取逻辑位置
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetLogicpos(IntPtr Handle,  //设备句柄，
                           int[] pos,           //用于保存逻辑位置的数组；单位为脉冲；
                           int axisnum);            //控制卡的轴数。
                                                    //读取逻辑速度
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetLogicVel(IntPtr Handle,  //设备句柄，
                           double[] vel,            //用于保存逻辑速度的数组；单位为脉冲/毫秒；
                           int axisnum);            //控制卡的轴数。
                                                    //获得所有轴的错误状态
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetErrorReg(IntPtr Handle,     //设备句柄，
                           UInt16[] err,            //用于保存所有轴的错误状态；有错误则相应的位会置1。
                           int axisnum);            //控制卡的轴数。
                                                    //获得所有轴的运动状态
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetMoving(IntPtr Handle,        //设备句柄，
                           UInt16[] moving,        //用于保存所有轴的运动状态，零：已停止运动； 非零：正在运动中
                           int axisnum);            //控制卡的轴数。
                                                    //获得所有轴的功能输入端口状态
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetAin(IntPtr Handle,       //设备句柄，
                           UInt16[,] ain,           //用于保存所有轴功能输入端的状态
                           int axisnum);            //控制卡的轴数。
                                                    //获得所有通用输入端口的状态
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetGin(IntPtr Handle,           //设备句柄，
                           UInt16[] gin);          //用于保存所有通用输入端的状态。
                                                   //获得所有输出端口的状态
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetGout(IntPtr Handle,      //设备句柄，
                           UInt16[] gout);         //用于保存所有输出端的状态

        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
             PKG_IMC_ClearError(IntPtr Handle,      //设备句柄，
                            int axis);              //需要清除错误的轴号
                                                    //其他功能函数
                                                    //所有轴立即急停或解除急停状态
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
           PKG_IMC_Emstop(IntPtr Handle,            //设备句柄，
                           int isStop);			//急停还是解除急停；非零：急停； 零：解除急停。
        //对所有轴立即暂停或解除暂停状态
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_Pause(IntPtr Handle,            //设备句柄，
                            int pause);				//暂停还是解除暂停状态；非零：暂停； 零：解除暂停。

        //立即改变进给倍率。当进给倍率设为0时，可实现暂停，再次设为非零则解除暂停
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetFeedrate(IntPtr handle,     //设备句柄，
                            double rate); 			//进给倍率； 
        //退出等待运动完成
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern void
            PKG_IMC_ExitWait();

        //当函数返回错误是，使用此函数获得错误提示
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern IntPtr
            PKG_IMC_GetFunErrStrW();
        //当函数返回错误是，使用此函数获得错误提示
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern IntPtr
            PKG_IMC_GetFunErrStrA();

        //获得错误寄存器是字符串说明
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern IntPtr
            PKG_IMC_GetRegErrorStrA(UInt16 err);

        //获得错误寄存器是字符串说明
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern IntPtr
            PKG_IMC_GetRegErrorStrW(UInt16 err);

        //ADD事件
        //将某个寄存器的值与另一个寄存器的值进行相加，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_ADD32(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis, 		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定数值进行相加，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_ADD32i(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            int data, 							//32位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与另一个寄存器的值进行相加，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_ADD48(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中 
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定数值进行相加，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_ADD48i(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            Int64 data, 						//64位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号

        //CMP事件
        //将某个寄存器的值与另一个寄存器的值进行相减，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_CMP32(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定数值进行相减，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_CMP32i(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            int data, 							//32位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与另一个寄存器的值进行相减，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_CMP48(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定数值进行相减，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_CMP48i(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            Int64 data, 						//64位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号

        //SCA事件
        //将某个寄存器的值乘以另一个寄存器指定的倍率，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_SCA32(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值乘以指定的倍率，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_SCA32i(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            double data, 							//32位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值乘以另一个寄存器指定的倍率，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_SCA48(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值乘以乘以指定的倍率，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_SCA48i(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            double data, 						//64位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //MUL事件
        //
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_MUL32L(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_MUL32iL(ref EventInfo info,//事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            int data, 							//32位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_MUL32A(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_MUL32iA(ref EventInfo info,//事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            int data, 							//32位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号

        //COP事件
        //将某个16位寄存器的值赋值给目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_COP16(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个32位寄存器的值赋值给目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_COP32(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个48位寄存器的值赋值给目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_COP48(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号

        //SET事件
        //将指定的数值赋值给16位目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_SET16(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short data, 						//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将指定的数值赋值给32位目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_SET32(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            int data, 							//32位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将指定的数值赋值给48位目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_SET48(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            Int64 data, 						//64位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号


        //OR事件
        //将某个寄存器的值与另一个寄存器的值进行或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_OR16(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与另一个寄存器的值进行或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_OR16B(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定的数值进行或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_OR16i(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,        //寄存器1的地址和其对应的轴号
                            short data, 						//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定的数值进行或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_OR16iB(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,        //寄存器1的地址和其对应的轴号
                            short data, 						//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号

        //AND事件
        //将某个寄存器的值与另一个寄存器的值进行与运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_AND16(ref EventInfo info,  //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与另一个寄存器的值进行与运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_AND16B(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定的数值进行与运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_AND16i(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,        //寄存器1的地址和其对应的轴号
                            short data, 						//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定的数值进行与运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_AND16iB(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,        //寄存器1的地址和其对应的轴号
                            short data, 						//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号


        //XOR事件
        //将某个寄存器的值与另一个寄存器的值进行异或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_XOR16(ref EventInfo info, //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与另一个寄存器的值进行异或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_XOR16B(ref EventInfo info,    //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis, 		//寄存器1的地址和其对应的轴号
                            short Src2, short Src2_axis,  		//寄存器2的地址和其对应的轴号
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定的数值进行异或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_XOR16i(ref EventInfo info,    //事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,        //寄存器1的地址和其对应的轴号
                            short data, 						//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号
        //将某个寄存器的值与指定的数值进行异或运算，将结果保存到目标寄存器
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int PKG_Fill_XOR16iB(ref EventInfo info,//事件结构体指针，事件指令将填充事件到此指针指向的地址中
                            short EventType,                    //事件类型，由枚举IMC_EventType中的成员赋值
                            short Src1, short Src1_axis,        //寄存器1的地址和其对应的轴号
                            short data,							//16位整数
                            short dest, short dest_axis);		//目标寄存器的地址和其对应的轴号


        //底层函数封装
        //设置多个寄存器为指定值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetMulParam(IntPtr Handle,      //设备句柄，
                                WR_MUL_DES[] pdes,  //WR_MUL_DES结构体数组；
                                int ArrNum,         //pdes数组中有效数据的个数；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //设置16位寄存器为指定值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetParam16(IntPtr Handle,       //设备句柄，
                                short paramloc,     //寄存器地址；
                                short data,         //16位整型数据
                                int axis,           //轴号；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //设置32位寄存器为指定值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetParam32(IntPtr Handle,       //设备句柄，
                                short paramloc,     //寄存器地址；
                                int data,           //32位整型数据
                                int axis,           //轴号；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //设置48位寄存器为指定值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetParam48(IntPtr Handle,       //设备句柄，
                                short paramloc,     //寄存器地址；
                                Int64 data,         //64位整型数据
                                int axis,           //轴号；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //将寄存器某个位的值设为指定值（1或者0）
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetParamBit(IntPtr Handle,      //设备句柄，
                                short paramloc,     //寄存器地址；
                                short bit,          //寄存器的某个位，范围 0 – 15；
                                short val,          //指定的值, 1或者0；
                                int axis,           //轴号；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //将寄存器指定的位的值由1变为0或者由0变为1
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_TurnParamBit(IntPtr Handle,     //设备句柄，
                            short paramloc,         //寄存器地址；
                            short bit,              //寄存器的某个位，范围 0 – 15；
                            int axis,               //轴号；
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //置位或清零寄存器的某些位
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_ORXORParam(IntPtr Handle,       //设备句柄，
                            short paramloc,         //寄存器地址；
                            short ORdata,           //与寄存器进行相或的值；
                            short XORdata,          //与寄存器进行相异或的值；
                            int axis,               //轴号；
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //阻塞FIFO执行后续指令，直到寄存器的某个位变为指定值或超时为止
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_WaitParamBit(IntPtr Handle,     //设备句柄，
                            short paramloc,         //寄存器地址；
                            short bit,              //寄存器的某个位，范围 0 – 15；
                            short val,              //指定值，1或0；
                            int timeout,            //超时时间，单位为毫秒；
                            int axis,               //轴号；
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //阻塞FIFO执行后续指令，直到超过设定的时间为止
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_WaitTime(IntPtr Handle,         //设备句柄，
                            int time,               //等待时间，单位为毫秒
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //阻塞FIFO执行后续指令，直到寄存器的值变为指定值或超时为止
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_WaitParam(IntPtr Handle,        //设备句柄，
                            short paramloc,         //寄存器地址；
                            short data,             //指定的值；
                            int timeout,            //超时时间，单位为毫秒；
                            int axis,               //轴号；
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //阻塞FIFO执行后续指令，直到寄存器的值与mask进行相与后的值与data值相等或超时为止
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_WaitParamMask(IntPtr Handle,    //设备句柄，
                            short paramloc,         //寄存器地址；
                            short mask,             //与寄存器进行相与的值
                            short data,             //用于比较的值；
                            int timeout,            //超时时间，单位为毫秒；
                            int axis,               //轴号；
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //读取多个寄存器的值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetMulParam(IntPtr Handle,      //设备句柄，
                            WR_MUL_DES[] pdes,      //WR_MUL_DES结构体数组；
                            int ArrNum);			//WR_MUL_DES结构体数组的有效成员个数。
        //读取16位寄存器的值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetParam16(IntPtr Handle,       //设备句柄，
                            short paramloc, 		//寄存器地址；
                            ref short data,         //16位整型变量的地址，用于保存16位寄存器的值；
                            int axis);				//轴号；
        //读取32位寄存器的值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetParam32(IntPtr Handle,       //设备句柄，
                            short paramloc, 		//寄存器地址；
                            ref int data,           //32位整型变量的地址，用于保存32位寄存器的值；
                            int axis);				//轴号；
        //读取48位寄存器的值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetParam48(IntPtr Handle,       //设备句柄，
                            short paramloc,         //寄存器地址；
                            ref Int64 data,         //64位整型变量的地址，用于保存48位寄存器的值；
                            int axis);				//轴号；


        //以下为模拟量输入专用函数
        //设置多个寄存器为指定值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetADMulParam(IntPtr Handle,	    //设备句柄，
                                WR_MUL_DES[] pdes,	//WR_MUL_DES结构体数组；
                                int ArrNum,			//pdes数组中有效数据的个数；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //设置16位寄存器为指定值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetADParam16(IntPtr Handle, 	    //设备句柄，
                                short paramloc,		//寄存器地址；
                                short data,			//16位整型数据
                                int axis,			//轴号；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //设置32位寄存器为指定值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetADParam32(IntPtr Handle, 	    //设备句柄，
                                short paramloc, 	//寄存器地址；
                                int data, 			//32位整型数据
                                int axis,			//轴号；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //将寄存器某个位的值设为指定值（1或者0）
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_SetADParamBit(IntPtr Handle, 	    //设备句柄，
                                short paramloc,    	//寄存器地址；
                                short bit,			//寄存器的某个位，范围 0 – 15；
                                short val, 			//指定的值, 1或者0；
                                int axis,			//轴号；
                                int fifo);			//指定将此指令发送到哪个FIFO中
        //将寄存器指定的位的值由1变为0或者由0变为1
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_TurnADParamBit(IntPtr Handle,     //设备句柄，
                            short paramloc, 		//寄存器地址；
                            short bit, 				//寄存器的某个位，范围 0 – 15；
                            int axis,				//轴号；
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //置位或清零寄存器的某些位
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_ORXORADParam(IntPtr Handle, 	    //设备句柄，
                            short paramloc, 		//寄存器地址；
                            short ORdata, 			//与寄存器进行相或的值；
                            short XORdata, 			//与寄存器进行相异或的值；
                            int axis,				//轴号；
                            int fifo);				//指定将此指令发送到哪个FIFO中
        //读取多个寄存器的值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetADMulParam(IntPtr Handle, 	    //设备句柄，
                            WR_MUL_DES[] pdes, 		//WR_MUL_DES结构体数组；
                            int ArrNum);			//WR_MUL_DES结构体数组的有效成员个数。
        //读取16位寄存器的值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetADParam16(IntPtr Handle, 		//设备句柄，
                            short paramloc, 		//寄存器地址；
                            ref short data, 		//16位整型变量的地址，用于保存16位寄存器的值；
                            int axis);				//轴号；
        //读取32位寄存器的值
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_GetADParam32(IntPtr Handle, 		//设备句柄，
                            short paramloc, 		//寄存器地址；
                            ref int data, 			//32位整型变量的地址，用于保存32位寄存器的值；
                            int axis);				//轴号；
        //将设置的事件安装到控制卡中
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_InstallEvent(IntPtr Handle,     //设备句柄，
                            EventInfo[] pEvent,     //事件结构体数组，由事件填充函数填充；
                            UInt16 EventNum         //事件指令的数量；
                            );
        //停止安装的事件运行
        [DllImport("IMC_Pkg4xxx.dll")]
        public static extern int
            PKG_IMC_StopEvent(IntPtr Handle); //设备句柄，
    }
}
