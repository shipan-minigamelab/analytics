using System;
using GameAnalyticsSDK;
using UnityEngine;
using Facebook.Unity;
using Firebase;
using Firebase.Extensions;
using Firebase.Analytics;
using System.Threading.Tasks;

namespace MGL.Analytics
{
    public class AnalyticsManager : MonoBehaviour, IGameAnalyticsATTListener
    {
        public static AnalyticsManager Instance;
        private bool firebaseInitialized = false;
        
        
        private void Awake()
        {
            Instance = this;
            InitializeFirebase();
            InitializeFacebook();
            InitializeGameAnalytics();
        }

        #region  FacebookSDK
        private void InitializeFacebook()
        {
            if (!FB.IsInitialized)
            {
                Debug.Log("Initialize the Facebook SDK");
                FB.Init(InitCallback, OnHideUnity);
            }
            else
            {
                Debug.Log("Already initialized, signal an app activation App Event");

                // 
                FB.ActivateApp();
            }
        }
        private void InitCallback()
        {
            if (FB.IsInitialized)
            {
                // 
                Debug.Log("Signal an app activation App Event");

                FB.ActivateApp();
                // 
                Debug.Log("Continue with Facebook SDK");
                // ...
            }
            else
            {
                Debug.Log("Failed to Initialize the Facebook SDK");
            }
        }
        private void OnHideUnity(bool isGameShown)
        {
            Debug.Log("OnHideUnity isGameShown  : " + isGameShown);

            // if (!isGameShown) {
            //     // Pause the game - we will need to hide
            //     Time.timeScale = 0;
            // } else {
            //     // Resume the game - we're getting focus again
            //     Time.timeScale = 1;
            // }
        }
        #endregion

