using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class AdManager : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener, IUnityAdsInitializationListener
{
	public static AdManager Instance;

	[Header("Meta data")]
	[SerializeField] private bool _testMode = true;
	[Space(10)]
	[SerializeField] private string _androidGameId;
	[SerializeField] private string _iOSGameId;
	private string _gameId;

	[Space(20)]

	//====================Interstitial AD Variables====================
	[Header("Interstitial AD Variables")]
	[SerializeField] private string _IandroidAdUnitId = "Interstitial_Android";
	[SerializeField] private string _IiOsAdUnitId = "Interstitial_iOS";
	private string _IadUnitId;

	[Space(20)]

	//==================Banner AD Variables======================
	[Header("Banner AD Variables")]

	// For the purpose of this example, these buttons are for functionality testing:
	[SerializeField] private Button _loadBannerButton;
	[SerializeField] private Button _showBannerButton;
	[SerializeField] private Button _hideBannerButton;

	[SerializeField] private string _BandroidAdUnitId = "Banner_Android";
	[SerializeField] private string _BiOsAdUnitId = "Banner_iOS";
	private string _BadUnitId;

	[SerializeField] private BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;

	[Space(20)]

	//=====================Rewarded AD Variables================
	[Header("Rewarded AD Variables")]

	[SerializeField] private Button _showAdButton;
	[SerializeField] private string _androidAdUnitId = "Rewarded_Android";
	[SerializeField] private string _iOSAdUnitId = "Rewarded_iOS";
	private string _adUnitId;

	private int RewardsEarned = 0;
	[SerializeField] private Text RewardEarnedText;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);

			// Get the Ad Unit ID for the current platform:
			_IadUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
				? _IiOsAdUnitId
				: _IandroidAdUnitId;

			_BadUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
				? _BiOsAdUnitId
				: _BandroidAdUnitId;

			//Rewarded AD
			_adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
				? _androidAdUnitId
				: _iOSAdUnitId;

			// Get the Ad Unit ID for the current platform:
			_adUnitId = null; // This will remain null for unsupported platforms
#if UNITY_IOS
		_adUnitId = _iOsAdUnitId;
#elif UNITY_ANDROID
			_adUnitId = _androidAdUnitId;
