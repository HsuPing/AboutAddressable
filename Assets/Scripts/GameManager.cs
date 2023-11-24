using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public List<string> CheckForCatalogUpdatesList = null;
    public List<string> UpdateCatalogStringList = null;

    public List<object> UpdateCatalogsList = null;

    private const string PRELOAD_LABEL = "preload";

    public Image ProgressBar;

    bool hotfixChecking = false;
    public AssetReferenceGameObject UIHotfixAR;
    public Transform CanvasTrans;
    public GameObject UIHotfixGO;

    public async void Hotfix()
    {
        if(hotfixChecking)
            return;
        hotfixChecking = true;
        HotfixProcessor hotfixProcessor = new HotfixProcessor();
        hotfixProcessor.DownloadStartCallback = () => Debug.Log("開始下載");
        hotfixProcessor.DownloadCompleteCallback = () => 
            {
                hotfixChecking = false;
                Debug.Log("執行完成");
            };
        hotfixProcessor.DownloadPercentCallback = (_value) => ProgressBar.fillAmount = _value;

        var downloadProcess = await hotfixProcessor.Excute();
        if(downloadProcess != null)
            StartCoroutine(downloadProcess);
    }

    public async void LoadUIHotfix()
    {
        if(!UIHotfixAR.IsDone || UIHotfixAR.IsValid())
            return;
        Debug.Log("Load UIHotfix");
        var handle = UIHotfixAR.LoadAssetAsync();
        await handle.Task;
        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            var go = Instantiate(UIHotfixAR.Asset as GameObject);
            go.transform.SetParent(CanvasTrans, false);
            UIHotfixGO = go;
        }
        else
        {
            UIHotfixAR.ReleaseAsset();
        }
    }

    public void ReleaseUIHotfix()
    {
        if(UIHotfixAR.IsValid())
        {
            Debug.Log("Release UIHotfix");
            UIHotfixAR.ReleaseAsset();
            Destroy(UIHotfixGO);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}