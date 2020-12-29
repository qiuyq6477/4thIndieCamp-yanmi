using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
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

[Serializable]
public struct LevelInfo
{
    public string filename;
    public BlockInfo[] blockInfos;
}

[Serializable]
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
        //     type = BLOCKTYPE.Holl,
        //     row = 4,
        //     col = 4,
        //     width = 1,
        //     height = 1,
        // },
        new BlockInfo()
        {
            type = BLOCKTYPE.RotateLeft,
            row = 6,
            col = 6,
            width = 1,
            height = 1,
        },
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
        // new BlockInfo()
        // {
        //     type = BLOCKTYPE.Horizontal,
        //     row = 7,
        //     col = 2,
        //     width = 2,
        //     height = 1,
        // },
        new BlockInfo()
        {
            type = BLOCKTYPE.Horizontal,
            row = 13,
            col = 2,
            width = 2,
            height = 1,
        },
        new BlockInfo()
        {
            type = BLOCKTYPE.Vertical,
            row = 9,
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
    public GameObject background;
    public GameObject MenuPanel;
    public GameObject LevelPanel;
    public GameObject GamePanel;
    public GameObject NextLevelPanel;
    public GameObject content;
    public const int mapWidth = 10;
    public const int mapHeight = 19;
    private int[] data;

    public Transform debug;
    public Transform mapContainer;
    public float piexlPerUnit = 100;
    public float blockSize = 90;
    public float moveUnit;
    public Player player;
    private List<Block> otherBlocks = new List<Block>();
    private List<Vector3> targetBlocks = new List<Vector3>();
    
    public int currentLevel;
    private List<LevelInfo> levelDatas = new List<LevelInfo>();
    
    Stack<Operation> operations = new Stack<Operation>();

    public Material activeBlock;
    public Material unactiveBlock;
    
    private void Awake()
    {
        Instance = this;
        
        moveUnit = blockSize / piexlPerUnit;
        blockSize = blockSize / piexlPerUnit;

        background.SetActive(false);
        
        MenuPanel.SetActive(true);
        LevelPanel.SetActive(false);
        GamePanel.SetActive(false);
        NextLevelPanel.SetActive(false);

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

    public void ResetGame()
    {
        Init();
    }

    public void DoOperation(MOVEDIR op, int angle, Vector3 pos)
    {
        operations.Push(new Operation()
        {
            dir = op,
            angle = angle,
            pos = pos,
        });
    }
    public void DoOperation(MOVEDIR op)
    {
        operations.Push(new Operation()
        {
            dir = op,
            angle = 0,
            pos = Vector3.zero,
        });
    }
    public void UndoOperation()
    {
        if (operations.Count == 0) return;
        player.undo = true;
        var op = operations.Pop();
        if (op.dir == MOVEDIR.RotateLeft || op.dir == MOVEDIR.RotateRight)
        {
            foreach (var block in player.blocks)
            {
                GameManager.Instance.StartCoroutine(block.Rotate(-op.angle, op.pos));
                GameManager.Instance.StartCoroutine(delaymove(op));
            }
        }
        else
        {
            switch (op.dir)
            {
                case MOVEDIR.Left:
                {
                    player.MoveBlock(MOVEDIR.Right);
                }break;
                case MOVEDIR.Right:
                {
                    player.MoveBlock(MOVEDIR.Left);
                }break;
                case MOVEDIR.Up:
                {
                    player.MoveBlock(MOVEDIR.Down);
                }break;
                case MOVEDIR.Down:
                {
                    player.MoveBlock(MOVEDIR.Up);
                }break;
            }
        }
    }

    IEnumerator delaymove(Operation op)
    {
        yield return new WaitForSeconds(0.3f);
        switch (op.dir)
        {
            case MOVEDIR.Left:
            {
                player.MoveBlock(MOVEDIR.Right);
            }break;
            case MOVEDIR.Right:
            {
                player.MoveBlock(MOVEDIR.Left);
            }break;
            case MOVEDIR.Up:
            {
                player.MoveBlock(MOVEDIR.Down);
            }break;
            case MOVEDIR.Down:
            {
                player.MoveBlock(MOVEDIR.Up);
            }break;
        }
    }

    public void StartGame()
    {
        AudioManager.PlayAudioEffectA(Resources.Load<AudioClip>("Audio/点击按钮"));
        MenuPanel.SetActive(false);
        LevelSelect();
    }

    public void BackToLevelSelect()
    {
        AudioManager.PlayAudioEffectA(Resources.Load<AudioClip>("Audio/点击按钮"));
        background.SetActive(false);
        
        MenuPanel.SetActive(false);
        LevelPanel.SetActive(true);
        GamePanel.SetActive(false);
        NextLevelPanel.SetActive(false);
        mapContainer.gameObject.SetActive(false);
        player.CanMove = false;
    }

    public void BackToMainMenu()
    {
        AudioManager.PlayAudioEffectA(Resources.Load<AudioClip>("Audio/点击按钮"));
        background.SetActive(false);
        
        MenuPanel.SetActive(true);
        LevelPanel.SetActive(false);
        GamePanel.SetActive(false);
        NextLevelPanel.SetActive(false);
    }
    
    public void LevelSelect()
    {
        AudioManager.PlayAudioEffectA(Resources.Load<AudioClip>("Audio/点击按钮"));
        foreach (Transform t in content.transform)
        {
            Destroy(t.gameObject);
        }
        LevelPanel.SetActive(true);
        for (int i = 0; i < levelDatas.Count; i++)
        {
            int index = i;
            GameObject go = Instantiate(Resources.Load<GameObject>("LevelSelectItem"));
            go.transform.SetParent(content.transform);
            var text = go.transform.GetComponentInChildren<Text>();
            var button = go.transform.GetComponentInChildren<Button>();
            go.transform.localScale = Vector3.one;
            text.text = levelDatas[i].filename.Substring(0, levelDatas[i].filename.Length-4);
            button.onClick.AddListener(() =>
            {
                background.SetActive(true);
                LevelPanel.SetActive(false);
                GamePanel.SetActive(true);
                currentLevel = index;
                Init();
                AudioManager.PlayAudioEffectA(Resources.Load<AudioClip>("Audio/点击按钮"));
            });
        }
    }
    public void StartGameEditor()
    {
        AudioManager.PlayAudioEffectA(Resources.Load<AudioClip>("Audio/点击按钮"));
        SceneManager.LoadScene("Editor");
    }

    public void NextLevel()
    {
        AudioManager.PlayAudioEffectA(Resources.Load<AudioClip>("Audio/通关成功"));
        player.CanMove = false;
        NextLevelPanel.SetActive(true);
    }

    public void GoToNextLevel()
    {
        AudioManager.PlayAudioEffectA(Resources.Load<AudioClip>("Audio/点击按钮"));
        NextLevelPanel.SetActive(false);
        if (currentLevel + 1 >= levelDatas.Count)
        {
            return;
        }
        currentLevel++;
        // rawData = info.blockInfos;
        Init();
    }
    public void Init()
    {
        mapContainer.gameObject.SetActive(true);
        if (player != null)
        {
            player.RemoveAllBlock();
        }

        foreach (var block in otherBlocks)
        {
            Destroy(block.gameObject);
        }
        otherBlocks.Clear();
        
        data = new int[mapWidth * mapHeight];
        rawData = levelDatas[currentLevel].blockInfos;
        targetBlocks.Clear();
        player = new Player();
        
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
                player.AddBlockImmeditaly(block);
            }
            else 
            {
                if (info.type == BLOCKTYPE.Holl)
                {
                    var points = block.GetAllPoint();
                    foreach (var point in points)
                    {
                        if (!targetBlocks.Contains(point))
                        {
                            targetBlocks.Add(point);
                        }
                    }
                }

                if (info.type == BLOCKTYPE.Fat || info.type == BLOCKTYPE.Horizontal || info.type == BLOCKTYPE.Vertical)
                {
                    block.GetComponentInChildren<SpriteRenderer>().material = unactiveBlock;
                }
                otherBlocks.Add(block);
            }
        }

        InitDebugInfo();
    }
    private Text[] debugText = new Text[mapWidth * mapHeight];
    public void InitDebugInfo()
    {
        for (int i = 0; i < data.Length; i++)
        {
            ArrayIndexToCoordinate(i, out int row, out int col);
            if (debug)
            {
                if (debugText[i])
                {
                    Destroy(debugText[i].gameObject);
                }
                GameObject go = Instantiate(Resources.Load<GameObject>("debug"));
                go.transform.SetParent(debug);
                Text text = go.GetComponent<Text>();
                text.text = "<color=#00ffffff><size=30>" + data[i] + "</size></color>\n<size=20>" + row + "," + col + ","+i+
                            "</size>";
                debugText[i] = text;
            }
        }
    }
    public void UpdateDebugInfo()
    {
        for (int i = 0; i < debugText.Length; i++)
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
        AudioManager.PlayBackground(Resources.Load<AudioClip>("Audio/游戏中的背景音乐"));
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

    public List<Block> CheckCollision(Block playerBlock, List<BLOCKTYPE> exclude = null)
    {
        List<Block> col = new List<Block>();
        for (int j = 0; j < otherBlocks.Count; j++)
        {
            if (playerBlock.checkSideCollision(otherBlocks[j]))
            {
                col.Add(otherBlocks[j]);
            }
        }

        if (exclude != null)
        {
            col.RemoveAll((block) =>
            {
                return exclude.Contains(block.type);
            });
        }
        return col;
    }

    public List<Block> CheckCollision2(Block playerBlock, List<BLOCKTYPE> exclude = null)
    {
        List<Block> col = new List<Block>();
        for (int j = 0; j < otherBlocks.Count; j++)
        {
            if (playerBlock.checkSideCollision2(otherBlocks[j]))
            {
                col.Add(otherBlocks[j]);
            }
        }

        if (exclude != null)
        {
            col.RemoveAll((block) =>
            {
                return exclude.Contains(block.type);
            });
        }
        return col;
    }

    public bool IsGameOver()
    {
        List<Vector3> list = player.GetAllPoint();
        if (list.Count < targetBlocks.Count)
        {
            return false;
        }

        int count = 0;
        foreach (var point in list)
        {
            PositionToCoordinate(point, out int row1, out int col1);
            foreach (var point2 in targetBlocks)
            {
                PositionToCoordinate(point2, out int row2, out int col2);
                if (row1 == row2 && col1 == col2)
                {
                    count++;
                }
            }
        }

        if (count != 0 && targetBlocks.Count == count)
        {
            return true;
        }

        return false;
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

    public BLOCKTYPE GetMapData(Vector3 pos)
    {
        PositionToCoordinate(pos, out int row, out int col);
        CoordinateToArrayIndex(row, col, out int index);
        return (BLOCKTYPE)data[index];
    }
}
