using UnityEngine;
using GoogleMobileAds.Api;

public class BannerAdManager : MonoBehaviour
{
    private BannerView bannerView;

    // App ID 用來初始化 SDK
#if UNITY_ANDROID
    private string appId = "ca-app-pub-1718346235780915~1813809960";
#elif UNITY_IOS
    private string appId = "ca-app-pub-xxxxxxxxxxxxxxxx~yyyyyyyyyy"; // 替換成你的 iOS App ID
#endif

    // 廣告單元 ID 用來顯示橫幅
#if UNITY_ANDROID
    private string adUnitId = "ca-app-pub-1718346235780915/7552511367";
#elif UNITY_IOS
    private string adUnitId = "ca-app-pub-xxxxxxxxxxxxxxxx/zzzzzzzzzz"; // 替換成你的 iOS 廣告單元 ID
#endif

    void Start()
    {
        // 初始化 Google Mobile Ads SDK
        MobileAds.Initialize(initStatus => { });

        // 建立並顯示橫幅廣告
        RequestBanner();
    }

    private void RequestBanner()
    {
        // 設定橫幅尺寸（SmartBanner 在新版 SDK 已經支援）
        AdSize adSize = AdSize.SmartBanner;

        // 建立橫幅物件
        bannerView = new BannerView(adUnitId, adSize, AdPosition.Bottom);

        // 建立廣告請求 (新版 SDK 不需要 Builder)
        AdRequest request = new AdRequest();

        // 載入廣告
        bannerView.LoadAd(request);
    }

    private void OnDestroy()
    {
        // 釋放橫幅物件
        if (bannerView != null)
        {
            bannerView.Destroy();
        }
    }
}
