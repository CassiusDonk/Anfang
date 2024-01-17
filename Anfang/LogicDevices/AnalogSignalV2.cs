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
        public AnalogSignalV2(List<string> InputLinks)
        {
            InputsAreBool = false;
            InputCountFixed = true;
            InputCount = 1;
            OutputsAreBool = false;
            this.InputLinks = InputLinks;
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
