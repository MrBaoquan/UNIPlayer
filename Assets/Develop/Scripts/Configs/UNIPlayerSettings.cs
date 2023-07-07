using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UNIHper;
using DNHper;
using IOToolkit_Extension;

namespace UNIPlayer
{
#region Cell Settings
    public class PausePoint : UNIView
    {
        [DefaultValueAttribute(0.0)]
        [XmlAttribute]
        public float time = 0.0f;

        private string _continueAction = "";

        [DefaultValueAttribute("")]
        [XmlAttribute("continueAction")]
        public string ContinueAction
        {
            get => _continueAction;
            set
            {
                _continueAction = value;
                Trigger = new IOBindParser(_continueAction);
            }
        }

        [XmlIgnore]
        public IOBindParser Trigger { get; protected set; }

        [DefaultValueAttribute(0)]
        [XmlAttribute]
        public int autoContinueTime = 0;

        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onContinue = "";
    }

    public class RangeTrigger : UNIView
    {
        [DefaultValueAttribute(0.0f)]
        [XmlAttribute]
        public float startTime = 0.0f;

        [DefaultValueAttribute(0.0f)]
        [XmlAttribute]
        public float endTime = 0.0f;

        private string _Action = "";

        [XmlAttribute("action")]
        public string Action
        {
            get => _Action;
            set
            {
                _Action = value;
                this.Trigger = new IOBindParser(Action);
            }
        }

        [XmlIgnore]
        public IOBindParser Trigger { get; protected set; }

        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onAction = "";

        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onEnter = "";

        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onLeave = "";
    }

    public class UNIView
    {
        [XmlElement("Button")]
        public List<UNIButton> Buttons = null;

        public bool ShouldSerializeButtons()
        {
            return Buttons != null && Buttons.Count > 0;
        }

        [XmlElement("Image")]
        public List<UNIImage> Images = null;

        public bool ShouldSerializeImages()
        {
            return Images != null && Images.Count > 0;
        }

        [DefaultValueAttribute(false)]
        [XmlAttribute]
        public bool animate = false;
    }

    public class IdlePage : UNIView
    {
        [XmlAttribute]
        public string url = string.Empty;

        [DefaultValueAttribute(0.0)]
        [XmlAttribute]
        public double startTime = 0.0;

        [DefaultValueAttribute(0.0)]
        [XmlAttribute]
        public double endTime = 0.0;
    }

    public class VideoPage : UNIView
    {
        private Dictionary<IOBindParser, MediaList> videoInfosGroupByTrigger = null;

        private Dictionary<IOBindParser, MediaList> refreshVideoInfoGroups()
        {
            videoInfosGroupByTrigger = _VideoInfos
                .GroupBy(_info => _info.RawTrigger)
                .ToDictionary(
                    _ => _.First().Trigger,
                    _ => new MediaList { Medias = _.OfType<UNIMedia>().ToList() }
                );
            return videoInfosGroupByTrigger;
        }

        [XmlIgnore]
        public Dictionary<IOBindParser, MediaList> VideoInfosGroupByTrigger =>
            videoInfosGroupByTrigger == null ? refreshVideoInfoGroups() : videoInfosGroupByTrigger;

        private List<UNIVideo> _VideoInfos = new List<UNIVideo>();

        [XmlElement("Video")]
        public List<UNIVideo> VideoInfos
        {
            get => _VideoInfos;
            set
            {
                _VideoInfos = value;
                refreshVideoInfoGroups();
            }
        }

        public bool ShouldSerializeVideoInfos()
        {
            return VideoInfos != null && VideoInfos.Count > 0;
        }
    }

    public class AudioPage : UNIView
    {
        [XmlElement("Audio")]
        public List<UNIAudio> AudioInfos = new List<UNIAudio>();

        public bool ShouldSerializeAudioInfos()
        {
            return AudioInfos != null && AudioInfos.Count > 0;
        }

        private Dictionary<IOBindParser, MediaList> audioInfosGroupByTrigger = null;

        private Dictionary<IOBindParser, MediaList> refreshAudioInfoGroups()
        {
            audioInfosGroupByTrigger = AudioInfos
                .GroupBy(_info => _info.RawTrigger)
                .ToDictionary(
                    _ => _.First().Trigger,
                    _ => new MediaList { Medias = _.Cast<UNIMedia>().ToList() }
                );
            return audioInfosGroupByTrigger;
        }

