using System.Xml.Schema;
using System.Xml;
using System.Collections.Generic;
using System.Net;
using System;
using System.Linq;
using IOToolkit;
using IOToolkit_Extension;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UNIHper;
using DNHper;

namespace UNIPlayer
{
    public class SceneEntryScript : SceneScriptBase
    {
        private System.Diagnostics.Stopwatch _idleTimer = new System.Diagnostics.Stopwatch();

        public bool canBreakIdle
        {
            get
            {
                Debug.Log("_idleTimer.ElapsedMilliseconds: " + _idleTimer.ElapsedMilliseconds);
                return _idleTimer.ElapsedMilliseconds
                    > Managements.Config.Get<UNIPlayerSettings>().MinIdleTime * 1000;
            }
        }

        /// <summary>
        ///  返回待机操作
        /// </summary>
        public void Back2Idle()
        {
            if (!Managements.UI.Get<IdleUI>().isShowing)
            {
                Managements.UI.Get<VideoPlayerUI>().StopVideo();
                Managements.UI.Hide<VideoPlayerUI>();
                Managements.UI.Show<IdleUI>();
                _idleTimer.Restart();
            }
        }

        public void PlayVideo(string Trigger)
        {
            var _video = uniSettings.NextVideo(Trigger);
            if (_video == null)
            {
                Debug.LogWarning($"未找到视频触发键: {Trigger}");
                return;
            }

            PlayVideo(_video);
        }

        public void PlaySoundEffect(string Trigger, float Volume = 1.0f)
        {
            var _audioInfo = uniSettings.AudioInfos
                .Where(_ => _.RawTrigger == Trigger)
                .FirstOrDefault();
            if (_audioInfo == null)
                return;
            PlaySoundEffect(_audioInfo);
        }

        public void PlaySound(string Trigger, float Volume = 1.0f)
        {
            var _audioInfo = uniSettings.AudioInfos
                .Where(_ => _.RawTrigger == Trigger)
                .FirstOrDefault();
            if (_audioInfo == null)
                return;
            PlaySound(_audioInfo, Volume);
        }

        public void PlayMusic(string Trigger, bool Loop = true, float Volume = 1.0f)
        {
            var _audioInfo = uniSettings.AudioInfos
                .Where(_ => _.RawTrigger == Trigger)
                .FirstOrDefault();
            if (_audioInfo == null)
                return;
            Managements.Audio.PlayMusic(
                Managements.Resource.Get<AudioClip>(_audioInfo.FileName),
                Volume,
                Loop
            );
        }

        /// <summary>
        /// 视频播放逻辑接口
        /// </summary>
        /// <param name="videoInfo"></param>
        public void PlayVideo(UNIVideo videoInfo)
        {
            if (!canBreakIdle)
            {
                Debug.LogWarning("当前待机无法中断, 请稍后再试...");
                return;
            }
            Managements.UI.Get<VideoPlayerUI>().PlayVideo(videoInfo);
        }

        public void PlaySoundEffect(UNIMedia audioInfo, float InVolume = 1.0f)
        {
            Interpreter.ExecuteStatement(audioInfo.onStarted).Subscribe();
            uniSettings.Events.ForEach(_event =>
            {
                Interpreter.ExecuteStatement(_event.onAudioStarted).Subscribe();
            });
            Managements.Audio.PlayEffect(
                Managements.Resource.Get<AudioClip>(audioInfo.FileName),
                InVolume
            );
        }

        public void PlaySound(UNIAudio audioInfo, float InVolume = 1.0f, bool bMute = false)
        {
            Interpreter.ExecuteStatement(audioInfo.onStarted).Subscribe();
            uniSettings.Events.ForEach(_event =>
            {
                Interpreter.ExecuteStatement(_event.onAudioStarted).Subscribe();
            });
            Managements.Audio
                .PlayMusic(
                    Managements.Resource.Get<AudioClip>(audioInfo.FileName),
                    InVolume,
                    audioInfo.loop,
                    1
                )
                .mute = bMute;
        }

