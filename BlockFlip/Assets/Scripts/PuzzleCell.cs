using UnityEngine;
using UnityEngine.UI;

public enum CellState
{
    White,
    Black
}

public class PuzzleCell : MonoBehaviour
{
    [SerializeField] private Image image;

    public int X { get; private set; }
    public int Y { get; private set; }

    public CellState State { get; private set; }

    public void Initialize(int x, int y, CellState initialState)
    {
        X = x;
        Y = y;
        SetState(initialState);
    }

    public void Flip()
    {
        SetState(State == CellState.White ? CellState.Black : CellState.White);
    }

    public void SetState(CellState state)
    {
        State = state;

        if (image != null)
        {
            image.color = state == CellState.White
                ? Color.white
                : Color.black;
        }
    }
}