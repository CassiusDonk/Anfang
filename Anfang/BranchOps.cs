using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Data;
using System.Collections.ObjectModel;
using System.Timers;
using System.Globalization;
using System.Windows.Threading;

namespace Anfang
{
    public class BranchOps
    {
        List<List<Branch>> output_loops = new List<List<Branch>>();
        public bool collection_changed = false;
        Transient transient = new Transient();
        CustomObservable branches_old = new CustomObservable();
        Vector<Complex32> currents = Vector<Complex32>.Build.Random(1);
        public BranchOps()
        {
        }
        public List<Branch> CreateCopy(List<Branch> input)
        {
            List<Branch> branches = new List<Branch>();

            foreach (var branch in input)
            {
                branches.Add(new Branch() { Number = branch.Number, Node1 = branch.Node1, Node2 = branch.Node2, Ohms = branch.Ohms, E = branch.E, Direction = branch.Direction, Reversed = branch.Reversed });
            }
            return branches;
        }
        public List<Branch> Combine(List<Branch> input, int scp)
        {
            List<Branch> branches = new List<Branch>();
            List<Branch> output_combined_branches = new List<Branch>();
            output_combined_branches.Clear();
            branches = input;
            while (true)
            {
                List<Branch> seq_pair = new List<Branch>();
                int Node1_1 = new int();
                int Node2_2 = new int();
                foreach (var branch in branches)
                {
                    seq_pair.Clear();
                    if (branches.FindAll(x => ((x.Node1 == branch.Node1 & x.Node2 != branch.Node2) | (x.Node1 == branch.Node2) & x.Node2 != branch.Node1) & x.Node1 != scp & x.Node1 != 0).Count == 1)
                    {
                        seq_pair.Add(branch);
                        seq_pair.Add(branches.Find(x => ((x.Node1 == branch.Node1 & x.Node2 != branch.Node2) | (x.Node1 == branch.Node2) & x.Node2 != branch.Node1) & x.Node1 != scp & x.Node1 != 0));
                        Node1_1 = seq_pair[1].Node2;
                        if (seq_pair[0].Node1 != seq_pair[1].Node1)
                        {
                            Node2_2 = seq_pair[0].Node1;
                        }
                        else
                        {
                            Node2_2 = seq_pair[0].Node2;
                        }
                        break;
                    }
                    if (branches.FindAll(x => ((x.Node2 == branch.Node2 & x.Node1 != branch.Node1) | (x.Node2 == branch.Node1 & x.Node1 != branch.Node2)) & x.Node2 != scp & x.Node2 != 0).Count == 1)
                    {
                        seq_pair.Add(branch);
                        seq_pair.Add(branches.Find(x => ((x.Node2 == branch.Node2 & x.Node1 != branch.Node1) | (x.Node2 == branch.Node1 & x.Node1 != branch.Node2)) & x.Node2 != scp & x.Node2 != 0));
                        Node1_1 = seq_pair[1].Node1;
                        if (seq_pair[0].Node2 != seq_pair[1].Node2)
                        {
                            Node2_2 = seq_pair[0].Node2;
                        }
                        else
                        {
                            Node2_2 = seq_pair[0].Node1;
                        }
                        break;
                    }
                }
                if (seq_pair.Count < 2)
                {
                    break;
                }
                else
                {
                    foreach (var branch in seq_pair)
                    {
                        branches.RemoveAt(branches.FindIndex(x => x == branch));
                    }
                    if (seq_pair.Count == 2)
                    {
                        branches.Add(new Branch() { Number = branches.Count + 1, Node1 = Node1_1, Node2 = Node2_2, Ohms = seq_pair[0].Ohms + seq_pair[1].Ohms, E = seq_pair[0].E + seq_pair[1].E });
                    }
                }
            }
            output_combined_branches = branches;
            return output_combined_branches;
        }
        public List<int> GetNodes(List<Branch> input)
        {
            List<int> output_nodes = new List<int>();
            List<Branch> branches = CreateCopy(input);
            foreach (var branch in branches)
            {
                if (output_nodes.FindIndex(x => x == branch.Node1) == -1)
                {
                    output_nodes.Add(branch.Node1);
                }
                if (output_nodes.FindIndex(x => x == branch.Node2) == -1)
                {
                    output_nodes.Add(branch.Node2);
                }
            }
            output_nodes.Sort();
            return output_nodes;
        }
        public List<Branch> GetConnectedBranches(List<Branch> input, Branch branch, bool search_at_node2)
        {
            List<Branch> output_connected_branches = new List<Branch>();
            List<Branch> branches = CreateCopy(input);
            if (search_at_node2 == true)
            {
                output_connected_branches = branches.FindAll(x => x.Node1 == branch.Node2 | x.Node2 == branch.Node2);
            }
            if (search_at_node2 == false)
            {
                output_connected_branches = branches.FindAll(x => x.Node1 == branch.Node1 | x.Node2 == branch.Node1);
            }
            output_connected_branches.Remove(output_connected_branches.Find(x => x.Number == branch.Number));
            return output_connected_branches;
        }
        public List<Branch> GetCommonNodeBranches(List<Branch> input, int node)
        {
            List<Branch> output_common_node_branches = new List<Branch>();
            List<Branch> branches = new List<Branch>();

            foreach (var branch in input)
            {
                branches.Add(new Branch() { Number = branch.Number, Node1 = branch.Node1, Node2 = branch.Node2, Ohms = branch.Ohms, E = branch.E });
            }

            output_common_node_branches = branches.FindAll(x => x.Node1 == node | x.Node2 == node);
            foreach (var branch in output_common_node_branches)
            {
                if ((branch.Direction == true & branch.Node2 == node) | (branch.Direction == false & branch.Node1 == node))
                {
                    branch.Ohms = -1;
                }
                else
                {
                    branch.Ohms = 1;
                }
            }
            return output_common_node_branches;
        }

