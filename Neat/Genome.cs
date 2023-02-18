namespace Connect4.Neat;

using Genes;

public class Genome {

    public readonly Neat Neat;

    public int Inputs => Neat.Inputs;
    public int Outputs => Neat.Outputs;
    
    public IList<Node> Nodes => _nodes.Keys;
    public IList<Connection> Connections {
        get {
            List<Connection> connections = new();
            foreach (List<Connection> cs in _nodes.Values) connections.AddRange(cs);
            connections.Sort();
            return connections;
        }
    }

    public Genome(Neat neat){
        Neat = neat;
        _nodes = new();
        foreach(Node node in neat.GetRequiredNodes()) AddNode(node);
    }

    public void AddNode(Node node) => _nodes.TryAdd(node, new());
    public void AddConnection(Connection con) {
        AddNode(con.From);
        AddNode(con.To);
        _nodes[con.To].Add(con);
    }

    public float[] GetOutputs(params float[] inputs){
        Dictionary<Node, float> states = new(){
            {_nodes.Keys[Neat.BIAIS_NODE], 1}
        };
        for (int n = 1; n < Inputs+1; n++) states[_nodes.Keys[n]] = inputs[n-1];

        float[] outputs = new float[Outputs];
        for (int n = 0; n < Outputs; n++) outputs[n] = GetState(_nodes.Keys[1+Inputs+n], states);
        return outputs;
    }
    public float GetState(Node node, Dictionary<Node, float>? states) {
        if(node.Id == Neat.BIAIS_NODE) return 1;
        if(states is null) states = new();
        else if (states.TryGetValue(node, out float state)) return state;
        float s = 0;
        foreach (Connection connection in _nodes[node]) {
            if (!connection.disabled) s += connection.weight * GetState(connection.From, states);
        }
        return states[node] = Neat.Sigmoid(s);
    }


    public float Distance(Genome other) {

        IList<Connection> connectionsA = Connections;
        IList<Connection> connectionsB = other.Connections;

        int inA = connectionsA.Count == 0 ? 0 : connectionsA[^1].Innovation;
        int inB = connectionsB.Count == 0 ? 0 : connectionsB[^1].Innovation;

        if(inB > inA) (connectionsA, connectionsB) = (connectionsB, connectionsA);

        int similar = 0, disjoint = 0;
        float weightDiff = 0f;

        int i = 0, j = 0;
        while (i < connectionsA.Count && j < connectionsB.Count) {
            if (connectionsA[i].Innovation == connectionsA[i].Innovation) {
                similar++;
                weightDiff += MathF.Abs(connectionsB[j].weight - connectionsA[i].weight);
                i++; j++;
            } else if (connectionsA[i].Innovation > connectionsA[i].Innovation) {
                disjoint++;
                j++;
            } else {
                disjoint++;
                i++;
            }
        }
        int excess = connectionsA.Count - i;
        int n = Math.Max(connectionsA.Count, connectionsB.Count);
        if (n < 20) n = 1;

        return Neat.CoefExcessGenes * excess / n + Neat.CoefDisjointGenes * disjoint / n + weightDiff / similar;
    }

    public Genome CrossOver(Genome other, bool equals = false){

        IList<Connection> connectionsA = Connections;
        IList<Connection> connectionsB = other.Connections;

        Genome child = new(Neat);

        int i = 0, j = 0;
        while (i < connectionsA.Count && j < connectionsB.Count) {
            if (connectionsA[i].Innovation == connectionsB[j].Innovation) {
                Connection con = Utility.Random.NextSingle() < 0.5f ? connectionsA[i] : connectionsB[j];
                child.AddConnection(con);
                i++; j++;
            } else if (connectionsA[i].Innovation < connectionsB[j].Innovation) {
                child.AddConnection(connectionsA[i]);
                i++;
            } else {
                if(equals) child.AddConnection(connectionsB[j]);
                j++;
            }
        }
        while(i < connectionsA.Count){
            child.AddConnection(connectionsA[i]);
            i++;
        }
        if (equals) {
            while (j < connectionsB.Count) {
                child.AddConnection(connectionsB[j]);
                j++;
            }
        }

        return child;
    }


    public void Mutate() {
        if(Utility.Random.NextSingle() < Neat.ProbaAddNode) MutateNode();
        if(Utility.Random.NextSingle() < Neat.ProbaAddConnection) MutateConnection();
        if(Utility.Random.NextSingle() < Neat.ProbaWeightChange){
            float rng = Utility.Random.NextSingle();
            if(rng < Neat.ProbaWeightShift) MutateWeightShift();
            else if(rng < Neat.ProbaWeightShift + Neat.ProbaWeightRandom) MutateRandomWeight();
            else MutateToogle();
        }
    }

    public void MutateNode() {
        IList<Connection> connections = Connections;
        if(connections.Count == 0) return;
        Connection con = connections[Utility.Random.Next(0, connections.Count)];
        _nodes[con.To].Remove(con);
        Node middle = Neat.AddNode((con.From.X + con.To.X) / 2);
        AddConnection(Neat.GetConnection(con.From, middle));
        Connection end = Neat.GetConnection(middle, con.To);
        // end.disabled = con.disabled;
        end.weight = con.weight;
        AddConnection(end);
    }
    public void MutateConnection() {
        IList<Node> nodes = Nodes;
        for (int t = 0; t < 100; t++) {
            Node from = nodes[Utility.Random.Next(0, nodes.Count)];
            Node to = nodes[Utility.Random.Next(0, nodes.Count)];

            if(from.X == to.X) continue;
            if(from.X > to.X) (from, to) = (to, from);

            if(_nodes[to].Exists(c => c.From.Id == from.Id)) continue;

            Connection con = Neat.GetConnection(from, to);
            con.weight = (Utility.Random.NextSingle() * 2 - 1) * Neat.StrengthRandomWeight;
            AddConnection(con);
            break;
        }
    }
    public void MutateWeightShift() {
        IList<Connection> connections = Connections;
        if (connections.Count == 0) return;
        Connection con = connections[Utility.Random.Next(0, connections.Count)];
        con.weight += (Utility.Random.NextSingle() * 2 - 1) * Neat.StrengthWeightShift;
        _nodes[con.To][_nodes[con.To].FindIndex(c => c.Innovation == con.Innovation)] = con;
    }
    public void MutateRandomWeight() {
        IList<Connection> connections = Connections;
        if (connections.Count == 0) return;
        Connection con = connections[Utility.Random.Next(0, connections.Count)];
        con.weight = (Utility.Random.NextSingle() * 2 - 1) * Neat.StrengthRandomWeight;
        _nodes[con.To][_nodes[con.To].FindIndex(c => c.Innovation == con.Innovation)] = con;
    }
    public void MutateToogle() {
        IList<Connection> connections = Connections;
        if (connections.Count == 0) return;
        Connection con = connections[Utility.Random.Next(0, connections.Count)];
        con.disabled = !con.disabled;
        _nodes[con.To][_nodes[con.To].FindIndex(c => c.Innovation == con.Innovation)] = con;
    }


    private readonly SortedList<Node, List<Connection>> _nodes;
}