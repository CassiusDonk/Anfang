using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anfang
{
    public class DiscreteInputLink
    {
        public int id { get; set; }

        public string phase { get; set; }

        public string label { get; set; }

        public DiscreteInputLink()
        {

        }

        public string ConvertToString()
        {
            string result = id.ToString() + ";" + phase + ";" + label;
            return result;
        }
    }
}
