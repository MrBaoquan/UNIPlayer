using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UNIHper;
using DNHper;
using UniRx;
using TMPro;
using Michsky.MUIP;

namespace UNIPlayer
{
    public class UNIPlayerConsole : UIBase
    {
        public void Initialize()
        {
            Observable
                .EveryUpdate()
                .Subscribe(_ =>
                {
                    if (Keyboard.current.f1Key.wasPressedThisFrame)
                    {
                        this.Toggle();
                    }
                })
                .AddTo(this);

            this.Get<ButtonManager>("layout_menu/btn_back")
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Managements.SceneScript<SceneEntryScript>().Back2Idle();
                });

            this.Get<ButtonManager>("layout_menu/btn_compile")
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Managements.SceneScript<SceneEntryScript>().ReCompile();
                });

            this.Get<ButtonManager>("layout_menu/btn_reload")
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Managements.SceneScript<SceneEntryScript>().RegenerateMedias();
                });
        }

        // Start is called before the first frame update
        private void Start() { }

        // Update is called once per frame
        private void Update() { }

        // Called when this ui is loaded
        protected override void OnLoaded() { }

        // Called when this ui is shown
        protected override void OnShown()
        {
            if (Managements.UI.Get<VideoPlayerUI>().isShowing)
                Managements.UI.Show<VideoControlUI>();
        }

        // Called when this ui is hidden
        protected override void OnHidden()
        {
            if (Managements.UI.Get<VideoControlUI>().isShowing)
            {
                Managements.UI.Hide<VideoControlUI>();
            }
        }
    }
}
