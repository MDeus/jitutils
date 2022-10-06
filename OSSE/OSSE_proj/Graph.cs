using System.Diagnostics;
using System.ComponentModel;

namespace Graphviz {

    class Vertex {
        private String label;
        private List<String> content_code;
        private Double weight;
        private Double score;

        public Vertex (String label, Double weight, Double score, List<String> content) {
            this.label = label;
            this.content_code = content;
            this.weight = weight;
            this.score = score;
        }

        public Boolean Equals (Vertex v1){
            return v1.label == label;
        }

        public void setContentCode (List<String> content) {
            this.content_code = content;
        }

        public List<String> getContentCode () {
            return content_code;
        }

        public void setWeight (int weight) {
            this.weight = weight;
        }

        public Double getWeight () {
            return weight;
        }

        public void setScore (int score) {
            this.score = score;
        }

        public Double getScore () {
            return score;
        }
        
        
        public String getLabel () {
            return label;
        }
    }

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
        
        public void setMethodName (String name) {
            this.method_name = name;
        }

        public String getMethodName () {
            return method_name;
        }
        public void setOptimized (Boolean o) {
            this.optimized = o;
        }

        public Boolean getOptimized () {
            return optimized;
        }
        public void setHashcode (String h) {
            this.hashcode = h;
        }

        public String getHashcode () {
            return hashcode;
        }

        public void setTotalBytes (int b) {
            this.total_bytes = b;
        }

        public int getTotalBytes () {
            return total_bytes;
        }

        public void setSystem (String s) {
            this.system = s;
        }

        public void AddVertex (Vertex v) {
            adjVertices.TryAdd(v, new List<Vertex>());
        }

        public void AddEdge (Vertex v1, Vertex v2) {
            adjVertices[v1].Add(v2);
        }

        public void setMaxWeight (Double weight) {
            this.maxWeight = weight;
        }

        public Double getMaxWeight () {
            return maxWeight;
        }

        public void setMaxScore (Double score) {
            this.maxScore = score;
        }

        public Double getMaxScore () {
            return maxScore;
        }

        public List<Vertex> getVertices() {
            return adjVertices.Keys.ToList();
        }

        public Vertex getVertex (String label) {
            List<Vertex> vertices = getVertices();

            foreach (Vertex v in vertices) {
                if (v.getLabel() == label) {
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
                if (score >= 0.2 && score < 0.4) {return "2";}
                else if (score >= 0.4 && score < 0.6) {return "3";}
                else if (score >= 0.6 && score < 0.8) {return "4";}
                else  {return "5";}
            }
            
        }

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
            return false;
        }

        private HashSet<List<Vertex>> detectCycle() {
            Stack<Vertex> visited = new Stack<Vertex>();
            Stack<Vertex> explore = new Stack<Vertex>();
            HashSet<List<Vertex>> cycles = new HashSet<List<Vertex>>();

            foreach (Vertex v in getVertices()) {
                if (!visited.Contains(v)) {
                    DFS(v, ref visited, ref explore, ref cycles);
                }
            }

            return cycles;
        }

