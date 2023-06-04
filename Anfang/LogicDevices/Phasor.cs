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
    public class Phasor : BaseLogic
    {
        public Phasor(string label)
        {
            this.label = label;
        }

        public override void UpdateOutput()
        {
            if (input_complex_list.Count > 1)
            {
                float phase_diff = MathF.Abs(RadsToDegs(input_complex_list[0].Phase) - RadsToDegs(input_complex_list[1].Phase));
                if (phase_diff > 180)
                {
                    phase_diff = 360 - phase_diff;
                }
                if (phase_diff > 90)
                {
                    // Power flows towards the measure point
                    output = false;
                }
                if (phase_diff < 90)
                {
                    // Power flows away from the measure point
                    output = true;
                }
            }
            float RadsToDegs(float rads)
            {
                float degs = rads * 180 / MathF.PI;
                return degs;
            }

            input_complex_list.Clear();
        }
    }
}
