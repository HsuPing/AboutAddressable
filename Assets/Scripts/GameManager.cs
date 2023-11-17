using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameManager : MonoBehaviour
{
    public List<string> CheckForCatalogUpdatesList = null;
    public List<object> UpdateCatalogsList = null;

    
    public async void CheckForCatalogUpdates()
    {
        Debug.Log("檢查Catalog");

        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        await checkHandle.Task;
        if(checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            CheckForCatalogUpdatesList = checkHandle.Result;
            if(CheckForCatalogUpdatesList.Any())
                Debug.Log("有需要更新的bundle");
        }
        else
        {
            Debug.LogFormat("檢查更新失敗: {0}", checkHandle.Status);
        }

        Addressables.Release(checkHandle);
    }

    public async void UpdateCatalogs()
    {
        if(CheckForCatalogUpdatesList != null && CheckForCatalogUpdatesList.Any())
        {
            Debug.Log("更新Catalog");
            var updateHandle = Addressables.UpdateCatalogs(true ,CheckForCatalogUpdatesList, false);
            await updateHandle.Task;

            if(updateHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogFormat("更新失敗: {0}", updateHandle.Status);
            }
            else
            {
                UpdateCatalogsList = new List<object>();
                var assets = updateHandle.Result;

                for(short index = 0; index < assets.Count; index++)
                {
                    UpdateCatalogsList.AddRange(assets[index].Keys);
                }
            }

            Addressables.Release(updateHandle);
        }
    }

    public async void DownloadUpdateAssets()
    {
        if(UpdateCatalogsList != null && UpdateCatalogsList.Any())
        {
            Debug.Log("下載開始");
            var downloadHandle = Addressables.DownloadDependenciesAsync(UpdateCatalogsList, Addressables.MergeMode.Union);
            await downloadHandle.Task;
            if(downloadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogFormat("下載失敗: {0}", downloadHandle.Status);
            }

            Addressables.Release(downloadHandle);
        }
    } 

    public void QuitGame()
    {
        Application.Quit();
    }
}