        public void print (String file_name, Boolean show_stats) {
            using(StreamWriter writetext = new StreamWriter(file_name)) {

                writetext.WriteLine("digraph FlowGraph {");
                writetext.WriteLine("\tnode [shape = \"Box\" fontname=\"Courier New\" rankdir = LR esep=1 colorscheme=purples9];");
                writetext.WriteLine("\tlabel= \"{0}\n{1}\nOptimized Code: {2}\n Total Bytes of Code {3}\n\"", method_name, system, optimized, total_bytes);
                foreach (var pair in adjVertices) {
                    Vertex v = pair.Key;

                    int count = v.getContentCode().Count + 30;
                    if(show_stats) {count = v.getContentCode().Count + 34;}

                    writetext.WriteLine("\t{0} [label =<", v.getLabel());
                    writetext.WriteLine("\t\t<table border=\"0\" cellborder=\"0\" cellspacing=\"1\" style=\"rounded\">");
                    writetext.WriteLine("\t\t<tr><td ROWSPAN=\"{0}\" bgcolor=\"/ylorrd9/{1}\" > </td></tr>", count, choose_color(v.getScore(), maxScore, true));
                    writetext.WriteLine("\t\t<tr><td align=\"center\" ><b><font POINT-SIZE=\"17\">{0}</font></b></td></tr>", v.getLabel());
                    if (show_stats) {
                        writetext.WriteLine("\t\t<tr><td align=\"center\"><font color=\"/blue/\" > Weight: {0} Perfscore: {1}</font></td></tr>", v.getWeight(), v.getScore());
                        writetext.WriteLine("\t\t<tr><td align=\"center\" >  </td></tr>");
                    }

                    foreach(String code in v.getContentCode()) {
                        List<String> code_arr = new List<string>();

                        if (code.Length > 50) {
                            int n = 50;
                            for (int i =0; i < code.Length; i=i+50) {
                                if (code.Length - i <= 50) { n = code.Length - i;}
                                code_arr.Add(code.Substring(i,n ));
                            }
                        } else {
                            code_arr.Add(code);
                        }

                        if (code.Contains(" IG") && !code.Contains("]")) {
                            writetext.WriteLine("\t\t<tr><td align=\"left\" color=\"blue\"><b>{0}</b></td></tr>", code_arr[0]);
                            for (int i=1; i < code_arr.Count; i++) {
                                writetext.WriteLine("\t\t<tr><td align=\"left\" color=\"blue\"><b>        {0}</b></td></tr>", code_arr[i]);
                            }
                        } else {
                            writetext.WriteLine("\t\t<tr><td align=\"left\">{0}</td></tr>", code_arr[0]);
                            for (int i=1; i < code_arr.Count; i++) {
                                writetext.WriteLine("\t\t<tr><td align=\"left\">          {0}</td></tr>", code_arr[i]);
                            }
                        }
                    }

                    writetext.WriteLine("\t\t</table>> style=\"solid\" style=filled, fillcolor={0}];", choose_color(v.getWeight(), maxWeight, false));
                }

                writetext.WriteLine("\n");

                HashSet<List<Vertex>> cycles = detectCycle();
                List<String> v_cycles = new List<String>();

                foreach (List<Vertex> v_list in cycles) {
                    v_list.Reverse();
                    for (int i=0; i < v_list.Count - 1; i++) {
                        String s = v_list[i].getLabel() + " -> " + v_list[i+1].getLabel();

                        if (!v_cycles.Contains(s)) {
                            writetext.Write("\t{0} -> {1} [color=red];", v_list[i].getLabel(), v_list[i+1].getLabel());
                            v_cycles.Add(s);
                        }
                    }
                }


                foreach (var pair in adjVertices) {
                    Vertex v = pair.Key;

                    foreach (var vi in pair.Value) {
                        if (!v_cycles.Contains(v.getLabel() + " -> " + vi.getLabel())) {
                            if (vi.Equals(v)) {
                                writetext.WriteLine("\t{0} -> {1} [color=red, dir=back];", v.getLabel(), vi.getLabel());
                            } else {
                                if (String.Compare(v.getLabel(), vi.getLabel()) > 0 ) {
                                    writetext.WriteLine("\t{0} -> {1} [color=green];", v.getLabel(), vi.getLabel());    
                                }
                                else {
                                    writetext.WriteLine("\t{0} -> {1} [color=blue];", v.getLabel(), vi.getLabel());
                                }
                            }
                        }
                    }
                }
                
                List<Vertex> vertices = getVertices();
                int t_vertices = vertices.Count;
                
                String edge = "\tsubgraph cluster_1 { color=transparent rank=same " + vertices[0].getLabel() +" -> ";
                for (int i =1; i < t_vertices - 1; i++) {
                    edge = edge + (vertices[i].getLabel() + " -> ");
                }

                writetext.WriteLine("\n\tedge[style=invis];");
                writetext.WriteLine(edge + vertices[t_vertices - 1].getLabel() + "};");      
                writetext.WriteLine( "}");   

                Console.WriteLine("Hash: {0}",hashcode);

            }

            // String name = "dot -Tps filename.dot -o " + hashcode +".png";
            // // Process pros = new Process();
            // Process.Start(name); 

            // ProcessStartInfo ProcessInfo;
            // Process Process;

            // ProcessInfo = new ProcessStartInfo("cmd.exe", "/K " + Command);
            // ProcessInfo.CreateNoWindow = true;
            // ProcessInfo.UseShellExecute = true;

            // Process = Process.Start(ProcessInfo);   
        }
    }
}