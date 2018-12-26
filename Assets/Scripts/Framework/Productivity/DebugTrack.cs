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

    /// <summary>
    /// 调试图表
    /// </summary>
    public class DebugTrack : MonoSingleton<DebugTrack>
    {
        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// 图表在屏幕内的显示区域，默认(0.05,0.05,0.9,0.9)
        /// </summary>
        public Rect Range { get; set; }

        /// <summary>
        /// 图表之间的间隔，默认0.05
        /// </summary>
        public float GraphMarginY { get; set; }

        internal List<TrackLine> DrawLines = new List<TrackLine>();
        internal List<TrackText> DrawTexts = new List<TrackText>();

        private readonly List<Graph> _allGraphs = new List<Graph>();
        private Material _lineMat;
        private GUIStyle _style;
        private Camera _camera;

        protected override void Init()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            _lineMat = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _lineMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            _lineMat.SetInt("_DestBlend", (int)BlendMode.OneMinusSrcAlpha);
            _lineMat.SetInt("_Cull", 0);
            _lineMat.SetInt("_ZWrite", 0);

            Logger.Assert(LogModule.Default, gameObject.GetComponent<Camera>() == null, "已经有Camera组件了！");
            _camera = gameObject.AddComponent<Camera>();
            _camera.cullingMask = 0;
            _camera.clearFlags = CameraClearFlags.Nothing;

            Range = new Rect(0.05f, 0.05f, 0.9f, 0.9f);
            GraphMarginY = 0.05f;
            _style = null;
        }

        public Graph AddGraph(string key, uint maxSamples)
        {
            var newOne = new Graph(key, maxSamples);
            try
            {
                _allGraphs.Add(newOne);
                RefreshGraphRange();
                return newOne;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Graph GetGraph(string key)
        {
            var ret = _allGraphs.Find(item => item.Key.Equals(key));
            return ret;
        }

        public bool RemoveGraph(string key)
        {
            var item = GetGraph(key);
            if (item != null)
            {
                _allGraphs.Remove(item);
                RefreshGraphRange();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 刷新各个Graph在屏幕上占用的区域
        /// </summary>
        private void RefreshGraphRange()
        {
            if (_allGraphs.Count <= 0)
            {
                return;
            }
            var h = (Range.height - (_allGraphs.Count - 1) * GraphMarginY) / _allGraphs.Count;
            for (var i = 0; i < _allGraphs.Count; ++i)
            {
                var item = _allGraphs[i];
                var y = Range.y + i * (h + GraphMarginY);
                item.Range = new Rect(Range.x, y, Range.width, h);
            }
        }

        private void OnGUI()
        {
            if (!Active)
            {
                return;
            }
            if (_style == null)
            {
                _style = GUI.skin.label;
                _style.fontSize = 20;
                _style.alignment = TextAnchor.MiddleRight;
                _style.clipping = TextClipping.Overflow;
            }
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;
            foreach (var item in DrawTexts)
            {
                GUI.Label(new Rect(item.Position.x * screenWidth - 10, (1 - item.Position.y) * screenHeight, 0, 0), item.Text, _style);
            }
        }

        private void OnPostRender()
        {
            if (!Active)
            {
                return;
            }
            GL.PushMatrix();
            GL.LoadOrtho();
            _lineMat.SetPass(0);
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
                item.Update();
            }
        }
    }

    /// <summary>
    /// 调试图表中的单个图表
    /// </summary>
    public class Graph
    {
        public string Key { get; }
        internal Rect Range { get; set; }

        public float Max
        {
            get
            {
                if (FixedMax != null)
                {
                    return FixedMax.Value;
                }
                var ret = float.MinValue;
                foreach (var item in _tracks)
                {
                    ret = Mathf.Max(ret, item.Value.MaxSample);
                }
                return ret;
            }
        }

        public float Min
        {
            get
            {
                if (FixedMin != null)
                {
                    return FixedMin.Value;
                }
                var ret = float.MaxValue;
                foreach (var item in _tracks)
                {
                    ret = Mathf.Min(ret, item.Value.MinSample);
                }
                return ret;
            }
        }
        /// <summary>
        /// 固定Y轴最小值
        /// </summary>
        public float? FixedMin { get; set; }
        /// <summary>
        /// 固定Y轴最大值
        /// </summary>
        public float? FixedMax { get; set; }
        private readonly uint _maxSamples;
        private readonly Dictionary<string, Track> _tracks = new Dictionary<string, Track>();

        public Graph(string key, uint maxSamples)
        {
            Key = key;
            _maxSamples = maxSamples;
        }

        public Track AddTrack(string key)
        {
            var newOne = new Track(key, _maxSamples, this);
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
            _tracks.TryGetValue(key, out var ret);
            return ret;
        }

        public bool RemoveTrack(string key)
        {
            if (_tracks.ContainsKey(key))
            {
                _tracks.Remove(key);
                return true;
            }
            return false;
        }

        internal void Update()
        {
            // 坐标轴
            DebugTrack.Instance.DrawLines.Add(new TrackLine
            {
                Start = new Vector2(Range.xMin, Range.yMin),
                End = new Vector2(Range.xMax, Range.yMin),
                Color = Color.white
            });
            DebugTrack.Instance.DrawLines.Add(new TrackLine
            {
                Start = new Vector2(Range.xMin, Range.yMin),
                End = new Vector2(Range.xMin, Range.yMax),
                Color = Color.white
            });
            // 图表名
            DebugTrack.Instance.DrawTexts.Add(new TrackText
            {
                Position = new Vector2(Range.xMin, Range.yMin + (Range.yMax - Range.yMin) * 0.7f),
                Text = Key
            });
            // Y轴数值标注
            var min = Min;
            var max = Max;
            var mid = min + (max - min) * 0.5f;
            DebugTrack.Instance.DrawTexts.Add(new TrackText
            {
                Position = new Vector2(Range.xMin, Range.yMin),
                Text = min.ToString("F2")
            });
            DebugTrack.Instance.DrawTexts.Add(new TrackText
            {
                Position = new Vector2(Range.xMin, Range.yMin + (Range.yMax - Range.yMin) * 0.5f),
                Text = mid.ToString("F2")
            });
            DebugTrack.Instance.DrawTexts.Add(new TrackText
            {
                Position = new Vector2(Range.xMin, Range.yMax),
                Text = max.ToString("F2")
            });
            foreach (var item in _tracks)
            {
                item.Value.Update();
            }
        }
    }

    /// <summary>
    /// 单个图表中的一条曲线
    /// </summary>
    public class Track
    {
        public string Key { get; }

        internal float MaxSample
        {
            get
            {
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

        internal float MinSample
        {
            get
            {
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
        private readonly LinkedList<float> _samples = new LinkedList<float>();
        private readonly uint _maxSamples;
        private readonly Graph _owner;

        internal Track(string key, uint maxSamples, Graph owner)
        {
            Key = key;
            _maxSamples = maxSamples;
            _owner = owner;
        }

        public void AddSample(float data)
        {
            _samples.AddLast(data);
            while (_samples.Count > _maxSamples)
            {
                _samples.RemoveFirst();
            }
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
            var max = _owner.Max;
            var min = _owner.Min;
            using (var enu = _samples.GetEnumerator())
            {
                while (enu.MoveNext())
                {
                    var curX = lastX + 1;
                    var curY = enu.Current;
                    if (!first)
                    {
                        var startX = 1f * lastX / _maxSamples;
                        var startY = (lastY - min) / (max - min);
                        var endX = 1f * curX / _maxSamples;
                        var endY = (curY - min) / (max - min);
                        DebugTrack.Instance.DrawLines.Add(new TrackLine
                        {
                            Start = new Vector2(startX * _owner.Range.width + _owner.Range.x, startY * _owner.Range.height + _owner.Range.y),
                            End = new Vector2(endX * _owner.Range.width + _owner.Range.x, endY * _owner.Range.height + _owner.Range.y),
                            Color = Color.gray
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
