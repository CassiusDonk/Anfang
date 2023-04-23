using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            foreach (var branch in input)
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
                branch.Ohms = branch.Ohms;
            }
            return output_common_node_branches;
        }

        public void CalculateNodeVoltages(List<Branch> input, int ground_node)
        {

            List<int> nodes = GetNodes(input);
            nodes.Remove(ground_node);

            Complex32[,] conductances = new Complex32[nodes.Count(), nodes.Count()];
            Complex32[] node_currents = new Complex32[nodes.Count()];

            var get_conductances = Task.Run(() =>
            {
                for (int m = 0; m < nodes.Count(); m++)
                {
                    for (int n = 0; n < nodes.Count(); n++)
                    {
                        List<Branch> branches_nm = new List<Branch>();
                        if (n == m)
                        {
                            branches_nm = GetCommonNodeBranches(input, nodes[n]);
                        }
                        else
                        {
                            branches_nm = input.FindAll(x => (x.Node1 == nodes[m] & x.Node2 == nodes[n]) | (x.Node1 == nodes[n] & x.Node2 == nodes[m]));
                        }
                        Complex32 conductance = new Complex32(0, 0);
                        foreach (var branch in branches_nm)
                        {
                            Complex32 ohms_inv;
                            Complex32 real_inv;
                            Complex32 imag_inv;
                            if (branch.Ohms.Real < 0)
                            {
                                real_inv = new Complex32(-branch.Ohms.Real, 0);
                            }
                            else
                            {
                                real_inv = new Complex32(branch.Ohms.Real, 0);
                            }
                            if (branch.Ohms.Imaginary < 0)
                            {
                                imag_inv = new Complex32(0, -branch.Ohms.Imaginary);
                            }
                            else
                            {
                                imag_inv = new Complex32(0, branch.Ohms.Imaginary);
                            }
                            ohms_inv = real_inv + imag_inv;
                            if (n == m)
                            {
                                conductance += 1 / branch.Ohms;
                            }
                            else
                            {
                                conductance += -1 / branch.Ohms;
                            }
                        }
                        conductances[m, n] = conductance;
                    }
                }
            });

            var get_node_currents = Task.Run(() =>
            {
                for (int m = 0; m < nodes.Count(); m++)
                {
                    List<Branch> branches_m = GetCommonNodeBranches(input, nodes[m]);
                    Complex32 node_current = new Complex32();
                    foreach (var branch in branches_m)
                    {
                        if (branch.Node1 == nodes[m])
                        {
                            node_current += -branch.E / branch.Ohms;
                        }
                        else
                        {
                            node_current += branch.E / branch.Ohms;
                        }
                    }
                    node_currents[m] = node_current;
                }
            });

            Task.WaitAll(get_conductances, get_node_currents);

            Matrix<Complex32> conductances_matrix = Matrix<Complex32>.Build.Random(1, 1);
            Vector<Complex32> node_currents_vector = Vector<Complex32>.Build.Random(1, 1);
            var a = Task.Run(() => conductances_matrix = Matrix<Complex32>.Build.DenseOfArray(conductances));
            var b = Task.Run(() => node_currents_vector = Vector<Complex32>.Build.DenseOfArray(node_currents));
            Task.WaitAll(a, b);
            Vector<Complex32> voltages = conductances_matrix.Solve(node_currents_vector);

            foreach (var branch in input)
            {
                if (branch.Node1 != 0)
                {
                    int node1_index = nodes.FindIndex(x => x == branch.Node1);
                    branch.Voltage_Node1 = voltages[node1_index];
                }
                if (branch.Node2 != 0)
                {
                    int node2_index = nodes.FindIndex(x => x == branch.Node2);
                    branch.Voltage_Node2 = voltages[node2_index];
                }
                Complex32 vdrop;
                if (branch.E != 0)
                {
                    vdrop = branch.Voltage_Node1 + branch.E - branch.Voltage_Node2;
                }
                else
                {
                    vdrop = branch.Voltage_Node1 - branch.Voltage_Node2;
                }
                Complex32 current = vdrop / branch.Ohms;
                branch.Current = current;
            }
        }
        public void CalculateNodeVoltagesCO(CustomObservable branches, int ground_node)
        {
            List<Branch> input = new List<Branch>();
            foreach (var branch in branches)
            {
                input.Add(branch);
            }
            List<int> nodes = GetNodes(input);
            nodes.Remove(ground_node);

            Complex32[,] conductances = new Complex32[nodes.Count(), nodes.Count()];
            Complex32[] node_currents = new Complex32[nodes.Count()];

            var get_conductances = Task.Run(() =>
            {
                for (int m = 0; m < nodes.Count(); m++)
                {
                    for (int n = 0; n < nodes.Count(); n++)
                    {
                        List<Branch> branches_nm = new List<Branch>();
                        if (n == m)
                        {
                            branches_nm = GetCommonNodeBranches(input, nodes[n]);
                        }
                        else
                        {
                            branches_nm = input.FindAll(x => (x.Node1 == nodes[m] & x.Node2 == nodes[n]) | (x.Node1 == nodes[n] & x.Node2 == nodes[m]));
                        }
                        Complex32 conductance = new Complex32(0, 0);
                        foreach (var branch in branches_nm)
                        {
                            Complex32 ohms_inv;
                            Complex32 real_inv;
                            Complex32 imag_inv;
                            if (branch.Ohms.Real < 0)
                            {
                                real_inv = new Complex32(-branch.Ohms.Real, 0);
                            }
                            else
                            {
                                real_inv = new Complex32(branch.Ohms.Real, 0);
                            }
                            if (branch.Ohms.Imaginary < 0)
                            {
                                imag_inv = new Complex32(0, -branch.Ohms.Imaginary);
                            }
                            else
                            {
                                imag_inv = new Complex32(0, branch.Ohms.Imaginary);
                            }
                            ohms_inv = real_inv + imag_inv;
                            if (n == m)
                            {
                                conductance += 1 / branch.Ohms;
                            }
                            else
                            {
                                conductance += -1 / branch.Ohms;
                            }
                        }
                        conductances[m, n] = conductance;
                    }
                }
            });

            var get_node_currents = Task.Run(() =>
            {
                for (int m = 0; m < nodes.Count(); m++)
                {
                    List<Branch> branches_m = GetCommonNodeBranches(input, nodes[m]);
                    Complex32 node_current = new Complex32();
                    foreach (var branch in branches_m)
                    {
                        if (branch.Node1 == nodes[m])
                        {
                            node_current += -branch.E / branch.Ohms;
                        }
                        else
                        {
                            node_current += branch.E / branch.Ohms;
                        }
                    }
                    node_currents[m] = node_current;
                }
            });

            Task.WaitAll(get_conductances, get_node_currents);

            Matrix<Complex32> conductances_matrix = Matrix<Complex32>.Build.Random(1, 1);
            Vector<Complex32> node_currents_vector = Vector<Complex32>.Build.Random(1, 1);
            var a = Task.Run(() => conductances_matrix = Matrix<Complex32>.Build.DenseOfArray(conductances));
            var b = Task.Run(() => node_currents_vector = Vector<Complex32>.Build.DenseOfArray(node_currents));
            Task.WaitAll(a, b);
            Vector<Complex32> voltages = conductances_matrix.Solve(node_currents_vector);

            for (int i = 0; i <= branches.Count() - 1; i++)
            {
                if (branches[i].Node1 != 0)
                {
                    int node1_index = nodes.FindIndex(x => x == branches[i].Node1);
                    branches.UpdateProperty(branches[i], "Voltage_Node1", voltages[node1_index], false);
                }
                if (branches[i].Node2 != 0)
                {
                    int node2_index = nodes.FindIndex(x => x == branches[i].Node2);
                    branches.UpdateProperty(branches[i], "Voltage_Node2", voltages[node2_index], false);
                }
                Complex32 vdrop = branches[i].Voltage_Node1 - branches[i].Voltage_Node2;
                if (branches[i].E != 0)
                {
                    vdrop = branches[i].E - branches[i].Voltage_Node2;
                }
                Complex32 current = vdrop / branches[i].Ohms;
                branches.UpdateProperty(branches[i], "Current", current, false);
                if (i < branches.Count() - 1)
                {
                    branches.UpdateProperty(branches[i], "Voltage_Drop", vdrop, false);
                }
                else
                {
                    branches.UpdateProperty(branches[i], "Voltage_Drop", vdrop, false);
                }
            }
        }
    }
}
