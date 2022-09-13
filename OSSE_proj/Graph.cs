class Vertex {
    private String label;
    private List<String> content_code;
    private int size;
    private double weight;
    private double score;

    public Vertex (String label, int size, double weight, double score, List<String> content) {
        this.label = label;
        this.content_code = content;
        this.weight = weight;
        this.score = score;
        this.size = size;
    }

    public Boolean Equals (Vertex v1){
        return v1.label == label;
    }

    public void setContentCode (List<String> content) {
        this.content_code = content;
    }

    public void setSize (int size) {
        this.size = size;
    }

    public int getSize () {
        return size;
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

    public void print() {
        // Console.WriteLine("\t{0} [label = \"{1}\\n\\n{2}\" style=\"solid\" penwidth={3}];",label, label, string.Join("\\l", content_code), weight);

        Console.WriteLine("\t{0} [label =<", label);
        Console.WriteLine("\t\t<table border=\"0\" cellborder=\"0\" cellspacing=\"1\">");
        Console.WriteLine("\t\t<tr><td align=\"center\"><b>{0}</b></td></tr>", label);

        foreach(var code in content_code) {
            Console.WriteLine("\t\t<tr><td align=\"left\">{0}</td></tr>", code);
        }
        Console.WriteLine("\t\t</table>> style=\"solid\" penwidth={0} ];", 1);
    }
}

class Graph {
    private Dictionary<Vertex, List<Vertex>> adjVertices;

    public Graph () {
        this.adjVertices = new Dictionary<Vertex, List<Vertex>>();
    }

    public void AddVertex (Vertex v) {
        adjVertices.TryAdd(v, new List<Vertex>());
    }

    public void AddEdge (Vertex v1, Vertex v2) {
        adjVertices[v1].Add(v2);
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

    public void print () {
        Console.WriteLine("digraph FlowGraph {");
        Console.WriteLine("\tsplines=fase;");
        Console.WriteLine("\tgraph [label = \"Bench()\"];");
        Console.WriteLine("\tnode [shape = \"Box\" fontname=\"Courier New\"];");

        foreach (var pair in adjVertices) {
            Vertex v = pair.Key;
            v.print();
        }

        Console.WriteLine("\n");

        foreach (var pair in adjVertices) {
            Vertex v = pair.Key;

            foreach (var vi in pair.Value) {
                if (vi.Equals(v)) {
                    Console.WriteLine("\t{0} -> {1} [color=red, dir=back];", v.getLabel(), vi.getLabel());
                } else {
                    if (String.Compare(v.getLabel(), vi.getLabel()) > 0 ) {
                        Console.WriteLine("\t{0} -> {1} [color=green];", v.getLabel(), vi.getLabel());    
                    }
                    else {
                        Console.WriteLine("\t{0} -> {1} [color=blue];", v.getLabel(), vi.getLabel());
                    }
                }
            }
        }

        Console.WriteLine("}");        
    }
}