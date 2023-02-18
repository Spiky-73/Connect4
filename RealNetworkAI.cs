using SharpNeat.Phenomes;

namespace Connect4;

public class RealNetworkAI : Core.IPlayer {

    private int _width, _height;
    private float[] _grid;
    private readonly IBlackBox _network;

    public RealNetworkAI(IBlackBox network) {
        (_width, _height) = (0, 0);
        _network = network;
        _grid = Array.Empty<float>();
    }

    public void Setup(int players, int w, int h) {
        if (w != 7 || h != 6 || players > 2) throw new NotImplementedException();
        _width = w;
        _height = h;
        _grid = new float[_width * _height];
    }

    public int Place() {
        _network.Activate();
        int col = -1;
        for (int c = 0; c < _network.OutputCount; c++) {
            if (_grid[c + (_height - 1) * _width] == 0 && (col == -1 || _network.OutputSignalArray[c] > _network.OutputSignalArray[col])) col = c;
        }
        return col;
    }

    public void OnPlacement(bool self, int player, int col) {
        int y = 0;
        while (_grid[col + _width * y] != 0) y++;
        _grid[col + _width * y] = self ? 1 : -1;
        _network.InputSignalArray[col + _width * y] = -1;
    }

    public static int Fitness(int win) => win switch {
        0 => -1,
        1 => 3,
        2 or _ => -2
    };
}