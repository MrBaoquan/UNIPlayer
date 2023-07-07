using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using Newtonsoft.Json;
using UniRx;

using UNIHper;

namespace UNIPlayer
{
    public class NetTask
    {
        public string TaskName;
        public List<string> TaskParams;
    }

    [SerializedAt(AppPath.StreamingDir)]
    public class NetBridge : UConfig
    {
        [JsonIgnore, XmlAttribute]
        public string LocalIP = "127.0.0.1";

        [JsonIgnore, XmlAttribute]
        public int LocalPort = 22222;

        [JsonIgnore]
        public string debugContent = string.Empty;

        // Write your comments here
        protected override string Comment()
        {
            return @"
        通过网络向本应用发送如下格式的数据包，即可执行相应的任务：
        {
            ""TaskName"": ""ExecuteScript"",    // 任务名称
            ""TaskParams"": [                   // 任务参数
                ""PlayVideo(v1);""
            ]
        }
        ";
        }

        // Called once after the config data is loaded
        protected override void OnLoaded()
        {
            debugContent = JsonConvert.SerializeObject(
                new NetTask
                {
                    TaskName = "ExecuteScript",
                    TaskParams = new List<string> { "PlayVideo(v1);" }
                }
            );
            this.Serialize();

            Managements.Network
                .BuildUdpListener(LocalIP, LocalPort, new StringMsgReceiver())
                .Listen()
                .OnReceivedAsObservable()
                .Subscribe(_netData =>
                {
                    var _netMessage = (_netData.Item1.Message as NetStringMessage);
                    var _content = _netMessage.Content;
                    Debug.Log("Received: " + _content);
                    try
                    {
                        var _task = JsonConvert.DeserializeObject<NetTask>(_content);
                        if (_task.TaskName == "ExecuteScript" && _task.TaskParams.Count > 0)
                        {
                            var _script = _task.TaskParams[0];
                            Interpreter.ExecuteStatement(_script).Subscribe();
                        }
                    }
                    catch (System.Exception e)
                    {
                        Managements.Network.Send2UdpClient(
                            e.Message.ToUTF8Bytes(),
                            _netMessage.RemoteIP,
                            _netMessage.RemotePort,
                            _netMessage.LocalKey
                        );
                    }
                });
        }

        // Called once after the application is quit
        protected override void OnUnloaded()
        {
            Managements.Network.Dispose();
        }
    }
}
