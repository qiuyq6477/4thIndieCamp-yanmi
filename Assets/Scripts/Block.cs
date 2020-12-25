using System;
using UnityEngine;

    public class Block : MonoBehaviour
    {
        public int row, col;
        public void Move(MOVEDIR dir)
        {
            Vector3 targetPos = transform.localPosition;
            switch (dir)
            {
                case MOVEDIR.Left:
                {
                    targetPos = transform.localPosition + new Vector3(-GameManager.Instance.moveUnit, 0, 0);
                }break;
                case MOVEDIR.Right:
                {
                    targetPos = transform.localPosition + new Vector3(GameManager.Instance.moveUnit, 0, 0);
                }break;
                case MOVEDIR.Down:
                {
                    targetPos = transform.localPosition + new Vector3(0, -GameManager.Instance.moveUnit, 0);
                }break;
                case MOVEDIR.Up:
                {
                    targetPos = transform.localPosition + new Vector3(0, GameManager.Instance.moveUnit, 0);
                }break;
            }

            if (GameManager.Instance.IsValidPosition(targetPos))
            {
                GameManager.Instance.SetMapData(transform.localPosition, BLOCKTYPE.None);
                transform.localPosition = targetPos;
                GameManager.Instance.SetMapData(targetPos, BLOCKTYPE.Main);
            }
        }
        // void OnGUI()  
        // {  
        //     var pos = Camera.main.WorldToScreenPoint(transform.localPosition);
        //     GUI.Label(new Rect(Screen.width - pos.x, Screen.height - pos.y,90,90),"Top Left");  
        // } 
        
    }
