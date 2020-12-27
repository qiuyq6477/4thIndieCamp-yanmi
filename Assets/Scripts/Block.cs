using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [HideInInspector]
    public int width, height;
    
    [HideInInspector]
    public int temp_width, temp_height;
    
    [HideInInspector]
    public Player player;

    [HideInInspector] 
    public BLOCKTYPE type;
    
    List<Vector3> pointList = new List<Vector3>();
    public List<Vector3> GetAllPoint()
    {
        pointList.Clear();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 currentPos = transform.localPosition + GameManager.Instance.blockSize * i * transform.right +  GameManager.Instance.blockSize * j * transform.up;
                pointList.Add(currentPos);
            }
        }
        return pointList;
    }

    public void Init(Player p, BLOCKTYPE t, int w, int h)
    {
        player = p;
        width = w;
        temp_width = w;
        temp_height = h;
        height = h;
        type = t;
    }
    bool checkPointCollision(int x, int y, Block block)
    {
        GameManager.Instance.PositionToCoordinate(block.transform.localPosition, out int x2, out int y2);
        
        Vector2 left_bottom = new Vector2(x2, y2);
        // Vector2 left_top = new Vector2(x2, y2+block.height-1);
        // Vector2 right_bottom = new Vector2(x2+block.width-1, y2);
        Vector2 right_top = new Vector2(x2+block.temp_height-1, y2+block.temp_width-1);
        
        if (x >= left_bottom.x && x <= right_top.x && y >= left_bottom.y &&
            y <= right_top.y)
        {
            return true;
        }
        return false;
    }

    public bool checkSideCollision(Block block)
    {
        var size = GameManager.Instance.blockSize;
        int w = temp_width + 1;
        int h = temp_height + 1;
        var localPosition = transform.localPosition;
        localPosition += -transform.right * GameManager.Instance.blockSize;
        localPosition += -transform.up * GameManager.Instance.blockSize;;
        List<Vector3> posList = new List<Vector3>();
        for (int i = 0; i <= w; i++)
        {
            for (int j = 0; j <= h; j++)
            {
                if ((i == 0 && j == 0) ||//左下角
                    (i == w && j == 0) ||//右下角
                    (i == 0 && j == h) ||//左上角
                    (i == w && j == h))//右上角
                {
                    continue;
                }
                Vector3 currentPos = localPosition + size * i * transform.right +  size * j * transform.up;
                posList.Add(currentPos);
            }
        }

        foreach (var pos in posList)
        {
            GameManager.Instance.PositionToCoordinate(pos, out int x1, out int y1);
            if (checkPointCollision(x1, y1, block))
            {
                return true;
            }
        }
                
        return false;
    }
    public bool checkSideCollision2(Block block)
    {
        var size = GameManager.Instance.blockSize;
        int w = temp_width;
        int h = temp_height;
        var localPosition = transform.localPosition;
        List<Vector3> posList = new List<Vector3>();
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                Vector3 currentPos = localPosition + size * i * transform.right +  size * j * transform.up;
                posList.Add(currentPos);
            }
        }

        foreach (var pos in posList)
        {
            GameManager.Instance.PositionToCoordinate(pos, out int x1, out int y1);
            if (checkPointCollision(x1, y1, block))
            {
                return true;
            }
        }
                
        return false;
    }
    public void Move(MOVEDIR dir)
    {
        Vector3 delta = Vector3.zero;
        bool isCollision = false;
        
        int checkLength = 0;
        switch (dir)
        {
            case MOVEDIR.Left:
            {
                delta = new Vector3(-GameManager.Instance.moveUnit, 0, 0);
                checkLength = temp_height;
            }break;
            case MOVEDIR.Right:
            {
                delta = new Vector3(GameManager.Instance.moveUnit, 0, 0);
                checkLength = temp_height;
            }break;
            case MOVEDIR.Down:
            {
                delta = new Vector3(0, -GameManager.Instance.moveUnit, 0);
                checkLength = temp_width;
            }break;
            case MOVEDIR.Up:
            {
                delta = new Vector3(0, GameManager.Instance.moveUnit, 0);
                checkLength = temp_width;
            }break;
        }

        var isvalid = checkPositionValid(dir, checkLength, delta, out List<Vector3> blockPos);
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
            int angle = 0;
            Vector3 pos = Vector3.zero;
            for (int i = 0; i < col.Count; i++)
            {
                if (col[i].type != BLOCKTYPE.Wall && col[i].type != BLOCKTYPE.Holl && col[i].type != BLOCKTYPE.RotateLeft && col[i].type != BLOCKTYPE.RotateRight)
                {
                    GameManager.Instance.RemoveBlock(col[i]);
                    player.AddBlock(col[i]);
                }

                if (col[i].type == BLOCKTYPE.RotateLeft)
                {
                    angle += 90;
                    if (angle > 0) pos = col[i].transform.position;
                }
                if (col[i].type == BLOCKTYPE.RotateRight)
                {
                    angle -= 90;
                    if (angle < 0) pos = col[i].transform.position;
                }
            }

            if (angle != 0)
            {
                player.Rotate(angle, pos);
            }
        }
    }
    public bool checkRotation(int angle, Vector3 pos)
    {
        float size = GameManager.Instance.blockSize;
        bool canRotate = true;
        transform.RotateAround(pos, Vector3.forward, angle);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 currentPos = transform.localPosition + size * i * transform.right +  size * j * transform.up;
                GameManager.Instance.PositionToCoordinate(currentPos, out int row, out int col);
                if (row < 0 || row >= GameManager.mapHeight || col < 0 || col >= GameManager.mapWidth)
                {
                    canRotate = false;
                    break;
                }
                var collsionList = GameManager.Instance.CheckCollision2(this,new List<BLOCKTYPE> {
                    BLOCKTYPE.RotateLeft,
                    BLOCKTYPE.RotateRight,
                });
                if (collsionList.Count > 0)
                {
                    canRotate = false;
                    break;
                }
            }
        }
        transform.RotateAround(pos, Vector3.forward, -angle);
        return canRotate;
    }
    public IEnumerator Rotate(int angle, Vector3 pos)
    {
        yield return new WaitForSeconds(0.2f);
        
        float size = GameManager.Instance.blockSize;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 currentPos = transform.localPosition + size * i * transform.right +  size * j * transform.up;
                GameManager.Instance.SetMapData(currentPos, BLOCKTYPE.None);
            }
        }
        // Vector3 position = transform.position;
        // Vector3 vector3 = Quaternion.AngleAxis(angle, Vector3.forward) * (position - pos);
        // transform.position = pos + vector3;
        // transform.Rotate(Vector3.forward, angle, Space.World);
        transform.RotateAround(pos, Vector3.forward, angle);

        StartCoroutine(delay());
    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(0.2f);

        int temp = temp_width;
        temp_width = temp_height;
        temp_height = temp; 
        float size = GameManager.Instance.blockSize;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 currentPos = transform.localPosition + size * i * transform.right +  size * j * transform.up;
                GameManager.Instance.SetMapData(currentPos, type);
            }
        }
        
        GameManager.Instance.UpdateDebugInfo();
    }

    bool checkPositionValid(MOVEDIR dir, int checkLength, Vector3 delta, out List<Vector3> blockPos)
    {
        var size = GameManager.Instance.blockSize;
        //设置内部方块位置
        blockPos = GetAllPoint();
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
        
        for (int i = 0; i < checkLength; i++)
        {
            Vector3 targetPos = blockPos[i] + delta;
            if (!GameManager.Instance.IsValidPosition(targetPos))
            {
                return false;
            }
        }

        return true;
    }


}