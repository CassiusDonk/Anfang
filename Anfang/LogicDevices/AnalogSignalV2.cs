using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anfang.LogicDevices
{
    public class AnalogSignalV2 : BaseLogicV2
    {
        public AnalogSignalV2()
        {
            InputsAreBool = false;
            InputCountFixed = true;
            InputCount = 1;
            OutputsAreBool = false;
        }
        public override void ProcessInputs()
        {
            if (InputsComplex32.Count() > 0)
            {
                OutputComplex32 = InputsComplex32[0];
            }
        }
    }
}
