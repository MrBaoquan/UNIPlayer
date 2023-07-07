using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IOToolkit;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UNIHper;
using DNHper;
using TMPro;
using UnityEngine.InputSystem;

namespace UNIPlayer
{
    public class VideoPlayerUI : UIBase
    {
        public void StopVideo()
        {
            MultiplePlayer?.Stop();
            currentVideoInfo.Value = null;
        }

        public void PlayVideo(UNIVideo videoInfo)
        {
            var _mediaSettings = Managements.Config.Get<UNIPlayerSettings>();
            if (currentVideoInfo.Value != null && !currentVideoInfo.Value.Interruptable)
            {
                Debug.Log($"{currentVideoInfo.Value.url} 不可被打断, 需播放完成后才可以播放其他视频");
                return;
            }
            currentVideoInfo.Value = videoInfo;

            if (Managements.UI.Get<IdleUI>().isShowing)
            {
                Managements.UI.Hide<IdleUI>();
            }
            if (!Managements.UI.Get<VideoPlayerUI>().isShowing)
            {
                Managements.UI.Show<VideoPlayerUI>();
            }
            Managements.SceneScript<SceneEntryScript>().ResetOperation();
            currentVideoKey.Value = videoInfo.url;

            Interpreter.ExecuteStatement(videoInfo.onStarted).Subscribe();
            _mediaSettings.Events.ForEach(_globalEvent =>
            {
                Interpreter.ExecuteStatement(_globalEvent.onVideoStarted).Subscribe();
            });
            prepareVideo();
        }

        /// <summary>
        /// 根据视频播放模式进行相应逻辑处理
        /// </summary>
        private void prepareVideo()
        {
            clearSyncHandler();
            clearAccHandler();
            var _videoInfo = currentVideoInfo.Value;
            if (_videoInfo.playMode == Driver.autoplay)
            {
                prepareAutoPlay();
            }
            else if (_videoInfo.playMode == Driver.sync)
            {
                prepareSync();
            }
            else if (_videoInfo.playMode == Driver.accumulate)
            {
                prepareAccumulate();
            }
        }

        /// <summary>
        /// 自动播放模式
        /// </summary>
        private void prepareAutoPlay()
        {
            var _videoInfo = currentVideoInfo.Value;
            var _mediaSettings = Managements.Config.Get<UNIPlayerSettings>();

            MultiplePlayer.Play(
                _videoInfo.url,
                _ =>
                {
                    if (_videoInfo.loop)
                        return;
                    Interpreter.ExecuteStatement(_videoInfo.onFinished).Subscribe();
                    _mediaSettings.Events.ForEach(_globalEvent =>
                    {
                        Interpreter.ExecuteStatement(_globalEvent.onVideoFinished).Subscribe();
                    });
                    currentVideoInfo.Value = null;
                    Managements.SceneScript<SceneEntryScript>().Back2Idle();
                },
                _videoInfo.loop,
                _videoInfo.startTime,
                _videoInfo.endTime
            );
            MultiplePlayer
                .OnPlayerChangedAsObservable()
                .First()
                .Subscribe(_ =>
                {
                    MultiplePlayer.SetPlaybackRate(_videoInfo.playbackRate);
                    MultiplePlayer.SetVolume(_videoInfo.Volume);
                    MultiplePlayer.MuteAudio(_videoInfo.Mute);
                });
        }

        /// <summary>
        /// 同步播放模式
        /// </summary>
        IDisposable syncHandler = null;

        private void clearSyncHandler()
        {
            if (syncHandler != null)
            {
                syncHandler.Dispose();
                syncHandler = null;
            }
        }

        private void prepareSync()
        {
            var _videoInfo = currentVideoInfo.Value;
            var _mediaSettings = Managements.Config.Get<UNIPlayerSettings>();
            MultiplePlayer.Play(
                _videoInfo.url,
                _ =>
                {
                    Interpreter.ExecuteStatement(_videoInfo.onFinished).Subscribe();
                },
                false,
                _videoInfo.startTime,
                _videoInfo.endTime
            );
            MultiplePlayer.SetPlaybackRate(0);
            var _ioBinder = _videoInfo.DataTrigger;
            var _ioDevice = IOToolkit.IODeviceController.GetIODevice(_ioBinder.Device);
            var _lastValue = float.MaxValue;
            syncHandler = Observable
                .Interval(TimeSpan.FromMilliseconds(50))
                .Subscribe(_ =>
                {
                    var _duration = MultiplePlayer.Duration;
                    if (_duration <= 0)
                        return;
                    var _realDuration =
                        _videoInfo.endTime == 0
                            ? _duration
                            : _videoInfo.endTime - _videoInfo.startTime;
                    var _val = _ioDevice.GetAxis(_ioBinder.Name);
                    if (_val == _lastValue)
                    {
                        return;
                    }
                    _lastValue = _val;
                    Managements.SceneScript<SceneEntryScript>().ResetOperation();

                    var _realTime = _videoInfo.startTime + _val * _realDuration;
                    mulPlayer.Seek(_realTime);
                });
        }

