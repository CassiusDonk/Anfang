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
        List<String> logic_config = new List<String>();
        List<Complex32> analogInputs = new List<Complex32>();
        List<bool> discreteInputs = new List<bool>();
        List<Complex32> tripLevels = new List<Complex32>();
        List<int> timer_delays = new List<int>();
        int sim_time = 0;
        bool init = false;
        bool trip = false;

        List<Object> comparators = new List<Object>();
        List<LogicDevices.Timer> timers = new List<LogicDevices.Timer>();
        List<LogicDevices.AnalogSignal> analogs = new List<Anfang.LogicDevices.AnalogSignal>();
        List<LogicDevices.DiscrSignal> discretes = new List<Anfang.LogicDevices.DiscrSignal>();
        List<LogicDevices.OR> ORs = new List<Anfang.LogicDevices.OR>();
        List<LogicDevices.AND> ANDs = new List<Anfang.LogicDevices.AND>();
        List<LogicDevices.Invert> inverters = new List<Anfang.LogicDevices.Invert>();

        public Protection()
        {

        }

        void Initiate_logic()
        {
            // populate logic devices lists
            foreach (var item in logic_config)
            {
                if (item.Contains("Comp") == true | item.Contains("comp") == true)
                {
                    comparators.Add(new LogicDevices.Comparator(item));
                }
                if (item.Contains("Timer") == true | item.Contains("timer") == true)
                {
                    timers.Add(new LogicDevices.Timer(item));
                }
                if (item.Contains("Analog") == true | item.Contains("analog") == true)
                {
                    analogs.Add(new LogicDevices.AnalogSignal(item));
                }
                if (item.Contains("Discrete") == true | item.Contains("discrete") == true)
                {
                    discretes.Add(new LogicDevices.DiscrSignal(item));
                }
                if (item.Contains("AND") == true | item.Contains("and") == true)
                {
                    ANDs.Add(new LogicDevices.AND(item));
                }
                if (item.Contains("OR") == true | item.Contains("or") == true)
                {
                    ORs.Add(new LogicDevices.OR(item));
                }
                if (item.Contains("Inv") == true | item.Contains("inv") == true)
                {
                    inverters.Add(new LogicDevices.Invert(item));
                }
            }

            // build logic links matrix

        }


    }
}