        [XmlIgnore]
        public Dictionary<IOBindParser, MediaList> AudioInfosGroupByTrigger =>
            audioInfosGroupByTrigger == null ? refreshAudioInfoGroups() : audioInfosGroupByTrigger;
    }

    public class UNIMedia : UNIView
    {
        private string _RawTrigger = string.Empty;

        [XmlAttribute("trigger")]
        public string RawTrigger
        {
            get => _RawTrigger;
            set
            {
                _RawTrigger = value;
                this.Trigger = new IOBindParser(_RawTrigger);
            }
        }

        [XmlIgnore]
        public string FullPath
        {
            get => Path.Combine(Application.streamingAssetsPath, url);
        }

        [XmlIgnore]
        public string FileName
        {
            get => Path.GetFileNameWithoutExtension(FullPath);
        }

        [XmlIgnore]
        public bool fileExists
        {
            get => File.Exists(FullPath);
        }

        [XmlIgnore]
        public IOBindParser Trigger { get; protected set; }

        [XmlAttribute]
        public string url = string.Empty;

        [XmlIgnore]
        public bool IsAudio
        {
            get => url.EndsWith(".mp3");
        }

        [XmlIgnore]
        public bool IsVideo
        {
            get => url.EndsWith(".mp4");
        }

        [DefaultValueAttribute(1.0f)]
        [XmlElement("volume")]
        public float Volume { get; set; } = 1.0f;

        [DefaultValueAttribute(false)]
        [XmlAttribute("mute")]
        public bool Mute { get; set; } = false;

        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onStarted = "";

        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onFinished = "";

        [DefaultValueAttribute(false)]
        [XmlAttribute]
        public bool loop = false;
    }

    public enum Driver
    {
        autoplay,
        accumulate,
        sync
    }

    public class UNIVideo : UNIMedia
    {
        [DefaultValueAttribute(Driver.autoplay)]
        [XmlAttribute]
        public Driver playMode = Driver.autoplay;

        private string _DataSource = string.Empty;

        [DefaultValueAttribute("")]
        [XmlAttribute("dataSource")]
        public string DataSource
        {
            get => _DataSource;
            set
            {
                _DataSource = value;
                DataTrigger = new IOBindParser(_DataSource);
            }
        }

        /// <summary>
        /// 视频进度控制数据源 @example: dataSource="dev:control"
        /// </summary>
        [XmlIgnore]
        public IOBindParser DataTrigger = null;

        [DefaultValueAttribute(0.0)]
        [XmlAttribute]
        public double startTime = 0.0;

        [DefaultValueAttribute(0.0)]
        [XmlAttribute]
        public double endTime = 0.0;

        [DefaultValueAttribute(1.0)]
        [XmlAttribute]
        public float playbackRate = 1.0f;

        [DefaultValueAttribute(true)]
        [XmlAttribute("interruptable")]
        public bool Interruptable = true;

        [XmlElement("PausePoint")]
        public List<PausePoint> PausePoints = new List<PausePoint>();

        public bool ShouldSerializePausePoints()
        {
            return PausePoints.Count > 0;
        }

        [XmlElement("RangeTrigger")]
        public List<RangeTrigger> RangeTriggers = new List<RangeTrigger>();

        public bool ShouldSerializeRangeTriggers()
        {
            return RangeTriggers.Count > 0;
        }
    }

    public class UNIAudio : UNIMedia { }

    public class GlobalEvent
    {
        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onEnterIdle = "";

        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onLeaveIdle = "";

        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onVideoStarted = "";

        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onVideoFinished = "";

        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onAudioStarted = "";

        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onAudioFinished = "";

        [DefaultValueAttribute("")]
        [XmlAttribute]
        public string onLongTimeNoInteractive = "Back2Idle();";
    }

    public class IOEvent
    {
        private string _RawIOContent = string.Empty;

        [XmlAttribute("trigger")]
        public string RawIOContent
        {
            get => _RawIOContent;
            set
            {
                _RawIOContent = value;
                Trigger = new IOBindParser(_RawIOContent);
            }
        }

        [XmlIgnore]
        public IOBindParser Trigger = null;

