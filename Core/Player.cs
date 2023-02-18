namespace Connect4.Core;

public class Player : IPlayer {

    public void Setup(Connect4 game, int players, int w, int h){}

    public int Place() {
        Console.Write("Select a column : ");
        return int.Parse(Console.ReadLine()!);
    }

    public void OnPlacement(bool self, int player, int col) { }

}