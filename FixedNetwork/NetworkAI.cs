namespace Connect4.FixedNetwork;

public class NetworkAI : Core.IPlayer {

    private int _width, _height;
    private float[] _grid;
    private readonly Network _network;

    public NetworkAI(Network network){
        (_width, _height) = (0, 0);
        _network = network;
        _grid = Array.Empty<float>();
    }

    public void Setup(Core.Connect4 game, int players, int w, int h) {
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

    public static int TestNetwork(Network child, Network other){
        int fitness = 0;
        Core.Connect4 game = new(7, 6, new NetworkAI(child), new NetworkAI(other));
        fitness += Fitness(game.Run(0)) + Fitness(game.Run(1));
        return fitness;
    }

    public static void Test() {

        (Network, int)[] roots = new (Network, int)[20];
        for (int i = 0; i < roots.Length; i++) roots[i] = (new(42, 16, 16, 16, 7), 0);

        int gen = 0;
        while (gen < 1000) {
            roots = Mutator.NextGeneration(gen, roots, 250, TestNetwork);
            gen++;
        }
        foreach ((Network, int) network in roots) {
            Core.Connect4 game = new(7, 6, new Core.Player(), new NetworkAI(network.Item1));
            game.Run(display: true);
        }
    }
}