        /// <summary>
        /// 累积播放模式
        /// </summary>
        IDisposable accuHandler = null;

        private void clearAccHandler()
        {
            if (accuHandler != null)
            {
                accuHandler.Dispose();
                accuHandler = null;
            }
        }

        private IDisposable accuPlayerFinishedHandler = null;

        private void prepareAccumulate()
        {
            var _videoInfo = currentVideoInfo.Value;
            var _mediaSettings = Managements.Config.Get<UNIPlayerSettings>();
            MultiplePlayer.Play(
                _videoInfo.url,
                _ =>
                {
                    currentVideoInfo.Value = null;
                    Interpreter.ExecuteStatement(_videoInfo.onFinished).Subscribe();
                },
                false,
                _videoInfo.startTime,
                _videoInfo.endTime
            );
            MultiplePlayer.SetPlaybackRate(0);
            var _ioBinder = _videoInfo.DataTrigger;
            var _ioDevice = IOToolkit.IODeviceController.GetIODevice(_ioBinder.Device);
            var _videoTime = _videoInfo.startTime;
            var _lastVal = float.MaxValue;

            if (accuPlayerFinishedHandler != null)
            {
                accuPlayerFinishedHandler.Dispose();
                accuPlayerFinishedHandler = null;
            }

            ReactiveProperty<bool> _isFinished = new ReactiveProperty<bool>(false);
            accuPlayerFinishedHandler = _isFinished.Subscribe(_ =>
            {
                if (_isFinished.Value)
                {
                    Interpreter.ExecuteStatement(_videoInfo.onFinished).Subscribe();
                    currentVideoInfo.Value = null;
                }
            });
            accuHandler = Observable
                .EveryUpdate()
                .Subscribe(_ =>
                {
                    var _duration = MultiplePlayer.Duration;
                    if (_duration <= 0)
                        return;
                    var _endTime =
                        _videoInfo.endTime <= 0
                            ? _duration
                            : _videoInfo.endTime - _videoInfo.startTime;
                    var _val = _ioDevice.GetAxis(_ioBinder.Name);
                    if (_val == _lastVal && _val == 0)
                    {
                        return;
                    }
                    _lastVal = _val;

                    Managements.SceneScript<SceneEntryScript>().ResetOperation();

                    _videoTime += _val;
                    _videoTime = Mathf.Clamp(
                        (float)_videoTime,
                        (float)_videoInfo.startTime,
                        (float)_endTime
                    );
                    mulPlayer.Seek(_videoTime);

                    _isFinished.Value = Mathf.Approximately((float)_videoTime, (float)_endTime);
                });
        }

        private MultipleAVProPlayer mulPlayer = null;
        public MultipleAVProPlayer MultiplePlayer
        {
            get => mulPlayer;
        }

        // 视频暂停点
        private Dictionary<string, List<PausePoint>> mediaPausePointDict =
            new Dictionary<string, List<PausePoint>>();

        private Dictionary<string, List<RangeTrigger>> mediaRangeTriggerDict =
            new Dictionary<string, List<RangeTrigger>>();

        private ReactiveProperty<string> currentVideoKey = new ReactiveProperty<string>();
        private ReactiveProperty<UNIVideo> currentVideoInfo = new ReactiveProperty<UNIVideo>();

        // Start is called before the first frame update
        public void Initialize()
        {
            reGenerateUIs();

            if (pausePointHandler != null)
            {
                pausePointHandler.Dispose();
                pausePointHandler = null;
            }
            if (rangeTriggerHandler != null)
            {
                rangeTriggerHandler.Dispose();
                rangeTriggerHandler = null;
            }

            // 播放新视频逻辑
            currentVideoKey.Subscribe(_newVideo =>
            {
                if (_newVideo == null)
                    return;

                videoUIRootDict.Values.ToList().ForEach(_go => _go.SetActive(false));
                videoUIRootDict[_newVideo].SetActive(true);
            });

            OnHideAsObservable()
                .Subscribe(_ =>
                {
                    currentVideoKey.Value = null;
                });
        }

