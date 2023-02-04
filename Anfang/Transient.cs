﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Data;
using System.Collections.ObjectModel;

namespace Anfang
{
    class Transient
    {
        int tran_time = 0;
        int sim_time_step = 0;
        int tran_time_stop = 0;
        List<Complex32> currents_now_list = new List<Complex32>();
        public Vector<Complex32> currents_now = Vector<Complex32>.Build.Random(1,1);

        public Transient(int sim_time_step)
        {
            this.sim_time_step = sim_time_step;
        }

        public Complex32[] Build_array(List<Complex32> currents_now_list)
        {
            Complex32[] currents_now_array = new Complex32[currents_now_list.Count];
            int i = 0;
            foreach (var current in currents_now_list)
            {
                currents_now_array[i] = current;
                i++;
            }
            return currents_now_array;
        }

        public void Do_a_Step_Linear(Vector<Complex32> currents_start, Vector<Complex32> currents_end)
        { //this is a test of a basic linear interpolator.
            tran_time_stop = 1000;
            Complex32 coeff = tran_time_stop / sim_time_step;

            if (tran_time == 0)
            {
                foreach (var current in currents_start)
                {
                    currents_now_list.Add(current);
                }
                tran_time = +sim_time_step;
            }

            else
            {
                if (tran_time > tran_time_stop)
                {
                    //stop the transient calc
                }
                else
                {
                    int i = 0;
                    while (i < currents_now_list.Count)
                    {
                        currents_now_list[i] = ((currents_end[i] - currents_start[i]) / coeff) + currents_now_list[i];
                        i++;
                    }
                    tran_time += sim_time_step;
                }
            }
            this.currents_now = Vector<Complex32>.Build.DenseOfArray(Build_array(currents_now_list));
        }

    }
}