        [XmlAttribute]
        public string onTrigger = string.Empty;
    }

    public class MediaList
    {
        private List<UNIMedia> medias;
        private Indexer indexer = new Indexer(0);

        [XmlIgnore]
        public List<UNIMedia> Medias
        {
            get => medias;
            set
            {
                medias = value;
                indexer.SetMax(medias.Count - 1);
                indexer.SetToMax();
            }
        }

        public UNIMedia NextMedia
        {
            get { return medias[indexer.Next()]; }
        }
    }

    /// <summary>
    /// 输入事件绑定解析器
    /// </summary>
    public class IOBindParser
    {
        public IOBindParser(string Content)
        {
            this.RawContent = Content;
            Reload();
        }

        /// <summary>
        /// 原始字符串
        /// </summary>
        public string RawContent = string.Empty;

        /// <summary>
        /// 事件名称
        /// </summary>
        /// <value></value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 设备名称
        /// </summary>
        /// <value></value>
        public string Device { get; set; } = "default";

        /// <summary>
        /// 事件类型
        /// </summary>
        public IOToolkit.InputEvent Event = IOToolkit.InputEvent.IE_Pressed;

        public override bool Equals(object obj)
        {
            var _other = obj as IOBindParser;
            return this.Device == _other.Device
                && this.Name == _other.Name
                && this.Event == _other.Event;
        }

        public override int GetHashCode()
        {
            return (Device, Name, Event).GetHashCode();
        }

        /// <summary>
        /// 重载解析器
        /// </summary>
        /// <returns></returns>
        public IOBindParser Reload()
        {
            if (string.IsNullOrEmpty(RawContent))
                return this;

            // 获取设备名
            if (RawContent.Contains(":"))
                this.Device = Regex.Replace(RawContent, ":.+", string.Empty);

            // 获取Action
            this.Name = RawContent;
            if (this.Name.Contains(":"))
                this.Name = Regex.Replace(this.Name, ".+:", string.Empty);
            if (this.Name.Contains("@"))
                this.Name = Regex.Replace(this.Name, "@.+", string.Empty);

            // 获取事件类型
            var _actionContent = Regex.Replace(RawContent, ".+:", string.Empty);
            var _regex = @".+@(.+)";
            var _match = Regex.Match(RawContent, _regex);
            var _groups = _match.Groups;
            if (_groups.Count() <= 1)
                return this;
            var _eventName = _groups[1].Value;
            if (_eventName == "released")
            {
                this.Event = IOToolkit.InputEvent.IE_Released;
            }
            else if (_eventName == "doubleClick")
            {
                this.Event = IOToolkit.InputEvent.IE_DoubleClick;
            }
            else if (_eventName == "axis")
            {
                this.Event = IOToolkit.InputEvent.IE_Axis;
            }
            return this;
        }
    }
#endregion
    [SerializedAt(AppPath.StreamingDir, Priority = 0)]
    public class UNIPlayerSettings : UConfig
    {
        [XmlAttribute]
        public float AutoBack2Idle = 600;

        [XmlAttribute]
        public float MinIdleTime = 0.1f;

        [XmlIgnore]
        public string IdleVideoPath
        {
            get => IdlePage.url;
        }

        [XmlElement("Event")]
        public List<GlobalEvent> Events = new List<GlobalEvent>();

        [XmlElement("IOEvent")]
        public List<IOEvent> IOEvents = new List<IOEvent>();

        public IdlePage IdlePage = new IdlePage() { url = "Videos/待机.mp4" };
        public VideoPage VideoPage = new VideoPage();
        public AudioPage AudioPage = new AudioPage();

        [XmlIgnore]
        public List<UNIVideo> VideoInfos
        {
            get { return VideoPage.VideoInfos; }
        }

        [XmlIgnore]
        public Dictionary<IOBindParser, MediaList> GroupVideoInfos
        {
            get => VideoPage.VideoInfosGroupByTrigger;
        }

        public UNIVideo NextVideo(string trigger)
        {
            var _key = new IOBindParser(trigger);
            if (!GroupVideoInfos.ContainsKey(_key))
                return null;
            return GroupVideoInfos[_key].NextMedia as UNIVideo;
        }

