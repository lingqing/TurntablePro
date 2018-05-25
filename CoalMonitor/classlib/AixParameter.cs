using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoalMonitor.classlib
{
    public class AixParameter
    {
        public String TitleName { set; get; }
        public String Position { set; get; }
        public bool limitUp { set; get; }
        public bool limitDown { set; get; }
        //public bool Limt { set; get; }

        public AixParameter(String name, String position = "N/A", bool up = false,bool down = false)
        {
            TitleName = name;
            Position = position;
            limitUp = up;
            limitDown = down;
        }
    }
}
