using System;
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



public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
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
    
    private int[] data = new[]
    {
        0, 0, 0, 1, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 1, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 3, 3, 3, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    };
    private Transform[] mMap;
    public Transform debug;
    public Transform mapContainer;
    public float piexlPerUnit = 100;
    public float blockSize = 90;
    public int mapWidth = 10;
    public int mapHeight = 19;
    public float moveUnit;
    private Player player;
    private void Awake()
    {
        Instance = this;
        
        player = new Player();

        moveUnit = blockSize / piexlPerUnit;
        blockSize = blockSize / piexlPerUnit;
        for (int i = 0; i < data.Length; i++)
        {
            ArrayIndexToCoordinate(i, out int row, out int col);
            if (data[i] != 0)
            {
                GameObject go = Instantiate(Resources.Load<GameObject>(prefabDir[data[i]]));
                float x = col * blockSize + blockSize / 2;
                float y = row * blockSize + blockSize / 2;
                go.transform.SetParent(mapContainer);
                go.transform.localPosition = new Vector3(x, y, 0);
                Block block = go.AddComponent<Block>();
                if (data[i] == (int)BLOCKTYPE.Main)
                {
                    player.AddBlock(block);
                }
            }
            
            if (debug)
            {
                GameObject go = Instantiate(Resources.Load<GameObject>("debug"));
                go.transform.SetParent(debug);
                Text text = go.GetComponent<Text>();
                text.text = row + ", " + col;
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
        if (data[arrayIndex] == (int) BLOCKTYPE.Main ||
            data[arrayIndex] == (int) BLOCKTYPE.Wall)
        {
            return false;
        } 
        return true;
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
