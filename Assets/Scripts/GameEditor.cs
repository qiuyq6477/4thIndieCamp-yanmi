using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEditor : MonoBehaviour
{
    public Text filename;

    private List<BlockInfo> rawData = new List<BlockInfo>();

    private GameObject hold;
    public Transform mapContainer;
    
    
    public const int mapWidth = 10;
    public const int mapHeight = 19;
    public float piexlPerUnit = 100;
    public float blockSize = 90;

    private void Awake()
    {
        blockSize = blockSize / piexlPerUnit;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnBackBtnClick()
    {
        SceneManager.LoadScene("Main");
    }
    public void OnSaveBtnClick()
    {
        var str = JsonUtility.ToJson(new LevelInfo()
        {
            filename = filename.text,
            blockInfos = rawData.ToArray(),
        });
        StreamWriter writer = new StreamWriter(Application.dataPath+"/Resources/"+filename.text, false);
        try
        {
            writer.Write(str);
        }
        finally
        {
            writer.Close();
            writer.Dispose();
        }
        AssetDatabase.Refresh();
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1);

        if (hold != null)
        {
            AttackBlock(pos);
        }
        if (Input.GetMouseButtonDown (0)) {
            RaycastHit2D hitInfo = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pos), Vector2.zero);
            if(hitInfo)
            {
                var name = hitInfo.transform.gameObject.name;
                Debug.Log(name);
                if (hold != null)
                {
                    Destroy(hold);
                }
                hold = Instantiate(Resources.Load<GameObject>(name));
                hold.transform.SetParent(mapContainer);
                hold.transform.position = mapContainer.TransformVector(Camera.main.ScreenToWorldPoint(pos));
            }
            else
            {
                PlaceBlock();
            }
        }
        
        
    }

    void PlaceBlock()
    {
        if (hold != null)
        {
            if (IsValidPosition(hold.transform.localPosition))
            {
                BlockInfo info = new BlockInfo();
                PositionToCoordinate(hold.transform.localPosition, out int row, out int col);
                var substring = hold.name.Substring(0, hold.name.Length - 7);
                switch (substring)
                {
                    case "MainBlock":
                    {
                        info = new BlockInfo()
                        {
                            type = BLOCKTYPE.Main,
                            row = row,
                            col = col,
                            width = 1,
                            height = 1,
                        };
                    }
                        break;
                    case "HollBlock":
                    {
                        info = new BlockInfo()
                        {
                            type = BLOCKTYPE.Holl,
                            row = row,
                            col = col,
                            width = 1,
                            height = 1,
                        };
                    }
                        break;
                    case "WallBlock":
                    {
                        info = new BlockInfo()
                        {
                            type = BLOCKTYPE.Wall,
                            row = row,
                            col = col,
                            width = 1,
                            height = 1,
                        };
                    }
                        break;
                    case "HorizontalBlock":
                    {
                        info = new BlockInfo()
                        {
                            type = BLOCKTYPE.Horizontal,
                            row = row,
                            col = col,
                            width = 2,
                            height = 1,
                        };
                    }
                        break;
                    case "VerticalBlock":
                    {
                        info = new BlockInfo()
                        {
                            type = BLOCKTYPE.Vertical,
                            row = row,
                            col = col,
                            width = 1,
                            height = 2,
                        };
                    }
                        break;
                    case "RotateLeftBlock":
                    {
                        info = new BlockInfo()
                        {
                            type = BLOCKTYPE.RotateLeft,
                            row = row,
                            col = col,
                            width = 1,
                            height = 1,
                        };
                    }
                        break;
                    case "RotateRightBlock":
                    {
                        info = new BlockInfo()
                        {
                            type = BLOCKTYPE.RotateRight,
                            row = row,
                            col = col,
                            width = 1,
                            height = 1,
                        };
                    }
                        break;
                    case "WaterBlock":
                    {
                        info = new BlockInfo()
                        {
                            type = BLOCKTYPE.Water,
                            row = row,
                            col = col,
                            width = 1,
                            height = 1,
                        };
                    }
                        break;
                    case "FatBlock":
                    {
                        info = new BlockInfo()
                        {
                            type = BLOCKTYPE.Fat,
                            row = row,
                            col = col,
                            width = 2,
                            height = 2,
                        };
                    }
                        break;
                    case "GasBlock":
                    {
                        info = new BlockInfo()
                        {
                            type = BLOCKTYPE.Gas,
                            row = row,
                            col = col,
                            width = 1,
                            height = 1,
                        };
                    }
                        break;
                }

                rawData.Add(info);

                var go = Instantiate(Resources.Load<GameObject>(hold.name.Substring(0, hold.name.Length - 7)));
                go.transform.SetParent(mapContainer);
                go.transform.localPosition = hold.transform.localPosition;
            }
        }
    }

    void AttackBlock(Vector3 pos)
    {
        hold.transform.position = mapContainer.TransformVector(Camera.main.ScreenToWorldPoint(pos));

        if (IsValidPosition(hold.transform.localPosition))
        {
            PositionToCoordinate(hold.transform.localPosition, out int row, out int col); 
            hold.transform.localPosition = CoordinateToPosition(row, col) + new Vector3(blockSize/2, blockSize/2, 0);
        }
    }
    public bool IsValidPosition(Vector3 pos)
    {
        PositionToCoordinate(pos, out int row, out int col);
        if (row < 0 || row >= mapHeight || col < 0 || col >= mapWidth)
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
    public Vector3 CoordinateToPosition(int row, int col)
    {
        var pos = Vector3.zero;
        pos.x = blockSize * col;
        pos.y = blockSize * row;
        return pos;
    }
    public void SetMapData(Vector3 pos, BLOCKTYPE type)
    {
        PositionToCoordinate(pos, out int row, out int col);
        CoordinateToArrayIndex(row, col, out int index);
        // data[index] = (int)type;
    }
}
