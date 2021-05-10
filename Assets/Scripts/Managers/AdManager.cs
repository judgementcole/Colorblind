using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using TMPro;

public class AdManager : MonoSingleton<AdManager>
{
    #region Variables
    [Header("Ad Conditions")]
    public bool _adFree;

    [Header("Banner Ads")]
    private BannerView _bannerView;

    [Header("Interstitial Ads")]
    public int deathThreshold = 4; // For displaying ads after n number of deaths
    [HideInInspector] public int deathCounter = 0; // For displaying ads after n number of deaths
    private InterstitialAd _imageInterstitialAd;
    private InterstitialAd _videoInterstitialAd;
    public Action OnInterstitialOpened;
    public Action OnInterstitialClosed;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        // Initialize the Google Mobile Ads SDK
        if (DisplayAdConditions())
        {
            MobileAds.Initialize(initStatus => { });

            RequestImageInterstitial();
            RequestVideoInterstitial();
        }
    }
    #endregion

    #region Banner Ads
    public void RequestBanner()
    {
        if (_bannerView == null)
        {
            #if UNITY_ANDROID
                string adUnitId = "ca-app-pub-3656094190232137/2354497140";
            #elif UNITY_IPHONE
                string adUnitId = "ca-app-pub-3940256099942544/2934735716";
            #else
                string adUnitId = "unexpected_platform";
            #endif

            //string adUnitId = "ca-app-pub-3656094190232137/2354497140";// Test ID: "ca-app-pub-3940256099942544/6300978111";

            // Create a smart banner at the bottom of the screen.
            _bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);

            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();

            // Load the banner with the request.
            _bannerView.LoadAd(request);
        }
        else
        {
            _bannerView.Show();
        }
    }

    public void HideBanner()
    {
        _bannerView.Hide();
    }
    #endregion

    #region Interstitial Ads
    public void RequestImageInterstitial()
    {
        #if UNITY_ANDROID
            string adUnitId = "ca-app-pub-3656094190232137/7040133994";
        #elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/2934735716";
        #else
            string adUnitId = "unexpected_platform";
        #endif

        //string adUnitId = "ca-app-pub-3656094190232137/7040133994"; //"ca-app-pub-3940256099942544/1033173712";

        // Initialize an InterstitialAd.
        _imageInterstitialAd = new InterstitialAd(adUnitId);

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the interstitial with the request.
        _imageInterstitialAd.LoadAd(request);
    }

    public void DisplayImageInterstitial()
    {
        if (_imageInterstitialAd.IsLoaded())
        {
            _imageInterstitialAd.OnAdOpening += HandleOnInterstitialOpened;
            _imageInterstitialAd.OnAdClosed += HandleOnInterstitialClosed;

            _imageInterstitialAd.Show();
        }
    }

    public void RequestVideoInterstitial()
    {
        #if UNITY_ANDROID
            string adUnitId = "ca-app-pub-3656094190232137/9069501154";
        #elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/2934735716";
        #else
            string adUnitId = "unexpected_platform";
        #endif

        //string adUnitId = "ca-app-pub-3656094190232137/9069501154"; //"ca-app-pub-3940256099942544/1033173712";

        // Initialize an InterstitialAd.
        _videoInterstitialAd = new InterstitialAd(adUnitId);

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the interstitial with the request.
        _videoInterstitialAd.LoadAd(request);
    }

    public void DisplayVideoInterstitial()
    {
        if (_imageInterstitialAd.IsLoaded())
        {
            _videoInterstitialAd.OnAdOpening += HandleOnInterstitialOpened;
            _videoInterstitialAd.OnAdClosed += HandleOnInterstitialClosed;

            _videoInterstitialAd.Show();
        }
    }

    public void HandleOnInterstitialOpened(object sender, EventArgs args)
    {
        if (AdManager.Instance.OnInterstitialOpened != null)
            AdManager.Instance.OnInterstitialOpened();
    }

    public void HandleOnInterstitialClosed(object sender, EventArgs args)
    {
        if (AdManager.Instance.OnInterstitialClosed != null)
            AdManager.Instance.OnInterstitialClosed();
    }
    #endregion

    #region Ad Conditions
    public bool DisplayAdConditions()
    {
        if (_adFree)
            return false;

        return true;
    }
    #endregion
}
