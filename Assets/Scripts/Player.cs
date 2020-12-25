using System.Collections.Generic;
using UnityEngine;

public enum MOVEDIR
{
    Left,
    Right,
    Down,
    Up,
}


public class Player
{
    private List<Block> blocks = new List<Block>();

    public void AddBlock(Block block)
    {
        blocks.Add(block);
    }

    public void RemoveBlock(Block block)
    {
        blocks.Remove(block);
    }

    public void MoveBlock(MOVEDIR dir)
    {
        blocks.Sort((block, block1) =>
        {
            switch (dir)
            {
                case MOVEDIR.Left:
                {
                    return block.transform.localPosition.x > block1.transform.localPosition.x ? 1 : -1;
                }
                case MOVEDIR.Right:
                {
                    return block.transform.localPosition.x < block1.transform.localPosition.x ? 1 : -1;
                }
                case MOVEDIR.Down:
                {
                    return block.transform.localPosition.y > block1.transform.localPosition.y ? 1 : -1;
                }
                case MOVEDIR.Up:
                {
                    return block.transform.localPosition.y < block1.transform.localPosition.y ? 1 : -1;
                }
            }
            return 1;
        });
        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].Move(dir);
        }
    }
}
