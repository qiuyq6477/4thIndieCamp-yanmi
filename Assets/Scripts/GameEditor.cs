using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    private bool isDelet;
    Dictionary<int, GameObject> golist = new Dictionary<int, GameObject>();
    private void Awake()
    {
        blockSize = blockSize / piexlPerUnit;
        
        string path = Application.persistentDataPath + "/Resources";
        if (Directory.Exists(path))
        {
            DirectoryInfo direction = new DirectoryInfo(path);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                Debug.Log("Name:" + files[i].Name);
                Debug.Log("FullName:" + files[i].FullName);
                Debug.Log("DirectoryName:" + files[i].DirectoryName);
                StreamReader reader = new StreamReader(Application.persistentDataPath+"/Resources/"+files[i].Name, false);
                try
                {
                    var text = reader.ReadToEnd();
                    LevelInfo info = JsonUtility.FromJson<LevelInfo>(text);
                    levelDatas.Add(info);
                }
                finally
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        var textAssets = Resources.LoadAll<TextAsset>("");
        foreach (var textAsset in textAssets)
        {
            LevelInfo info = JsonUtility.FromJson<LevelInfo>(textAsset.text);
            levelDatas.Add(info);
        }
    }
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Open PersistentDataPath")]
    static void Open(){
        UnityEditor.EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
#endif
    public void OnBackBtnClick()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnDeleteBlockBtnClick()
    {
        if (hold)
        {
            Destroy(hold.gameObject);
            hold = null;
        }

        isDelet = true;
    }
    
    private List<LevelInfo> levelDatas = new List<LevelInfo>();
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
    public void OnLoadBtnClick()
    {
        bool finddata = false;
        foreach (var levelData in levelDatas)
        {
            if (levelData.filename == filename.text)
            {
                rawData = new List<BlockInfo>(levelData.blockInfos);
                finddata = true;
                break;
            }
        }

        if (!finddata) return;
        
        mapContainer.gameObject.SetActive(true);

        var ie = golist.GetEnumerator();
        while (ie.MoveNext())
        {
            Destroy(ie.Current.Value);
        }
        ie.Dispose();
        
        for (int i = 0; i < rawData.Count; i++)
        {
            BlockInfo info = rawData[i];
            
            GameObject go = Instantiate(Resources.Load<GameObject>(prefabDir[(int)info.type]));
            float x = info.col * blockSize + blockSize / 2;
            float y = info.row * blockSize + blockSize / 2;
            go.transform.SetParent(mapContainer);
            go.transform.localPosition = new Vector3(x, y, 0);
            PositionToCoordinate( go.transform.localPosition, out int row, out int col);
            CoordinateToArrayIndex(row, col, out int index);
            golist[index] = go;

        }
    }
    public void OnSaveBtnClick()
    {
        var str = JsonUtility.ToJson(new LevelInfo()
        {
            filename = filename.text,
            blockInfos = rawData.ToArray(),
        });
        
        StreamWriter writer = new StreamWriter(GetPath()+"/levels/"+filename.text, false);
        try
        {
            writer.Write(str);
        }
        finally
        {
            writer.Close();
            writer.Dispose();
        }
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
    string GetPath()
    {
        string path = "";
#if UNITY_EDITOR
        path = Application.dataPath+"/Resources";
#else
        path = Application.persistentDataPath+"/Resources";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
#endif
        return path;
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
                isDelet = false;
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
                PositionToCoordinate(hold.transform.localPosition, out int row, out int col);
                if (!isDelet)
                {
                    BlockInfo info = new BlockInfo();
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
                    CoordinateToArrayIndex(row, col, out int index);
                    golist[index] = go;
                }
                
            }
        }
        else
        {
            Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1);
            PositionToCoordinate(mapContainer.InverseTransformPoint(Camera.main.ScreenToWorldPoint(pos)), out int row, out int col);
            int i = 0;
            for (; i < rawData.Count; i++)
            {
                if (rawData[i].col == col && rawData[i].row == row)
                {
                    break;
                }
            }

            if (i != rawData.Count)
            {
                CoordinateToArrayIndex(row, col, out int index);
                Destroy(golist[index].gameObject);
                golist.Remove(index);
                
                rawData.RemoveAt(i);
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
