using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 需要被熱更的物件都需要在Addressable Groups將Label標記為'preload' (基本上只有Remote的物件才需要設定)
/// 執行Update a Previous Build產生的Content Update Group物件記得設定Label
/// TODO: 失敗狀態處理
/// </summary>
public class HotfixProcessor
{
    public Action<float> DownloadPercentCallback;
    public Action DownloadStartCallback;
    public Action DownloadCompleteCallback;

    private const string HOTFIX_LABEL = "preload";
    private List<string> updateList;
    private bool needDownload = false;

    public async Task<IEnumerator> Excute()
    {
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        await checkForCatalogUpdates();

        var ts = watch.Elapsed;
        string time = string.Format("{0:00}.{1:0000}s", ts.Seconds, ts.Milliseconds);
        Debug.Log("checkForCatalog: " + time);

        await updateCatalog();

        ts = watch.Elapsed;
        time = string.Format("{0:00}.{1:0000}s", ts.Seconds, ts.Milliseconds);
        Debug.Log("updateCatalogP: " + time);

        await getDownloadSize();

        ts = watch.Elapsed;
        time = string.Format("{0:00}.{1:0000}s", ts.Seconds, ts.Milliseconds);
        Debug.Log("getDownloadSize: " + time);

        if(needDownload)
            return DownloadPreloadAssets();
        else
            DownloadCompleteCallback?.Invoke();

        return null;
    }

    private async Task checkForCatalogUpdates()
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
            return;

        var handle = Addressables.CheckForCatalogUpdates(false);
        await handle.Task;
        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            updateList = handle.Result;
        }
        else
        {
            Debug.LogError("HotfixManager: CheckForCatalogUpdates Fail! - " + handle.Status.ToString());
        }

        Addressables.Release(handle);
    }

    private async Task updateCatalog()
    {
        if(!updateList.Any())
            return;

        var handle = Addressables.UpdateCatalogs(true, updateList, false);
        await handle.Task;
        if(handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("HotfixManager: UpdateCatalogs Fail! - " + handle.Status.ToString());
        }

        Addressables.Release(handle);
    }

    private async Task getDownloadSize()
    {
        var handle = Addressables.GetDownloadSizeAsync(HOTFIX_LABEL);
        await handle.Task;
        if(handle.Result > 0)
        {
            Debug.Log("下載檔案大小：" + handle.Result);
            needDownload = true;
        }

        Addressables.Release(handle);
    }

    IEnumerator DownloadPreloadAssets()
    {
        DownloadStartCallback?.Invoke();
        var downloadHandle = Addressables.DownloadDependenciesAsync(HOTFIX_LABEL);
        downloadHandle.Completed += (handle) => DownloadCompleteCallback?.Invoke();

        while(!downloadHandle.IsDone)
        {
            DownloadPercentCallback?.Invoke(downloadHandle.GetDownloadStatus().Percent);
            Debug.Log("下載進度：" + downloadHandle.GetDownloadStatus().Percent);
            yield return new WaitForSeconds(0.05f);
        }

        DownloadPercentCallback?.Invoke(1);

        Debug.Log("下載完成");
        Addressables.Release(downloadHandle);
    }
}
