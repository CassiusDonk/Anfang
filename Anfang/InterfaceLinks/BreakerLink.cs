using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anfang
{
    public class BreakerLink
    {
        public string discreteOutput { get; set; }
        public int brerakerID { get; set; }

        public string ConvertToString()
        {
            string result = discreteOutput.ToString() + ";" + brerakerID.ToString();
            return result;
        }

    }
}
