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
            inputType = "Boolean";
            outputType = "Boolean";
        }

        public override void UpdateOutput()
        {
            if (sim_time == 0)
            {
                output = false;
            }
            if (input_bool_list[0] == true)
            {
                if (internal_time >= delay)
                {
                    output = true;
                }
                else
                {
                    internal_time += sim_time_step;
                }
            }
            if (input_bool_list[0] == false)
            {
                output = false;
                internal_time = 0;
            }
            input_bool_list.Clear();
        }
    }
}
