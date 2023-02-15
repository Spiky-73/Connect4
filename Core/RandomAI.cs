namespace Connect4.Core;

public class RandomAI : IPlayer {

    private int[] _placed = Array.Empty<int>();
    private int _height;

    public void Setup(int players, int w, int h) {
        _placed = new int[w];
        _height = h;
    }

    public void OnPlacement(bool self, int player, int col) => _placed[col]++;

    public int Place(){
        int col = 0;
        List<int> free = new();
        for (int c = 0; c < _placed.Length; c++){
            if(_placed[c] < _height) free.Add(c);
        }
        lock (Utility.RNGLock){
            col = free[Utility.Random.Next(free.Count)];
        }
        return col;
    }

}