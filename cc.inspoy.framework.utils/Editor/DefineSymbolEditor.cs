/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Utils.Editor
 * FileName: DefineSymbolEditor.cs
 * Created on 2020/09/14 by inspoy
 * All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Instech.Framework.Utils.Editor
{
    public class DefineSymbolEditor : EditorWindow
    {
        private List<string> _symbols = new List<string>();

        public static void Open()
        {
            var wnd = GetWindow<DefineSymbolEditor>();
            wnd.titleContent = new GUIContent("DefineSymbolEditor");
            wnd.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Define Symbols");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load"))
            {
                Load();
            }
            if (GUILayout.Button("Save"))
            {
                Save();
            }
            GUILayout.EndHorizontal();
            PaintSymbols();
        }

        private void PaintSymbols()
        {
            if (_symbols.Count == 0 || !string.IsNullOrEmpty(_symbols[_symbols.Count - 1]))
            {
                _symbols.Add(string.Empty);
            }
            for (var i = 0; i < _symbols.Count; ++i)
            {
                _symbols[i] = GUILayout.TextField(_symbols[i]);
            }
        }

        private void Load()
        {
            _symbols = DefineSymbolManager.GetAllDefines().ToList();
        }

        private void Save()
        {
            foreach (var item in DefineSymbolManager.GetAllDefines())
            {
                DefineSymbolManager.RemoveDefine(item);
            }
            foreach (var item in _symbols)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    DefineSymbolManager.AddDefine(item);
                }
            }
        }
    }
}
