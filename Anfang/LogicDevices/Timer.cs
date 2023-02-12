using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Anfang.LogicDevices
{
    class Timer : BaseLogic
    {
        public Timer(string label)
        {
            this.label = label;
        }

        public override void NotifyInputBoolChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change. This uses a single bool input.
            if (input_bool == true)
            {
                init = true;
            }
            if (input_bool == false)
            {
                output = false;
                init = false;
            }
        }

        public override void NotifySimTimeChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on sim time change.
            if (sim_time == 0)
            {
                output = false;
            }
            if (init == true)
            {
                if (internal_time >= delay)
                {
                    output = input_bool;
                    internal_time = 0;
                    init = false;
                }
                internal_time += sim_time_step;
            }
        }
    }
}
