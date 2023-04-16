using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anfang
{
    public class AnalogInputLink
    {
        public int id { get; set; }

        public int side { get; set; }

        public bool isVoltage { get; set; }

        public string phase { get; set; }

        public AnalogInputLink()
        {

        }

    }
}
