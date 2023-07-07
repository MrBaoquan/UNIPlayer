using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DNHper;
using UniRx;
using UnityEngine;

namespace UNIPlayer {

    public class Interpreter {
        public string rawCode = string.Empty;
        public static IObservable<Unit> ExecuteStatement (string StatementContent, params object[] builtinArgs) {
            if (StatementContent.Trim () == string.Empty) return Observable.Empty<Unit> ();
            var _statements = StatementContent.Split (';');
            return Observable.Concat (_statements.Where (_statement => _statement != string.Empty)
                .ToList ()
                .Select (_statement => {
                    try {
                        var _regex = @"([A-Za-z0-9]+)\((.*)\)";
                        var _match = Regex.Match (_statement, _regex);
                        var _groups = _match.Groups;
                        var _funcName = _groups[1].Value;
                        var _parameters = new string[0];

                        if (_groups[2].Value != string.Empty) {
                            _parameters = _groups[2].Value.Split (',');
                        }
                        return FunctionLibrary.MagicMethod (_funcName, _parameters, builtinArgs);
                    } catch (System.Exception err) {
                        Debug.LogWarning (err.Message);
                        return Observable.Empty<Unit> ();
                    }

                }));

        }

    }

}