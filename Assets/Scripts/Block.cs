using System.Collections.Generic;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;

public class Block : MonoBehaviour
{
    [HideInInspector]
    public int width, height;
    [HideInInspector]
    public Player player;

    [HideInInspector] private BLOCKTYPE type;
    List<Vector3> blockPos = new List<Vector3>();

    public void Init(Player p, BLOCKTYPE t, int w, int h)
    {
        player = p;
        width = w;
        height = h;
        type = t;
    }
    bool checkPointCollision(int x, int y, Block block)
    {
        GameManager.Instance.PositionToCoordinate(block.transform.localPosition, out int x2, out int y2);
        
        Vector2 left_bottom = new Vector2(x2, y2);
        // Vector2 left_top = new Vector2(x2, y2+block.height-1);
        // Vector2 right_bottom = new Vector2(x2+block.width-1, y2);
        Vector2 right_top = new Vector2(x2+block.height-1, y2+block.width-1);
        
        if (x >= left_bottom.x && x <= right_top.x && y >= left_bottom.y &&
            y <= right_top.y)
        {
            return true;
        }
        return false;
    }

    public bool checkSideCollision(Block block)
    {
        GameManager.Instance.PositionToCoordinate(transform.localPosition, out int x1, out int y1);
        x1 -= 1;
        y1 -= 1;
        int w = width + 1;
        int h = height + 1;
        for (int i = 0; i <= w; i++)
        {
            for(int j = 0; j<=h;j++)
            {
                if ((i == 0 && j == 0) ||//左下角
                    (i == w && j == 0) ||//右下角
                    (i == 0 && j == h) ||//左上角
                    (i == w && j == h))//右上角
                {
                    continue;
                }

                if (checkPointCollision(x1 + j, y1 + i, block))
                {
                    return true;
                }
            }
        }
        return false;
    }
    public void Move(MOVEDIR dir)
    {
        float size = GameManager.Instance.blockSize;
        Vector3 delta = Vector3.zero;
        bool isvalid = true;
        bool isCollision = false;
        
        //设置内部方块位置
        blockPos.Clear();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 currentPos = transform.localPosition + new Vector3(size * i, size * j, 0);
                blockPos.Add(currentPos);
            }
        }
        //根据方向排序, 优先检测哪些位置
        blockPos.Sort((pos, pos1) =>
        {
            switch (dir)
            {
                case MOVEDIR.Left:
                {
                    return pos.x > pos1.x ? 1 : -1;
                }
                case MOVEDIR.Right:
                {
                    return pos.x < pos1.x ? 1 : -1;
                }
                case MOVEDIR.Down:
                {
                    return pos.y > pos1.y ? 1 : -1;
                }
                case MOVEDIR.Up:
                {
                    return pos.y < pos1.y ? 1 : -1;
                }
            }
            return 1;
        });
        int checkLength = 0;
        switch (dir)
        {
            case MOVEDIR.Left:
            {
                delta = new Vector3(-GameManager.Instance.moveUnit, 0, 0);
                checkLength = height;
            }break;
            case MOVEDIR.Right:
            {
                delta = new Vector3(GameManager.Instance.moveUnit, 0, 0);
                checkLength = height;
            }break;
            case MOVEDIR.Down:
            {
                delta = new Vector3(0, -GameManager.Instance.moveUnit, 0);
                checkLength = width;
            }break;
            case MOVEDIR.Up:
            {
                delta = new Vector3(0, GameManager.Instance.moveUnit, 0);
                checkLength = width;
            }break;
        }
        for (int i = 0; i < checkLength; i++)
        {
            Vector3 targetPos = blockPos[i] + delta;
            if (!GameManager.Instance.IsValidPosition(targetPos))
            {
                isvalid = false;
                break;
            }
        }
        if (isvalid)
        {
            for (int i = 0; i < blockPos.Count; i++)
            {
                Vector3 targetPos = blockPos[i] + delta;
                GameManager.Instance.SetMapData(targetPos, type);
                GameManager.Instance.SetMapData(blockPos[i], BLOCKTYPE.None);
            }
            transform.localPosition = transform.localPosition + delta;
        }
        
        var col = GameManager.Instance.CheckCollision(this);
        if (col.Count != 0)
        {
            for (int i = 0; i < col.Count; i++)
            {
                if (col[i].type != BLOCKTYPE.Wall && col[i].type != BLOCKTYPE.Holl)
                {
                    GameManager.Instance.RemoveBlock(col[i]);
                    player.AddBlock(col[i]);
                }
            }
        }
    }
}