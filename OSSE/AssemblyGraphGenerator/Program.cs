namespace AssemblyGraphGenerator;
using System.Diagnostics;
using AssemblyGraph;
using System;
using CommandLine;

public class Program {
    /// <summary>
    /// Adds a list of edges to a given graph
    /// </summary>
    /// <param name="graph"> graph to add edges to </param>
    /// <param name="edges"> edges to add to the graph </param>
    /// <returns> graph with added edges </returns>
    static Graph AddEdges(Graph graph, Dictionary<String, List<String>> edges) {
        foreach (var pair in edges) {
            String label = pair.Key;

            try {
                Vertex v1 = graph.GetVertex(label);
                foreach (var v in pair.Value) {
                    try {
                        graph.AddEdge(v1, graph.GetVertex(v));
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

    /// <summary>
    /// function to end a graph and add it to the graph list
    /// </summary>
    /// <param name="temp_graph"> current graph to be added to list</param>
    /// <param name="current_node"> current/final node in graph </param>
    /// <param name="temp_weight"> weight of last node in graph </param>
    /// <param name="temp_score"> score of last node in graph </param>
    /// <param name="temp_conn"> all nodes with edges to current node </param>
    /// <param name="temp_cont"> all the lines of code in the node </param>
    /// <param name="graph_list"> list of all graphs generated from input file </param>
    /// <param name="edges"> dictionary of node and list of all connected nodes </param>
    static void EndGraph(ref Graph temp_graph, ref String current_node, 
                                    ref Double temp_weight, ref Double temp_score, ref HashSet<String> temp_conn,
                                    ref List<String> temp_cont, ref List<Graph> graph_list,
                                    ref Dictionary<String, List<String>> edges) {

        temp_graph.AddVertex(new Vertex(current_node, temp_weight, temp_score, temp_cont));
        edges.Add(current_node, temp_conn.ToList());
        graph_list.Add(AddEdges(temp_graph, edges));

        edges       = new Dictionary<String, List<String>>();
        temp_graph  = new Graph();
        temp_conn   = new HashSet<String>(); 
        temp_cont   = new List<String>(); 
        temp_weight = 0; temp_score = 0; 
    }

    /// <summary>
    /// Handles all the cases where a line starts with a semicolon (;)
    /// </summary>
    /// <param name="temp_graph"> current graph being created </param>
    /// <param name="line"> current line with start of semicolon </param>
    /// <param name="current_node"> current node in graph </param>
    /// <param name="temp_weight"> weight of current node in graph </param>
    /// <param name="temp_score"> perfscore of current node in graph </param>
    /// <param name="temp_conn"> all nodes with edges to current node </param>
    /// <param name="temp_cont">  all the lines of code in the node </param>
    /// <param name="graph_list"> list of all graphs generated from input file </param>
    /// <param name="edges"> dictionary of node and list of all connected nodes </param>
    /// <param name="in_group"> boolean of whether last line read was still in a group </param>
    /// <param name="j"> used to know when at start of method and first group not fully read yet</param>
    static void CaseWithSemicolon(ref Graph temp_graph, String line, ref String current_node, 
                                    ref Double temp_weight, ref Double temp_score, ref HashSet<String> temp_conn,
                                    ref List<String> temp_cont, ref List<Graph> graph_list,
                                    ref Dictionary<String, List<String>> edges, ref Boolean in_group, ref int j) {
        
        if (line.Contains("MethodHash=")) { 
            String [] arr = line.Split("MethodHash=", 2);

            temp_graph.SetHashcode(arr[1].Split(')')[0]); // Get hashcode
            temp_graph.SetTotalBytes(Convert.ToInt32(arr[0].Split("code")[1].Trim().Split(',')[0])); // Get total bytes
            temp_graph.SetMethodName(arr[1].Split("for method ")[1].Trim()); // Get method name
        }
        else if (line.Contains("ARM64") || line.Contains("X64")) { // Get system type
            temp_graph.SetSystem(line.Split("for")[1].Trim());
        }
        else if (line.CompareTo("; optimized code") == 0) { // Get if optimized
            temp_graph.SetOptimized(true);
        }
        else if (line.Contains("bbWeight")) { // Get the weight and score
            temp_weight = Convert.ToDouble(line.Split("bbWeight=")[1].Split(" ")[0]);
            temp_score  = Convert.ToDouble(line.Split("PerfScore ")[1].Split(" ")[0]);

            if (temp_weight > temp_graph.GetMaxWeight()) {temp_graph.SetMaxWeight(temp_weight);}
            if (temp_score  > temp_graph.GetMaxScore()) {temp_graph.SetMaxScore(temp_score);}
        }
        else if (line.Contains("Assembly listing for method") && in_group) { // if encounter start of new method, add graph to list
            EndGraph(ref temp_graph, ref current_node, ref temp_weight, ref temp_score, ref temp_conn, ref temp_cont, ref graph_list, ref edges); in_group = false; j=0;
        }
    }

    /// <summary>
    /// Handles all cases when tehre is a group present in the line of code
    /// </summary>
    /// <param name="first_word"> first word in line of code, excluding hash code if present </param>
    /// <param name="line"> current line of code </param>
    /// <param name="connectToNextNode"> boolean on whether to connect to successive node</param>
    /// <param name="current_node"> current node in graph </param>
    /// <param name="temp_graph"> current graph being created </param>
    /// <param name="temp_weight"> weight of current node in graph </param>
    /// <param name="temp_score"> perfscore of current node in graph </param>
    /// <param name="temp_conn"> all nodes with edges to current node </param>
    /// <param name="temp_cont">  all the lines of code in the node </param>
    /// <param name="edges"> dictionary of node and list of all connected nodes </param>
    /// <param name="j"> used to know when at start of method and first group not fully read yet</param>
    static void CaseWithGroups(String first_word, String line, ref Boolean connectToNextNode, ref String current_node, 
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

    /// <summary>
    /// creates a graph(s) base on input file
    /// </summary>
    /// <param name="lines"> all lines in the input file </param>
    /// <param name="keep_hash_code"> boolean on whether to keep the hascode of each line of code </param>
    /// <returns> list of Graphs generated </returns>
    static List<Graph> CreateGraphs(String[] lines, Boolean keep_hash_code) {
        int j = 0; // used to know when at start of method and first group not fully read yet
        String current_node = ""; // holds the current group/node
        HashSet<String> temp_conn = new HashSet<String>(); // holds all the nodes or edges that are connected to the current node
        List<String>    temp_cont = new List<String>(); // holds code block inside the current group
        List<Graph>    graph_list = new List<Graph>(); // list of all graphs generated from file
        Graph          temp_graph = new Graph(); // current graph
        Double        temp_weight = 0; // weight of current group
        Double         temp_score = 0; // prefscore of current group
        Boolean connectToNextNode = true; // boolean on whether to connect to next node
        Boolean          in_group = false; // to know if currently inside a group
        Dictionary<String, List<String>> edges = new Dictionary<String, List<String>>(); // dict of connected nodes and their edges
        Boolean unwind = false; // helps keep track in x64 case with unwind info

        // iterate over the lines of the file
        for (int i = 0; i < lines.Length; i++) {
            String line = lines[i].Trim(); // remove leading spaces

            if (line.CompareTo("") == 0 ) { unwind = true; continue;} // skip empty lines

            // Get method info
            if (line[0].CompareTo(';') == 0) {
                CaseWithSemicolon(ref temp_graph, line, ref current_node, ref temp_weight, ref temp_score, ref temp_conn,
                                ref temp_cont, ref graph_list, ref edges, ref in_group, ref j); 
                continue;
            }
            
            String first_word = line.Split(null, 2)[0];
            
            // Get the first word excluding the "hash code" if it exists
            if (!char.IsLower(first_word[0]) && !first_word.Contains("G_M")) {
                String new_line = line.Split(null, 2)[1].Trim();
                first_word = new_line.Split(null, 2)[0];
                if (!keep_hash_code) {line = new_line;}
            } 

            // unconditional jump or ret so dont ad edge
            if (first_word.CompareTo("ret") == 0 || first_word.CompareTo("b") == 0 
                || first_word.CompareTo("jmp") == 0 || (line.Contains("HELP_THROW")) 
                || line.Contains("Throw") || line.Contains("HELP_OVERFLOW")) {
                connectToNextNode = false; // dont connect to next node, there is an unconditional jump
            }

            // check for group
            if (line.Contains("G_M")){ 
                in_group = true;
                unwind = false;
                CaseWithGroups(first_word, line, ref connectToNextNode, ref current_node, ref temp_graph, ref temp_weight, 
                                ref temp_score, ref temp_conn, ref temp_cont, ref edges, ref j);
            }
            else { // add everything else to content dict
                if (!unwind) {
                    if (in_group) { temp_cont.Add(line); } 
                }
            }
        }

        if (in_group == true) {
            EndGraph(ref temp_graph, ref current_node, ref temp_weight, ref temp_score, ref temp_conn, ref temp_cont, ref graph_list, ref edges); in_group = false; j =0;
        }

        return graph_list;
    }
    
    /// <summary>
    /// creates SVG files from the graph list 
    /// </summary>
    /// <param name="graph_list"> list of graphs </param>
    /// <param name="output_folder_path"> path to output folder for SVG files</param>
    /// <param name="show_weight_score"> boolean on whether to show weight and perfScore on each node in SVG graph output </param>
    static List<String> CreateSVGOrHTML(List<Graph> graph_list, String output_folder_path, Boolean show_weight_score, Boolean html) {
        Process process = new Process();
        int i = 0;;
        List<String> svg_list = new List<String>();
        foreach (Graph g in graph_list) {

            String graph_name = "graph_" + i;
            if (g.GetHashcode() != "") {graph_name = g.GetHashcode();}

            process.StartInfo.FileName = "dot.exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            String inputfile = Path.Combine(output_folder_path, graph_name) + ".dot";
            String svgFile =  Path.Combine(output_folder_path, graph_name) + ".svg";
            process.StartInfo.Arguments = $"-Tsvg \"{inputfile}\" -o \"{svgFile}\"";

            g.CreateDotFile(inputfile, show_weight_score);

            process.Start();
            process.WaitForExit();

            if (File.Exists(inputfile)) { File.Delete(inputfile);}
            if (html){CreateHTMLFile(graph_name, output_folder_path);}

            svg_list.Add(svgFile);
        }

        return svg_list;

    }

    /// <summary>
    /// creates HTML file from input SVG file
    /// </summary>
    /// <param name="filename"> SVG file name </param>
    /// <param name="folderPath"> output folder for HTML file </param>
    static void CreateHTMLFile(String filename, String folderPath) {
        String leaderline_file = "<script src=\"https://cdnjs.cloudflare.com/ajax/libs/leader-line/1.0.7/leader-line.min.js\"></script>";
        // String script_file = "<script src=\"script.js\"></script>";
        String svgFile = Path.Combine(folderPath, filename) + ".svg";

        // javascript draggable code
        String srcipt_file_content = 
        @"<script>
        var node_list = document.querySelectorAll("".node"")
        var edge_list = document.querySelectorAll("".edge"")
        var map = {};

        // create leaderlines
        for (var i = 0; i < edge_list.length; i++) {
            var edge_title = edge_list[i].getElementsByTagName('title')[0].textContent.split(""->"")
            var s_num = parseInt(edge_title[0].split('G')[1]).toString()
            var e_num = parseInt(edge_title[1].split('G')[1]).toString()

            if(s_num == e_num) {continue;}

            // get start and end element of edge
            var startElem = document.getElementById('node'+s_num)
            var endElem = document.getElementById('node'+e_num)

            // get line color
            var color = edge_list[i].getElementsByTagName('polygon')[0].getAttribute('fill')
            var options = {color: color, size: 2, path:'straight'}

            if (parseInt(e_num) != parseInt(s_num) + 1 && parseInt(e_num) != parseInt(s_num) - 1) { 
                var startSocket, endSocket;
                if (color == 'green') {startSocket= 'left'; endSocket= 'left';}
                else {startSocket= 'right'; endSocket= 'right';}
                
                options = {color: color, size: 2, path:'fluid', hoverStyle:{dash:{Animation:true}} ,startSocket:startSocket, endSocket:endSocket}
            }

            // create leader line
            var line = new LeaderLine((startElem), endElem, options)
            
            addValueToList(startElem.id, line)
            addValueToList(endElem.id, line)
            edge_list[i].remove()
        }
        
        // draggable = new PlainDraggable(element);
        function addValueToList(key, value) {
            //if the list is already created for the ""key"", then uses it
            //else creates new list for the ""key"" to store multiple values in it.
            map[key] = map[key] || [];
            map[key].push(value);
        }

        for (var i = 0; i < node_list.length; i++) {
            var id = node_list[i].id
            var o = new Draggable(node_list[i], map[id])
        }

        // https://stackoverflow.com/questions/41514967/yes-no-is-there-a-way-to-improve-mouse-dragging-with-pure-svg-tools/41518545#41518545
        function Draggable(elem, lines) {
            this.target = elem
            this.clickPoint = this.target.ownerSVGElement.createSVGPoint()
            this.lastMove = this.target.ownerSVGElement.createSVGPoint()
            this.currentMove = this.target.ownerSVGElement.createSVGPoint()
            this.target.addEventListener(""mousedown"", this)
            
            this.handleEvent = function(evt) {
                evt.preventDefault()
                this.clickPoint = globalToLocalCoords(elem, evt.clientX, evt.clientY)
                this.target.classList.add(""dragged"")
                this.target.setAttribute(""pointer-events"", ""none"")
                this.target.ownerSVGElement.addEventListener(""mousemove"", this.move)
                this.target.ownerSVGElement.addEventListener(""mouseup"", this.endMove)
            }

            this.move = function(evt) {
                var p = globalToLocalCoords(elem, evt.clientX, evt.clientY)
                this.currentMove.x = this.lastMove.x + (p.x - this.clickPoint.x)
                this.currentMove.y = this.lastMove.y + (p.y - this.clickPoint.y)
                this.target.setAttribute(""transform"", ""translate("" + this.currentMove.x + "","" + this.currentMove.y + "")"")
                for (var i = 0; i < lines.length; i++) {lines[i].position();}
            }.bind(this)

            this.endMove = function(evt) {
                evt.preventDefault()
                this.lastMove.x = this.currentMove.x
                this.lastMove.y = this.currentMove.y
                this.target.classList.remove(""dragged"")
                this.target.setAttribute(""pointer-events"", ""all"")
                this.target.ownerSVGElement.removeEventListener(""mousemove"", this.move)
                this.target.ownerSVGElement.removeEventListener(""mouseup"", this.endMove)
                
            }.bind(this)

            function globalToLocalCoords(elem, x, y) {
                var p = elem.ownerSVGElement.createSVGPoint()
                var m = elem.parentNode.getScreenCTM()
                p.x = x
                p.y = y
                return p.matrixTransform(m.inverse())
            }
        }
        
    </script>";

        String text = File.ReadAllText(svgFile);
        text = text.Replace("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\"", "<!DOCTYPE html >");
        text = text.Replace("\"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">", "");
        text = text.Replace("<svg", "<html>\n<body>\n<svg");
        text = text.Replace("</svg>", "</svg>\n"+leaderline_file+"\n"+srcipt_file_content+"\n</body>\n</html>");

        File.WriteAllText(Path.Combine(folderPath, filename) + ".html", text);
        if (File.Exists(svgFile)) { File.Delete(svgFile);}
    }

// -----------------------------------------------------------------------------------------------------------------------------
// Test Functions

    /// <summary>
    /// Tests whether the graph is able to detect all cycles in the file
    /// </summary>
    /// <param name="file"> input file </param>
    /// <returns> list of cycles found </returns>
    static public String [] TestCycleDetection(String file ) {
        String[] lines = File.ReadAllLines(Path.GetFullPath(file));
        List<Graph> graphs = CreateGraphs(lines, false);
        HashSet<List<Vertex>> cycles = graphs[0].DetectCycle();
        List<String> cycle_string = new List<String>();

        foreach (List<Vertex> v_ls in cycles) {
            string s = "";
            foreach (Vertex v in v_ls) {
                s = s + " " + v.GetLabel();
            }
            // Console.WriteLine(s.Trim());
            cycle_string.Add(s.Trim());
        }
        return cycle_string.ToArray();
    }

    /// <summary>
    /// returns list of all verties
    /// </summary>
    /// <param name="file"> input file </param>
    /// <returns></returns>
    static public String TestAllVerticesDetected(String file) {
        String[] lines = File.ReadAllLines(Path.GetFullPath(file));
        List<Graph> graphs = CreateGraphs(lines, false);
        List<Vertex> vertices = graphs[0].GetVertices();
        String s = "";
        foreach(Vertex v in vertices) {
            s = s + " " + v.GetLabel();
        }
        return s.Trim();
    }

    /// <summary>
    /// generates SVG file
    /// </summary>
    /// <param name="file"> input file </param>
    /// <returns> string array of generated file content </returns>
    static public String[] TestSVGCreated(String file) {
        List<string> svgContent = new List<String>(); 
        String in_path = Path.GetFullPath(file);

        String[] lines = File.ReadAllLines(in_path);       
        List<Graph> graphs = CreateGraphs(lines, false);
        String out_path = Path.GetDirectoryName(in_path);

        List<String> svg_list =  CreateSVGOrHTML(graphs, out_path, false, true);
        foreach(String svg in svg_list) {
            svgContent.Add(File.ReadAllText(svg));
            if (File.Exists(svg)) { File.Delete(svg);}
        }

        return svgContent.ToArray();
    }

// ----------------------------------------------------------------------------------------------------------

    class Options
    {
        [Option("input-file", Required = true, HelpText = "Input file to be processed.")]
        public string InputFile { get; set; }

        [Option("output-folder", Required = true,  HelpText = "The folder for the SVG/HTML graph outputs")]
        public string OutputFolder { get; set; }

        // Omitting long name, defaults to name of property, ie "--verbose"
        [Option("show-hash-code", Required = false, Default = false, HelpText = "Boolean to represent whether to show the hash code for each instruction")]
        public bool ShowHashCode { get; set; }

        [Option("show-weight-perfscore", Required = false,  Default = false, HelpText = "Boolean on whether to show the weight and perfscore of each group in the instruction")]
        public bool ShowWeightPerfscore { get; set; }

        [Option("html", Required = false,  Default = false, HelpText = "outputs HTML file if true, SVG if false")]
        public bool html { get; set; }
    }

    static public void Main(String[] args) {
        bool show_hash_code = false;
        bool show_weight_score = false;
        bool html = false;
        String input_file = "";
        String output_folder = "";

        Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {   
                if (o.ShowHashCode) { show_hash_code = true;}
                if (o.ShowWeightPerfscore) {show_weight_score = true;}
                if (o.html) {html=true;}

                input_file = o.InputFile;
                output_folder = o.OutputFolder;
            }
        );

        // check input file path 
        if (!File.Exists(input_file)) {
            throw new ArgumentException ("ERROR: Incorrect input file path " + input_file);
        }

        // check output file path
        String output_folder_path = "";
        try {
            output_folder_path = Path.GetFullPath(output_folder);
        } catch(Exception) {
            Console.WriteLine("ERROR: Incorrect output folder path " + output_folder);
        }

        String[] lines = File.ReadAllLines(input_file); 
        if (lines.Length != 0) {CreateSVGOrHTML(CreateGraphs(lines, show_hash_code), output_folder_path, show_weight_score, html);}
    }
}
