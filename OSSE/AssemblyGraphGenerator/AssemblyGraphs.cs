using System;
using System.Diagnostics;

namespace AssemblyGraph {
    /// <summary>
    /// vertex class which represents a group in a method
    /// </summary>
    class Vertex {
        private String label; // name of the of the group e.g. IG01, IG02...
        private List<String> content_code; // the lines of assembly code in the group
        private Double weight; // the weight of the group
        private Double score; // the perfoscore of the group

        /// <summary>
        /// vertex constructor
        /// </summary>
        /// <param name="label"></param>
        /// <param name="weight"></param>
        /// <param name="score"></param>
        /// <param name="content"></param>
        public Vertex (String label, Double weight, Double score, List<String> content) {
            this.label = label;
            this.content_code = content;
            this.weight = weight;
            this.score = score;
        }

        /// <summary>
        /// returns true or false if the current vertex equals v1
        /// </summary>
        /// <param name="v1"></param>
        /// <returns></returns>
        public Boolean Equals (Vertex v1){
            return v1.label == label;
        }

        public void SetContentCode (List<String> content) {
            this.content_code = content;
        }

        public List<String> GetContentCode () {
            return content_code;
        }

        public void SetWeight (int weight) {
            this.weight = weight;
        }

        public Double GetWeight () {
            return weight;
        }

        public void SetScore (int score) {
            this.score = score;
        }

        public Double GetScore () {
            return score;
        }
        
        
        public String GetLabel () {
            return label;
        }
    }

    /// <summary>
    /// This graph class represents a method
    /// </summary>

    class Graph {
        private Dictionary<Vertex, List<Vertex>> adjVertices;
        private String hashcode = "";
        private Double maxWeight = 0;
        private Double maxScore = 0;
        private int total_bytes = 0;
        private String system = "";
        private Boolean optimized = false;
        private String method_name = "";

        public Graph () {
            this.adjVertices = new Dictionary<Vertex, List<Vertex>>();
        }
        
        public void SetMethodName (String name) {
            this.method_name = name;
        }

        public String GetMethodName () {
            return method_name;
        }
        public void SetOptimized (Boolean o) {
            this.optimized = o;
        }

        public Boolean GetOptimized () {
            return optimized;
        }
        public void SetHashcode (String h) {
            this.hashcode = h;
        }

        public String GetHashcode () {
            return hashcode;
        }

        public void SetTotalBytes (int b) {
            this.total_bytes = b;
        }

        public int GetTotalBytes () {
            return total_bytes;
        }

        public void SetSystem (String s) {
            this.system = s;
        }

        public void SetMaxWeight (Double weight) {
            this.maxWeight = weight;
        }

        public Double GetMaxWeight () {
            return maxWeight;
        }

        public void SetMaxScore (Double score) {
            this.maxScore = score;
        }

        public Double GetMaxScore () {
            return maxScore;
        }

        /// <summary>
        /// adds a vertex v to the graph
        /// </summary>
        /// <param name="v"></param>
        public void AddVertex (Vertex v) {
            adjVertices.TryAdd(v, new List<Vertex>());
        }

        /// <summary>
        /// adds an edge v1->v2 to the graph
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public void AddEdge (Vertex v1, Vertex v2) {
            adjVertices[v1].Add(v2);
        }

        /// <summary>
        /// returns the list of vertices currently in the graph
        /// </summary>
        /// <returns></returns>
        public List<Vertex> GetVertices() {
            return adjVertices.Keys.ToList();
        }

