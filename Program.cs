using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.DistanceMetrics;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using SharpNeat.SpeciationStrategies;

namespace Connect4;

public class Program {

    public static void Main(string[] args) {
        // Neat.Neat neat = new(2, 1, Neat.NetworkAI.Evaluate);

        // neat.StartEvolution(100);

        // foreach (Neat.Specie s in neat.Species) {
        //     Core.Connect4 game = new(7, 6, new Core.Player(), new Neat.NetworkAI(s.Networks[0]));
        //     game.Run(display:true);
        // }

        int inputCount = 42; // Count of inout neurons
        int outputCount = 7; // Count of output neurons
        int specimenCount = 150; // Specimen count in each generation

        var neatGenomeFactory = new NeatGenomeFactory(inputCount, outputCount);
        var genomeList = neatGenomeFactory.CreateGenomeList(specimenCount, 0);
        var neatParameters = new NeatEvolutionAlgorithmParameters {
            SpecieCount = specimenCount
        };

        var distanceMetric = new ManhattanDistanceMetric();
        var speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>
            (distanceMetric);

        var complexityRegulationStrategy = new NullComplexityRegulationStrategy();

        network = new NeatEvolutionAlgorithm<NeatGenome>
            (neatParameters, speciationStrategy, complexityRegulationStrategy);

        var activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(1);
        var genomeDecoder = new NeatGenomeDecoder(activationScheme);

        var phenomeEvaluator = new YourPhenomeEvaluator();
        var genomeListEvaluator =
            new ParallelGenomeListEvaluator<NeatGenome,IBlackBox>(genomeDecoder, phenomeEvaluator);

        network.Initialize(genomeListEvaluator, neatGenomeFactory, genomeList);

        //Optional
        network.UpdateScheme = new UpdateScheme(5);
        network.UpdateEvent += Ea_UpdateEvent;

        network.StartContinue();
        while (network.RunState != RunState.Paused) {
            Thread.Sleep(100);
        }
        network.Stop();
        Core.Connect4 game = new(7, 6, new Core.Player(), new RealNetworkAI((IBlackBox)network.SpecieList[0].GenomeList[0].CachedPhenome));
        game.Run(display:true);

    }

    public static void Ea_UpdateEvent(object? sender, EventArgs e) {
        var network = (NeatEvolutionAlgorithm<NeatGenome>)sender!;
        Console.WriteLine($"Generation={network.CurrentGeneration} bestFitness ={ network.Statistics._maxFitness:N6} meanFitness ={ network.Statistics._meanFitness:N6}");
    }

    public static NeatEvolutionAlgorithm<NeatGenome> network;

}

class YourPhenomeEvaluator : IPhenomeEvaluator<IBlackBox> {
    private ulong _evals = 0;

    public ulong EvaluationCount => _evals;

    public bool StopConditionSatisfied => EvaluationCount > 150*2000;
    public FitnessInfo Evaluate(IBlackBox phenome) {
        _evals++;
        float fitness = 0;
        if(Program.network.SpecieList is null){
            return new FitnessInfo(fitness, fitness);
        }
        foreach(Specie<NeatGenome> specie in Program.network.SpecieList){
            for (int i = 0; i < specie.GenomeList.Count; i++) {
                Core.Connect4 game = new(7,6, new RealNetworkAI(phenome), new RealNetworkAI((IBlackBox)specie.GenomeList[i].CachedPhenome));
                fitness += Fitness(game.Run(0)) + Fitness(game.Run(1));
            }
        }
        return new FitnessInfo(fitness, fitness);
    }
    public void Reset() { }

    public static int Fitness(int win) => win switch {
        0 => 1,
        1 => 5,
        2 or _ => 0
    };
}