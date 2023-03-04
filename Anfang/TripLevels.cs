using System;
using MathNet.Numerics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Anfang
{
    public class TripLevels : INotifyPropertyChanged
    {
        private float Trip_Act_Value;
        private float Trip_React_Value;

        public float Trip_Act
        {
            get
            {
                return Trip_Act_Value;
            }
            set
            {
                if (value != this.Trip_Act_Value)
                {
                    this.Trip_Act_Value = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public float Trip_React
        {
            get
            {
                return Trip_React_Value;
            }
            set
            {
                if (value != this.Trip_React_Value)
                {
                    this.Trip_React_Value = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Complex32 TripLevel = new Complex32();

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            TripLevel = new Complex32(Trip_Act, Trip_React);
        }
    }
}