        [XmlIgnore]
        public List<UNIAudio> AudioInfos
        {
            get { return AudioPage.AudioInfos; }
        }

        [XmlIgnore]
        public Dictionary<IOBindParser, MediaList> GroupAudioInfos
        {
            get => AudioPage.AudioInfosGroupByTrigger;
        }

        public void RegenerateMedias()
        {
            regenerateVideos();
            regenerateAudios();
            this.Serialize();
            Debug.Log("Regenerate All Medias Successful");
        }

        protected override void OnLoaded()
        {
            bool _isDirty = false;
            if (Events.Count <= 0)
            {
                Events = new List<GlobalEvent> { new GlobalEvent() };
                _isDirty = true;
            }

            if (VideoPage.VideoInfos.Count <= 0)
            {
                regenerateVideos();
                _isDirty = true;
            }
            else
            {
                VideoPage.VideoInfos
                    .Where(_ => !_.fileExists)
                    .ToList()
                    .ForEach(_ =>
                    {
                        Debug.LogWarning($"Video {_.FullPath} not exists.");
                    });
            }

            if (AudioPage.AudioInfos.Count <= 0)
            {
                regenerateAudios();
                _isDirty = true;
            }
            else
            {
                AudioPage.AudioInfos
                    .Where(_ => !_.fileExists)
                    .ToList()
                    .ForEach(_ =>
                    {
                        Debug.LogWarning($"Audio {_.FullPath} not exists.");
                    });
            }

            new List<string> { "Audios", "Textures", "Videos" }
                .Select(_path => Path.Combine(Application.streamingAssetsPath, _path))
                .ToList()
                .ForEach(_path =>
                {
                    if (!Directory.Exists(_path))
                    {
                        Directory.CreateDirectory(_path);
                    }
                });
            if (_isDirty)
                this.Serialize();
        }

        protected void regenerateVideos()
        {
            var _idx = 0;
            var _videoDir = Path.Combine(Application.streamingAssetsPath, "Videos");
            if (!Directory.Exists(_videoDir))
                return;
            VideoPage.VideoInfos = Directory
                .GetFiles(_videoDir)
                .Where(
                    _path =>
                        Path.GetExtension(_path) == ".avi" || Path.GetExtension(_path) == ".mp4"
                )
                .Where(
                    _path =>
                        Path.GetFileNameWithoutExtension(_path)
                        != Path.GetFileNameWithoutExtension(IdleVideoPath)
                )
                .Select(
                    _path =>
                        new UNIVideo
                        {
                            RawTrigger = $"v{++_idx}",
                            url = _path.Replace(Application.streamingAssetsPath + "\\", "")
                        }
                )
                .ToList();
        }

        protected void regenerateAudios()
        {
            var _idx = 0;
            var _audioDir = Path.Combine(Application.streamingAssetsPath, "Audios");
            if (!Directory.Exists(_audioDir))
                return;
            AudioPage.AudioInfos = Directory
                .GetFiles(_audioDir)
                .Where(
                    _path =>
                        Path.GetExtension(_path) == ".wav" || Path.GetExtension(_path) == ".mp3"
                )
                .Select(
                    _path =>
                        new UNIAudio
                        {
                            RawTrigger = $"a{++_idx}",
                            url = _path.Replace(Application.streamingAssetsPath + "\\", "")
                        }
                )
                .ToList();
        }

        protected override void OnSerializing() { }

