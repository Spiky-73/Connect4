namespace Connect4.Core;

public interface IPlayer{
    void Setup(int players, int w, int h);
    int Place();
    void OnPlacement(bool self, int player, int col);
}