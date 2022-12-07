namespace AssemblyGraph;

/// <summary>
/// vertex class which represents a group in a method
/// </summary>
class Vertex {
    /// <summary>
    /// name of the of the group e.g. IG01, IG02...
    /// </summary>
    private String label;
    /// <summary>
    /// the lines of assembly code in the group
    /// </summary>
    private List<String> content_code;
    /// <summary>
    /// group weight
    /// </summary>
    private Double weight; 
    /// <summary>
    /// group perfScore
    /// </summary>
    private Double score;

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
    /// <param name="v1"> vertex the current vertx is being compared to </param>
    /// <returns></returns>
    public Boolean Equals (Vertex v1){
        return v1.label == label;
    }

    public int Compare(Vertex v2) {
        return label.CompareTo(v2.GetLabel());
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
    /// <summary>
    /// adjacency list representing method
    /// </summary>
    private Dictionary<Vertex, List<Vertex>> adjVertices;
    /// <summary>
    /// hashcode of method if exist
    /// </summary>
    private String hashcode = "";
    /// <summary>
    /// max weight found in entire method
    /// </summary>
    private Double maxWeight = 0;
    /// <summary>
    /// max perfScore found in entire method
    /// </summary>
    private Double maxScore = 0;
    /// <summary>
    /// total bytes of code in the method
    /// </summary>
    private int total_bytes = 0;
    /// <summary>
    /// Assembly system, X64 or ARM64
    /// </summary>
    private String system = "";
    /// <summary>
    /// Whether method is optimized
    /// </summary>
    private Boolean optimized = false;
    /// <summary>
    /// name of the method
    /// </summary>
    private String method_name = "";

    public Graph () {
        this.adjVertices = new Dictionary<Vertex, List<Vertex>>();
    }
    
    public void SetMethodName (String name) {
        this.method_name = name;
    }


    public void print() {
        foreach(Vertex v in GetVertices()) {
            Console.Write("\nVertex: "+v.GetLabel() +" - ");
            foreach (Vertex v1 in adjVertices[v]) {
                Console.Write(" " + v1.GetLabel());
            }

        }
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
    /// <param name="v"> vertex </param>
    public void AddVertex (Vertex v) {
        adjVertices.TryAdd(v, new List<Vertex>());
    }

    /// <summary>
    /// adds an edge v1->v2 to the graph
    /// </summary>
    /// <param name="v1"> vertex 1 </param>
    /// <param name="v2"> vertex 2 </param>
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
    /// finds vertex given label
    /// </summary>
    /// <param name="label"></param>
    /// <returns> vertex if exist, throws InvalidDataException exception if the label does not exist </returns>
    public Vertex GetVertex (String label) {
        List<Vertex> vertices = GetVertices();

        foreach (Vertex v in vertices) {
            if (v.GetLabel() == label) {
                return v;
            }
        }    

        throw new InvalidDataException("This vertex does not exist");
    }

    /// <summary>
    /// using the perfScore of the group, it calculates a ratio using the max perfScore
    /// </summary>
    /// <param name="score"></param>
    /// <returns> a number representing the heatmap intensity or the color of the border of the group </returns>
    private string choose_heat_color(Double score) {
        if(maxScore == 0) {return "white";}
        Double ratio = Math.Round(score / maxScore, 1 );

        if (ratio < 0.2) {; return "1";}
        else if (ratio >= 0.2 && ratio < 0.3) {return "2";}
        else if (ratio >= 0.3 && ratio < 0.4) {return "3";}
        else if (ratio >= 0.4 && ratio < 0.5) {return "4";}
        else if (ratio >= 0.5 && ratio < 0.6) {return "5";}
        else if (ratio >= 0.6 && ratio < 0.7) {return "6";}
        else if (ratio >= 0.7 && ratio < 0.8) {return "7";}
        else if (ratio >= 0.8 && ratio < 0.9) {return "8";}
        else  {return "9";}
    }

    /// <summary>
    /// using the weight of the group, it calculates a ratio using the max weight of the overall method
    /// </summary>
    /// <param name="weight"></param>
    /// <returns> a number representing the color of the group </returns>
    private string choose_group_color(Double weight) {
        if(maxWeight == 0) {return "1";}

        Double ratio = Math.Round(weight / maxWeight, 1 );
        if (weight <= 1 || ratio < 0.2) {return "1";}
        else {
            if (weight <= 1.5 || (ratio >= 0.2 && ratio < 0.4)) {return "2";}
            else if (ratio >= 0.4 && ratio < 0.6) {return "3";}
            else if (ratio >= 0.6 && ratio < 0.8) {return "4";}
            else  {return "5";}
        }
    }

    /// <summary>
    /// DFS implementaton to detect and return all cycles found along a path
    /// </summary>
    /// <param name="v"> current vertex </param>
    /// <param name="visited"> stack of all the visited vertices </param>
    /// <param name="explore"> stack of all the fully explored vertices</param>
    /// <param name="cycles"> cycles found </param>
    /// <returns> true if cycle found, false otherwise </returns>
    private bool DFS (Vertex v, ref Stack<Vertex> visited, ref Stack<Vertex> explore, ref HashSet<List<Vertex>> cycles) {
        // Console.Write(" " +v.GetLabel()+ " ");
        if (explore.Contains(v)) { // found a cycle
            // Console.WriteLine("\nCYCLE: "+ v.GetLabel());
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
            // visited.Push(v);
            cycles.Add(cycle);
            return true;
        }

        explore.Push(v);
        foreach (Vertex v1 in adjVertices[v]) {
            // if(v.GetLabel() == "IG06") {Console.WriteLine("IN IG6: "+ v1.GetLabel());}
            if (!visited.Contains(v1)) {
                Boolean cycle = DFS(v1, ref visited, ref explore, ref cycles);
                if (!cycle) {
                    while (!explore.Peek().Equals(v1)) {
                        // Console.WriteLine("POP: "+ explore.Peek().GetLabel());
                        explore.Pop();
                    }
                    // Console.WriteLine("POP: "+ explore.Peek().GetLabel());
                    explore.Pop();
                } 
            }
            
        }

        visited.Push(v);
        return false; // no cycle found
    }

    /// <summary>
    /// detects the cycles found in the graph
    /// </summary>
    /// <returns> all the cycles found </returns>
    public HashSet<List<Vertex>> DetectCycle() {
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
    /// Wraps text if line of code is more than 50 characters long
    /// </summary>
    /// <param name="code"> the line of code </param>
    /// <param name="code_arr"> list of final lines of code </param>
    private void TexWrap(String code, ref List<String> code_arr) {
        if (code.Length > 50) {
            int n = 50;
            for (int i =0; i < code.Length; i=i+50) {
                if (code.Length - i <= 50) { n = code.Length - i;}
                code_arr.Add(code.Substring(i,n ).Replace("<", "&lt;").Replace(">", "&gt;"));
            }
        } else {
            code_arr.Add(code.Replace("<", "&lt;").Replace(">", "&gt;"));
        }
    }
    
    /// <summary>
    /// Adds all the code within the vertex/node/group to the file
    /// </summary>
    /// <param name="v"> current vertex </param>
    /// <param name="file_content"> output file content </param>
    private void AddVertexContentToFile(Vertex v, ref List<String> file_content) {
        foreach(String code in v.GetContentCode()) {
            List<String> code_arr = new List<string>();

            TexWrap(code, ref code_arr);

            if (code.Contains(" IG") && !code.Contains("]")) {
                file_content.Add("\t\t<tr><td align=\"left\" ><b>"+code_arr[0]+"</b></td></tr>");
                for (int i=1; i < code_arr.Count; i++) {
                    file_content.Add("\t\t<tr><td align=\"left\" ><b>        "+code_arr[i]+"</b></td></tr>");
                }
            } else {
                file_content.Add("\t\t<tr><td align=\"left\">"+code_arr[0]+"</td></tr>");
                for (int i=1; i < code_arr.Count; i++) {
                    file_content.Add("\t\t<tr><td align=\"left\">          "+ code_arr[i]+"</td></tr>");
                }
            }
        }
    }

    /// <summary>
    /// Finds all cycles and adds them to file content
    /// with the color red representing cycles
    /// </summary>
    /// <param name="v_cycles"> all cycle edges </param>
    /// <param name="file_content"> output file content </param>
    private void AddCyclesToFile(ref List<String> v_cycles, ref List<String> file_content) {
        HashSet<List<Vertex>> cycles = DetectCycle();

        foreach (List<Vertex> v_list in cycles) {
            v_list.Reverse();
            for (int i=0; i < v_list.Count - 1; i++) {
                String s = v_list[i].GetLabel() + " -> " + v_list[i+1].GetLabel();

                if (!v_cycles.Contains(s)) {
                    file_content.Add("\t"+v_list[i].GetLabel()+" -> "+v_list[i+1].GetLabel()+" [color=red];");
                    v_cycles.Add(s);
                }
            }
        }
    }

    /// <summary>
    /// writes all connected edges to the file content and sets color
    /// red   - cycles, green - backward edges, blue  - forward edges
    /// </summary>
    /// <param name="v_cycles"> all cycle edges </param>
    /// <param name="file_content"> output file content </param>
    private void AddEdgeConnectionsToFile(ref List<String> v_cycles, ref List<String> file_content) {
        foreach (var pair in adjVertices) {
            Vertex v = pair.Key;

            foreach (var vi in pair.Value) {
                // remove repeated edge
                if (!v_cycles.Contains(v.GetLabel() + " -> " + vi.GetLabel())) {
                    if (vi.Equals(v)) {
                        file_content.Add("\t"+v.GetLabel()+" -> "+vi.GetLabel()+" [color=red, dir=back];");
                    } else {
                        if (String.Compare(v.GetLabel(), vi.GetLabel()) > 0 ) {
                            file_content.Add("\t"+v.GetLabel()+" -> "+vi.GetLabel()+" [color=green];");    
                        }
                        else {
                            file_content.Add("\t"+v.GetLabel()+" -> "+vi.GetLabel()+" [color=blue];");
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// creates invisible edges from one vertex to next to keep graph align
    /// </summary>
    /// <param name="file_content"> output file content </param>
    private void AddInvisibleEdges(ref List<String> file_content) {
        List<Vertex> vertices = GetVertices();
        int t_vertices = vertices.Count;
        
        String edge = "\tsubgraph cluster_1 { color=transparent rank=same " + vertices[0].GetLabel() +" -> ";
        for (int i =1; i < t_vertices - 1; i++) {
            edge = edge + (vertices[i].GetLabel() + " -> ");
        }

        file_content.Add("\n\tedge[style=invis];");
        file_content.Add(edge + vertices[t_vertices - 1].GetLabel() + "};");  
    }

    /// <summary>
    /// generate contents of dot file
    /// </summary>
    /// <param name="show_weight_score"> whether to display perfScore and Weight on each node  </param>
    /// <returns> generated file content </returns>
    public List<String> GenerateDotFileContent (Boolean show_weight_score) {
        // creating graphviz file
        List<String> file_content = new List<String>();

        file_content.Add("digraph FlowGraph {");
        file_content.Add("\tnode [shape = \"Box\" fontname=\"Courier New\" rankdir = LR esep=1 colorscheme=ylorrd9];");
        file_content.Add("\tlabel= \""+method_name+"\n"+system+"\nOptimized Code: "+optimized+"\n Total Bytes of Code "+total_bytes+"\n\"");
        foreach (var pair in adjVertices) {
            Vertex v = pair.Key;

            file_content.Add("\t"+v.GetLabel()+" [label =<");
            file_content.Add("\t\t<table border=\"0\" cellspacing=\"10\" bgcolor=\"/purples9/"+choose_group_color(v.GetWeight())+"\">");
            file_content.Add("\t\t<tr><td align=\"center\" ><b><font POINT-SIZE=\"17\">"+v.GetLabel()+"</font></b></td></tr>");
            if (show_weight_score) {
                file_content.Add("\t\t<tr><td align=\"center\"><font  > Weight: "+v.GetWeight()+" Perfscore: "+v.GetScore()+"</font></td></tr>");
                file_content.Add("\t\t<tr><td align=\"center\" >  </td></tr>");
            }

            AddVertexContentToFile(v, ref file_content); // add code in group to file content
            file_content.Add("\t\t</table>> style=\"solid\" style=filled, fillcolor="+choose_heat_color(v.GetScore())+"];");
        }

        file_content.Add("\n");
        List<String> v_cycles = new List<String>();

        AddCyclesToFile(ref v_cycles, ref file_content); // get cycles and add them to the file content
        AddEdgeConnectionsToFile(ref v_cycles, ref file_content); // add all the edges to the file content
        AddInvisibleEdges(ref file_content);  // created and add invisible connections to keep tables in order

        file_content.Add( "}");   
        return file_content;
    }

    /// <summary>
    /// creates dot file
    /// </summary>
    /// <param name="folder_path"> path to output folder </param>
    /// <param name="show_weight_score">whether to display perfScore and Weight on each node</param>
    public void CreateDotFile (String folder_path, Boolean show_weight_score) {
        List<String> SVG_content = GenerateDotFileContent(show_weight_score); // get dot file content

        // creating graphviz file
        StreamWriter writetext = new StreamWriter(folder_path);
        foreach (String s in SVG_content) {writetext.WriteLine(s);}
        writetext.Close();
    }
}
