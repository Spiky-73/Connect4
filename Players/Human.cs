namespace Connect4.Players;

public class Human : IPlayer {

    public void Setup(int players, int w, int h){}

    public int Place() {
        Console.Write("Select a column : ");
        return int.Parse(Console.ReadLine()!);
    }

    public void OnPlacement(int player, int col) { }

}