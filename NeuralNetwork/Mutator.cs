namespace Connect4.NeuralNetwork;

public record struct Mutation(float Chance, float Strengh);

public static class Mutator {

    public const int AnsynchUnits = 10;

    public static void Evolve(int inputLayer, int[] layers, Func<NeuralNetwork, NeuralNetwork, int> FitnessTest, int savedNetworks, int maxNetworks, string saveDir, int generations = -1){
        NeuralNetwork[] roots = new NeuralNetwork[savedNetworks];
        for (int i = 0; i < roots.Length; i++) roots[i] = new(inputLayer, layers);

        int gen = 0;
        while (generations == -1 || gen < generations) {
            NextGeneration(gen, roots, maxNetworks, FitnessTest, $"{saveDir}/Generation#{gen+1}");
            gen++;
        }
    }

    public static (NeuralNetwork, int)[] NextGeneration(int currentGen, NeuralNetwork[] roots, int maxNetworks, Func<NeuralNetwork, NeuralNetwork, int> FitnessTest, string? saveDir = null){
        int finished = 0;
        
        List<(NeuralNetwork, int)> bestNetworks = new(roots.Length+1);

        List<NeuralNetwork> childs = new(roots.Length);
        List<Task<int>> runningTests = new(AnsynchUnits);

        void UpdateDisplay() => Console.Write($"Generation {currentGen+1}... (finished: {finished}/{maxNetworks}, running: {runningTests.Count})\r");

        int StartTest(NeuralNetwork child){
            int fitness = 0;
            for (int r = 0; r < roots.Length; r++) fitness += FitnessTest(child, roots[r]);
            return fitness;
        }
        int n = 0;
        while(finished < maxNetworks){
            while(runningTests.Count < AnsynchUnits){
                NeuralNetwork child = runningTests.Count + finished < roots.Length ? roots[n] : roots[n].Mutate(new Mutation(0.05f, 0.5f), new Mutation(0.01f, 3f));
                childs.Add(child);
                runningTests.Add(Task.Run(() => StartTest(child)));
                UpdateDisplay();
                n = (n + 1) % roots.Length;
            }
            int t = Task.WaitAny(runningTests.ToArray());
            int fitness = runningTests[t].Result;
            if(bestNetworks.Count < roots.Length){
                bestNetworks.Add((childs[t], fitness));
            }else {
                int i = bestNetworks.FindIndex(((NeuralNetwork n, int f) res) => res.f < fitness);
                if(i != -1) {
                    bestNetworks.Insert(i, (childs[t], fitness));
                    while (bestNetworks.Count > roots.Length) bestNetworks.RemoveAt(bestNetworks.Count - 1);
                }
            }
            runningTests.RemoveAt(t);
            childs.RemoveAt(t);
            finished++;
            UpdateDisplay();
        }

        Console.Write($"Generation {currentGen+1} finished! Best fitness: ");
        foreach((NeuralNetwork, int) bests in bestNetworks) Console.Write($"{bests.Item2: +#;-#;0} ");
        Console.WriteLine();

        if(saveDir is not null){
            for (int bn = 0; bn < bestNetworks.Count; bn++){
                bestNetworks[bn].Item1.Save(saveDir, $"network#{bn+1}");
            }
        }
        return bestNetworks.ToArray();
    }

    public static NeuralNetwork Mutate(this NeuralNetwork root, params Mutation[] mutations){
        float GetMutation(){
            lock(Utility.RNGLock){
                foreach(Mutation mutation in mutations){
                    if(Utility.Random.NextSingle() < mutation.Chance) return (Utility.Random.NextSingle()-0.5f)*mutation.Strengh*2;
                }
            }
            return 0;
        }

        float[][] biaises = new float[root.layers.Length][];
        float[][][] weights = new float[root.layers.Length][][];

        for (int l = 0; l < root.layers.Length; l++){
            biaises[l] = new float[root.layers[l]];
            weights[l] = new float[root.layers[l]][];

            for (int n = 0; n < root.layers[l]; n++) {
                biaises[l][n] = root.biaises[l][n] + GetMutation();
                
                int pLayer = l == 0 ? root.inputLayer : root.layers[l - 1];
                weights[l][n] = new float[pLayer];
                for (int p = 0; p < pLayer; p++) weights[l][n][p] = root.weights[l][n][p] + GetMutation();
            }
        }

        return new(root.inputLayer, root.layers, biaises, weights);
    }
}
