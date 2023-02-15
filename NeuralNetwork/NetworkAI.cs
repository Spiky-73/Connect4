namespace Connect4.NeuralNetwork;

public class NetworkAI : Core.IPlayer {

    private int _width, _height;
    private float[] _grid;
    private readonly NeuralNetwork _network;

    public NetworkAI(NeuralNetwork network){
        (_width, _height) = (0, 0);
        _network = network;
        _grid = Array.Empty<float>();
    }

    public void Setup(int players, int w, int h) {
        if(w != 7 || h != 6 || players > 2) throw new NotImplementedException();
        _width = w;
        _height = h;
        _grid = new float[_width * _height];
    }

    public int Place() {
        float[] outs = _network.Compute(_grid);
        int col = -1;
        for (int c = 0; c < outs.Length; c++){
            if(_grid[c + (_height - 1) * _width] == 0 && (col == -1 || outs[c] > outs[col])) col = c;
        }
        return col;
    }

    public void OnPlacement(bool self, int player, int col) {
        int y = 0;
        while(_grid[col + _width*y] != 0) y++;
        _grid[col + _width * y] = self ? 1 : -1;
    }

    public static int Fitness(int win) => win switch {
        0 => -1,
        1 => 3,
        2 or _ => -2
    };

    public static int TestNetwork(NeuralNetwork child, NeuralNetwork other){
        int fitness = 0;
        Core.Connect4 game = new(7, 6, new NetworkAI(child), new NetworkAI(other));
        fitness += Fitness(game.Run(0)) + Fitness(game.Run(1));
        return fitness;
    }
}