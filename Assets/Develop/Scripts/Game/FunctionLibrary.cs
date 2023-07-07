using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DNHper;
using IOToolkit;
using UniRx;
using UnityEngine;
using UNIHper;

namespace UNIPlayer
{
    public static class FunctionLibrary
    {
        public static IObservable<Unit> MagicMethod(
            string funcName,
            string[] args,
            object[] builtinArgs
        )
        {
            return Observable.Create<Unit>(_observer =>
            {
                object[] _newArgs = autoConvertParams(funcName, args, builtinArgs);
                var _function = typeof(FunctionLibrary).GetMethod(
                    funcName,
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    _newArgs.Select(_arg => _arg.GetType()).ToArray(),
                    null
                );

                if (_function != null)
                {
                    bool _isAsync = _function.ReturnType != typeof(void);
                    //Debug.LogWarning ($"{funcName} async: {_isAsync}");
                    if (_isAsync)
                    {
                        (_function.Invoke(null, _newArgs) as IObservable<Unit>).Subscribe(_ =>
                        {
                            _observer.OnNext(Unit.Default);
                            _observer.OnCompleted();
                        });
                    }
                    else
                    {
                        _function.Invoke(null, _newArgs);
                        _observer.OnNext(Unit.Default);
                        _observer.OnCompleted();
                    }
                }
                else
                {
                    Debug.LogWarning($"无法解析函数: {funcName}");
                    _observer.OnNext(Unit.Default);
                    _observer.OnCompleted();
                }

                return new CancellationDisposable();
            });
        }

        public static object[] autoConvertParams(
            string funcName,
            string[] args,
            object[] builtinArgs
        )
        {
            object[] _newArgs = new object[args.Length];
            Array.Copy(args, _newArgs, args.Length);
            if (funcName == "Await")
            {
                _newArgs[0] = args[0].Parse2Int();
            }
            else if (funcName == "SetDO")
            {
                var _outputValue = _newArgs[args.Length - 1] as string;
                if (Regex.IsMatch(_outputValue, "arg[0-9]+"))
                {
                    var _match = Regex.Match(_outputValue, @"arg([0-9]+)$");
                    _newArgs[args.Length - 1] = builtinArgs[_match.Groups[1].Value.Parse2Int()];
                }
                else
                {
                    _newArgs[args.Length - 1] = args[args.Length - 1].Parse2Float();
                }
            }
            else if (funcName == "SetDOKey")
            {
                var _outputValue = _newArgs[args.Length - 1] as string;
                if (Regex.IsMatch(_outputValue, "arg[0-9]+"))
                {
                    var _match = Regex.Match(_outputValue, @"arg([0-9]+)$");
                    _newArgs[args.Length - 1] = builtinArgs[_match.Groups[1].Value.Parse2Int()];
                }
                else
                {
                    _newArgs[args.Length - 1] = args[args.Length - 1].Parse2Float();
                }
            }
            else if (funcName == "PlayBGMusic")
            {
                if (_newArgs.Length == 2)
                    _newArgs[1] = args[1].Parse2Float();
                else if (_newArgs.Length == 1)
                    _newArgs = _newArgs.Append(0.5f).ToArray();
                Debug.LogWarning(_newArgs.Length);
            }
            return _newArgs;
        }

        public static IObservable<Unit> Await(int Milliseconds)
        {
            return Observable
                .Timer(TimeSpan.FromMilliseconds(Milliseconds))
                .Select(_val => Unit.Default);
        }

        public static void Back2Idle()
        {
            Managements.SceneScript<SceneEntryScript>().Back2Idle();
        }

        public static void PlayVideo(string Trigger)
        {
            Managements.SceneScript<SceneEntryScript>().PlayVideo(Trigger);
        }

        public static void PlaySound(string Trigger)
        {
            Managements.SceneScript<SceneEntryScript>().PlaySound(Trigger);
        }

        public static void PlaySoundEffect(string Trigger)
        {
            Managements.SceneScript<SceneEntryScript>().PlaySoundEffect(Trigger);
        }

        public static void PlayBGMusic(string Trigger, float InVolume = 0.5f)
        {
            Managements.SceneScript<SceneEntryScript>().PlayMusic(Trigger, true, InVolume);
        }

        public static void StopBGMusic()
        {
            Managements.Audio.StopMusic();
        }

        public static void StopSound()
        {
            Managements.Audio.StopMusic(1);
        }

        public static void StopSoundEffect()
        {
            Managements.Audio.StopEffect();
        }

        public static void SetDOOn(string oActionName)
        {
            defaultDevice.SetDOOn(oActionName);
        }

        public static void SetDOOn(string deviceName, string oActionName)
        {
            IODeviceController.GetIODevice(deviceName).SetDOOn(oActionName);
        }

        public static void SetDOOff(string oActionName)
        {
            defaultDevice.SetDOOff(oActionName);
        }

        public static void SetDOOff(string deviceName, string oActionName)
        {
            IODeviceController.GetIODevice(deviceName).SetDOOff(oActionName);
        }

        public static void SetDO(string oActionName, float val)
        {
            defaultDevice.SetDO(oActionName, val);
        }

        public static void SetDO(string deviceName, string oActionName, float val)
        {
            IODeviceController.GetIODevice(deviceName).SetDO(oActionName, val);
        }

        public static void SetDOKey(string keyName, float val)
        {
            IOToolkit.Key _key = keyName;
            defaultDevice.SetDO(_key, val);
        }

        public static void SetDOKey(string deviceName, string keyName, float val)
        {
            IOToolkit.Key _key = keyName;
            IODeviceController.GetIODevice(deviceName).SetDO(_key, val);
        }

        private static IODevice defaultDevice
        {
            get => IODeviceController.GetIODevice("default");
        }
    }
}
