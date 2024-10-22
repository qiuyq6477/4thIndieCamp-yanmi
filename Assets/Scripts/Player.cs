﻿using System.Collections.Generic;
using UnityEngine;

public enum MOVEDIR
{
    Left,
    Right,
    Down,
    Up,
    RotateLeft,
    RotateRight,
}

public struct Operation
{
    public MOVEDIR dir;
    public int angle;
    public Vector3 pos;
}

public class Player 
{
    public List<Block> blocks = new List<Block>();
    private List<Block> templist = new List<Block>();
    private int angle = 0;
    private Vector3 pos;
    
    List<Vector3> pointList = new List<Vector3>();

    public bool CanMove = true;
    public bool undo = false;

    private Vector3 lastRotatePos;
    private bool lastRotate;
    
    public List<Vector3> GetAllPoint()
    {
        pointList.Clear();
        foreach (var block in blocks)
        {
            pointList.AddRange(block.GetAllPoint());
        }

        return pointList;
    }
    
    public void AddBlock(Block block)
    {
        if(!templist.Contains(block))
            templist.Add(block);
    }
    public void AddBlockImmeditaly(Block block)
    {
        blocks.Add(block);
    }
    public void RemoveBlock(Block block)
    {
        blocks.Remove(block);
    }
    public void RemoveAllBlock()
    {
        foreach (var block in blocks)
        {
            GameObject.Destroy(block.gameObject);
        }
        blocks.Clear();
    }
    public void Rotate(int a, Vector3 p)
    {
        angle = a;
        pos = p;
    }
    public void MoveBlock(MOVEDIR dir)
    {
        if (!CanMove) return;
        AudioManager.PlayAudioEffectA(Resources.Load<AudioClip>("Audio/移动"));
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
        GameManager.Instance.UpdateDebugInfo();

        if (templist.Count != 0)
        {
            var collionList = new List<Block>();
            var waitCheck = new List<Block>(templist);
            blocks.AddRange(templist);
            templist.Clear();
checkCollision:
            foreach (var block in waitCheck)
            {
                var col = GameManager.Instance.CheckCollision(block);
                for (int i = 0; i < col.Count; i++)
                {
                    if (col[i].type != BLOCKTYPE.Wall && col[i].type != BLOCKTYPE.Holl && col[i].type != BLOCKTYPE.RotateLeft && col[i].type != BLOCKTYPE.RotateRight)
                    {
                        GameManager.Instance.RemoveBlock(col[i]);
                        collionList.Add(col[i]);
                        col[i].transform.GetComponentInChildren<SpriteRenderer>().material =
                            GameManager.Instance.activeBlock;
                    }
                }
            }

            if (collionList.Count != 0)
            {
                blocks.AddRange(collionList);
                waitCheck.AddRange(collionList);
                collionList.Clear();
                goto checkCollision;
            }
        }
        
        if(!undo)
            GameManager.Instance.DoOperation(dir);
        
        if (angle != 0)
        {
            if (!lastRotate || lastRotatePos != pos)
            {
                var rotate = true;
                foreach (var block in blocks)
                {
                    if (!block.checkRotation(angle, pos))
                    {
                        rotate = false;
                        break;
                    }
                }
                lastRotate = rotate;
                lastRotatePos = pos;
                if (rotate)
                {
                    AudioManager.PlayAudioEffectA(Resources.Load<AudioClip>("Audio/旋转"));
                    foreach (var block in blocks)
                    {
                        GameManager.Instance.StartCoroutine(block.Rotate(angle, pos));
                    }
                    if(!undo)
                        GameManager.Instance.DoOperation(angle>0?MOVEDIR.RotateRight : MOVEDIR.RotateLeft, angle, pos);
                    //checkcollision
                    
                }
            }
            
            angle = 0;
        }
        else
        {
            lastRotate = false;
        }
        if (GameManager.Instance.IsGameOver())
        {
            GameManager.Instance.NextLevel();
        }

        undo = false;
    }
}
