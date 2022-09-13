
class Program {
    static public void Main(String[] args) {
        // Read a text file line by line.  
        string[] lines = File.ReadAllLines("assembly2.txt");  

        int j = -1;
        String current_node = ""; // holds the current node
        var nodes     = new List<String>(); 
        var temp_conn = new HashSet<String>(); // holds all the nodes or edges that are connected to the current node
        var temp_cont = new List<String>(); // holds code block inside the current node
        Dictionary<String, List<String>> arrows = new Dictionary<String, List<String>>(); // dict of connected nodes and their edges
        Dictionary<String, List<String>> node_content = new Dictionary<String, List<String>>(); // dict of nodes and the code block inside
        Dictionary<String, String> weight_dict = new Dictionary<String, String>(); // dict of nodes and weight
        Dictionary<String, String> score_dict = new Dictionary<String, String>(); // dict of nodes and score
        Dictionary<String, String> size_dict = new Dictionary<String, String>(); // dict of nodes and size

        // iterate over the lines of the file
        for (int i = 0; i < lines.Length; i++) {
            // remove leading spaces
            String line = lines[i].Trim();
            String[] arr = line.Split(null, 2);

            // get the line excluding the "bar code" if it exists
            if (arr.Length > 1) {
                if (!Char.IsLower(arr[0][0])) {
                    line = arr[1].Trim();
                } 
            } 
            else {line = arr[0];}

            // get the first word
            String first_word = line.Split(null, 2)[0];
            
            // check that this is the start of a group
            if (first_word.Contains("G_M")) {
                // add new node to list
                String node = "IG" + first_word.Split("_IG")[1];
                node = node.Substring(0, node.Length - 1);
                nodes.Add(node);

                if (j >= 0) {
                
                    // add the nodes connection nodes to dict
                    temp_conn.Add(node);
                    arrows.Add(current_node, temp_conn.ToList());
                    temp_conn = new HashSet<String>(); 

                    // add the nodes content code to dict
                    node_content.Add(current_node, temp_cont);
                    temp_cont = new List<String>(); 

                }

                j++;
                current_node = node;
            }
            // get nodes connected to current node
            // else if (first_word[0] == 'j' || first_word.Contains("cb") || first_word == "b"||
            //         (first_word[0] == 'b' && (first_word != "bl" && first_word != "blr" && first_word != "bkpt" && !first_word.Contains("bbWeight")))) {
            else if (first_word[0] == 'j' || line.Contains("G_M")){ // check that its a jump condition

                temp_conn.Add("IG" + line.Split("_IG")[1]);
                temp_cont.Add(lines[i].Trim());
            }
            // get the weight and score of current node
            else if (line.Contains("bbWeight=")) { // get the group stats

                String weight = line.Split("bbWeight=")[1].Split(" ")[0];
                String score = line.Split("PerfScore ")[1].Split(" ")[0];
                String size = "0";
                if (line.Contains("size=")) {
                    size = line.Split("size=")[1].Split(" ")[0];
                }

                weight_dict.Add(current_node, weight);
                score_dict.Add(current_node, score);
                size_dict.Add(current_node, size);
            }
            // add everything else to content dict
            else { 

                temp_cont.Add(lines[i].Trim());
            }

            if (lines.Length == i + 1) {

                // add the nodes connection nodes to dict
                arrows.Add(current_node, temp_conn.ToList());
                temp_conn = new HashSet<String>(); 

                // add the nodes content code to dict
                node_content.Add(current_node, temp_cont);
                temp_cont = new List<String>(); 

            }
        }

        // create graph instantiation
        Graph graph = new Graph();

        // create vertices
        foreach (var n in nodes) {

            double sc = Convert.ToDouble(score_dict[n]);
            double w = Convert.ToDouble(weight_dict[n]);
            int s = Convert.ToInt32(size_dict[n]);

            List<String> content = node_content[n];

            Vertex v = new Vertex(n, s, w, sc, content);

            graph.AddVertex(v);
        }

        // add edges to graph
        foreach (var pair in arrows) {
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

        // print graphviz file
        graph.print();
    }
}