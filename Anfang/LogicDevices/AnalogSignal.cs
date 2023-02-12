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
    public class AnalogSignal : INotifyPropertyChanged
    {
        public Complex32 input_old = new Complex32();
        public Complex32 input
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
        public Complex32 output = new Complex32();

        public string label = "";

        public AnalogSignal(string label)
        {
            this.label = label;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyInputChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            output = input;
        }
    }
}
