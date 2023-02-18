namespace Connect4.Neat;

public class Specie {

    public readonly int Id;
    private static int s_generated = 0;

    public Genome Repesentative { get; private set; }
    public readonly List<Network> Networks;
    public float Fitness { get; private set; }

    public Specie(Network representative) {
        Id = ++s_generated;
        Repesentative = representative.genome;
        Networks = new(){representative};
    }
    public Specie(Genome representative) {
        Id = ++s_generated;
        Repesentative = representative;
        Networks = new();
    }

    public void ClearNetworks(){
        Repesentative = Networks[Utility.Random.Next(Networks.Count)].genome;
        Networks.Clear();
    }

    public bool TryAdd(Network network){
        if(Repesentative.Distance(network.genome) > Repesentative.Neat.SpeciesDistance) return false;
        ForceAdd(network);
        return true;
    }
    public void ForceAdd(Network genome) => Networks.Add(genome);

    public Network ChooseNetwork() => Utility.WeigthedChoice(Networks, n => n.fitness);

    public void MutateAll(){
        int start = 0;
        if(Networks.Count >= 5) start = 1;
        for (int i = start; i < Networks.Count; i++)  Networks[i].genome.Mutate();
    }

    public void UpdateFitness(){
        // TODO asynch
        Fitness = 0;
        for (int n = 0; n < Networks.Count; n++) {
            Networks[n].UpdateFitness();
            Fitness += Networks[n].fitness;
        }
        Fitness /= Networks.Count;
        Networks.Sort();
        Networks.Reverse();
    }

    public bool Kill(float precent){
        int toKill = (int)(Networks.Count * precent);
        for (int i = 0; i < toKill; i++){
            Networks[^1].alive = false;
            Networks.RemoveAt(Networks.Count-1);
        }
        return Networks.Count < 2;
    }
}