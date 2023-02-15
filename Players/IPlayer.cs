namespace Connect4.Players;

public interface IPlayer{
    void Setup(int players, int w, int h);
    int Place();
    void OnPlacement(int player, int col);
}