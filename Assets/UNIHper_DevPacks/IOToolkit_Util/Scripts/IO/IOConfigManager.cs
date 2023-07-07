using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UNIHper;
using UnityEngine;
using DNHper;
using IOToolkit;

namespace IOToolkit_Extension
{
    // 定义XML节点对应的类结构
    public class IORoot : Singleton<IORoot>
    {
        [XmlElement("Device")]
        public List<Device> Devices { get; set; } = new List<Device>();

        public Device FirstOrCreate(string deviceName)
        {
            var _device = Devices.Where(_ => _.Name == deviceName).FirstOrDefault();
            if (_device == null)
            {
                _device = new Device { Name = deviceName };
                this.Devices.Add(_device);
            }
            return _device;
        }

        public void Load()
        {
            var _configPath = IOToolkitUtil.ConfigPath;
            if (!File.Exists(_configPath))
            {
                Debug.LogError("IOConfigManager::Load: IODevice.xml not found!");
                return;
            }
            var _xml = File.ReadAllText(_configPath);
            var _serializer = new XmlSerializer(typeof(IORoot));
            var _reader = new StringReader(_xml);
            var _root = _serializer.Deserialize(_reader) as IORoot;
            _reader.Close();
            if (_root == null)
            {
                Debug.LogError("IOConfigManager::Load: IODevice.xml deserialize failed!");
                return;
            }
            this.Devices = _root.Devices;
        }

        public void Print()
        {
            Devices.ForEach(_device =>
            {
                _device.Actions.ForEach(_action =>
                {
                    _action.Keys.ForEach(_key =>
                    {
                        Debug.Log(_key.ToString());
                    });
                });
            });
        }

        public void Save()
        {
            var _configPath = Path.Combine(Application.dataPath, IOToolkitUtil.ConfigPath);
            var _serializer = new XmlSerializer(typeof(IORoot));
            var _writer = new StreamWriter(_configPath);
            _serializer.Serialize(_writer, this);
            _writer.Close();
        }
    }

    public class Device
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Type")]
        public string Type { get; set; } = "External";

        [XmlAttribute("DllName")]
        public string DllName { get; set; } = "IOUI";

        [XmlAttribute("Index")]
        public int Index { get; set; }

        [XmlElement("Properties")]
        public Properties Properties { get; set; } = null;

        [XmlElement("Action")]
        public List<Action> Actions { get; set; } = new List<Action>();

        [XmlElement("Axis")]
        public List<Axis> Axes { get; set; } = new List<Axis>();

        [XmlElement("OAction")]
        public List<OAction> OActions { get; set; } = new List<OAction>();

        public Action FirstOrCreateAction(string actionName)
        {
            var _action = Actions.Where(_ => _.Name == actionName).FirstOrDefault();
            if (_action == null)
            {
                _action = new Action { Name = actionName };
                Actions.Add(_action);
            }
            return _action;
        }

        public OAction FirstOrCreateOAction(string actionName)
        {
            var _action = OActions.Where(_ => _.Name == actionName).FirstOrDefault();
            if (_action == null)
            {
                _action = new OAction { Name = actionName };
                OActions.Add(_action);
            }
            return _action;
        }

        public Axis FirstOrCreateAxis(string axisName)
        {
            var _axis = Axes.Where(_ => _.Name == axisName).FirstOrDefault();
            if (_axis == null)
            {
                _axis = new Axis { Name = axisName };
                Axes.Add(_axis);
            }
            return _axis;
        }
    }

    public class Key
    {
        [XmlAttribute("Name"), DefaultValueAttribute("")]
        public string Name { get; set; } = "";

        [XmlAttribute("PreOffset"), DefaultValueAttribute(0)]
        public float PreOffset { get; set; } = 0;

        [XmlAttribute("PreScale"), DefaultValueAttribute(1)]
        public float PreScale { get; set; } = 1;

        [XmlAttribute("Min"), DefaultValueAttribute(int.MinValue)]
        public int Min { get; set; } = int.MinValue;

        [XmlAttribute("Max"), DefaultValueAttribute(int.MaxValue)]
        public int Max { get; set; } = int.MaxValue;

        [XmlAttribute("DeadZone"), DefaultValueAttribute(0)]
        public float DeadZone { get; set; } = 0;

        [XmlAttribute("Sensitivity"), DefaultValueAttribute(1)]
        public float Sensitivity { get; set; } = 1;

        [XmlAttribute("Exponent"), DefaultValueAttribute(1)]
        public float Exponent { get; set; } = 1;

        [XmlAttribute("Invert"), DefaultValueAttribute("False")]
        public string Invert { get; set; } = "False";

        [XmlAttribute("InvertEvent"), DefaultValueAttribute(false)]
        public string InvertEvent { get; set; } = "False";

        [XmlAttribute("Scale"), DefaultValueAttribute(1)]
        public float Scale { get; set; } = 1;
    }

    public class IOKeysBase
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("Key")]
        public List<Key> Keys { get; set; } = new List<Key>();

        public Key FirstOrCreate(string keyName)
        {
            var _key = Keys.Where(_ => _.Name == keyName).FirstOrDefault();
            if (_key == null)
            {
                _key = new Key { Name = keyName };
                Keys.Add(_key);
            }
            return _key;
        }
    }

    public class Properties : IOKeysBase { }

    public class Action : IOKeysBase { }

    public class Axis : IOKeysBase { }

    public class OAction : IOKeysBase { }
}
