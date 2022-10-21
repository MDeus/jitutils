using Graphviz;
using System.Diagnostics;

class Program {

    public static Graph addEdges(Graph graph, Dictionary<String, List<String>> edges) {
        foreach (var pair in edges) {
            String label = pair.Key;

            try {
                Vertex v1 = graph.getVertex(label);
                foreach (var v in pair.Value) {
                    try {
                        graph.AddEdge(v1, graph.getVertex(v));
                    } catch(InvalidDataException) {
                        Console.WriteLine("ERROR: The vertext with label {0} does not exist", v);
                    }
                }   
            } catch(InvalidDataException) {
                Console.WriteLine("ERROR: The vertext with label {0} does not exist", label);
            }
        }
        return graph;
    }

    public static void caseWithSemicolon(ref Graph temp_graph, String line, ref String current_node, 
                                    ref Double temp_weight, ref Double temp_score, ref HashSet<String> temp_conn,
                                    ref List<String> temp_cont, ref List<Graph> graph_list,
                                    ref Dictionary<String, List<String>> edges, ref Boolean in_group, ref int j) {
        
        if (line.Contains("MethodHash=")) { 
            String [] arr = line.Split("MethodHash=", 2);

            temp_graph.setHashcode(arr[1].Split(')')[0]); // get hashcode
            temp_graph.setTotalBytes(Convert.ToInt32(arr[0].Split("code")[1].Trim().Split(',')[0])); // get total bytes
            temp_graph.setMethodName(arr[1].Split("for method ")[1].Trim()); // get method name
        }
        else if (line.Contains("ARM64") || line.Contains("X64")) { // get system type
            temp_graph.setSystem(line.Split("for")[1].Trim());
        }
        else if (line.CompareTo("; optimized code") == 0) { // get if optimized
            temp_graph.setOptimized(true);
        }
        else if (line.Contains("bbWeight")) { // get the weight and score

            temp_weight = Convert.ToDouble(line.Split("bbWeight=")[1].Split(" ")[0]);
            temp_score  = Convert.ToDouble(line.Split("PerfScore ")[1].Split(" ")[0]);

            if (temp_weight > temp_graph.getMaxWeight()) {temp_graph.setMaxWeight(temp_weight);}
            if (temp_score  > temp_graph.getMaxScore()) {temp_graph.setMaxScore(temp_score);}
        }
        else if ((line.Contains("; ==") || line.Contains("Assembly listing for method")) && in_group == true) { // if end of method add graph to list

            temp_graph.AddVertex(new Vertex(current_node, temp_weight, temp_score, temp_cont));
            graph_list.Add(addEdges(temp_graph, edges));

            edges      = new Dictionary<String, List<String>>();
            temp_graph = new Graph();
            temp_conn  = new HashSet<String>(); 
            temp_cont  = new List<String>(); 
            in_group   = false; j = 0; 
        }
    }

    public static void caseWithGroups(String first_word, String line, ref Boolean connectToNextNode, ref String current_node, 
                                    ref Graph temp_graph, ref Double temp_weight, ref Double temp_score, ref HashSet<String> temp_conn,
                                    ref List<String> temp_cont, ref Dictionary<String, List<String>> edges, ref int j) {
        
        if (first_word.Contains("G_M")) { // start of new group
            String new_node = "IG" + first_word.Split("_IG")[1];
            new_node = new_node.Substring(0, new_node.Length - 1);

            if (j > 0) {
                if (connectToNextNode){ temp_conn.Add(new_node); }
                
                temp_graph.AddVertex(new Vertex(current_node, temp_weight, temp_score, temp_cont));
                edges.Add(current_node, temp_conn.ToList());
                temp_conn = new HashSet<String>(); 
                temp_cont = new List<String>(); 
                temp_weight = 0; temp_score = 0;
            }

            j++;
            current_node = new_node;
            connectToNextNode = true;
        }
        else { // not group start but contains jump to group
            String[] line_arr = line.Split("G_M");
            String temp_node = "IG" + line_arr[1].Split("_IG")[1];
            temp_conn.Add(temp_node);
            temp_cont.Add(line_arr[0] + temp_node);
        }
}