        IDisposable pausePointHandler = null;

        public void BindPausePointEvents()
        {
            Indexer _bpIndexer = new Indexer(0);
            List<PausePoint> _breakPoints = new List<PausePoint>();

            // 能否进行暂停点检测的标识
            bool _canCheckPause = false;
            IDisposable _autoPlayHandler = null;

            currentVideoKey.Subscribe(_newVideo =>
            {
                if (_newVideo == null)
                    return;

                videoUIRootDict.Values.ToList().ForEach(_go => _go.SetActive(false));
                videoUIRootDict[_newVideo].SetActive(true);
                pausePointsDict[_newVideo].ForEach(_pausePoint =>
                {
                    _pausePoint.SetActive(false);
                });

                _bpIndexer = new Indexer(Mathf.Max(mediaPausePointDict[_newVideo].Count - 1, 0));
                _breakPoints = mediaPausePointDict[_newVideo];
                _canCheckPause = _breakPoints != null && _breakPoints.Count > 0;
            });

            // 清除自动播放逻辑
            Action _clearAutoPlay = () =>
            {
                if (_autoPlayHandler != null)
                {
                    _autoPlayHandler.Dispose();
                    _autoPlayHandler = null;
                }
            };

            Action _checkNext = () =>
            {
                if (_bpIndexer.CheckNextOverflow())
                {
                    _breakPoints = null;
                }
                else
                {
                    _canCheckPause = true;
                    _bpIndexer.Next();
                }
            };

            Action _restorePlay = () =>
            {
                if (isShowing && mulPlayer.IsPaused)
                {
                    mulPlayer.Play(false);
                    pausePointsDict[currentVideoKey.Value].ForEach(_pausePointRoot =>
                    {
                        _pausePointRoot.SetActive(false);
                    });
                }
            };

            // 按下按钮触发继续播放逻辑
            mediaPausePointDict
                .ToList()
                .ForEach(_kv =>
                {
                    _kv.Value
                        .WithIndex()
                        .ToList()
                        .ForEach(_item =>
                        {
                            IODeviceController
                                .GetIODevice(_item.item.Trigger.Device)
                                .BindAction(
                                    _item.item.Trigger.Name,
                                    _item.item.Trigger.Event,
                                    (_videoKey, breakIndex) =>
                                    {
                                        if (!mulPlayer.IsPaused)
                                            return;
                                        if (
                                            _videoKey == currentVideoKey.Value
                                            && breakIndex == _bpIndexer.Current
                                        )
                                        {
                                            Interpreter
                                                .ExecuteStatement(
                                                    mediaPausePointDict[currentVideoKey.Value][
                                                        breakIndex
                                                    ].onContinue
                                                )
                                                .Subscribe();
                                            _restorePlay();
                                            _clearAutoPlay();
                                            _checkNext();
                                        }
                                    },
                                    _kv.Key,
                                    _item.index
                                );
                        });
                });

            // 自动恢复播放
            Action<float> _autoPlay = (_autoContinueTime) =>
            {
                _clearAutoPlay();
                _autoPlayHandler = Observable
                    .Timer(TimeSpan.FromSeconds(_autoContinueTime))
                    .Subscribe(_ =>
                    {
                        _restorePlay();
                        _checkNext();
                    });
            };

            //   断点暂停视频
            pausePointHandler = Observable
                .EveryUpdate()
                .Subscribe(_ =>
                {
                    if (!isShowing || !_canCheckPause)
                        return;

                    var _breakPoint = _breakPoints[_bpIndexer.Current];
                    var _bpTime = _breakPoint.time;
                    if (mulPlayer.CurrentTime >= _bpTime)
                    {
                        mulPlayer.Pause();
                        if (_breakPoint.autoContinueTime > 0)
                            _autoPlay(_breakPoint.autoContinueTime);
                        pausePointsDict[currentVideoKey.Value].ForEach(_pausePoint =>
                        {
                            _pausePoint.SetActive(false);
                        });
                        pausePointsDict[currentVideoKey.Value][_bpIndexer.Current].SetActive(true);
                        _canCheckPause = false;
                    }
                });
        }

        IDisposable rangeTriggerHandler = null;

