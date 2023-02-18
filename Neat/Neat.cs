namespace Connect4.Neat;

public class Neat {

    public readonly int Inputs;
    public readonly int Outputs;

    public int MaxNetworks { get; init; } = 150;
    public readonly Func<Network, float> Evaluate;


    public readonly List<Specie> Species;
    public Network[] Networks { get; private set; }

    private readonly List<Genes.Node> _nodes;
    private readonly List<(Genes.Node, Genes.Node)> _connection;


    public float CoefExcessGenes { get; init; } = 1f;
    public float CoefDisjointGenes { get; init; } = 1f;
    public float CoefWeightDiff { get; init; } = 0.4f;


    public float ProbaAddNode { get; init; } = 0.03f;
    public float ProbaAddConnection { get; init; } = 0.05f;
    public float ProbaWeightChange { get; init; } = 0.8f;
    public float ProbaWeightShift { get; init; } = 0.85f;
    public float ProbaWeightRandom { get; init; } = 0.1f;

    public float StrengthWeightShift { get; init; } = 0.2f;
    public float StrengthRandomWeight { get; init; } = 3f;


    public float SurvivorPerGeneration { get; init; } = 0.25f;
    public float SpeciesDistance { get; init; } = 3f;


    public const int BIAIS_NODE = 0;
    public const float INPUT_NODE_X = 0f;
    public const float OUPUT_NODE_X = 1f;


    public Neat(int inputs, int outputs, Func<Network, float> evaluate) {
        Inputs = inputs;
        Outputs = outputs;
        _nodes = new();
        _connection = new();

        Networks = Array.Empty<Network>();
        Species = new();
        Evaluate = evaluate;
    }


    public Genes.Connection GetConnection(Genes.Node from, Genes.Node to){
        int innov = _connection.IndexOf((from, to))+1;
        if(innov != 0) return new(innov, from, to);
        _connection.Add((from, to));
        return new(_connection.Count, from, to);
    }

    public IEnumerable<Genes.Node> GetRequiredNodes(){
        for (int n = 0; n < Inputs + Outputs + 1; n++) yield return GetNode(n);
    }
    public Genes.Node AddNode(float x){
        _nodes.Add(new(_nodes.Count, x));
        return _nodes[^1];
    }
    public Genes.Node GetNode(int id) => _nodes[id];


    public void StartEvolution(int maxGeneration = -1, bool display = true) {
        _nodes.Clear();
        _nodes.Add(new(BIAIS_NODE, INPUT_NODE_X));
        for (int n = 0; n < Inputs; n++) AddNode(INPUT_NODE_X);
        for (int n = 0; n < Outputs; n++) AddNode(OUPUT_NODE_X);
        
        _connection.Clear();

        Networks = new Network[MaxNetworks];
        for (int n = 0; n < MaxNetworks; n++) Networks[n] = new(new(this));
        
        Species.Clear();

        int gen = 0;
        while (gen != maxGeneration){
            _consolePrefix = $"Gen #{gen}: ";
            Evolve();
            gen++;
        }
    }
    private void Evolve() {
        Console.Write(_consolePrefix + "Generating species...             \r");
        foreach(Specie specie in Species) specie.ClearNetworks();
        foreach(Network network in Networks){
            bool added = false;
            foreach (Specie specie in Species){
                if(added = specie.TryAdd(network)) break;
            }
            if (!added) Species.Add(new(network));
        }


        // TODO asynch
        Console.Write(_consolePrefix + "Computing scores...              \r");
        for (int i = Species.Count - 1; i >= 0; i--) {
            Species[i].UpdateFitness();
            if (Species[i].Kill(1 - SurvivorPerGeneration)) {
                Species[i].Kill(1);
                Species.RemoveAt(i);
            }
        }

        // TODO asynch
        Console.Write(_consolePrefix + "Muttating...                  \r");
        for (int n = 0; n < Networks.Length; n++) {
            if (!Networks[n].alive) {
                Specie specie = Utility.WeigthedChoice(Species, s => s.Fitness);
                specie.ForceAdd(Networks[n] = Network.CrossOver(specie.ChooseNetwork(), specie.ChooseNetwork()));
            }
        }
        foreach (Specie specie in Species) specie.MutateAll();

        Console.WriteLine(_consolePrefix + "Finished !                   ");
        foreach (Specie s in Species) {
            Console.WriteLine($"   {nameof(Specie)}#{s.Id} : {s.Fitness} pts, best {s.Networks[0].fitness}pts ({s.Networks.Count} networks)");
        }
    }

    public static float Sigmoid(float x) => 1f / (1f + MathF.Exp(-4.9f * x));

    private string _consolePrefix = "";
}