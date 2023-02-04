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
    public class BranchOps
    {
        List<List<Branch>> output_loops = new List<List<Branch>>();
        public BranchOps()
        {
        }
        public List<Branch> CreateCopy (List<Branch> input)
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
        public List<List<Branch>> GetLoops(List<Branch> input, Branch starting_branch, bool starting_direction)
        {
            output_loops.Clear();
            List<Branch> branches = CreateCopy(input);

            int endpoint = new int();

            if (starting_direction == true)
            {
                endpoint = starting_branch.Node1;
            }
            else
            {
                endpoint = starting_branch.Node2;
            }

            bool direction = new bool();

            while (output_loops.Count() < input.Count - this.GetNodes(input).Count + 1)
            {
                List<Branch> output_loop = new List<Branch>();

                List<int> nodes = this.GetNodes(branches);

                direction = starting_direction;

                output_loop.Add(starting_branch);
                output_loop[0].Direction = direction;
                bool one_branch_deleted = false;

                while (true)
                {
                    List<Branch> connected = GetConnectedBranches(branches, output_loop[output_loop.Count - 1], direction);
                    if (direction)
                    {
                        nodes.Remove(output_loop[output_loop.Count - 1].Node2);
                    }
                    else
                    {
                        nodes.Remove(output_loop[output_loop.Count - 1].Node1);
                    }
                    foreach (var branch in connected)
                    {
                        if (nodes.Contains(branch.Node1))
                        {
                            direction = false;
                            input.Find(x => x.Number == branch.Number).Direction = direction;
                            output_loop.Add(new Branch() { Number = branch.Number, Node1 = branch.Node1, Node2 = branch.Node2, Ohms = -branch.Ohms, E = branch.E, Reversed = true });
                            if (connected.Count() > 1 & one_branch_deleted == false)
                            {
                                branches.Remove(branches.Find(x => x.Number == branch.Number));
                                one_branch_deleted = true;
                            }
                            break;
                        }
                        if (nodes.Contains(branch.Node2))
                        {
                            direction = true;
                            input.Find(x => x.Number == branch.Number).Direction = direction;
                            output_loop.Add(new Branch() { Number = branch.Number, Node1 = branch.Node1, Node2 = branch.Node2, Ohms = branch.Ohms, E = branch.E });
                            if (connected.Count() > 1 & one_branch_deleted == false)
                            {
                                branches.Remove(branches.Find(x => x.Number == branch.Number));
                                one_branch_deleted = true;
                            }
                            break;
                        }
                        else
                        {
                            throw new TopologyException("No loops found - check topology");
                        }
                    }
                    if (output_loop.Count() >= 2 & (output_loop[output_loop.Count() - 1].Node1 == endpoint | output_loop[output_loop.Count() - 1].Node2 == endpoint) )
                    {
                        break;
                    }
                }
                output_loops.Add(output_loop);
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
        public Vector<Complex32> CalcCurrents(List<Branch> input)
        {
            Complex32[,] matrix = BuildMatrix(input);
            Complex32[] evector = BuildEVector(GetLoops(input, input[0], true), input.Count);

            var matrix_num = Matrix<Complex32>.Build.DenseOfArray(matrix);
            var evector_num = Vector<Complex32>.Build.DenseOfArray(evector);
            Vector<Complex32> currents_num = matrix_num.Solve(evector_num);
            return currents_num;
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