        private LongTimeNoOperation longTimeNoOperation = null;

        public void ResetOperation()
        {
            if (longTimeNoOperation == null)
            {
                longTimeNoOperation = new LongTimeNoOperation(
                    Managements.Config.Get<UNIPlayerSettings>().AutoBack2Idle,
                    () =>
                    {
                        if (Managements.UI.Get<IdleUI>().isShowing)
                            return;
                        Debug.Log("LongTimeNoOperation, execute onLongTimeNoInteractive...");
                        uniSettings.Events.ForEach(_event =>
                        {
                            Interpreter
                                .ExecuteStatement(_event.onLongTimeNoInteractive)
                                .Subscribe();
                        });
                    }
                );
            }
            longTimeNoOperation.ResetOperation();
        }

        private void runTestCode()
        {
            Interpreter
                .ExecuteStatement(@"SetDOOn(TurnOnLight); Delay(1000); SetDOOff(TurnOnLight);")
                .Subscribe(_ =>
                {
                    Debug.LogWarning("执行完成...");
                });
        }

        static void checkXMLValid(string filePath)
        {
            Action<object, ValidationEventArgs> ValidationCallBack = (sender, args) =>
            {
                Debug.LogError("XML Validation Error: " + args.Message);
            };

            try
            {
                var _xml = new System.Xml.XmlDocument();
                _xml.Load(filePath);

                XmlReaderSettings _settings = new XmlReaderSettings();
                _settings.ValidationType = ValidationType.Schema;
                _settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                _settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

                XmlReader _reader = XmlReader.Create(new XmlNodeReader(_xml), _settings);
                while (_reader.Read()) { }
            }
            catch (System.Exception e)
            {
                Debug.LogError("XML Validation Error: " + e.Message);
                SRDebug.Instance.ShowDebugPanel(SRDebugger.DefaultTabs.Console);
            }
        }

        UNIPlayerSettings uniSettings;

        public async void ReCompile()
        {
            try
            {
                Debug.LogWarning("Compile Start...");
                try
                {
                    uniSettings = Managements.Config.Reload<UNIPlayerSettings>();
                }
                catch (System.Exception)
                {
                    checkXMLValid(uniSettings.FilePath);
                    Debug.LogError("配置文件加载失败, 请检查配置文件格式是否正确...");
                    return;
                }

                Managements.UI.Get<IdleUI>().RegenerateUI();
                Managements.UI.Get<VideoPlayerUI>().Initialize();

                // 加载视频
                var _videoPaths = uniSettings.VideoInfos
                    .Where(_video => _video.fileExists)
                    .Select(_video => _video.url);

                await Managements.UI
                    .Show<VideoPlayerUI>()
                    .MultiplePlayer.PrepareVideos(_videoPaths);
                Managements.UI.Get<VideoPlayerUI>().Initialize();
                Managements.UI.Hide<VideoPlayerUI>();
                await Managements.Resource.AppendAudioClips(
                    uniSettings.AudioInfos.Select(_info => _info.FullPath)
                );
                Back2Idle();
                regenerateIOConfig();
                rebindIOEvents();
                Debug.LogWarning("Compile Finished...");
                SRDebug.Instance.HideDebugPanel();
                Managements.UI.Show<TipUI>().ShowInfo("通知", "编译成功");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Compile Error...");
                Debug.LogError("stackTrace: " + e.StackTrace);
                Debug.LogError("message: " + e.Message);
                SRDebug.Instance.ShowDebugPanel(SRDebugger.DefaultTabs.Console);
            }
        }

        // Called once after scene is loaded
        private void Start()
        {
            uniSettings = Managements.Config.Get<UNIPlayerSettings>();
            Managements.UI.Get<UNIPlayerConsole>().Initialize();
            ReCompile();
        }