        #region FirebaseSDK
        void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var  dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available) {
                    if (!firebaseInitialized)
                    {
                        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                   
                        FirebaseAnalytics.SetUserId(SystemInfo.deviceUniqueIdentifier);
                        // // Set default session duration values.
                        // FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));
                        firebaseInitialized = true;
                    }
                } else {
                    Debug.LogError(
                        "Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
            
            
        }
        
        private string GetFirebaseLevelKey(LevelStatus levelStatus)
        {
            switch (levelStatus)
            {
                case LevelStatus.FAIL:
                    return EventKeyData.LevelFaileKey;
                case LevelStatus.START:
                    return EventKeyData.LevelStartKey;
                case LevelStatus.COMPLETE:
                    return EventKeyData.LevelCompleteKey;
                default:
                    return null;
            }
        }
        
        private string GetFirebaseButtonKey(ButtonName buttonName)
        {
            switch (buttonName)
            {
                case ButtonName.SETTING:
                    return EventKeyData.SettingButtonKey;
                case ButtonName.PHOTO_MODE_EFFECT:
                    return EventKeyData.PhotoModeEffectKey;
                case ButtonName.PHOTO_MODE_FILTER:
                    return EventKeyData.PhotoModeFilterKey;
                case ButtonName.PHOTO_MODE_BACKGROUND:
                    return EventKeyData.PhotoModeBackgroundKey;
                case ButtonName.PHOTO_SHARE_CAPTURE:
                    return EventKeyData.PhotoShareCaptureKey;
                case ButtonName.PHOTO_SHARE_GALLERY:
                    return EventKeyData.PhotoShareGalleryKey;
                case ButtonName.PHOTO_ZOOM_GALLERY:
                    return EventKeyData.PhotoZoomGalleryKey;
                case ButtonName.PHOTO_ZOOM_REFERENCE:
                    return EventKeyData.PhotoZoomReferenceKey;
                case ButtonName.PHOTO_GALLERY:
                    return EventKeyData.GalleryButtonKey;
                default:
                    return null;
            }
        }

        public void FirebaseLevelEvent(LevelStatus levelStatus, string level, string score = null)
        {
            string status = GetFirebaseLevelKey(levelStatus);

            if (String.IsNullOrEmpty(status) == false && firebaseInitialized)
            {
                if (String.IsNullOrEmpty(score))
                {
                    FirebaseAnalytics.LogEvent(
                        GetFirebaseLevelKey(levelStatus),
                        new Parameter("level", level));
                }
                else
                {
                    FirebaseAnalytics.LogEvent(
                        GetFirebaseLevelKey(levelStatus),
                        new Parameter("level", level),
                        new Parameter("score", score));
                }
            }
        }
        
        public void FirebaseLogEvent(string eventName, string eventData = null)
        {
            if (String.IsNullOrEmpty(eventName) == false && firebaseInitialized)
            {
                if (String.IsNullOrEmpty(eventData))
                {
                    FirebaseAnalytics.LogEvent(eventName);
                }
                else
                {
                    FirebaseAnalytics.LogEvent( eventName, new Parameter("data", eventData));
                }
            }
        }


        #endregion

        #region GameAnalyticsSDK
        private void InitializeGameAnalytics()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                GameAnalytics.RequestTrackingAuthorization(this);
            }
            else
            {
                GameAnalytics.Initialize();
            }
        }
        
        public void GameAnalyticsATTListenerNotDetermined()
        {
            Debug.Log("GameAnalyticsATTListenerNotDetermined");
            GameAnalytics.Initialize();
        }

        public void GameAnalyticsATTListenerRestricted()
        {
            Debug.Log("GameAnalyticsATTListenerRestricted");

            GameAnalytics.Initialize();
        }

        public void GameAnalyticsATTListenerDenied()
        {
            Debug.Log("GameAnalyticsATTListenerDenied");

            GameAnalytics.Initialize();
        }

        public void GameAnalyticsATTListenerAuthorized()
        {
            Debug.Log("GameAnalyticsATTListenerAuthorized");
            GameAnalytics.Initialize();
        }
        
        private GAProgressionStatus GetLevelStatus(LevelStatus levelStatus)
        {
            switch (levelStatus)
            {
                case LevelStatus.FAIL:
                    return GAProgressionStatus.Fail;
                case LevelStatus.START:
                    return GAProgressionStatus.Start;
                case LevelStatus.COMPLETE:
                    return GAProgressionStatus.Complete;
                default:
                    return GAProgressionStatus.Undefined;
            }
        }

        public void GameAnalyticsProgressionEvent(LevelStatus levelStatus, string level, string score = null)
        {
            GAProgressionStatus status = GetLevelStatus(levelStatus);

            if (status != GAProgressionStatus.Undefined)
            {
                if (String.IsNullOrEmpty(score))
                {
                    GameAnalytics.NewProgressionEvent(status,  level);
                }
                else
                {
                    GameAnalytics.NewProgressionEvent(status,  level, score);
                }
            }
        }

        public void GameAnalyticsLogEvent(string eventName, string eventData = null)
        {
            GameAnalytics.NewDesignEvent (eventName+":"+eventData);
        }
        
        #endregion
        
        public void LevelEvent(LevelStatus levelStatus, string level, string score = null)
        {
            GameAnalyticsProgressionEvent(levelStatus, level, score);
            FirebaseLevelEvent(levelStatus, level, score);
        }

        public void ButtonEvent(string buttonName)
        {
            LogEvent("button_click", buttonName);
        }
        
        public void ButtonEvent(ButtonName buttonName)
        {
            string buttonNameKey = GetFirebaseButtonKey(buttonName);
            LogEvent("button_click", buttonNameKey);
        }

        public void LogEvent(string eventName, string eventData = null)
        {
            GameAnalyticsLogEvent(eventName, eventData);
            FirebaseLogEvent(eventName, eventData);
        }
        
    }


    public enum LevelStatus
    {
        FAIL,
        START,
        COMPLETE,
    }

    public enum ButtonName
    {
        SETTING,
        PHOTO_GALLERY,
        PHOTO_SHARE_CAPTURE,
        PHOTO_SHARE_GALLERY,
        PHOTO_ZOOM_GALLERY,
        PHOTO_ZOOM_REFERENCE,
        PHOTO_MODE_BACKGROUND,
        PHOTO_MODE_FILTER,
        PHOTO_MODE_EFFECT
    }

    public static class EventKeyData
    {
        public static string LevelStartKey = "level_start";
        public static string LevelCompleteKey = "level_complete";
        public static string LevelFaileKey = "level_fail";
        
        public static string SettingButtonKey = "setting";
        public static string GalleryButtonKey = "gallery";
        public static string PhotoShareCaptureKey = "photo_share_capture";
        public static string PhotoShareGalleryKey = "photo_share_gallery";
        public static string PhotoZoomGalleryKey = "photo_zoom_gallery";
        public static string PhotoZoomReferenceKey = "photo_zoom_reference";
        public static string PhotoModeBackgroundKey = "photo_mode_background";
        public static string PhotoModeFilterKey = "photo_mode_filter";
        public static string PhotoModeEffectKey = "photo_mode_effect";
}
}