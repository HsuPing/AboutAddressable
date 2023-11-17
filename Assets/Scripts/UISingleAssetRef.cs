using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Diagnostics;

public class UISingleAssetRef : MonoBehaviour
{
    public Text TitleText;
    public Image MainImage;
    public Button LoadButton;
    public Button ReleaseButton;
    public AssetReferenceSprite AssetReferenceSprite;

    private void Awake() 
    {
        LoadButton.onClick.AddListener(loadAsset);
        ReleaseButton.onClick.AddListener(releaseAsset);
    }

    private async void loadAsset()
    {
        if(AssetReferenceSprite.IsDone)
        {
            if(AssetReferenceSprite.IsValid())
            {
                UnityEngine.Debug.LogFormat("已經預載過該資源 \nIsValid: {0}, IsDone: {1}", AssetReferenceSprite.IsValid(), AssetReferenceSprite.IsDone);
                MainImage.sprite = AssetReferenceSprite.Asset as Sprite;
            }
            else
            {
                UnityEngine.Debug.LogFormat("開始載入 \nIsValid: {0}, IsDone: {1}", AssetReferenceSprite.IsValid(), AssetReferenceSprite.IsDone);
                var handle = AssetReferenceSprite.LoadAssetAsync();

                Stopwatch loadingTime = new Stopwatch();
                loadingTime.Start();
                await handle.Task;
                loadingTime.Stop();
                var ts = loadingTime.Elapsed;
                string time = string.Format("{0:00}.{1:0000}s", ts.Seconds, ts.Milliseconds);
                UnityEngine.Debug.Log("載入時間：" + time);
                if(handle.Status == AsyncOperationStatus.Succeeded)
                {
                    MainImage.sprite = handle.Result;
                    TitleText.text = AssetReferenceSprite.Asset.name + "\n" + time;
                }
                else
                    UnityEngine.Debug.LogFormat("載入失敗: {0} \nIsValid: {1}, IsDone: {2}", handle.Status.ToString() ,AssetReferenceSprite.IsValid(), AssetReferenceSprite.IsDone);
            }
        }
        else
        {
            UnityEngine.Debug.LogFormat("載入中... \nIsValid: {0}, IsDone: {1}", AssetReferenceSprite.IsValid(), AssetReferenceSprite.IsDone);
        }
    }

    private void releaseAsset()
    {
        if(AssetReferenceSprite.Asset != null)
            AssetReferenceSprite.ReleaseAsset();
    }

    private void OnDestroy() 
    {
        LoadButton.onClick.RemoveAllListeners();
        ReleaseButton.onClick.RemoveAllListeners();
    }
}
