using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anfang.LogicDevices
{
    public class TimerV2 : BaseLogicV2
    {
        public int TimerRiseDelay = new int();
        public TimerV2()
        {
            InputsAreBool = true;
            InputCountFixed = true;
            InputCount = 1;
            OutputsAreBool = true;
        }
        public TimerV2(List<string> InputLinks, int TimerRiseDelay)
        {
            InputsAreBool = true;
            InputCountFixed = true;
            InputCount = 1;
            OutputsAreBool = true;
            this.InputLinks = InputLinks;
            this.TimerRiseDelay = TimerRiseDelay;
        }

        public override void ProcessInputs()
        {
            if (InputsBool.Count() > 0)
            {
                if (SimTime == 0)
                {
                    OutputBool = false;
                }
                if (InputsBool[0] == true)
                {
                    if (InternalTime >= TimerRiseDelay)
                    {
                        OutputBool = true;
                    }
                    else
                    {
                        InternalTime += SimTimeStep;
                    }
                }
                if (InputsBool[0] == false)
                {
                    OutputBool = false;
                    InternalTime = 0;
                }
            }
        }
        public override List<string> ConvertToString()
        {
            List<string> result = new List<string>();
            result.Add("LogicDeviceStart");
            result.Add(Label);
            result.Add(this.GetType().ToString());
            result.Add("InputLinksStart");
            foreach (var inputLink in InputLinks)
            {
                result.Add(inputLink);
            }
            result.Add("InputLinksEnd");
            result.Add(TimerRiseDelay.ToString());
            result.Add("LogicDeviceEnd");
            return result;
        }

    }
}
