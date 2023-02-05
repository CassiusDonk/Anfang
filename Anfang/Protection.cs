using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Anfang
{
    public class Protection
    {
        List<Complex32> inputSignals = new List<Complex32>();
        List<Complex32> tripLevels = new List<Complex32>();
        List<int> timer_delays = new List<int>();
        bool init = false;
        bool trip = false;

        public Protection()
        {

        }


    }
}
