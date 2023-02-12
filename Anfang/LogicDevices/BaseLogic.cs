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
    public class BaseLogic : INotifyPropertyChanged
    {
        public Complex32 input_complex_old = new Complex32();
        public Complex32 input_complex
        {
            get
            {
                return input_complex_old;
            }
            set
            {
                if (value != this.input_complex_old)
                {
                    this.input_complex_old = value;
                    NotifyInputComplexChanged();
                }
            }
        }

        public bool input_bool_old = false;
        public bool input_bool
        {
            get
            {
                return input_bool_old;
            }
            set
            {
                if (value != this.input_bool_old)
                {
                    this.input_bool_old = value;
                    NotifyInputBoolChanged();
                }
            }
        }

        public List<bool> input_bool_list_old = new List<bool>();
        public List<bool> input_bool_list
        {
            get
            {
                return input_bool_list_old;
            }
            set
            {
                if (value != this.input_bool_list_old)
                {
                    this.input_bool_list_old = value;
                    NotifyInputBoolListChanged();
                }
            }
        }

        public bool init = false;
        public int delay = 0;
        public int sim_time_old = 0;
        public int sim_time
        {
            get
            {
                return sim_time_old;
            }
            set
            {
                if (value != this.sim_time_old)
                {
                    this.sim_time_old = value;
                    NotifySimTimeChanged();
                }
            }
        }

        public Complex32 triplevel = new Complex32();

        public int internal_time = 0;
        public int sim_time_step = 0;

        public bool output = false;

        public string label = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void NotifyInputComplexChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change.

        }

        public virtual void NotifyInputBoolChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change.

        }

        public virtual void NotifyInputBoolListChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change.

        }

        public virtual void NotifySimTimeChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on sim time change.

        }

    }
}
