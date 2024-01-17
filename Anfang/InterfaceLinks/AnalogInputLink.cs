using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anfang
{
    public class AnalogInputLink
    {
        public override string ToString()
        {
            string result = $"{id},{side},{isVoltage},{phase},{label}";
            return result;
        }
        public int id { get; set; }

        public int side { get; set; }

        public bool isVoltage { get; set; }

        public string phase { get; set; }

        public string label { get; set; }

        public AnalogInputLink()
        {

        }

        public string ConvertToString()
        {
            string result = id.ToString() + ";" + side.ToString() + ";" + isVoltage.ToString() + ";" + phase + ";" + label;
            return result;
        }
    }
}
