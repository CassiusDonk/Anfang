using Anfang.Powersystem;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

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
        public bool trip_old;
        public ObservableCollection<PowSysElementBase> powersystem;
        public bool trip
        {
            get
            {
                return trip_old;
            }
            set
            {
                if (value != this.trip_old)
                {
                    this.trip_old = value;
                    TripBreaker();
                }
            }
        }
        public string label { get; set; }
        public string init_label { get; set; }
        public string trip_label { get; set; }

        public List<LogicDevices.BaseLogic> logic_devices = new List<LogicDevices.BaseLogic>();

        public List<AnalogInputLink> analogInputLinks = new List<AnalogInputLink>();

        public List<int> breaker_numbers = new List<int>();

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
            foreach (var logic_device in logic_devices) // iterate through devices and build connections
            {
                int n = 0; // next device
                n = i + 1;

                if (logic_device.GetType() == analog)
                {
                    logic_device.input_complex = analogInputs[analogInputNumber]; // connect analog value to this analog signal
                    analogInputNumber++;
                    if (logic_devices[n].GetType() == comp)
                    {
                        logic_devices[n].triplevel = tripLevels[triplevelNumber];
                        triplevelNumber++;
                        logic_devices[n].input_complex = logic_device.output_complex;
                    }
                }
                if (logic_device.GetType() == comp |
                    logic_device.GetType() == and |
                    logic_device.GetType() == invert |
                    logic_device.GetType() == or |
                    logic_device.GetType() == timer)
                {
                    if (logic_device.GetType() == timer) // set timer params
                    {
                        logic_device.delay = timer_delays[delayNumber];
                        delayNumber++;
                        logic_device.sim_time_step = sim_time_step;
                        logic_device.sim_time = sim_time;
                    }
                    while (true) // find next suitable logic device and connect to it
                    {
                        if (logic_devices[n].GetType() == discr |
                        logic_devices[n].GetType() == invert |
                        logic_devices[n].GetType() == timer)
                        {
                            logic_devices[n].input_bool = logic_device.output;
                            break;
                        }
                        if (logic_devices[n].GetType() == and |
                        logic_devices[n].GetType() == or)
                        {
                            logic_devices[n].sim_time = sim_time; // this lets the device know the new cycle started
                            logic_devices[n].input_bool_list.Add(logic_device.output);
                            break;
                        }
                        else
                        {
                            n++;
                        }
                    }
                }
                if (init_label != "")
                {
                    this.init = logic_devices.Find(x => x.label == init_label).output;
                }
                if (trip_label != "")
                {
                    this.trip = logic_devices.Find(x => x.label == trip_label).output;
                }
                i++;
            }
        }

        public void TryEvaluateLogic()
        {
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    analogInputs.Add(new Complex32(0, 0));
                }
                EvaluateLogic();
            }
            catch (Exception ex)
            {
                if (ex is System.ArgumentOutOfRangeException || ex is System.NullReferenceException)
                {
                    throw new Exception();
                }
                else
                {

                }
            }
        }

        public void getAnalogs()
        {

            if (analogInputs.Count() == 0) // populate analog inputs
            {
                foreach (var analogInputLink in analogInputLinks)
                {
                    PowSysElementBase element = FindByID(powersystem, analogInputLink.id);
                    if (analogInputLink.isVoltage == false) // currents
                    {
                        if (analogInputLink.side == 1)
                        {
                            if (analogInputLink.phase == "A")
                            {
                                analogInputs.Add(element.currents_side1[0]);
                            }
                            if (analogInputLink.phase == "B")
                            {
                                analogInputs.Add(element.currents_side1[1]);
                            }
                            if (analogInputLink.phase == "C")
                            {
                                analogInputs.Add(element.currents_side1[2]);
                            }
                        }
                        if (analogInputLink.side == 2)
                        {
                            if (analogInputLink.phase == "A")
                            {
                                analogInputs.Add(element.currents_side2[0]);
                            }
                            if (analogInputLink.phase == "B")
                            {
                                analogInputs.Add(element.currents_side2[1]);
                            }
                            if (analogInputLink.phase == "C")
                            {
                                analogInputs.Add(element.currents_side2[2]);
                            }
                        }
                    }
                    else // voltages
                    {

                    }
                }
            }
            else // update analog inputs
            {
                int i = 0;
                foreach (var analogInputLink in analogInputLinks)
                {
                    PowSysElementBase element = FindByID(powersystem, analogInputLink.id);
                    if (analogInputLink.isVoltage == false)
                    {
                        if (analogInputLink.side == 1)
                        {
                            if (analogInputLink.phase == "A")
                            {
                                analogInputs[i] = element.currents_side1[0];
                            }
                            if (analogInputLink.phase == "B")
                            {
                                analogInputs[i] = element.currents_side1[1];
                            }
                            if (analogInputLink.phase == "C")
                            {
                                analogInputs[i] = element.currents_side1[2];
                            }
                        }
                        if (analogInputLink.side == 2)
                        {
                            if (analogInputLink.phase == "A")
                            {
                                analogInputs[i] = element.currents_side2[0];
                            }
                            if (analogInputLink.phase == "B")
                            {
                                analogInputs[i] = element.currents_side2[1];
                            }
                            if (analogInputLink.phase == "C")
                            {
                                analogInputs[i] = element.currents_side2[2];
                            }
                        }
                    }
                    else
                    {

                    }
                    i++;
                }
            }
        }

        public PowSysElementBase FindByID(ObservableCollection<PowSysElementBase> powersystem, int id)
        {
            PowSysElementBase result = new PowSysElementBase();
            foreach (var item in powersystem)
            {
                if (item.id == id)
                {
                    result = item;
                    return result;
                }
            }
            return result;
        }

        public event PropertyChangedEventHandler Trip;
        public void TripBreaker([CallerMemberName] String propertyName = "")
        {
            Trip?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            foreach (var breaker_number in breaker_numbers)
            {
                PowSysElementBase breaker = FindByID(powersystem, breaker_number);
                breaker.property1 = 0;
                breaker.property2 = 0;
                breaker.property3 = 0;
            }
        }

        public void ResetLogic()
        {
            logic_devices.Clear();
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
