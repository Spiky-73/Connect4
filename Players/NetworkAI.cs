namespace Connect4.Players;

public class NetworkAI : IPlayer {

    private int _width, _height;

    private float[] _grid;

    private readonly NeuralNetwork _network;

    public NetworkAI(){
        _network = new(6*7, 32, 32, 32, 7); // TODO load data
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
        int col = 0;
        for (int c = 0; c < outs.Length; c++){
            if(outs[c] > outs[col] && _grid[c+(_height-1)*_width] == 0) col = c;
        }
        return col;
    }

    public void OnPlacement(int player, int col) {
        int y = 0;
        while(_grid[col + _width*y] != 0) y++;
        _grid[col + _width * y] = player;
    }
}