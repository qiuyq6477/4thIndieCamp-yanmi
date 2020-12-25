﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BLOCKTYPE{
    None,
    Main,
    Holl,
    Wall,
    Horizontal,
    Vertical,
    RotateLeft,
    RotateRight,
    Water,
    Fat,
    Gas,
}

public struct BlockInfo
{
    public BLOCKTYPE type;
    public int row, col;
    public int width, height;
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private BlockInfo[] rawData = new[]
    {
        new BlockInfo()
        {
            type = BLOCKTYPE.Main,
            row = 3,
            col = 3,
            width = 1,
            height = 1,
        },
        // new BlockInfo()
        // {
        //     type = BLOCKTYPE.Main,
        //     row = 4,
        //     col = 4,
        //     width = 1,
        //     height = 1,
        // },
        // new BlockInfo()
        // {
        //     type = BLOCKTYPE.Main,
        //     row = 6,
        //     col = 6,
        //     width = 1,
        //     height = 1,
        // },
        new BlockInfo()
        {
            type = BLOCKTYPE.Wall,
            row = 8,
            col = 8,
            width = 1,
            height = 1,
        },
        new BlockInfo()
        {
            type = BLOCKTYPE.Fat,
            row = 1,
            col = 7,
            width = 2,
            height = 2,
        },
        new BlockInfo()
        {
            type = BLOCKTYPE.Horizontal,
            row = 7,
            col = 2,
            width = 2,
            height = 1,
        },
        new BlockInfo()
        {
            type = BLOCKTYPE.Vertical,
            row = 13,
            col = 2,
            width = 1,
            height = 2,
        },
    };
    private string[] prefabDir = new[]
    {
        "",
        "MainBlock",
        "HollBlock",
        "WallBlock",
        "HorizontalBlock",
        "VerticalBlock",
        "RotateLeftBlock",
        "RotateRightBlock",
        "WaterBlock",
        "FatBlock",
        "GasBlock",
    };
    
    private const string mapdir = "map/";
    public const int mapWidth = 10;
    public const int mapHeight = 19;
    private int[] data = new int[mapWidth * mapHeight];

    private Transform[] mMap;
    public Transform debug;
    public Transform mapContainer;
    public float piexlPerUnit = 100;
    public float blockSize = 90;
    public float moveUnit;
    private Player player;
    private List<Block> otherBlocks = new List<Block>();
    private void Awake()
    {
        Instance = this;
        
        player = new Player();

        moveUnit = blockSize / piexlPerUnit;
        blockSize = blockSize / piexlPerUnit;

        for (int i = 0; i < rawData.Length; i++)
        {
            BlockInfo info = rawData[i];
            for (int w = 0; w < info.width; w++)
            {
                for (int h = 0; h < info.height; h++)
                {
                    CoordinateToArrayIndex(info.row + h, info.col + w, out int index);
                    data[index] = (int)info.type;
                }
            }
            
            GameObject go = Instantiate(Resources.Load<GameObject>(prefabDir[(int)info.type]));
            float x = info.col * blockSize + blockSize / 2;
            float y = info.row * blockSize + blockSize / 2;
            go.transform.SetParent(mapContainer);
            go.transform.localPosition = new Vector3(x, y, 0);
            Block block = go.AddComponent<Block>();
            block.Init(player, info.type, info.width, info.height);
            if(info.type == BLOCKTYPE.Main)
            {
                player.AddBlock(block);
            }
            else
            {
                otherBlocks.Add(block);
            }
        }

        InitDebugInfo();
    }

    private List<Text> debugText = new List<Text>();
    public void InitDebugInfo()
    {
        for (int i = 0; i < data.Length; i++)
        {
            ArrayIndexToCoordinate(i, out int row, out int col);
            if (debug)
            {
                GameObject go = Instantiate(Resources.Load<GameObject>("debug"));
                go.transform.SetParent(debug);
                Text text = go.GetComponent<Text>();
                text.text = "<color=#00ffffff><size=30>" + data[i] + "</size></color>\n<size=20>" + row + "," + col + ","+i+
                            "</size>";
                debugText.Add(text);
            }
        }
    }
    public void UpdateDebugInfo()
    {
        for (int i = 0; i < debugText.Count; i++)
        {
            ArrayIndexToCoordinate(i, out int row, out int col);
            if (debug)
            {
                Text text = debugText[i];
                text.text = "<color=#00ffffff><size=30>" + data[i] + "</size></color>\n<size=20>" + row + "," + col + ","+i+
                            "</size>";
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            player.MoveBlock(MOVEDIR.Left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            player.MoveBlock(MOVEDIR.Right);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            player.MoveBlock(MOVEDIR.Up);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            player.MoveBlock(MOVEDIR.Down);
        }
    }

    public bool IsValidPosition(Vector3 pos)
    {
        PositionToCoordinate(pos, out int row, out int col);
        if (row < 0 || row >= mapHeight || col < 0 || col >= mapWidth)
        {
            return false;
        }

        CoordinateToArrayIndex(row, col, out int arrayIndex);
        if (data[arrayIndex] == (int) BLOCKTYPE.None ||
            data[arrayIndex] == (int) BLOCKTYPE.Holl)
        {
            return true;
        } 
        return false;
    }

    public List<Block> CheckCollision(Block playerBlock)
    {
        List<Block> col = new List<Block>();
        for (int j = 0; j < otherBlocks.Count; j++)
        {
            if (playerBlock.checkSideCollision(otherBlocks[j]))
            {
                col.Add(otherBlocks[j]);
            }
        }
        return col;
    }

    public void RemoveBlock(Block block)
    {
        otherBlocks.Remove(block);
    }
    public void RemoveBlock(List<Block> blocks)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            otherBlocks.Remove(blocks[i]);
        }
    }
    public void ArrayIndexToCoordinate(int arrayIndex, out int row, out int col)
    {
        row = -1;
        col = -1;
        row = arrayIndex / mapWidth;
        col = arrayIndex % mapWidth;
    }
    public void CoordinateToArrayIndex(int row, int col, out int arrayIndex)
    {
        arrayIndex = row * mapWidth + col;
    }
    public void PositionToCoordinate(Vector3 pos, out int row, out int col)
    {
        row = -1;
        col = -1;
        row = (int)Mathf.Floor(pos.y / blockSize);
        col = (int)Mathf.Floor(pos.x / blockSize);
    }

    public void SetMapData(Vector3 pos, BLOCKTYPE type)
    {
        PositionToCoordinate(pos, out int row, out int col);
        CoordinateToArrayIndex(row, col, out int index);
        data[index] = (int)type;
    }
}