#endif

			//Disable button until ad is ready to show
			_showAdButton.interactable = false;

			RewardsEarned = PlayerPrefs.GetInt("RewardsEarned");
			RewardEarnedText.text = "Reward Earned: " + RewardsEarned;

			InitializeAds();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		InitializeBannerAD();
	}

	#region Initializing AD
	public void InitializeAds()
	{
		Debug.Log("Initializing Ads");

		_gameId = (Application.platform == RuntimePlatform.IPhonePlayer)
			? _iOSGameId
			: _androidGameId;
		Advertisement.Initialize(_gameId, _testMode, this);
	}

	public void OnInitializationComplete()
	{
		Debug.Log("Unity Ads initialization complete.");
	}

	public void OnInitializationFailed(UnityAdsInitializationError error, string message)
	{
		Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
	}
	#endregion

	#region InterstitialAD
	// Load content to the Ad Unit:
	public void LoadInterstitialAd()
	{
		// IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
		Debug.Log("Loading Ad: " + _IadUnitId);
		Advertisement.Load(_IadUnitId, this);
	}

	// Show the loaded content in the Ad Unit: 
	public void ShowInterstitialAd()
	{
		// Note that if the ad content wasn't previously loaded, this method will fail
		Debug.Log("Showing Ad: " + _IadUnitId);
		Advertisement.Show(_IadUnitId, this);
	}
	#endregion

	#region BannerAds
	private void InitializeBannerAD()
	{
		// Disable the button until an ad is ready to show:
		_showBannerButton.interactable = false;
		_hideBannerButton.interactable = false;

		// Set the banner position:
		Advertisement.Banner.SetPosition(_bannerPosition);

		// Configure the Load Banner button to call the LoadBanner() method when clicked:
		_loadBannerButton.onClick.AddListener(LoadBanner);
		_loadBannerButton.interactable = true;
	}

	// Implement a method to call when the Load Banner button is clicked:
	public void LoadBanner()
	{
		// Set up options to notify the SDK of load events:
		BannerLoadOptions options = new BannerLoadOptions
		{
			loadCallback = OnBannerLoaded,
			errorCallback = OnBannerError
		};

		// Load the Ad Unit with banner content:
		Advertisement.Banner.Load(_BadUnitId, options);
	}

	// Implement code to execute when the loadCallback event triggers:
	void OnBannerLoaded()
	{
		Debug.Log("Banner loaded");

		// Configure the Show Banner button to call the ShowBannerAd() method when clicked:
		_showBannerButton.onClick.AddListener(ShowBannerAd);
		// Configure the Hide Banner button to call the HideBannerAd() method when clicked:
		_hideBannerButton.onClick.AddListener(HideBannerAd);

		// Enable both buttons:
		_showBannerButton.interactable = true;
		_hideBannerButton.interactable = true;
	}

	// Implement code to execute when the load errorCallback event triggers:
	void OnBannerError(string message)
	{
		Debug.Log($"Banner Error: {message}");
		// Optionally execute additional code, such as attempting to load another ad.
	}

	// Implement a method to call when the Show Banner button is clicked:
	void ShowBannerAd()
	{
		// Set up options to notify the SDK of show events:
		BannerOptions options = new BannerOptions
		{
			clickCallback = OnBannerClicked,
			hideCallback = OnBannerHidden,
			showCallback = OnBannerShown
		};

		// Show the loaded Banner Ad Unit:
		Advertisement.Banner.Show(_BadUnitId, options);
	}

	// Implement a method to call when the Hide Banner button is clicked:
	void HideBannerAd()
	{
		// Hide the banner:
		Advertisement.Banner.Hide();
	}

	void OnBannerClicked() { }
	void OnBannerShown() { }
	void OnBannerHidden() { }

	#endregion

	#region RewardedAds
	// Load content to the Ad Unit:
	public void LoadAd()
	{
		// IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
		Debug.Log("Loading Ad: " + _adUnitId);
		Advertisement.Load(_adUnitId, this);
	}

	// Implement a method to execute when the user clicks the button.
	public void ShowAd()
	{
		// Disable the button: 
		_showAdButton.interactable = false;
		// Then show the ad:
		Advertisement.Show(_adUnitId, this);
	}
	#endregion

	#region Callbacks
	// If the ad successfully loads, add a listener to the button and enable it:
	public void OnUnityAdsAdLoaded(string adUnitId)
	{
		Debug.Log("Ad Loaded: " + adUnitId);

		if (adUnitId.Equals(_adUnitId))
		{
			// Configure the button to call the ShowAd() method when clicked:
			_showAdButton.onClick.RemoveAllListeners();
			_showAdButton.onClick.AddListener(ShowAd);

			// Enable the button for users to click:
			_showAdButton.interactable = true;
		}
	}
	// Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
	public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
	{
		if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
		{
			Debug.Log("Unity Ads Rewarded Ad Completed");
			// Grant a reward.
			RewardsEarned += 1;
			PlayerPrefs.SetInt("RewardsEarned", RewardsEarned);
			RewardEarnedText.text = "Reward Earned: " + RewardsEarned;
			// Load another ad:
			Advertisement.Load(_adUnitId, this);
		}
	}

	// Implement Load and Show Listener error callbacks:
	public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
	{
		Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
		// Use the error details to determine whether to try to load another ad.
	}

	public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
	{
		Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
		// Use the error details to determine whether to try to load another ad.
	}

	public void OnUnityAdsShowStart(string adUnitId) { }
	public void OnUnityAdsShowClick(string adUnitId) { }
	#endregion

	public void ResetPlayerPrefs()
	{
		PlayerPrefs.DeleteAll();
		RewardsEarned = PlayerPrefs.GetInt("RewardsEarned");
		RewardEarnedText.text = "Reward Earned: " + RewardsEarned;
	}
	private void OnDestroy()
	{
		// Clean up the listeners

		_loadBannerButton.onClick.RemoveAllListeners();
		_showBannerButton.onClick.RemoveAllListeners();
		_hideBannerButton.onClick.RemoveAllListeners();

		_showAdButton.onClick.RemoveAllListeners();
	}
}