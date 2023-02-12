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
        public List<String> logic_config = new List<String>();
        public List<Complex32> analogInputs = new List<Complex32>();
        public List<bool> discreteInputs = new List<bool>();
        public List<Complex32> tripLevels = new List<Complex32>();
        public List<int> timer_delays = new List<int>();
        public int sim_time = 0;
        public int sim_time_step = 0;
        public bool init = false;
        public bool trip = false;

        List<LogicDevices.BaseLogic> logic_devices = new List<LogicDevices.BaseLogic>();

        public Protection()
        {

        }

        public void Initiate_logic()
        {
            // populate logic devices lists
            foreach (var item in logic_config)
            {
                if (item.Contains("Comp") == true | item.Contains("comp") == true)
                {
                    logic_devices.Add(new LogicDevices.Comparator(item));
                }
                if (item.Contains("Timer") == true | item.Contains("timer") == true)
                {
                    logic_devices.Add(new LogicDevices.Timer(item));
                }
                if (item.Contains("Analog") == true | item.Contains("analog") == true)
                {
                    logic_devices.Add(new LogicDevices.AnalogSignal(item));
                }
                if (item.Contains("Discrete") == true | item.Contains("discrete") == true)
                {
                    logic_devices.Add(new LogicDevices.DiscrSignal(item));
                }
                if (item.Contains("AND") == true | item.Contains("and") == true)
                {
                    logic_devices.Add(new LogicDevices.AND(item));
                }
                if (item.Contains("OR") == true | item.Contains("or") == true)
                {
                    logic_devices.Add(new LogicDevices.OR(item));
                }
                if (item.Contains("Inv") == true | item.Contains("inv") == true)
                {
                    logic_devices.Add(new LogicDevices.Invert(item));
                }
            }
        }

        public void EvaluateLogic()
        {
            Type analog = GetType("Anfang.LogicDevices.AnalogSignal");
            Type and = GetType("Anfang.LogicDevices.AND");
            Type comp = GetType("Anfang.LogicDevices.Comparator");
            Type discr = GetType("Anfang.LogicDevices.DiscrSignal");
            Type invert = GetType("Anfang.LogicDevices.Invert");
            Type or = GetType("Anfang.LogicDevices.OR");
            Type timer = GetType("Anfang.LogicDevices.Timer");

            int analogInputNumber = 0;
            int triplevelNumber = 0;
            int delayNumber = 0;

            int i = 0; // current device
            foreach (var logic_device in logic_devices)
            {
                int n = 0; // next device
                n = i + 1;

                if (logic_device.GetType() == analog)
                {
                    logic_device.input_complex = analogInputs[analogInputNumber];
                    analogInputNumber++;
                    if (logic_devices[n].GetType() == comp)
                    {
                        logic_devices[n].input_complex = logic_device.output_complex;
                    }
                }
                if (logic_device.GetType() == comp |
                    logic_device.GetType() == and |
                    logic_device.GetType() == invert |
                    logic_device.GetType() == or |
                    logic_device.GetType() == timer)
                {
                    if (logic_device.GetType() == comp) // set comparator params
                    {
                        logic_device.triplevel = tripLevels[triplevelNumber];
                        triplevelNumber++;
                    }
                    if (logic_device.GetType() == timer) // set timer params
                    {
                        logic_device.delay = timer_delays[delayNumber];
                        delayNumber++;
                        logic_device.sim_time_step = sim_time_step;
                        logic_device.sim_time = sim_time;
                    }
                    while (true) // find next suitable logic device and connect to it
                    {
                        if (logic_devices[n].GetType() == and |
                        logic_devices[n].GetType() == discr |
                        logic_devices[n].GetType() == invert |
                        logic_devices[n].GetType() == or |
                        logic_devices[n].GetType() == timer)
                        {
                            logic_devices[n].input_bool = logic_device.output;
                            break;
                        }
                        else
                        {
                            n++;
                        }
                    }
                }
                if (logic_device.GetType() == discr) 
                {
                    this.trip = logic_device.output;
                }
                i++;
            }
        }

        public Type GetType(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
            {
                return type;
            }
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                {
                    return type;
                }
            }
            return type;
        }


    }
}
