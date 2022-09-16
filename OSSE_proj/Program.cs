
class Program {

    public static Graph addEdges(Graph graph, Dictionary<String, List<String>> edges) {
        foreach (var pair in edges) {
            String label = pair.Key;
            Vertex v1;
            try {
                v1 = graph.getVertex(label);

                foreach (var v in pair.Value) {
                    Vertex v2;
                    
                    try {
                        v2 = graph.getVertex(v);
                        graph.AddEdge(v1, v2);

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

    static public void Main(String[] args) {
        // Read a text file line by line.  
        if (!File.Exists(args[0])) {
            throw new InvalidDataException ("ERROR: Incorrect file path " + args[0]);
        }

        string[] lines = File.ReadAllLines(args[0]);  

        int j = -1;
        String current_node = ""; // holds the current node
        var temp_conn = new HashSet<String>(); // holds all the nodes or edges that are connected to the current node
        var temp_cont = new List<String>(); // holds code block inside the current node
        Dictionary<String, List<String>> arrows = new Dictionary<String, List<String>>(); // dict of connected nodes and their edges
        List<Graph> graph_list = new List<Graph>();
        Graph temp_graph = new Graph();
        double max_w = 0.0;
        double max_s = 0.0;
        double temp_weight = 0;
        double temp_score = 0;
        int g_count = 0;

        Boolean connect = true;
        Boolean keep_bar_code = false;

        if (args.Length > 1) {
            try {
                keep_bar_code = Convert.ToBoolean(args[1]);
            } catch (FormatException) {
                Console.WriteLine ("ERROR: Incorrect format for boolean");
            }
        }


        // iterate over the lines of the file
        for (int i = 0; i < lines.Length; i++) {
            if (g_count == 5) {break;}
            // remove leading spaces
            String line = lines[i].Trim();
            String[] arr = line.Split(null, 2);
            String first_word = line.Split(null, 2)[0];

            if (line.CompareTo("") == 0 || (line[0].CompareTo(';') == 0 && !line.Contains("Weight"))) {continue;}

            // get the line excluding the "bar code" if it exists
            if (!keep_bar_code) {
                if (arr.Length > 1) {
                    if (!Char.IsLower(arr[0][0])) {
                        line = arr[1].Trim();
                    }

                    first_word = line.Split(null, 2)[0];
                } 
            } else {
                if (arr.Length > 1) {
                    if (!Char.IsLower(arr[0][0])) {
                        first_word = arr[1].Trim().Split(null, 2)[0];
                    }
                }
            }

            if (first_word.CompareTo("ret") == 0 || first_word.CompareTo("b") == 0 ||  first_word.CompareTo("jmp") == 0) {
                connect = false;
            }
            
            // check that this is the start of a group
            if (first_word.Contains("G_M")) {

                String node = "IG" + first_word.Split("_IG")[1];
                node = node.Substring(0, node.Length - 1);

                if (node.CompareTo("IG01") == 0) {
                    
                    if (j >= 0) {

                        Vertex v = new Vertex(current_node, temp_weight, temp_score, temp_cont);
                        temp_graph.AddVertex(v);
                        temp_graph.setMaxWeight(max_w);
                        temp_graph.setMaxScore(max_s);
                        graph_list.Add(addEdges(temp_graph, arrows));

                        arrows = new Dictionary<String, List<String>>();
                        temp_graph = new Graph();
                        temp_conn = new HashSet<String>(); 
                        temp_cont = new List<String>(); 
                        j = -1;
                        max_w = 0; max_s = 0;

                        g_count++;
                    }

                }

                if (j >= 0) {

                    // add the nodes connection nodes to dict
                    if (connect){
                        temp_conn.Add(node);
                    }

                    Vertex v = new Vertex(current_node, temp_weight, temp_score, temp_cont);
                    temp_graph.AddVertex(v);
                    
                    // keep track of edges
                    arrows.Add(current_node, temp_conn.ToList());

                    temp_conn = new HashSet<String>(); 
                    temp_cont = new List<String>(); 
                    temp_weight = 0; temp_score = 0;

                }

                j++;
                current_node = node;
                connect = true;
            }
            // get nodes connected to current node
            else if (line.Contains("G_M")){ // check that its a jump condition
                String[] line_arr = line.Split("G_M");
                String temp_node = "IG" + line_arr[1].Split("_IG")[1];
        
                temp_conn.Add(temp_node);
                temp_cont.Add(line_arr[0] + temp_node);
            }
            // get the weight and score of current node
            else if (line.Contains("bbWeight=")) { // get the group stats

                temp_weight = Convert.ToDouble(line.Split("bbWeight=")[1].Split(" ")[0]);
                temp_score = Convert.ToDouble(line.Split("PerfScore ")[1].Split(" ")[0]);

                if (temp_weight > max_w) {max_w = temp_weight;}
                if (temp_score > max_s) {max_s = temp_score;}

            }
            // add everything else to content dict
            else { 
                temp_cont.Add(line);
            }

            if (lines.Length == i + 1) {
                Vertex v = new Vertex(current_node, temp_weight, temp_score, temp_cont);
                temp_graph.AddVertex(v);

                temp_graph.setMaxWeight(max_w);
                temp_graph.setMaxScore(max_s);

                graph_list.Add(addEdges(temp_graph, arrows));
            }
        }

        // print graphviz file
        int z = 0;
        foreach (var g in graph_list) {
            Console.WriteLine(z);
            g.print("graph"+z+".dot");
            z++;
        }
    }
}
