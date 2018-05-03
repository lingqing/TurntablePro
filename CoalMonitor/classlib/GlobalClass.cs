using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoalMonitor.classlib
{
    public enum CardType
    {
        iMC3xx2E,
        iMC4xxE_A
    }
    public class GlobalClass
    {
        public static CardType NowCarType = CardType.iMC3xx2E;
        public static string TestString = "";
    }
}
