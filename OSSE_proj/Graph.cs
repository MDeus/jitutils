class Vertex {
    private String label;
    private List<String> content_code;
    private double weight;
    private double score;

    public Vertex (String label, double weight, double score, List<String> content) {
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

    public double getWeight () {
        return weight;
    }

    public void setScore (int score) {
        this.score = score;
    }

    public double getScore () {
        return score;
    }

    public String getLabel () {
        return label;
    }

    // public void print() {
  
    //     Console.WriteLine("\t{0} [label =<", label);
    //     Console.WriteLine("\t\t<table border=\"0\" cellborder=\"0\" cellspacing=\"1\">");
    //     Console.WriteLine("\t\t<tr><td align=\"center\"><b>{0}</b></td></tr>", label);

    //     foreach(var code in content_code) {
    //         Console.WriteLine("\t\t<tr><td align=\"left\">{0}</td></tr>", code);
    //     }
    //     Console.WriteLine("\t\t</table>> style=\"solid\" penwidth={0} ];", 1);
    // }
}

class Graph {
    private Dictionary<Vertex, List<Vertex>> adjVertices;

    private double maxWeight = 0;

    private double maxScore = 0;

    public Graph () {
        this.adjVertices = new Dictionary<Vertex, List<Vertex>>();
    }

    public void AddVertex (Vertex v) {
        adjVertices.TryAdd(v, new List<Vertex>());
    }

    public void AddEdge (Vertex v1, Vertex v2) {
        adjVertices[v1].Add(v2);
    }

    public void setMaxWeight (double weight) {
        this.maxWeight = weight;
    }

    public double getMaxWeight () {
        return maxWeight;
    }

    public void setMaxScore (double score) {
        this.maxScore = score;
    }

    public double getMaxScore () {
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

    private int choose_color (double score) {
        if (score < 0.2) {return 1;}
        else if (score >= 0.2 && score < 0.4) {return 2;}
        else if (score >= 0.4 && score < 0.6) {return 3;}
        else if (score >= 0.6 && score < 0.8) {return 4;}
        else  {return 5;}

    }

    public void print (String file_name) {
        using(StreamWriter writetext = new StreamWriter(file_name)) {

            writetext.WriteLine("digraph FlowGraph {");
            writetext.WriteLine("\tsplines=fase;");
            writetext.WriteLine("\tgraph [label = \"Bench()\"];");
            writetext.WriteLine("\tnode [shape = \"Box\" fontname=\"Courier New\" colorscheme=oranges9];");

            foreach (var pair in adjVertices) {
                Vertex v = pair.Key;

                writetext.WriteLine("\t{0} [label =<", v.getLabel());
                writetext.WriteLine("\t\t<table border=\"0\" cellborder=\"0\" cellspacing=\"1\">");
                writetext.WriteLine("\t\t<tr><td align=\"center\"><b>{0}</b></td></tr>", v.getLabel());

                foreach(var code in v.getContentCode()) {
                    writetext.WriteLine("\t\t<tr><td align=\"left\">{0}</td></tr>", code);
                }
                writetext.WriteLine("\t\t</table>> style=\"solid\" penwidth={0} style=filled, fillcolor={1}];", Math.Round((v.getWeight() / maxWeight), 1 ) + 0.1, choose_color(Math.Round(v.getScore() / maxScore, 2)));
            }

            writetext.WriteLine("\n");

            foreach (var pair in adjVertices) {
                Vertex v = pair.Key;

                foreach (var vi in pair.Value) {
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
            
            List<Vertex> vertices = getVertices();
            int t_vertices = vertices.Count;
            
            String edge = "\tsubgraph cluster_1 {" + vertices[0].getLabel() +" -> ";
            for (int i =1; i < t_vertices - 1; i++) {
                edge = edge + (vertices[i].getLabel() + " -> ");
            }

            writetext.WriteLine("\n\tedge[style=invis];");
            writetext.WriteLine(edge + vertices[t_vertices - 1].getLabel() + "};");      

            writetext.WriteLine( "}");   

        }     
    }
}