        /// <summary>
        /// given a label the function returns the vertex associated with that label
        /// or throws an exception if the vertex with such label is currently not in the graph
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public Vertex GetVertex (String label) {
            List<Vertex> vertices = GetVertices();

            foreach (Vertex v in vertices) {
                if (v.GetLabel() == label) {
                    return v;
                }
            }    

            throw new InvalidDataException("This vertex does not exist");
        }

    
        private String choose_color (Double val, Double max, Boolean heatMap) {

            Double score = Math.Round(val / max, 1 );

            if(heatMap) {
                if (score < 0.2) {; return "1";}
                else if (score >= 0.2 && score < 0.3) {return "2";}
                else if (score >= 0.3 && score < 0.4) {return "3";}
                else if (score >= 0.4 && score < 0.5) {return "4";}
                else if (score >= 0.5 && score < 0.6) {return "5";}
                else if (score >= 0.6 && score < 0.7) {return "6";}
                else if (score >= 0.7 && score < 0.8) {return "7";}
                else if (score >= 0.8 && score < 0.9) {return "8";}
                else  {return "9";}
            }

            if (val <= 1 || score < 0.2) {return "1";}
            // else if (val == 1) {return "1";}
            else {
                if (val <= 1.5 || (score >= 0.2 && score < 0.4)) {return "2";}
                else if (score >= 0.4 && score < 0.6) {return "3";}
                else if (score >= 0.6 && score < 0.8) {return "4";}
                else  {return "5";}
            }
            
        }

        /// <summary>
        /// DFS implementaton to detect and return all cycles found along a path
        /// </summary>
        /// <param name="v"></param>
        /// <param name="visited"></param>
        /// <param name="explore"></param>
        /// <param name="cycles"></param>
        /// <returns></returns>
        private bool DFS (Vertex v, ref Stack<Vertex> visited, ref Stack<Vertex> explore, ref HashSet<List<Vertex>> cycles) {
            if (explore.Contains(v)) {
                // found a cycle
                List<Vertex> cycle = new List<Vertex>();
                cycle.Add(v);
                while (!explore.Peek().Equals(v)) {
                    Vertex v_c = explore.Pop();
                    cycle.Add(v_c);
                }
                
                for (int i=cycle.Count -1; i > 0; i--) {
                    explore.Push(cycle[i]);
                }

                cycle.Add(v);
                visited.Push(v);
                cycles.Add(cycle);
                return true;
            }

            explore.Push(v);
            foreach (Vertex v1 in adjVertices[v]) {
                if (!visited.Contains(v1)) {
                    Boolean cycle = DFS(v1, ref visited, ref explore, ref cycles);
                    if (!cycle) {
                        while (!explore.Peek().Equals(v1)) {
                            explore.Pop();
                        }
                        explore.Pop();
                    } 
                }
                
            }

            visited.Push(v);
            return false; // no cycle found
        }

        /// <summary>
        /// returns a list of all the cycles found in the graph
        /// </summary>
        /// <returns></returns>
        private HashSet<List<Vertex>> DetectCycle() {
            Stack<Vertex> visited = new Stack<Vertex>();
            Stack<Vertex> explore = new Stack<Vertex>();
            HashSet<List<Vertex>> cycles = new HashSet<List<Vertex>>();

            foreach (Vertex v in GetVertices()) {
                if (!visited.Contains(v)) {
                    DFS(v, ref visited, ref explore, ref cycles);
                }
            }

            return cycles;
        }

