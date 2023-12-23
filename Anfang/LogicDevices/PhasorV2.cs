using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anfang.LogicDevices
{
    public class PhasorV2 : BaseLogicV2
    {
        public PhasorV2()
        {
            InputsAreBool = true;
            InputCountFixed = true;
            InputCount = 2;
            OutputsAreBool = true;
        }

        public override void ProcessInputs()
        {
            if (InputsComplex32.Count() > 0)
            {
                float phase_diff = MathF.Abs(RadsToDegs(InputsComplex32[0].Phase) - RadsToDegs(InputsComplex32[0].Phase));
                if (phase_diff > 180)
                {
                    phase_diff = 360 - phase_diff;
                }
                if (phase_diff > 90)
                {
                    // Power flows towards the measure point
                    OutputBool = false;
                }
                if (phase_diff < 90)
                {
                    // Power flows away from the measure point
                    OutputBool = true;
                }
                float RadsToDegs(float rads)
                {
                    float degs = rads * 180 / MathF.PI;
                    return degs;
                }
            }
        }
    }
}