    public static List<Graph> createGraphs(String[] lines, Boolean keep_hash_code) {
        int j = 0; 
        String current_node = ""; // holds the current group/node
        HashSet<String> temp_conn = new HashSet<String>(); // holds all the nodes or edges that are connected to the current node
        List<String>    temp_cont = new List<String>(); // holds code block inside the current group
        List<Graph>    graph_list = new List<Graph>(); 
        Graph          temp_graph = new Graph(); // current graph
        Double        temp_weight = 0; 
        Double         temp_score = 0;
        Boolean connectToNextNode = true;
        Boolean          in_group = false;
        Dictionary<String, List<String>> edges = new Dictionary<String, List<String>>(); // dict of connected nodes and their edges

        // iterate over the lines of the file
        for (int i = 0; i < lines.Length; i++) {
            String line = lines[i].Trim(); // remove leading spaces

            if (line.CompareTo("") == 0) {continue;}

            if (line[0].CompareTo(';') == 0) {
                // get method info
                caseWithSemicolon(ref temp_graph, line, ref current_node, ref temp_weight, ref temp_score, ref temp_conn,
                                ref temp_cont, ref graph_list, ref edges, ref in_group, ref j); 
                continue;
            }
            
            String first_word = line.Split(null, 2)[0];
            
            // get the first word excluding the "hash code" if it exists
            if (!char.IsLower(first_word[0]) && !first_word.Contains("G_M")) {
                String new_line = line.Split(null, 2)[1].Trim();
                first_word = new_line.Split(null, 2)[0];
                if (!keep_hash_code) {line = new_line;}
            } 

            // unconditional jump or ret so dont connect
            if (first_word.CompareTo("ret") == 0 || first_word.CompareTo("b") == 0 
                || first_word.CompareTo("jmp") == 0 || (line.Contains("HELP_THROW")) 
                || line.Contains("Throw") || line.Contains("HELP_OVERFLOW")) {
                connectToNextNode = false;
            }

            // check for group
            if (line.Contains("G_M")){ 
                in_group = true;
                caseWithGroups(first_word, line, ref connectToNextNode, ref current_node, ref temp_graph, ref temp_weight, 
                                ref temp_score, ref temp_conn, ref temp_cont, ref edges, ref j);
            }
            else { // add everything else to content dict
                if (in_group) { temp_cont.Add(line); }
            }
        }

        return graph_list;
    }

    static public void printGraphs(List<Graph> graph_list, String output_folder_path, Boolean stats) {
        Process process = new Process();
        // specify dot.exe path
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        foreach (Graph g in graph_list) {
            g.createSVG(output_folder_path, stats, process);
        }

        process.StandardInput.Close();
        process.WaitForExit();

        // File.Delete(output_folder_path + "\\*.dot" );
    }

    static public void Main(String[] args) {
        if (args.Length != 4) {
            throw new ArgumentException ("ERROR: Missing arguments");
        }

        // check input file path 
        if (!File.Exists(args[0])) {
            throw new ArgumentException ("ERROR: Incorrect input file path " + args[0]);
        }

        // check output file path
        String output_folder_path = "";
        try {
            output_folder_path = Path.GetFullPath(args[2]);
        } catch(Exception) {
            Console.WriteLine("ERROR: Incorrect output folder path " + args[2]);
        }

        // check hash code parameter is boolean
        Boolean keep_hash_code = false;
        if (args.Length > 1) {
            try {
                keep_hash_code = Convert.ToBoolean(args[1]);
            } catch (FormatException) {
                Console.WriteLine ("ERROR: Incorrect format for boolean");
            }
        }

        // check hash code parameter is boolean
        Boolean stats = false;
        if (args.Length > 1) {
            try {
                stats = Convert.ToBoolean(args[3]);
            } catch (FormatException) {
                Console.WriteLine ("ERROR: Incorrect format for boolean");
            }
        }

        String[] lines = File.ReadAllLines(args[0]); 
        printGraphs(createGraphs(lines, keep_hash_code), output_folder_path, stats);
    }
}
