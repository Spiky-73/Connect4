namespace Connect4.Core;

public class Connect4 {

    public int Width { get; private set; }
    public int Height { get; private set; }

    public readonly IPlayer[] Players;
    private int _turn;

    public readonly List<int>[] Grid;
    private int _placed;


    public Connect4(int w, int h, params IPlayer[] players) {
        Width = w;
        Height = h;

        Players = players;

        _placed = -1;
        _turn = -1;
        Grid = new List<int>[Width];
    }

    public void PrepareGame(int turn = -1){
        for (int p = 0; p < Players.Length; p++) Players[p].Setup(this, Players.Length, Width, Height);

        if(turn == -1) lock (Utility.RNGLock) _turn = Utility.Random.Next(0, Players.Length);
        else _turn = turn;

        _placed = 0;
        for (int c = 0; c < Width; c++) Grid[c] = new();
    }

    public int Run(int turn = -1, bool display = false) {
        PrepareGame(turn);


        while(_placed < Width * Height){
            if(display) DisplayGrid();
            int col = Players[_turn].Place();

            if (col < 0 || col >= Width || Grid[col].Count >= Height)
                return (_turn + 1) % Players.Length+1;

            Grid[col].Add(_turn);
            _placed++;

            if (Wins(_turn, col)) {
                if (display) DisplayGrid();
                return _turn + 1;
            }

            for (int p = 0; p < Players.Length; p++)Players[p].OnPlacement(_placed == turn, _turn+1, col);

            _turn = (_turn + 1) % Players.Length;
        }
        if (display) DisplayGrid();
        return 0;
    }

    public bool ColumnFull(int col) => Grid[col].Count >= Height;


    private bool Wins(int player, int col){
        foreach((int dx,int dy) in Directions){
            int count = 1;
            int x = col;
            int y = Grid[col].Count - 1;
            do {
                x += dx;
                y += dy;
            } while (0 <= x && x < Width && 0 <= y && y < Grid[x].Count && Grid[x][y] == player && ++count < 4);
            x = col;
            y = Grid[col].Count - 1;
            do {
                x -= dx;
                y -= dy;
            } while (0 <= x && x < Width && 0 <= y && y < Grid[x].Count && Grid[x][y] == player && ++count < 4);
            if(count >= 4) return true;
        }
        return false;
    }
    
    public static readonly (int, int)[] Directions = new (int, int)[]{ (1,0), (0,1), (1,1), (1,-1) };

    public void DisplayGrid(){
        Console.WriteLine();
        for (int r = 0; r < Height; r++) {
            for (int c = 0; c < Width; c++) {
                Console.Write(Grid[c].Count > Height - r - 1 ? (Grid[c][Height - r - 1] + 1).ToString() : ".");
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