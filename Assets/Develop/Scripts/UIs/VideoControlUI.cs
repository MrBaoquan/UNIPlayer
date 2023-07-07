using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UNIHper;
using DNHper;
using UniRx;
using Michsky.MUIP;
using TMPro;

public class VideoControlUI : UIBase
{
    public void SetPlayer(AVProPlayer _player)
    {
        Player.Value = _player;
    }

    private ReactiveProperty<AVProPlayer> Player = new ReactiveProperty<AVProPlayer>();

    // Start is called before the first frame update
    private void Start() { }

    // Update is called once per frame
    private void Update() { }

    // Called when this ui is loaded
    protected override void OnLoaded()
    {
        var _progressSlider = this.Get<SliderManager>("slider_progress");
        var _textCurrent = this.Get<TextMeshProUGUI>("slider_progress/text_current");
        var _textDuration = this.Get<TextMeshProUGUI>("slider_progress/text_duration");
        var _textSpeed = this.Get<TextMeshProUGUI>("slider_progress/text_speed");

        ReactiveProperty<bool> _isPlaying = new ReactiveProperty<bool>(false);

        _isPlaying.Subscribe(_isPlaying =>
        {
            var _btnIndex = _isPlaying ? 1 : 0;
            this.Get("slider_progress/control").SetChildrenActive(true, _btnIndex, _btnIndex, true);
        });

        this.Get<ButtonManager>("slider_progress/control/btn_play")
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                Player.Value?.Play();
            });
        this.Get<ButtonManager>("slider_progress/control/btn_pause")
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                Player.Value?.Pause();
            });

        Player.Subscribe(_player =>
        {
            if (_player == null)
                return;
            _progressSlider.mainSlider.maxValue = (float)_player.Duration;
            _textDuration.text = FormatTime(_player.Duration);
        });

        Observable
            .EveryUpdate()
            .Subscribe(_ =>
            {
                if (Player.Value == null)
                    return;
                _isPlaying.Value = Player.Value.IsPlaying;
                var _player = Player.Value;
                var _duration = _player.Duration;
                var _time = _player.CurrentTime;
                _progressSlider.mainSlider.value = (float)_time;
                _textCurrent.text = FormatTime(_time);
                _textSpeed.text = $"{_player.PlaybackRate.ToString("0.00")}x";
            })
            .AddTo(this);
    }

    private string FormatTime(double _time)
    {
        var _timeSpan = TimeSpan.FromSeconds(_time);
        if (_timeSpan.TotalMinutes >= 60)
        {
            return $"{TimeSpan.FromSeconds(_time).ToString(@"hh\:mm\:ss")} ({_time.ToString("0.000")})";
        }
        else
        {
            return $"{TimeSpan.FromSeconds(_time).ToString(@"mm\:ss")} ({_time.ToString("0.000")})";
        }
    }

    // Called when this ui is shown
    protected override void OnShown() { }

    // Called when this ui is hidden
    protected override void OnHidden() { }
}
