using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILog : MonoBehaviour
{
    public Text LogText;
    public Button ClearLogButton;


    private void Awake() 
    {
        Application.logMessageReceived += receiveLog;
        ClearLogButton.onClick.AddListener(OnClear);
    }

    private void receiveLog(string condition, string stackTrace, LogType type)
    {
        LogText.text =  LogText.text + "\n" + condition;
    }

    public void OnClear()
    {
        LogText.text = "";
    }

    private void OnDestroy() 
    {
        Application.logMessageReceived -= receiveLog;
    }
}
