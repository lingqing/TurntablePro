using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CoalMonitor.classlib
{
    public class WaitForTime
    {
        public delegate void DoWithNoPara();
        DateTime NeedTime = DateTime.Now;
        DoWithNoPara mDoWithNoPara = null;
        Thread tmp = null;


        public WaitForTime(float detalttime, DoWithNoPara OKdo)
        {

            this.NeedTime = DateTime.Now.AddSeconds(detalttime);
            this.mDoWithNoPara = OKdo;
            tmp = new Thread(CheckTimes);
            tmp.Start();
        }
        void CheckTimes()
        {

            while (true)
            {
                if (DateTime.Now > NeedTime)
                {
                    break;
                }
            }
            if (mDoWithNoPara != null)
            {
                mDoWithNoPara();
            }
            tmp.Abort();
            return;
        }

    }
}
