namespace Connect4;

public class NeuralNetwork {

    public int inputLayer;      // layer
    public int[] layers;        // layer
    public float[][] biaises;   // layer, neuron
    public float[][][] weights; // layer, neuron*link

    public NeuralNetwork(string path) {
        string[] lines = File.ReadAllLines(path);
        string[] parts = lines[0].Split();

        layers = Array.Empty<int>();
        biaises = Array.Empty<float[]>();
        weights = Array.Empty<float[][]>();
        int l = -1;
        int n = 0;
        for (int i = 0; i < lines.Length; i++){
            parts = lines[i].Split();
            if (lines[i] == "") {
                l++;
                n = 0;
                biaises[l] = new float[layers[l]];
                weights[l] = new float[layers[l]][];
                continue;
            }
            if(l == -1) {
                inputLayer = int.Parse(parts[0]);
                layers = parts[1..].Select(int.Parse).ToArray();
                biaises = new float[layers.Length][];
                weights = new float[layers.Length][][];
                continue;
            }
            biaises[l][n] = float.Parse(parts[0]);
            weights[l][n] = parts[1..].Select(float.Parse).ToArray();
            n++;
        }


    }
    public NeuralNetwork(int inputLayer, int[] layers, float[][] biaises, float[][][] weights) {
        this.inputLayer = inputLayer;
        this.layers = layers;
        this.biaises = biaises;
        this.weights = weights;
    }
    public NeuralNetwork(int inputLayer, params int[] layers) {
        this.inputLayer = inputLayer;
        this.layers = layers;

        biaises = new float[layers.Length][];
        weights = new float[layers.Length][][];

        for (int l = 0; l < layers.Length; l++) {
            biaises[l] = RandomArray(layers[l]);
            weights[l] = new float[layers[l]][];
            for (int n = 0; n < layers[l]; n++) weights[l][n] = RandomArray(l == 0 ? this.inputLayer : layers[l - 1]);
        }
    }

    public float[] Compute(float[] inputs) {
        for (int l = 0; l < layers.Length; l++) inputs = ComputeLayer(inputs, l);
        return inputs;
    }
    private float[] ComputeLayer(float[] inputs, int layer) {
        float[] results = new float[layers[layer]];
        List<Task> cells = new();
        for (int n = 0; n < results.Length; n++) {
            int neuron = n;
            cells.Add(Task.Run(() => results[neuron] = Activation(ComputeNeuron(inputs, layer, neuron))));
        }
        Task.WaitAll(cells.ToArray());
        return results;
    }
    private float ComputeNeuron(float[] inputs, int layer, int neuron){
        float sum = biaises[layer][neuron];
        for (int p = 0; p < inputs.Length; p++) {
            sum += inputs[p] * weights[layer][neuron][p];
        }
        return sum;
    }

    private static float Activation(float res) => res < 0 ? 0 : res;

    public static float[] RandomArray(int length){
        float[] array = new float[length];
        lock (Connect4.RandLock) {
            for (int i = 0; i < array.Length; i++) array[i] = (Connect4.Random.NextSingle() - 0.5f)*4;
        }
        return array;
    }

    public void Save(string path){
        List<string> lines = new() {
            $"{inputLayer} {string.Join(" ", layers)}"
        };
        for (int l = 0; l < layers.Length; l++){
            for (int n = 0; n < layers[l]; n++) lines.Add($"{biaises[l][n]} {string.Join(" ", weights[l][n])}");
            lines.Add("\n");
        }
        File.WriteAllLines(path, lines);
    }

}