        private void rebindIOEvents()
        {
            IODeviceController.UnLoad();
            IODeviceController.Load();

            // 为视频绑定IO事件
            uniSettings.GroupVideoInfos
                .ToList()
                .ForEach(_kv =>
                {
                    IODeviceController
                        .GetIODevice(_kv.Key.Device)
                        .BindAction(
                            _kv.Key.Name,
                            _kv.Key.Event,
                            () =>
                            {
                                var _video = _kv.Value.NextMedia as UNIVideo;
                                if (!_video.fileExists)
                                {
                                    Debug.LogWarning($"文件不存在:{_video.FullPath}");
                                    return;
                                }
                                PlayVideo(_video);
                            }
                        );
                });

            // 为视频暂停点绑定IO事件
            Managements.UI.Get<VideoPlayerUI>().BindPausePointEvents();

            // 为区间触发器绑定IO事件
            Managements.UI.Get<VideoPlayerUI>().BindRangeTriggerEvents();

            // 为音频绑定IO事件
            uniSettings.GroupAudioInfos
                .ToList()
                .ForEach(_kv =>
                {
                    IODeviceController
                        .GetIODevice(_kv.Key.Device)
                        .BindAction(
                            _kv.Key.Name,
                            _kv.Key.Event,
                            () =>
                            {
                                var _audioInfo = _kv.Value.NextMedia as UNIAudio;
                                PlaySound(_audioInfo, _audioInfo.Volume, _audioInfo.Mute);
                            }
                        );
                });

            // IO Axis 事件处理
            uniSettings.IOEvents
                .Where(_event => _event.Trigger.Event == InputEvent.IE_Axis)
                .ToList()
                .ForEach(_ioEvent =>
                {
                    IODeviceController
                        .GetIODevice(_ioEvent.Trigger.Device)
                        .BindAxis(
                            _ioEvent.Trigger.Name,
                            _val =>
                            {
                                //Debug.Log(_val);
                                Interpreter.ExecuteStatement(_ioEvent.onTrigger, _val).Subscribe();
                            }
                        );
                });

            // IOAction 事件处理
            uniSettings.IOEvents
                .Where(_event => _event.Trigger.Event != InputEvent.IE_Axis)
                .ToList()
                .ForEach(_ioEvent =>
                {
                    IODeviceController
                        .GetIODevice(_ioEvent.Trigger.Device)
                        .BindAction(
                            _ioEvent.Trigger.Name,
                            _ioEvent.Trigger.Event,
                            () =>
                            {
                                Interpreter.ExecuteStatement(_ioEvent.onTrigger).Subscribe();
                            }
                        );
                });
        }

        private IORoot ioRoot => IORoot.Instance;
        List<string> keys = new List<string>()
        {
            "One",
            "Two",
            "Three",
            "Four",
            "Five",
            "Six",
            "Seven",
            "Eight",
            "Nine",
            "Zero",
            "NumPadZero",
            "NumPadOne",
            "NumPadTwo",
            "NumPadThree",
            "NumPadFour",
            "NumPadFive",
            "NumPadSix",
            "NumPadSeven",
            "NumPadEight",
            "NumPadNine",
            "Q",
            "W",
            "E",
            "R",
            "T",
            "Y",
            "U",
            "I",
            "O",
            "P",
            "A",
            "S",
            "D",
            "F",
            "G",
            "H",
            "J",
            "K",
            "L",
            "Z",
            "X",
            "C",
            "V",
            "B",
            "N",
            "M",
        };

