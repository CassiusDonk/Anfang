using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anfang.LogicDevices
{
    public class ANDV2 : BaseLogicV2
    {
        public ANDV2()
        {
            InputsAreBool = true;
            InputCountFixed = false;
            OutputsAreBool = true;
        }
        public ANDV2(List<string> InputLinks)
        {
            InputsAreBool = true;
            InputCountFixed = false;
            OutputsAreBool = true;
            this.InputLinks = InputLinks;
        }
        public override void ProcessInputs()
        {
            if (InputsBool.Contains(false))
            {
                OutputBool = false;
            }
            else
            {
                OutputBool = true;
            }
        }
    }
}
