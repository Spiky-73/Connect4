namespace Connect4;

public class Connect4 {
    public static readonly Random Random = new();
    public static readonly object RandLock = new();

    public int Width { get; private set; }
    public int Height { get; private set; }

    private readonly Players.IPlayer[] _players;
    private int _turn;

    private readonly List<int>[] _grid;
    private int _placed;


    public Connect4(int w, int h, params Players.IPlayer[] players) {
        Width = w;
        Height = h;

        _players = players;

        _placed = -1;
        _turn = -1;
        _grid = new List<int>[Width];
    }

    public void PrepareGame(){
        for (int p = 0; p < _players.Length; p++) _players[p].Setup(_players.Length, Width, Height);

        lock (RandLock) _turn = Random.Next(0, _players.Length);
        _placed = 0;
        for (int c = 0; c < Width; c++) _grid[c] = new();
    }

    public int Run(bool display = false) {
        PrepareGame();

        if (display) DisplayGrid();

        while(_placed < Width * Height){
            int col = _players[_turn].Place();

            if (col < 0 || col >= Width || _grid[col].Count >= Height) return (_turn + 1) % _players.Length+1;

            _grid[col].Add(_turn);
            _placed++;
            if(display) DisplayGrid();

            if(Wins(_turn, col)) return _turn+1;
            
            for (int p = 0; p < _players.Length; p++)_players[p].OnPlacement(_turn+1, col);

            _turn = (_turn + 1) % _players.Length;
        }
        return 0;
    }


    private bool Wins(int player, int col){
        foreach((int dx,int dy) in Directions){
            int count = 1;
            int x = col;
            int y = _grid[col].Count - 1;
            do {
                x += dx;
                y += dy;
            } while (0 <= x && x < Width && 0 <= y && y < _grid[x].Count && _grid[x][y] == player && ++count < 4);
            x = col;
            y = _grid[col].Count - 1;
            do {
                x -= dx;
                y -= dy;
            } while (0 <= x && x < Width && 0 <= y && y < _grid[x].Count && _grid[x][y] == player && ++count < 4);
            if(count >= 4) return true;
        }
        return false;
    }
    
    public static readonly (int, int)[] Directions = new (int, int)[]{ (1,0), (0,1), (1,1), (1,-1) };

    public void DisplayGrid(){
        Console.WriteLine();
        for (int r = 0; r < Height; r++) {
            for (int c = 0; c < Width; c++) {
                Console.Write(_grid[c].Count > Height - r - 1 ? (_grid[c][Height - r - 1] + 1).ToString() : ".");
            }
            string ui = r switch {
                1 => $"    Turn {_placed + 1}/{Width * Height}",
                4 => $"    Player {_turn + 1} turn",
                _ => ""
            };
            Console.WriteLine(ui);
        }
    }
}