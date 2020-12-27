using System.Collections.Generic;
using UnityEngine;

public enum MOVEDIR
{
    Left,
    Right,
    Down,
    Up,
}


public class Player : MonoBehaviour
{
    private List<Block> blocks = new List<Block>();
    private List<Block> templist = new List<Block>();
    private int angle = 0;
    private Vector3 pos;
    
    List<Vector3> pointList = new List<Vector3>();

    public bool CanMove = true;
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
    public void RemoveBlock(Block block)
    {
        blocks.Remove(block);
    }
    public void RemoveAllBlock()
    {
        foreach (var block in blocks)
        {
            Destroy(block.gameObject);
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
        
        blocks.AddRange(templist);
        templist.Clear();

        if (angle != 0)
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

            if (rotate)
            {
                AudioManager.PlayAudioEffectA(Resources.Load<AudioClip>("Audio/旋转"));
                foreach (var block in blocks)
                {
                    StartCoroutine(block.Rotate(angle, pos));
                }
            }
            //checkcollision
            angle = 0;
        }

        if (GameManager.Instance.IsGameOver())
        {
            GameManager.Instance.NextLevel();
        }
    }
}
