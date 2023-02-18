namespace Connect4.Core;

public interface IPlayer{
    void Setup(Connect4 game, int players, int w, int h);
    int Place();
    void OnPlacement(bool self, int player, int col);
}