        public void BindRangeTriggerEvents()
        {
            // 播放新视频逻辑
            currentVideoKey.Subscribe(_newVideo =>
            {
                if (_newVideo == null)
                    return;

                rangeTriggersDict[_newVideo].ForEach(_rangeTrigger =>
                {
                    _rangeTrigger.SetActive(false);
                });

                handleRangeTriggers();
            });
            mediaRangeTriggerDict
                .ToList()
                .ForEach(_kv =>
                {
                    _kv.Value
                        .WithIndex()
                        .ToList()
                        .ForEach(_item =>
                        {
                            if (_item.item.Action == string.Empty)
                                return;
                            IODeviceController
                                .GetIODevice(_item.item.Trigger.Device)
                                .BindAction(
                                    _item.item.Trigger.Name,
                                    _item.item.Trigger.Event,
                                    () =>
                                    {
                                        if (currentVideoKey.Value != _kv.Key)
                                            return;
                                        var _rangeTriggerRoot = rangeTriggersDict[
                                            currentVideoKey.Value
                                        ][_item.index];
                                        var _event = rangeTriggerEvents[_item.index];
                                        if (_event.IsHit && !_rangeTriggerRoot.activeInHierarchy)
                                        {
                                            _rangeTriggerRoot.SetActive(true);
                                            Interpreter
                                                .ExecuteStatement(_item.item.onAction)
                                                .Subscribe();
                                        }
                                    }
                                );
                        });
                });

            // 更新区域触发器逻辑
            rangeTriggerHandler = Observable
                .EveryUpdate()
                .Subscribe(_ =>
                {
                    var _videoTime = MultiplePlayer.CurrentTime;
                    this.rangeTriggerEvents.ForEach(_event => _event.UpdateLogic(_videoTime));
                })
                .AddTo(this);
        }

        // 记录视频对应的暂停点记录
        Dictionary<string, List<GameObject>> pausePointsDict =
            new Dictionary<string, List<GameObject>>();
        Dictionary<string, List<GameObject>> rangeTriggersDict =
            new Dictionary<string, List<GameObject>>();

        // 视频的UI根节点
        Dictionary<string, GameObject> videoUIRootDict = null;

        private void reGenerateUIs()
        {
            // 销毁旧的UI
            if (videoUIRootDict != null)
            {
                videoUIRootDict.Values.ToList().ForEach(_root => Destroy(_root));
            }
            pausePointsDict.Clear();
            rangeTriggersDict.Clear();

            // 创建UI结构
            var _uniSettings = Managements.Config.Get<UNIPlayerSettings>();

            GetComponent<RectTransform>().AppendView(_uniSettings.VideoPage);

            this.mediaPausePointDict = _uniSettings.VideoInfos.ToDictionary(
                _mediaInfo => _mediaInfo.url,
                _mediaInfo =>
                    _mediaInfo.PausePoints == null
                        ? new List<PausePoint>()
                        : _mediaInfo.PausePoints.OrderBy(_ => _.time).ToList()
            );
            this.mediaRangeTriggerDict = _uniSettings.VideoInfos.ToDictionary(
                _mediaInfo => _mediaInfo.url,
                _mediaInfo =>
                    _mediaInfo.RangeTriggers == null
                        ? new List<RangeTrigger>()
                        : _mediaInfo.RangeTriggers
            );

            videoUIRootDict = _uniSettings.VideoInfos.ToDictionary(
                _ => _.url,
                _video =>
                {
                    var _videoUIRoot = new GameObject(_video.url);
                    _videoUIRoot.transform.parent = this.transform;
                    var _rectTransform = _videoUIRoot.AddComponent<RectTransform>();
                    _rectTransform.anchorMin = Vector2.zero;
                    _rectTransform.anchorMax = Vector2.one;
                    _rectTransform.offsetMin = Vector2.zero;
                    _rectTransform.offsetMax = Vector2.zero;
                    _rectTransform.localScale = Vector3.one;

                    _rectTransform.AppendView(_video);
                    return _videoUIRoot;
                }
            );

            // 创建暂停点UI
            mediaPausePointDict
                .ToList()
                .ForEach(_kv =>
                {
                    pausePointsDict.Add(_kv.Key, new List<GameObject>());
                    _kv.Value.ForEach(_breakPoint =>
                    {
                        var _breakPointsRoot = new GameObject(_breakPoint.time.ToString());
                        pausePointsDict[_kv.Key].Add(_breakPointsRoot);
                        _breakPointsRoot.transform.parent = videoUIRootDict[_kv.Key].transform;
                        var _rectTransform = _breakPointsRoot.AddComponent<RectTransform>();
                        _rectTransform.anchorMin = Vector2.zero;
                        _rectTransform.anchorMax = Vector2.one;
                        _rectTransform.offsetMin = Vector2.zero;
                        _rectTransform.offsetMax = Vector2.zero;
                        _rectTransform.localScale = Vector3.one;
                        _rectTransform.AppendView(_breakPoint);
                    });
                });

            // 创建区间触发器UI
            mediaRangeTriggerDict
                .ToList()
                .ForEach(_kv =>
                {
                    rangeTriggersDict.Add(_kv.Key, new List<GameObject>());
                    _kv.Value.ForEach(_rangeTrigger =>
                    {
                        var _rangeTriggerRoot = new GameObject(
                            _rangeTrigger.startTime + "-" + _rangeTrigger.endTime
                        );
                        _rangeTriggerRoot.transform.parent = videoUIRootDict[_kv.Key].transform;
                        rangeTriggersDict[_kv.Key].Add(_rangeTriggerRoot);
                        var _rectTransform = _rangeTriggerRoot.AddComponent<RectTransform>();
                        _rectTransform.anchorMin = Vector2.zero;
                        _rectTransform.anchorMax = Vector2.one;
                        _rectTransform.offsetMin = Vector2.zero;
                        _rectTransform.offsetMax = Vector2.zero;
                        _rectTransform.localScale = Vector3.one;
                        _rectTransform.AppendView(_rangeTrigger);
                    });
                });
        }

