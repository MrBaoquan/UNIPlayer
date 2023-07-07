using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UNIHper;

namespace UNIPlayer
{
    public class IdleUI : UIBase
    {
        public void RegenerateUI()
        {
            var _uniSettings = Managements.Config.Get<UNIPlayerSettings>();
            this.Get<RectTransform>("child_uiroot")
                .Children()
                .ForEach(_child =>
                {
                    GameObject.DestroyImmediate(_child.gameObject);
                });
            this.Get<RectTransform>("child_uiroot").AppendView(_uniSettings.IdlePage);
        }

        // Start is called before the first frame update
        private void Start() { }

        // Update is called once per frame
        private void Update() { }

        // Called when this ui is loaded
        protected override void OnLoaded()
        {
            if (!Managements.Config.Get<UNIPlayerSettings>().VideoPage.animate)
            {
                Destroy(this.Get<UNIHper.UI.AnimatedUI>());
            }
        }

        private Texture2D idleImage = null;
        private IdlePage pageInfo = null;

        private string mediaPath => Path.Combine(Application.streamingAssetsPath, pageInfo.url);
        private bool IsIdleVideo
        {
            get => Path.GetExtension(mediaPath) == ".mp4";
        }
        private bool IsIdleImage
        {
            get => Path.GetExtension(mediaPath) == ".jpg" || Path.GetExtension(mediaPath) == ".png";
        }

        // Called when this ui is showing
        protected override void OnShown()
        {
            var _mediaSettings = Managements.Config.Get<UNIPlayerSettings>();
            _mediaSettings.Events.ForEach(_event =>
            {
                Interpreter.ExecuteStatement(_event.onEnterIdle).Subscribe();
            });

            if (pageInfo == null)
                pageInfo = _mediaSettings.IdlePage;

            if (!File.Exists(mediaPath))
                return;

            if (IsIdleVideo)
            {
                this.Get<AVProPlayer>("idle_video")
                    .Play(
                        mediaPath,
                        _ => { },
                        true,
                        _mediaSettings.IdlePage.startTime,
                        _mediaSettings.IdlePage.endTime
                    );
            }
            else if (IsIdleImage)
            {
                Debug.Log("加载背景图:" + mediaPath);
                if (idleImage == null)
                {
                    Managements.Resource
                        .LoadTexture2D(mediaPath)
                        .Subscribe(_tex =>
                        {
                            Debug.Log("背景图加载完成...");
                            idleImage = _tex;
                            this.Get<RawImage>("idle_image").texture = idleImage;
                            this.Get<RawImage>("idle_image").DOColor(Color.white, 1.0f);
                        });
                }
                else
                {
                    this.Get<RawImage>("idle_image").texture = idleImage;
                }
            }
        }

        // Called when this ui is hidden
        protected override void OnHidden()
        {
            Managements.Config
                .Get<UNIPlayerSettings>()
                .Events.ForEach(_event =>
                {
                    Interpreter.ExecuteStatement(_event.onLeaveIdle).Subscribe();
                });

            if (IsIdleVideo)
                this.Get<AVProPlayer>("idle_video").Stop();
        }
    }
}
