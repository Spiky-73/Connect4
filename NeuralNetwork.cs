namespace Connect4;

public class NeuralNetwork {

    public int _inputCount;          // layer
    public int[] _layers;        // layer
    public float[][] _biaises;   // layer, neuron
    public float[][][] _weights; // layer, neuron*link

    public NeuralNetwork(int inputCount, params int[] layers) {
        _inputCount = inputCount;
        _layers = layers;
        _biaises = new float[layers.Length][];
        _weights = new float[layers.Length][][];

        for (int l = 0; l < layers.Length; l++) {
            _biaises[l] = RandomArray(layers[l]); // new float[layers[l]];
            _weights[l] = new float[layers[l]][];
            for (int p = 0; p < layers[l]; p++) _weights[l][p] = RandomArray(l == 0 ? _inputCount : layers[l-1]); // new float[layers[l - 1]];
        }
    }

    public float[] Compute(float[] inputs) {
        for (int l = 0; l < _layers.Length; l++) inputs = ComputeLayer(inputs, l);
        return inputs;
    }
    private float[] ComputeLayer(float[] inputs, int layer) {
        float[] results = new float[_layers[layer]];
        List<Task> cells = new();
        for (int n = 0; n < results.Length; n++) {
            int neuron = n;
            cells.Add(Task.Run(() => results[neuron] = Activation(ComputeNeuron(inputs, layer, neuron))));
        }
        Task.WaitAll(cells.ToArray());
        return results;
    }
    private float ComputeNeuron(float[] inputs, int layer, int neuron){
        float sum = _biaises[layer][neuron];
        for (int p = 0; p < inputs.Length; p++) {
            sum += inputs[p] * _weights[layer][neuron][p];
        }
        return sum;
    }

    private static float Activation(float res) => res < 0 ? 0 : res;

    public static float[] RandomArray(int length){
        float[] array = new float[length];
        lock (Connect4.RandLock) {
            for (int i = 0; i < array.Length; i++) array[i] = Connect4.Random.NextSingle() - 0.5f;
        }
        return array;
    }

}