        class RangeTriggerEvent
        {
            public RangeTrigger rangeTrigger = new RangeTrigger();
            public ReactiveProperty<bool> onEnter = new ReactiveProperty<bool>(false);
            public ReactiveProperty<bool> onLeave = new ReactiveProperty<bool>(false);

            public bool IsHit
            {
                get => onEnter.Value;
            }

            public void UpdateLogic(double videoTime)
            {
                onEnter.Value =
                    videoTime >= rangeTrigger.startTime && videoTime <= rangeTrigger.endTime;
                onLeave.Value =
                    videoTime > rangeTrigger.endTime || videoTime < rangeTrigger.startTime;
            }
        }

        List<RangeTriggerEvent> rangeTriggerEvents = new List<RangeTriggerEvent>();

        private void handleRangeTriggers()
        {
            this.rangeTriggerEvents = mediaRangeTriggerDict[currentVideoKey.Value]
                .WithIndex()
                .Select(_item =>
                {
                    var _event = new RangeTriggerEvent { rangeTrigger = _item.item };
                    var _rangeTriggerRoot = rangeTriggersDict[currentVideoKey.Value][_item.index];
                    _event.onEnter.Subscribe(_val =>
                    {
                        if (
                            _val
                            && _item.item.Action == string.Empty
                            && !_rangeTriggerRoot.activeInHierarchy
                        )
                        {
                            _rangeTriggerRoot.SetActive(true);
                            Interpreter.ExecuteStatement(_item.item.onEnter).Subscribe();
                        }
                    });
                    _event.onLeave.Subscribe(_val =>
                    {
                        if (_val && _rangeTriggerRoot.activeInHierarchy)
                        {
                            _rangeTriggerRoot.SetActive(false);
                            Interpreter.ExecuteStatement(_item.item.onLeave).Subscribe();
                        }
                    });
                    return _event;
                })
                .ToList();
        }

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                mulPlayer.TogglePlay();
            }
        }

        // Called when this ui is loaded
        protected override void OnLoaded()
        {
            mulPlayer = this.Get<MultipleAVProPlayer>("mediaPlayer");
            if (!Managements.Config.Get<UNIPlayerSettings>().IdlePage.animate)
            {
                Destroy(this.Get<UNIHper.UI.AnimatedUI>());
            }
            mulPlayer
                .OnPlayerChangedAsObservable()
                .Subscribe(_avProPlayer =>
                {
                    Managements.UI.Get<VideoControlUI>().SetPlayer(_avProPlayer);
                });
        }

        // Called when this ui is showing
        protected override void OnShown()
        {
            if (Managements.UI.Get<UNIPlayerConsole>().isShowing)
                Managements.UI.Show<VideoControlUI>();
        }

        // Called when this ui is hidden
        protected override void OnHidden()
        {
            clearSyncHandler();
            clearAccHandler();
            Managements.UI.Hide<VideoControlUI>();
        }
    }
}
