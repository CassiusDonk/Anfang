using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anfang.LogicDevices
{
    public class ComparatorV2 : BaseLogicV2
    {
        public float tripLevel = new float();
        public ComparatorV2()
        {
            InputsAreBool = false;
            InputCountFixed = true;
            InputCount = 1;
            OutputsAreBool = true;
        }
        public override void ProcessInputs()
        {
            if (InputsComplex32.Count() > 0)
            {
                if (InputsComplex32[0].Magnitude >= tripLevel)
                {
                    OutputBool = true;
                }
                else
                {
                    OutputBool = false;
                }
            }
        }
    }
}
