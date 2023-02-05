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
    class Timer : INotifyPropertyChanged
    {
        public bool input_old = false;
        public bool input
        {
            get
            {
                return input_old;
            }
            set
            {
                if (value != this.input_old)
                {
                    this.input_old = value;
                    NotifyInputChanged();
                }
            }
        }
        bool init = false;
        public bool output = false;
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
        public int internal_time = 0;
        public int sim_time_step = 0;
        public Timer(int sim_time_step)
        {
            this.sim_time_step = sim_time_step;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyInputChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (input == true)
            {
                init = true;
            }
            if (input == false)
            {
                output = false;
                init = false;
            }
        }

        private void NotifySimTimeChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (sim_time == 0)
            {
                output = false;
            }
            if (init == true)
            {
                if (internal_time >= delay)
                {
                    output = input;
                    internal_time = 0;
                    init = false;
                }
                internal_time += sim_time_step;
            }
        }
    }
}
