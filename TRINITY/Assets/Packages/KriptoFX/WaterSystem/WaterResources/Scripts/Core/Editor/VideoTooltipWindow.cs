#if UNITY_EDITOR
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;


namespace KWS
{
    internal class VideoTooltipWindow : EditorWindow
    {
        public string VideoClipFileURI;

        GameObject  tempGO;
        VideoClip   clip;
        VideoPlayer player;
        Texture     currentRT;
        Texture2D   DefaultTexture;

        void OnGUI()
        {
            Repaint();

            if (clip == null)
            {
                if (player == null)
                {
                    tempGO           = new GameObject("WaterVideoWindowHelp");
                    tempGO.hideFlags = HideFlags.DontSave;

                    player = tempGO.AddComponent<VideoPlayer>();

                    player.url       = VideoClipFileURI;
                    player.isLooping = true;

                    player.prepareCompleted += PlayerOnprepareCompleted;
                    player.Prepare();
                }

            }

            if (currentRT == null)
            {
                if (DefaultTexture == null) DefaultTexture = Resources.Load<Texture2D>(KWS_Settings.ResourcesPaths.KWS_DefaultVideoLoading);
            }

            EditorGUI.DrawPreviewTexture(new Rect(0, 0, position.width, position.height), currentRT == null ? DefaultTexture : currentRT);
        }

        private void PlayerOnprepareCompleted(VideoPlayer source)
        {
            player.Play();
            currentRT = source.texture;
        }

        void OnDisable()
        {
            if (player != null)
            {
                player.Stop();
                player.targetTexture = null;
            }

            KW_Extensions.SafeDestroy(tempGO);
            if (DefaultTexture != null) Resources.UnloadAsset(DefaultTexture);
        }

        //todo check what cause WindowsMediaFoundation received empty file  
        /*
        public string VideoClipFileURI;

        private GameObject tempGO;
        private VideoPlayer player;
        private RenderTexture currentRT;
        private Texture2D DefaultTexture;

        private const string
        FILE_EXTENSION = ".mp4",
        PREP_COMPLETE = "Video player preparation completed.",
        LOAD_ERROR = "Failed to load video clip: ",
        ASSET_TYPE_ERROR = "Asset is not a video clip: ",
        URL_REQUEST_ERROR = "Request error: ",
        INIT_ERROR = "Initalization error: ";

        public void OnEnable()
        {
            InitializeVideoPlayer();
        }
        public void Init(string videoClipFileURI)
        {
            VideoClipFileURI = videoClipFileURI;
            InitializeVideoPlayer();
        }

        private void OnGUI()
        {
            Repaint();

            if (currentRT != null)
                EditorGUI.DrawPreviewTexture(new Rect(0, 0, position.width, position.height), currentRT);
            else
            {
                if (DefaultTexture == null)
                    DefaultTexture = Resources.Load<Texture2D>(KWS_Settings.ResourcesPaths.KWS_DefaultVideoLoading);
                EditorGUI.DrawPreviewTexture(new Rect(0, 0, position.width, position.height), DefaultTexture);
            }

        }

        private async void InitializeVideoPlayer()
        {
            try
            {
                if (player == null)
                {
                    tempGO = new GameObject("WaterVideoWindowHelp");
                    tempGO.hideFlags = HideFlags.DontSave;

                    await Task.Delay(100); //not needed if you set replace OnEnable with Init(VideoClipFileURI)

                    player = tempGO.AddComponent<VideoPlayer>();
                    var url = VideoClipFileURI.EndsWith(FILE_EXTENSION) ? VideoClipFileURI : VideoClipFileURI + FILE_EXTENSION;

                    if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    {
                        await LoadVideoFromURL(url);
                    }
                    else
                    {
                        var filename = Path.GetFileNameWithoutExtension(VideoClipFileURI);
                        var clipRequest = Resources.LoadAsync<VideoClip>(filename);
                        await Task.Yield();
                        if (clipRequest.asset != null)
                        {

                            if (clipRequest.asset is VideoClip clip)
                                OnClipLoaded(clip);
                            else
                                OnClipLoadError(ASSET_TYPE_ERROR);

                        }
                        else
                        {
                            Debug.LogError(LOAD_ERROR + VideoClipFileURI);
                            Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                OnClipLoadError(INIT_ERROR + e.Message);
            }
        }

        private async Task LoadVideoFromURL(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var videoBytes = await response.Content.ReadAsByteArrayAsync();

                    var filename = Path.GetFileName(url);

                    string tempPath = Path.GetFullPath(Path.Combine(Application.temporaryCachePath, filename));

                    if (!File.Exists(tempPath))
                    {
                        File.WriteAllBytes(tempPath, videoBytes);
                    }

                    var uri = new Uri(tempPath, UriKind.Absolute);

                    player.source = VideoSource.Url;
                    player.url = uri.AbsoluteUri;
                    player.isLooping = true;
                    player.audioOutputMode = VideoAudioOutputMode.None;

                    currentRT = new RenderTexture(960, 540, 0);
                    player.targetTexture = currentRT;
                    player.Play();


                }
                catch (HttpRequestException e)
                {
                    OnClipLoadError(URL_REQUEST_ERROR + e.Message);
                }
            }
        }

        private void LogInfo(string message)
        {
            this.WaterLog(message, KW_Extensions.WaterLogMessageType.Info);
            // Debug.Log(message);
        }

        private void OnClipLoadError(string info = null)
        {
            string errorstr = string.IsNullOrEmpty(info) ? "" : $" - {info}";
            // this.WaterLog(LOAD_ERROR + VideoClipFileURI + errorstr, KW_Extensions.WaterLogMessageType.Error);
            Debug.LogError(LOAD_ERROR + VideoClipFileURI + errorstr);
            Close();
        }

        private void OnClipLoaded(VideoClip clip)
        {

            if (clip == null)
            {
                OnClipLoadError("clip is null");
                return;
            }

            player.clip = clip;
            player.isLooping = true;
            player.audioOutputMode = VideoAudioOutputMode.None;

            currentRT = new RenderTexture(960, 540, 0);
            player.targetTexture = currentRT;

            player.Play();

            // this.WaterLog(PREP_COMPLETE, KW_Extensions.WaterLogMessageType.Info);
            LogInfo(PREP_COMPLETE);
        }

        private void OnDisable()
        {
            if (player != null)
            {
                player.Stop();
                player.targetTexture = null;
            }

            KW_Extensions.SafeDestroy(tempGO);
            KW_Extensions.SafeDestroy(currentRT);
            if (DefaultTexture != null) Resources.UnloadAsset(DefaultTexture);
        }
        */


    }
}

#endif
