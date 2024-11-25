using System.Collections;
using System.Collections.Generic;

public class MazeSpace
{
    public int Row {  get; private set; }
    public int Column { get; private set; }

    public MazeSpace(int row, int column)
    {
        Row = row;
        Column = column;
    }
}