        public List<int> GetJunctions(List<Branch> input)
        {
            List<Branch> branches = CreateCopy(input);
            List<int> result = new List<int>();
            List<int> nodes = GetNodes(branches);
            foreach (var node in nodes)
            {
                if (branches.FindAll(x => x.Node1 == node | x.Node2 == node).Count() > 2)
                {
                    result.Add(node);
                }
            }
            return result;
        }
        public List<List<Branch>> GetLoops(List<Branch> input, Branch starting_branch, bool starting_direction)
        {
            output_loops.Clear();

            List<Branch> branches = CreateCopy(input);
            List<int> junctions = GetJunctions(branches);

            bool direction = new bool();

            bool[] passed_branches = new bool[branches.Count()];

            while (Array.TrueForAll(passed_branches, x => x == true) == false) // determine whether all branches are included in the loops
            {
                List<Branch> output_loop = new List<Branch>();

                List<int> nodes = this.GetNodes(branches);

                direction = starting_direction;

                if (output_loops.Count() == 0) // use first branch as a starting one
                {
                    output_loop.Add(starting_branch);
                    output_loop[0].Direction = direction;
                }
                else // use one of the skipped branches as a starting one to make a unique loop
                {
                    int lol = Array.FindIndex(passed_branches, x => x == false);
                    output_loop.Add(branches[Array.FindIndex(passed_branches, x => x == false)]);
                    output_loop[0].Direction = direction;
                }

                int endpoint = new int();
                if (direction) // determine the endpoint
                {
                    endpoint = output_loop[0].Node1;
                }
                else
                {
                    endpoint = output_loop[0].Node2;
                }

                bool record_path = false;
                List<List<Branch>> recorded_path = new List<List<Branch>>(); // container for a possible wrong path
                foreach (var item in junctions)
                {
                    recorded_path.Add(new List<Branch>());
                }

                int junction = -1; // this number is needed so we could fall back to the latest junction instead of the first one 

                while (true) // loop-building procedure
                {
                    List<Branch> connected = GetConnectedBranches(branches, output_loop[output_loop.Count() - 1], direction); // looking for connected branches
                    if (direction) // removing the node ensures that we pass each branch only once
                    {
                        nodes.Remove(output_loop[output_loop.Count - 1].Node2);
                    }
                    else
                    {
                        nodes.Remove(output_loop[output_loop.Count - 1].Node1);
                    }
                    if (connected.Count() > 1) // there are multiple connected branches - we could face an error if we go in the wrong direction
                    {                          // therefore, we start recording our path
                        record_path = true;
                        junction++;
                    }
                    foreach (var branch in connected)
                    {
                        if (nodes.Contains(branch.Node1))
                        {
                            direction = false;
                            input.Find(x => x.Number == branch.Number).Direction = direction;
                            output_loop.Add(new Branch()
                            {
                                Number = branch.Number,
                                Node1 = branch.Node1,
                                Node2 = branch.Node2,
                                Ohms = -branch.Ohms,
                                E = branch.E,
                                Reversed = true
                            });
                            if (record_path == true)
                            {
                                recorded_path[junction].Add(new Branch() { Number = branch.Number });
                            }
                            break;
                        }
                        if (nodes.Contains(branch.Node2))
                        {
                            direction = true;
                            input.Find(x => x.Number == branch.Number).Direction = direction;
                            output_loop.Add(new Branch()
                            {
                                Number = branch.Number,
                                Node1 = branch.Node1,
                                Node2 = branch.Node2,
                                Ohms = branch.Ohms,
                                E = branch.E
                            });
                            if (record_path == true)
                            {
                                recorded_path[junction].Add(new Branch() { Number = branch.Number });
                            }
                            break;
                        }
                        if (connected.FindIndex(x => x == branch) == connected.Count() - 1) // we checked all connected branches and found no suitable ones
                        {
                            if (recorded_path.Count() - 1 < junction) // we have no available branches right at the junction
                            {                                         // we have to fall back to the previous junction
                                foreach (var recorded_branch in recorded_path[junction - 1])
                                {
                                    output_loop.RemoveAll(x => x.Number == recorded_branch.Number);
                                }
                                recorded_path.RemoveAt(junction - 1);
                                //recorded_path.TrimExcess();
                                junction--;
                                junction--;
                            }
                            else // we have no available branches somewhere after the junction, so we fall back to it
                            {
                                foreach (var recorded_branch in recorded_path[junction])
                                {
                                    output_loop.RemoveAll(x => x.Number == recorded_branch.Number);
                                }
                                recorded_path.RemoveAt(junction);
                                //recorded_path.TrimExcess();
                                junction--;
                            }
                        }
                    }
                    if (output_loop.Count() >= 2 & (output_loop[output_loop.Count() - 1].Node1 == endpoint | output_loop[output_loop.Count() - 1].Node2 == endpoint))
                    {
                        break;
                    }
                }
                output_loops.Add(output_loop);
                foreach (var item in output_loop)
                {
                    passed_branches[item.Number - 1] = true;
                }
            }
            if (output_loops.Count == 0)
            {
                throw new TopologyException("No loops found - check topology");
            }
            return output_loops;
        }
        public Complex32[,] BuildMatrix(List<Branch> input)
        {
            int matrix_size = input.Count();
            Complex32[,] matrix = new Complex32[matrix_size, matrix_size];
            if (input.Count == 0)
            {
                throw new TopologyException("No loops found - input is empty");
            }
            List<List<Branch>> loops = this.GetLoops(input, input[0], true);
            List<int> nodes = this.GetNodes(input);

            int i = new int();
            while (loops.Count < input.Count)
            {
                loops.Add(GetCommonNodeBranches(input, nodes[i]));
                i++;
            }

            int j = 0;
            foreach (var loop in loops)
            {
                foreach (var branch in loop)
                {
                    matrix[j, branch.Number - 1] = branch.Ohms;
                }
                j++;
            }
            return matrix;
        }
        public Complex32[] BuildEVector(List<List<Branch>> loops, int size)
        {
            Complex32[] EVector = new Complex32[size];
            int i = 0;
            foreach (var loop in loops)
            {
                foreach (var branch in loop)
                {
                    if (branch.Reversed == true)
                    {
                        EVector[i] = EVector[i] - branch.E;
                    }
                    else
                    {
                        EVector[i] = EVector[i] + branch.E;
                    }
                }
                i++;
            }
            while (i < size - 1)
            {
                EVector[i] = 0;
                i++;
            }
            return EVector;
        }
        public Vector<Complex32> CalcCurrents(CustomObservable input_raw)
        {
            List<Branch> input = new List<Branch>();
            foreach (var branch in input_raw)
            {
                if (branch.IsBreaker == true)
                {
                    branch.E_Act = 0;
                    branch.E_React = 0;
                    if (branch.Enabled == false)
                    {
                        branch.Ohms_Act = 999999999;
                        branch.Ohms_React = 999999999;
                    }
                    else
                    {
                        branch.Ohms_Act = 0;
                        branch.Ohms_React = 0;
                    }
                    input.Add(branch);
                }
                else
                {
                    input.Add(branch);
                }
            }

            Complex32[,] matrix = BuildMatrix(input);
            Complex32[] evector = BuildEVector(GetLoops(input, input[0], true), input.Count);

            var matrix_num = Matrix<Complex32>.Build.DenseOfArray(matrix);
            var evector_num = Vector<Complex32>.Build.DenseOfArray(evector);
            Vector<Complex32> currents_num = matrix_num.Solve(evector_num);
            return currents_num;
        }
        public void CalcCurrents_to_branches(CustomObservable input_raw, bool force_update_event)
        {
            List<Branch> input = new List<Branch>();
            foreach (var branch in input_raw)
            {
                if (branch.IsBreaker == true)
                {
                    branch.E_Act = 0;
                    branch.E_React = 0;
                    if (branch.Enabled == false)
                    {
                        branch.Ohms_Act = 999999999;
                        branch.Ohms_React = 999999999;
                    }
                    else
                    {
                        branch.Ohms_Act = 0;
                        branch.Ohms_React = 0;
                    }
                    input.Add(branch);
                }
                else
                {
                    input.Add(branch);
                }
            }

            Complex32[,] matrix = BuildMatrix(input);
            Complex32[] evector = BuildEVector(GetLoops(input, input[0], true), input.Count);

            var matrix_num = Matrix<Complex32>.Build.DenseOfArray(matrix);
            var evector_num = Vector<Complex32>.Build.DenseOfArray(evector);
            Vector<Complex32> currents_num = matrix_num.Solve(evector_num);
            int i = 0;
            foreach (var current in currents_num)
            {
                input_raw.UpdateProperty(input_raw[i], "Current", current, force_update_event);
                i++;
            }
        }
        public void CalcTranCurrents_to_branches(CustomObservable branches, int sim_time_step)
        {
            // First we have to determine whether any changes occured to topology.
            // Changes can occur in two main ways: new branch is added and "Enabled" property is changed.
            // First one is easy, just compare the Count of collections.
            // Second one is more tricky. We could compare each individual IsBreaker branch, but
            // this might be too taxing in terms of perfomance.
            // It would be much faster to rely on CollectionChanged event implemented in the main program.
            // When event is triggered we set CollectionChanged declared here as "true".
            // Inportant: we have to set CollectionChanged back to false at the end of the cycle.

            // If changes occured (CollectionChanged = true), we first check whether transient calculation is already running.
            if (collection_changed == true)
            {
                // If it isn't, we calculate currents before and after and start the transient calculation.
                if (transient.enabled == false)
                {
                    Vector<Complex32> currents_start = CalcCurrents(branches_old);
                    Vector<Complex32> currents_end = CalcCurrents(branches);

                    // It will also be easier to feed currents before and after to the Transient class and store them there.
                    transient.currents_start = currents_start.Clone(); // .Clone() is used to create a completely independent copy.
                    transient.currents_end = currents_end.Clone();
                    transient.enabled = true;
                    transient.sim_time_step = sim_time_step;
                    transient.Do_a_Step_Linear();
                }

                // If it is, we use currents at this moment as "start" currents, and calculate only "end" currents,
                // then restart the transient calculation.
                else
                {
                    Vector<Complex32> currents_start = transient.currents_now.Clone();
                    Vector<Complex32> currents_end = CalcCurrents(branches);

                    transient.Reset();

                    transient.currents_start = currents_start.Clone();
                    transient.currents_end = currents_end.Clone();
                    transient.enabled = true;
                    transient.sim_time_step = sim_time_step;
                    transient.Do_a_Step_Linear();
                }

                // update currents in branches
                currents = transient.currents_now.Clone();
                int i = 0;
                foreach (var current in currents)
                {
                    branches.UpdateProperty(branches[i], "Current", current, false);
                    i++;
                }
                collection_changed = false;
            }

            // If no changes occured:
            else
            {
                // First we check if transient calculation is enabled.
                // If it is, we perform a step.
                if (transient.enabled == true)
                {
                    transient.Do_a_Step_Linear();
                    currents = transient.currents_now.Clone();
                    int i = 0;
                    foreach (var current in currents)
                    {
                        branches.UpdateProperty(branches[i], "Current", current, false);
                        i++;
                    }
                }

                else
                {
                    // If it isn't, we check if have any currents calculated at all.
                    if (currents.Count() > 1)
                    {
                        // If currents are present, we do nothing.
                    }
                    else
                    {
                        // If there are no currents, we calculate them.
                        currents = CalcCurrents(branches).Clone();
                        int i = 0;
                        foreach (var current in currents)
                        {
                            branches.UpdateProperty(branches[i], "Current", current, false);
                            i++;
                        }
                    }
                }

            }
            branches_old.Clear();
            foreach (var branch in branches)
            {
                branches_old.Add(branch);
            }
        }

        public void ResetTran()
        {
            transient.Reset();
            collection_changed = false;
            branches_old.Clear();
            currents = Vector<Complex32>.Build.Random(1);
        }
        public Vector<Complex32> CalcShockCurrents(List<Branch> input, int shocknode, Complex32 shockresistance)
        {
            List<Branch> branches = CreateCopy(input);
            if (GetNodes(branches).Contains(0) == false)
            {
                throw new TopologyException("No ground node (node 0) found - check topology");
            }
            branches.Add(new Branch() { Number = branches.Count + 1, Node1 = 0, Node2 = shocknode, Ohms = shockresistance });

            Complex32[,] matrix = BuildMatrix(branches);
            Complex32[] evector = BuildEVector(GetLoops(branches, branches[0], true), branches.Count);

            var matrix_num = Matrix<Complex32>.Build.DenseOfArray(matrix);
            var evector_num = Vector<Complex32>.Build.DenseOfArray(evector);
            Vector<Complex32> currents_num = matrix_num.Solve(evector_num);
            return currents_num;
        }
    }
}
