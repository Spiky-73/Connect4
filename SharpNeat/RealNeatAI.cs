using SharpNeat.Core;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.DistanceMetrics;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.SpeciationStrategies;

namespace Connect4.SharpNeat;

public class RealNeatAI : Core.IPlayer {

    public int Id;

    private readonly IBlackBox _network;
    public Core.Connect4 Game = null!;

    public RealNeatAI(IBlackBox network) {
        _network = network;
    }

    public void Setup(Core.Connect4 game, int players, int w, int h) {
        Game = game;
        Id = Array.IndexOf(game.Players, this);
    }

    public int Place() {

        double max = 0;
        int col = -1;
        for (int c = 0; c < Game.Width; c++) {
            int r = Game.Grid[c].Count;
            if (Game.ColumnFull(c)) continue;
            for (int y = -3; y < 4; y++) {
                for (int x = -3; x < 4; x++) {
                    double val = (c+x).InRange(0, Game.Width) && (r+y).InRange(0, Game.Grid[c+x].Count) ? (Game.Grid[c+x][r+y] == Id ? 1 : -1) : 0;
                    _network.InputSignalArray[(x+3) + (y+3) * Game.Width] = val;
                }
            }
            _network.InputSignalArray[c + r * Game.Width] = (float) c / (Game.Width-1);
            _network.Activate();
            double m = _network.OutputSignalArray[0];
            if (m > max) {
                col = c;
                max = m;
            }
        }
        return col;
    }

    public void OnPlacement(bool self, int player, int col) {}

    public static NeatEvolutionAlgorithm<NeatGenome> network = null!;

    public static void Test() {
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

        var phenomeEvaluator = new SharpNeat.Evaluator();
        var genomeListEvaluator =
            new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, phenomeEvaluator);

        network.Initialize(genomeListEvaluator, neatGenomeFactory, genomeList);

        //Optional
        network.UpdateScheme = new UpdateScheme(5);
        network.UpdateEvent += SharpNeat.Evaluator.Ea_UpdateEvent;

        network.StartContinue();
        while (network.RunState != RunState.Paused) {
            Thread.Sleep(100);
        }
        network.Stop();
        Core.Connect4 game = new(7, 6, new Core.Player(), new SharpNeat.RealNeatAI((IBlackBox)network.SpecieList[0].GenomeList[0].CachedPhenome));
        game.Run(display: true);
    }
}

public class Evaluator : IPhenomeEvaluator<IBlackBox> {
    private ulong _evals = 0;

    public ulong EvaluationCount => _evals;

    public bool StopConditionSatisfied => EvaluationCount > 150 * 200;
    public FitnessInfo Evaluate(IBlackBox phenome) {
        _evals++;
        float fitness = 0;
        if (RealNeatAI.network.SpecieList is null) return new FitnessInfo(fitness, fitness);
        foreach (Specie<NeatGenome> specie in RealNeatAI.network.SpecieList) {
            for (int i = 0; i < specie.GenomeList.Count; i++) {
                Core.Connect4 game = new(7, 6, new RealNeatAI(phenome), new RealNeatAI((IBlackBox)specie.GenomeList[i].CachedPhenome));
                fitness += Fitness(game.Run(0)) + Fitness(game.Run(1));
            }
        }
        return new FitnessInfo(fitness, fitness);
    }

    public static int Fitness(int win) => win switch {
        0 => 1,
        1 => 5,
        2 or _ => 0
    };
    public void Reset() { }

    public static void Ea_UpdateEvent(object? sender, EventArgs e) {
        var network = (NeatEvolutionAlgorithm<NeatGenome>)sender!;
        Console.WriteLine($"Generation={network.CurrentGeneration} bestFitness ={network.Statistics._maxFitness:N6} meanFitness ={network.Statistics._meanFitness:N6}");
    }
}