        /// <summary>
        /// generates an SVG file from created dot file
        /// </summary>
        /// <param name="folder_path"></param>
        /// <param name="show_stats"></param>
        /// <param name="process"></param>
        public void CreateSVG (String folder_path, Boolean show_stats, Process process) {
            // creating graphviz file
            String file_path = folder_path + "\\" + GetHashcode();
            using(StreamWriter writetext = new StreamWriter(file_path + ".dot")) {

                writetext.WriteLine("digraph FlowGraph {");
                writetext.WriteLine("\tnode [shape = \"Box\" fontname=\"Courier New\" rankdir = LR esep=1 colorscheme=ylorrd9];");
                writetext.WriteLine("\tlabel= \"{0}\n{1}\nOptimized Code: {2}\n Total Bytes of Code {3}\n\"", method_name, system, optimized, total_bytes);
                foreach (var pair in adjVertices) {
                    Vertex v = pair.Key;

                    writetext.WriteLine("\t{0} [label =<", v.GetLabel());
                    writetext.WriteLine("\t\t<table border=\"0\" cellspacing=\"10\" bgcolor=\"/purples9/{0}\">", choose_color(v.GetWeight(), maxWeight, false));
                    writetext.WriteLine("\t\t<tr><td align=\"center\" ><b><font POINT-SIZE=\"17\">{0}</font></b></td></tr>", v.GetLabel());
                    if (show_stats) {
                        writetext.WriteLine("\t\t<tr><td align=\"center\"><font  > Weight: {0} Perfscore: {1}</font></td></tr>", v.GetWeight(), v.GetScore());
                        writetext.WriteLine("\t\t<tr><td align=\"center\" >  </td></tr>");
                    }

                    // Get assembly code in group
                    foreach(String code in v.GetContentCode()) {
                        List<String> code_arr = new List<string>();
                        // text wrapping
                        if (code.Length > 50) {
                            int n = 50;
                            for (int i =0; i < code.Length; i=i+50) {
                                if (code.Length - i <= 50) { n = code.Length - i;}
                                code_arr.Add(code.Substring(i,n ).Replace("<", "&lt;").Replace(">", "&gt;"));
                            }
                        } else {
                            code_arr.Add(code.Replace("<", "&lt;").Replace(">", "&gt;"));
                        }

                        if (code.Contains(" IG") && !code.Contains("]")) {
                            writetext.WriteLine("\t\t<tr><td align=\"left\" ><b>{0}</b></td></tr>", code_arr[0]);
                            for (int i=1; i < code_arr.Count; i++) {
                                writetext.WriteLine("\t\t<tr><td align=\"left\" ><b>        {0}</b></td></tr>", code_arr[i]);
                            }
                        } else {
                            writetext.WriteLine("\t\t<tr><td align=\"left\">{0}</td></tr>", code_arr[0]);
                            for (int i=1; i < code_arr.Count; i++) {
                                writetext.WriteLine("\t\t<tr><td align=\"left\">          {0}</td></tr>", code_arr[i]);
                            }
                        }
                    }

                    writetext.WriteLine("\t\t</table>> style=\"solid\" style=filled, fillcolor={0}];", choose_color(v.GetScore(), maxScore, true));
                }

                writetext.WriteLine("\n");

                // Getting cycles
                HashSet<List<Vertex>> cycles = DetectCycle();
                List<String> v_cycles = new List<String>();

                foreach (List<Vertex> v_list in cycles) {
                    v_list.Reverse();
                    for (int i=0; i < v_list.Count - 1; i++) {
                        String s = v_list[i].GetLabel() + " -> " + v_list[i+1].GetLabel();

                        if (!v_cycles.Contains(s)) {
                            writetext.Write("\t{0} -> {1} [color=red];", v_list[i].GetLabel(), v_list[i+1].GetLabel());
                            v_cycles.Add(s);
                        }
                    }
                }

                // connecting edges
                foreach (var pair in adjVertices) {
                    Vertex v = pair.Key;

                    foreach (var vi in pair.Value) {
                        // remove reapeated edge
                        if (!v_cycles.Contains(v.GetLabel() + " -> " + vi.GetLabel())) {
                            if (vi.Equals(v)) {
                                writetext.WriteLine("\t{0} -> {1} [color=red, dir=back];", v.GetLabel(), vi.GetLabel());
                            } else {
                                if (String.Compare(v.GetLabel(), vi.GetLabel()) > 0 ) {
                                    writetext.WriteLine("\t{0} -> {1} [color=green];", v.GetLabel(), vi.GetLabel());    
                                }
                                else {
                                    writetext.WriteLine("\t{0} -> {1} [color=blue];", v.GetLabel(), vi.GetLabel());
                                }
                            }
                        }
                    }
                }
                
                // creating invisible connections to keep tables in order
                List<Vertex> vertices = GetVertices();
                int t_vertices = vertices.Count;
                
                String edge = "\tsubgraph cluster_1 { color=transparent rank=same " + vertices[0].GetLabel() +" -> ";
                for (int i =1; i < t_vertices - 1; i++) {
                    edge = edge + (vertices[i].GetLabel() + " -> ");
                }

                writetext.WriteLine("\n\tedge[style=invis];");
                writetext.WriteLine(edge + vertices[t_vertices - 1].GetLabel() + "};");      
                writetext.WriteLine( "}");   

                Console.WriteLine("Hash: {0}",hashcode);
            }
        }
    }
}
