/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: DebugTrack.cs
 * Created on 2018/06/24 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Instech.Framework
{
    internal struct TrackLine
    {
        public Vector2 Start;
        public Vector2 End;
        public Color Color;
    }

    internal struct TrackText
    {
        public string Text;
        public Vector2 Position;
    }

    public class DebugTrack : MonoSingleton<DebugTrack>
    {
        public bool Active { get; set; }

        internal List<TrackLine> DrawLines = new List<TrackLine>();
        internal List<TrackText> DrawTexts = new List<TrackText>();

        private readonly Dictionary<string, Graph> _allGraphs = new Dictionary<string, Graph>();
        private Material _lineMat;

        public Graph AddGraph(string key, uint maxSamples)
        {
            var newOne = new Graph(key, maxSamples);
            try
            {
                _allGraphs.Add(key, newOne);
                return newOne;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Graph GetGraph(string key)
        {
            Graph ret;
            _allGraphs.TryGetValue(key, out ret);
            return ret;
        }

        public bool RemoveGraph(string key)
        {
            Graph target;
            if (_allGraphs.TryGetValue(key, out target))
            {
                _allGraphs.Remove(key);
                return true;
            }
            return false;
        }

        protected override void Init()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            _lineMat = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _lineMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            _lineMat.SetInt("DestBlend", (int)BlendMode.OneMinusSrcAlpha);
            _lineMat.SetInt("_Cull", 0);
            _lineMat.SetInt("_ZWrite", 0);
        }

        private void OnGUI()
        {
            if (!Active)
            {
                return;
            }
            GL.PushMatrix();
            _lineMat.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.LINES);
            foreach (var trackLine in DrawLines)
            {
                GL.Color(trackLine.Color);
                GL.Vertex3(trackLine.Start.x, trackLine.Start.y, 0);
                GL.Vertex3(trackLine.End.x, trackLine.End.y, 0);
            }
            GL.End();
            GL.PopMatrix();
        }

        private void Update()
        {
            DrawLines.Clear();
            DrawTexts.Clear();
            foreach (var item in _allGraphs)
            {
                item.Value.Update();
            }
        }
    }

    public class Graph
    {
        public string Key { get; }
        private readonly uint _maxSamples;
        private readonly Dictionary<string, Track> _tracks = new Dictionary<string, Track>();

        public Graph(string key, uint maxSamples)
        {
            Key = key;
            _maxSamples = maxSamples;
        }

        public Track AddTrack(string key)
        {
            var newOne = new Track(key, _maxSamples);
            try
            {
                _tracks.Add(key, newOne);
                return newOne;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Track GetTrack(string key)
        {
            Track ret;
            _tracks.TryGetValue(key, out ret);
            return ret;
        }

        public bool RemoveTrack(string key)
        {
            Track target;
            if (_tracks.TryGetValue(key, out target))
            {
                _tracks.Remove(key);
                return true;
            }
            return false;
        }

        internal void Update()
        {
            foreach (var item in _tracks)
            {
                item.Value.Update();
            }
        }
    }

    public class Track
    {
        public string Key { get; }

        public float Max
        {
            get
            {
                if (_fixedMaxY != null) return _fixedMaxY.Value;
                var ret = float.MinValue;
                foreach (var item in _samples)
                {
                    if (item > ret)
                    {
                        ret = item;
                    }
                }
                return ret;
            }
        }

        public float Min
        {
            get
            {
                if (_fixedMinY != null) return _fixedMinY.Value;
                var ret = float.MaxValue;
                foreach (var item in _samples)
                {
                    if (item < ret)
                    {
                        ret = item;
                    }
                }
                return ret;
            }
        }

        private float? _fixedMinY;
        private float? _fixedMaxY;
        private readonly LinkedList<float> _samples = new LinkedList<float>();
        private readonly uint _maxSamples;

        public Track(string key, uint maxSamples)
        {
            Key = key;
            _maxSamples = maxSamples;
        }

        public void AddSample(float data)
        {
            _samples.AddLast(data);
            while (_samples.Count > _maxSamples)
            {
                _samples.RemoveFirst();
            }
        }

        public void SetFixedY(float? min = null, float? max = null)
        {
            _fixedMinY = min;
            _fixedMaxY = max;
        }

        internal void Update()
        {
            if (_samples.Count <= 0)
            {
                return;
            }
            var lastX = 0;
            var lastY = 0f;
            var first = true;
            var max = Max;
            var min = Min;
            using (var enu = _samples.GetEnumerator())
            {
                while (enu.MoveNext())
                {
                    var curX = lastX + 1;
                    var curY = enu.Current;
                    if (!first)
                    {
                        DebugTrack.Instance.DrawLines.Add(new TrackLine
                        {
                            Start = new Vector2(1f * lastX / _maxSamples, (lastY - min) / (max - min)),
                            End = new Vector2(1f * curX / _maxSamples, (curY - min) / (max - min)),
                            Color = Color.white
                        });
                    }
                    lastX = curX;
                    lastY = curY;
                    first = false;
                }
            }
        }
    }
}
