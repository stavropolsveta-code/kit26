using System;
using System.Collections;
using UnityEngine;
using Agava.YandexGames;
using EchoOfAncients.Unity;

namespace EchoOfAncients.Yandex
{
    public class YandexBridge : MonoBehaviour
    {
        public GameManager gameManager;

        public bool IsReady { get; private set; }

        private void Start()
        {
            if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.SaveRequested += SaveCloud;
            }

            StartCoroutine(Application.isEditor ? EditorInit() : RunYandexInit());
        }

        private IEnumerator EditorInit()
        {
            IsReady = true;
            Debug.Log("[Эхо Древних] YandexBridge работает в редакторе (SDK выключен).");
            yield break;
        }

        private IEnumerator RunYandexInit()
        {
            yield return YandexGamesSdk.Initialize();

            if (!YandexGamesSdk.IsInitialized)
            {
                Debug.LogError("[Эхо Древних] SDK Яндекс Игр не инициализировался.");
                yield break;
            }

            YandexGamesSdk.GameReady();
            IsReady = true;
            Debug.Log("[Эхо Древних] SDK Яндекс Игр инициализирован.");

            if (PlayerAccount.IsAuthorized)
            {
                LoadCloudSave();
            }

            PlayerAccount.AuthorizedInBackground += LoadCloudSave;
        }

        private bool CanUseSdk => !Application.isEditor && IsReady && YandexGamesSdk.IsInitialized;

        public void Authorize(Action onDone = null)
        {
            if (!CanUseSdk) { onDone?.Invoke(); return; }

            if (PlayerAccount.IsAuthorized)
            {
                PlayerAccount.RequestPersonalProfileDataPermission(onDone, null);
                return;
            }

            PlayerAccount.Authorize(
                onSuccessCallback: () =>
                {
                    PlayerAccount.RequestPersonalProfileDataPermission(
                        onSuccessCallback: onDone,
                        onErrorCallback: null);
                },
                onErrorCallback: null);
        }

        public void ShowRewardedVideo(Action onRewarded, Action<string> onError = null)
        {
            if (!CanUseSdk)
            {
                onRewarded?.Invoke();
                return;
            }

            VideoAd.Show(
                onOpenCallback: null,
                onRewardedCallback: onRewarded,
                onCloseCallback: null,
                onErrorCallback: onError ?? (msg => Debug.LogWarning($"[Эхо Древних] RV error: {msg}")));
        }

        public void ShowInterstitial(Action onClose = null)
        {
            if (!CanUseSdk)
            {
                onClose?.Invoke();
                return;
            }

            InterstitialAd.Show(
                onOpenCallback: null,
                onCloseCallback: closed => onClose?.Invoke(),
                onErrorCallback: msg => Debug.LogWarning($"[Эхо Древних] Interstitial error: {msg}"));
        }

        public void SaveCloud(string json)
        {
            if (!CanUseSdk || !PlayerAccount.IsAuthorized) return;

            PlayerAccount.SetCloudSaveData(
                json,
                onSuccessCallback: null,
                onErrorCallback: msg => Debug.LogWarning($"[Эхо Древних] Cloud save error: {msg}"));
        }

        public void SaveCloud()
        {
            if (gameManager != null && CanUseSdk && PlayerAccount.IsAuthorized)
            {
                SaveCloud(gameManager.SerializeSave());
            }
        }

        public void LoadCloudSave()
        {
            if (!CanUseSdk || !PlayerAccount.IsAuthorized) return;

            PlayerAccount.GetCloudSaveData(
                onSuccessCallback: json => gameManager?.ApplySerializedSave(json),
                onErrorCallback: msg => Debug.LogWarning($"[Эхо Древних] Cloud load error: {msg}"));
        }

        public void SubmitScore(string leaderboardName, int score)
        {
            if (!CanUseSdk) return;

            Leaderboard.SetScore(
                leaderboardName,
                score,
                onSuccessCallback: null,
                onErrorCallback: msg => Debug.LogWarning($"[Эхо Древних] Leaderboard error: {msg}"));
        }

        public void ShowLeaderboard(string leaderboardName)
        {
            if (!CanUseSdk) return;

            Leaderboard.GetEntries(
                leaderboardName,
                onSuccessCallback: response => Debug.Log($"[Эхо Древних] Лидерборд загружен: {response.entries.Length} записей."),
                onErrorCallback: msg => Debug.LogWarning($"[Эхо Древних] Leaderboard load error: {msg}"),
                topPlayersCount: 5,
                competingPlayersCount: 5);
        }

        private void OnGUI()
        {
            if (!IsReady) return;

            GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 300, 170), GUI.skin.box);
            GUILayout.Label($"Yandex: готово={IsReady}  авторизован={(CanUseSdk ? PlayerAccount.IsAuthorized.ToString() : "-")}");
            if (GUILayout.Button("Авторизация (Яндекс)"))
            {
                Authorize();
            }
            if (GUILayout.Button("RV-реклама (+бонус)"))
            {
                ShowRewardedVideo(() =>
                {
                    gameManager?.Wallet.Add(EchoOfAncients.Meta.ResourceType.Amber, 5);
                    Debug.Log("[Эхо Древних] Награда за RV +5 Янтаря.");
                });
            }
            if (GUILayout.Button("Облачное сохранение"))
            {
                SaveCloud();
            }
            if (GUILayout.Button("Загрузить из облака"))
            {
                LoadCloudSave();
            }
            if (GUILayout.Button("Лидерборд"))
            {
                SubmitScore("score", (int)(gameManager != null && gameManager.Session != null ? gameManager.Session.Score : 0));
            }
            GUILayout.EndArea();
        }
    }
}