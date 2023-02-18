namespace Connect4.Neat;

public class Network : IComparable<Network> {
    public bool alive;
    public Genome genome;
    public float fitness;

    public Network(Genome genome, float score = 0) {
        this.genome = genome;
        this.fitness = score;
        alive = true;
    }

    public void UpdateFitness() => fitness = genome.Neat.Evaluate(this);

    public float[] GetOutputs(params float[] inputs) => genome.GetOutputs(inputs);

    public static Network CrossOver(Network a, Network b) {
        if (a.CompareTo(b) > 0) return new(a.genome.CrossOver(b.genome));
        return new(b.genome.CrossOver(a.genome));
    }
    
    public int CompareTo(Network? other) => other is null ? 1 : fitness.CompareTo(other.fitness);

}