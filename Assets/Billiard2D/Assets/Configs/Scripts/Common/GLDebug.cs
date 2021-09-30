﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class GLDebug : MonoBehaviour
{
    private struct Line
    {
        public Vector3 start;
        public Vector3 end;
        public Color color;
        public float startTime;
        public float duration;

        public Line(Vector3 start, Vector3 end, Color color, float startTime, float duration)
        {
            this.start = start;
            this.end = end;
            this.color = color;
            this.startTime = startTime;
            this.duration = duration;
        }

        public bool DurationElapsed(bool drawLine)
        {
            if (drawLine)
            {
                GL.Color(color);
                GL.Vertex(start);
                GL.Vertex(end);
            }
            return Time.time - startTime >= duration;
        }
    }

    private static GLDebug instance;
    private static Material matZOn;
    private static Material matZOff;

    public KeyCode toggleKey;
    public bool displayLines = true;
#if UNITY_EDITOR
        public bool displayGizmos = true;
#endif

    private List<Line> linesZOn;
    private List<Line> linesZOff;
    private float milliseconds;

    void Awake()
    {
        if (instance)
        {
            DestroyImmediate(this);
            return;
        }
        instance = this;
        SetMaterial();
        linesZOn = new List<Line>();
        linesZOff = new List<Line>();
    }

    void SetMaterial()
    {
        matZOn = new Material(
@"Shader ""GLlineZOn"" {
        SubShader {
                Pass {
                        Blend SrcAlpha OneMinusSrcAlpha
                        ZWrite Off
                        Cull Off
                        BindChannels {
                                Bind ""vertex"", vertex
                                Bind ""color"", color
                        }
                }
        }
}
");
        matZOn.hideFlags = HideFlags.HideAndDontSave;
        matZOn.shader.hideFlags = HideFlags.HideAndDontSave;
        matZOff = new Material(
@"Shader ""GLlineZOff"" {
        SubShader {
                Pass {
                        Blend SrcAlpha OneMinusSrcAlpha
                        ZWrite Off
                        ZTest Always
                        Cull Off
                        BindChannels {
                                Bind ""vertex"", vertex
                                Bind ""color"", color
                        }
                }
        }
}
");
        matZOff.hideFlags = HideFlags.HideAndDontSave;
        matZOff.shader.hideFlags = HideFlags.HideAndDontSave;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            displayLines = !displayLines;

        if (!displayLines)
        {
            Stopwatch timer = Stopwatch.StartNew();

            linesZOn = linesZOn.Where(l => !l.DurationElapsed(false)).ToList();
            linesZOff = linesZOff.Where(l => !l.DurationElapsed(false)).ToList();

            timer.Stop();
            milliseconds = timer.Elapsed.Ticks / 10000f;
        }
    }
#if UNITY_EDITOR
        void OnDrawGizmos ()
        {
                if (!displayGizmos || !Application.isPlaying)
                        return;
                for (int i = 0; i < linesZOn.Count; i++)
                {
                        Gizmos.color = linesZOn[i].color;
                        Gizmos.DrawLine (linesZOn[i].start, linesZOn[i].end);
                }
                for (int i = 0; i < linesZOff.Count; i++)
                {
                        Gizmos.color = linesZOff[i].color;
                        Gizmos.DrawLine (linesZOff[i].start, linesZOff[i].end);
                }
        }
#endif

    void OnPostRender()
    {
        if (!displayLines) return;

        Stopwatch timer = Stopwatch.StartNew();

        matZOn.SetPass(0);
        GL.Begin(GL.LINES);
        linesZOn = linesZOn.Where(l => !l.DurationElapsed(true)).ToList();
        GL.End();

        matZOff.SetPass(0);
        GL.Begin(GL.LINES);
        linesZOff = linesZOff.Where(l => !l.DurationElapsed(true)).ToList();
        GL.End();

        timer.Stop();
        milliseconds = timer.Elapsed.Ticks / 10000f;
    }

    private static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0, bool depthTest = false)
    {
        if (duration == 0 && !instance.displayLines)
            return;
        if (start == end)
            return;
        if (depthTest)
            instance.linesZOn.Add(new Line(start, end, color, Time.time, duration));
        else
            instance.linesZOff.Add(new Line(start, end, color, Time.time, duration));
    }
    public static void DrawLine(Vector3 start, Vector3 end, Color? color = null, float duration = 0, bool depthTest = false)
    {
        DrawLine(start, end, color ?? Color.white, duration, depthTest);
    }
    public static void DrawRay(Vector3 start, Vector3 dir, Color? color = null, float duration = 0, bool depthTest = false)
    {
        if (dir == Vector3.zero)
            return;
        DrawLine(start, start + dir, color, duration, depthTest);
    }
    public static void DrawLineArrow(Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
    {
        DrawArrow(start, end - start, arrowHeadLength, arrowHeadAngle, color, duration, depthTest);
    }
    public static void DrawArrow(Vector3 start, Vector3 dir, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
    {
        if (dir == Vector3.zero)
            return;
        DrawRay(start, dir, color, duration, depthTest);
        Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
        DrawRay(start + dir, right * arrowHeadLength, color, duration, depthTest);
        DrawRay(start + dir, left * arrowHeadLength, color, duration, depthTest);
    }
    public static void DrawSquare(Vector3 pos, Vector3? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
    {
        DrawSquare(Matrix4x4.TRS(pos, Quaternion.Euler(rot ?? Vector3.zero), scale ?? Vector3.one), color, duration, depthTest);
    }
    public static void DrawSquare(Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
    {
        DrawSquare(Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color, duration, depthTest);
    }
    public static void DrawSquare(Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
    {
        Vector3
                p_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, .5f)),
                p_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, -.5f)),
                p_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, -.5f)),
                p_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, .5f));

        DrawLine(p_1, p_2, color, duration, depthTest);
        DrawLine(p_2, p_3, color, duration, depthTest);
        DrawLine(p_3, p_4, color, duration, depthTest);
        DrawLine(p_4, p_1, color, duration, depthTest);
    }
    public static void DrawCube(Vector3 pos, Vector3? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
    {
        DrawCube(Matrix4x4.TRS(pos, Quaternion.Euler(rot ?? Vector3.zero), scale ?? Vector3.one), color, duration, depthTest);
    }
    public static void DrawCube(Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
    {
        DrawCube(Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color, duration, depthTest);
    }
    public static void DrawCube(Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
    {
        Vector3
                down_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, -.5f, .5f)),
                down_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, -.5f, -.5f)),
                down_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, -.5f, -.5f)),
                down_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, -.5f, .5f)),
                up_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, .5f, .5f)),
                up_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, .5f, -.5f)),
                up_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, .5f, -.5f)),
                up_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, .5f, .5f));

        DrawLine(down_1, down_2, color, duration, depthTest);
        DrawLine(down_2, down_3, color, duration, depthTest);
        DrawLine(down_3, down_4, color, duration, depthTest);
        DrawLine(down_4, down_1, color, duration, depthTest);

        DrawLine(down_1, up_1, color, duration, depthTest);
        DrawLine(down_2, up_2, color, duration, depthTest);
        DrawLine(down_3, up_3, color, duration, depthTest);
        DrawLine(down_4, up_4, color, duration, depthTest);

        DrawLine(up_1, up_2, color, duration, depthTest);
        DrawLine(up_2, up_3, color, duration, depthTest);
        DrawLine(up_3, up_4, color, duration, depthTest);
        DrawLine(up_4, up_1, color, duration, depthTest);
    }

    public static void DrawCircle(Vector3 center, float radius, Color? color = null, float duration = 0, bool depthTest = false)
    {
        float degRad = Mathf.PI / 180;
        for (float theta = 0.0f; theta < (2 * Mathf.PI); theta += 0.2f)
        {
            Vector3 ci = (new Vector3(Mathf.Cos(theta) * radius + center.x, Mathf.Sin(theta) * radius + center.y, center.z));
            DrawLine(ci, ci + new Vector3(0, 0.02f, 0), color, duration, depthTest);
        }
    }

}