        private void regenerateIOConfig(bool clear = false)
        {
            // 视频类相关IO事件
            var _videoBinds = uniSettings.VideoInfos.Select(_videoInfo => _videoInfo.Trigger);

            var _videoDataBinds = uniSettings.VideoInfos
                .Where(_videoInfo => _videoInfo.DataTrigger != null)
                .Select(_videoInfo =>
                {
                    _videoInfo.DataTrigger.Event = InputEvent.IE_Axis;
                    return _videoInfo.DataTrigger;
                });

            // 暂停点相关IO事件
            var _pauseBinds = uniSettings.VideoInfos
                .SelectMany(_videoInfo => _videoInfo.PausePoints)
                .Select(_pausePoint => _pausePoint.Trigger);

            // 区间触发器相关IO事件
            var _rangeBinds = uniSettings.VideoInfos
                .SelectMany(_videoInfo => _videoInfo.RangeTriggers)
                .Select(_rangeTrigger => _rangeTrigger.Trigger);

            // 音频类相关IO事件
            var _audioBinds = uniSettings.AudioInfos.Select(_audioInfo => _audioInfo.Trigger);

            // 自定义IO事件
            var _customBinds = uniSettings.IOEvents.Select(_ioEvent => _ioEvent.Trigger);

            var _allBinds = new List<IOBindParser>()
                .Concat(_videoBinds)
                .Concat(_videoDataBinds)
                .Concat(_audioBinds)
                .Concat(_customBinds)
                .Concat(_pauseBinds)
                .Concat(_rangeBinds)
                .Where(_bind => _bind != null && !string.IsNullOrEmpty(_bind.Name))
                .ToList();
            ioRoot.Load();

            if (clear)
            {
                ioRoot.Devices.ForEach(_device =>
                {
                    _device.Actions.Clear();
                    _device.Axes.Clear();
                });
            }

            // remove unused device
            ioRoot.Devices
                .Where(_device => !_allBinds.Any(_bind => _bind.Device == _device.Name))
                .ToList()
                .ForEach(_device => ioRoot.Devices.Remove(_device));

            _allBinds
                .Where(_bind => _bind.Event != InputEvent.IE_Axis)
                .WithIndex()
                .ToList()
                .ForEach(_item =>
                {
                    var _actionArgs = _item.item;
                    var _device = ioRoot.FirstOrCreate(_actionArgs.Device);
                    var _action = _device.FirstOrCreateAction(_actionArgs.Name);
                    if (_action.Keys.Count <= 0) // 仅在Action结点没有Key时才添加
                    {
                        _action.FirstOrCreate($"Button_{(_item.index).ToString("00")}");
                        _action.FirstOrCreate(keys[_item.index]);
                    }
                });

            _allBinds
                .Where(_bind => _bind.Event == InputEvent.IE_Axis)
                .WithIndex()
                .ToList()
                .ForEach(_item =>
                {
                    var _axisArgs = _item.item;
                    var _device = ioRoot.FirstOrCreate(_axisArgs.Device);
                    var _axis = _device.FirstOrCreateAxis(_axisArgs.Name);
                    if (_axis.Keys.Count <= 0) // 仅在Axis结点没有Key时才添加
                    {
                        _axis.FirstOrCreate($"Axis_{(_item.index).ToString("00")}");
                    }
                });
            ioRoot.Save();
        }

        public void RegenerateMedias()
        {
            Managements.UI.ShowConfirmPanel(
                "重新扫描并加载媒体资源将会重置[Video/Audio]结点, 且IODevice.xml也会进行Action&Axis结点同步, 请谨慎操作! (建议在非生产环境下进行此操作)。",
                () =>
                {
                    Managements.Config.Get<UNIPlayerSettings>().RegenerateMedias();
                    regenerateIOConfig(true);
                    rebindIOEvents();
                }
            );
        }

        // Called once per frame after Start
        private void Update()
        {
            IODeviceController.Update();
            if (Keyboard.current.f4Key.wasPressedThisFrame)
            {
                Back2Idle();
            }
            if (Keyboard.current.f5Key.wasPressedThisFrame)
            {
                ReCompile();
            }
            if (Keyboard.current.f6Key.wasPressedThisFrame)
            {
                RegenerateMedias();
            }
        }

        // Called when scene is unloaded
        private void OnDestroy() { }

        // Called when application is quit
        private void OnApplicationQuit()
        {
            IODeviceController.UnLoad();
        }
    }
}