        protected override string Comment()
        {
            return @"
    ========================================  注释 开始 ========================================
    - AutoBack2Idle: 自动返回待机时间
    - MinIdleTime:   最小待机时间 (秒)  进入待机界面后, 该时间内不会触发视频页

    <Event>
        - onEnterIdle:      进入待机界面时调用
        - onLeaveIdle:      离开待机界面时调用
        - onVideoStarted:   任意视频开始播放时调用
        - onVideoFinished:  任意视频播放完成时调用
        - onAudioStarted:   任意音频播放时调用
        - onAudioFinished:  任意音频播放结束时调用 (未实现)

    <IOEvent>
        - trigger:     触发器标识   用法见下文 <IO绑定形式约定>
        - onTrigger:    触发时调用

    <Video/Audio>
        - trigger:      触发器标识   用法见下文 <IO绑定形式约定>
        - url:          视频/音频路径   *.mp4   *.mp3   *.wav
        - startTime:    开始时间[仅视频有效]
        - endTime:      结束时间[仅视频有效]
        - loop:         是否循环播放[仅视频有效]
        - volume:       音量[0-1]
        - mute:         是否静音播放
        - playbackRate  播放速度[仅视频有效]
        - interruptable 播放中是否可被中断[仅视频有效]
        - onStarted:    视频开始播放执行
        - onFinished:   视频播放到结尾时执行
        - playmode:     播放模式    默认为autoplay   autoplay 自动播放      sync 同步播放       accumulate 累积播放 [仅视频有效]
        - dataSource:   与playmode结合使用   作为模式为非autoplay模式外的播放进度映射数据源                          [仅视频有效]
    
    <PausePoint>
        功能描述: 视频播放到某一时间点时暂停, 等待用户操作后继续播放        [Video结点下可用]
        - time:                 暂停时间点
        - autoContinueTime:     自动继续时间
        - continueAction:       触发该操作时继续播放
    
    <RangeTrigger>
        功能描述: 视频播放到某一时间段时执行相关操作                        [Video结点下可用]
        - startTime:            起始时间
        - endTime:              结束时间
        - onEnter:              进入区间时执行
        - onLeave:              离开区间时执行
        - action:               action绑定
        - onAction:             在区间内, 触发action时执行

    事件属性下可用函数:
        - Await(Time):                  例 Await(3000)                  解释: 等待3000ms后执行后续语句
        - Back2Idle():                                                  解释: 返回待机
        - PlayVideo(Trigger):           例 PlayVideo(v1)                解释: 播放trigger为v1的视频
        - PlaySound(Trigger):           例 PlayAudio(a1)                解释: 播放trigger为a1的音频 (播放会打断上一个正在播放的音频)
        - PlaySoundEffect(Trigger):     例 PlaySound(a1)                解释: 播放trigger为a1的音效 (可以同时播放多个音效)
        - PlayBGMusic(Trigger,Volume)   例: PlayBGMusic(a1,0.5)         解释: 播放trigger为a1的背景音乐 音量调为0.5
        - StopSound():                  例: StopSound()                 解释: 停止播放音频
        - StopSoundEffect():            例: StopSoundEffect()           解释: 停止播放所有音效
        - StopBGMusic():                例: StopBGMusic()               解释: 停止播放背景音乐
        - SetDOOn(OAction):             例 SetDOOn(TurnLight)           解释: IO模块调用default设备下OAction[Name]为TurnOnLight的结点
        - SetDOOn(Device,OAction):      例 SetDOOn(PCI2312A,TurnLight)  解释: IO模块调用PCI2312A设备下OAction[Name]为TurnOnLight的结点
        - SetDOOff(OAction), SetDOOff(Device,OAction): 同上
        - SetDO(OAction,Value):         例 SetDO(Pitch,10)              解释: IO模块调用default设备OAction[Name]为Pitch的结点, 并将输出值设为10
        - SetDO(Device,OAction,Value):  例 SetDO(PCI8735,Pitch,10)      解释: IO模块调用PCI8735设备OAction[Name]为Pitch的结点, 并将输出值设为10
        - SetDOKey(KeyName,Value):      例 SetDOKey(OAxis_00, 1)        解释: IO模块将default设备的输出通道0设置输出值为1
        - SetDOKey(Device,KeyName,Value): 例 SetDOKey(PCI8735,OAxis_00, 1) 解释: IO模块将PCI8735设备的输出通道0设置输出值为1

    @IO绑定形式约定
        例: trigger='startGame'             绑定default设备下的startGame的pressed事件
        例: trigger='default:v1@pressed'    绑定default设备下v1的pressed事件
        例: trigger='default:Pitch@axis'    绑定default设备下的Pitch的axis数据

    @函数示例:
        例: Await(3000);Back2Idle();                    解释: 3秒后返回待机
        例: Await(5000);PlayVideo(v1);                  解释: 5秒后播放Video[trigger]为v1的视频
        例: Await(3000);SetDOOn(TurnOnLight);           解释: 3秒后 将default设备OAction[Name]为TurnOnLight的结点置位高电平
    ========================================  注释 结束 ========================================
    ";
        }
    }
}
