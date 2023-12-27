using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class QuaternionEditor : CommonEditor, IUpdateSceneGUI
{
    [Range(-360, 360)] public float m_angle = 0f;
    public Vector3 m_axis = new Vector3(0, 1, 0);
    
    private SerializedObject obj;
    private SerializedProperty propAngle;
    private SerializedProperty propAxis;
    
    private List<Vector3> vertices;
    
    private void OnEnable()
    {
        obj = new SerializedObject(this);
        propAngle = obj.FindProperty("m_angle");
        propAxis = obj.FindProperty("m_axis");
        
        SceneView.duringSceneGui += SceneGUI;
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= SceneGUI;
    }
    
    [MenuItem("Tools/Quaternion")]
    public static void ShowWindow()
    {
        GetWindow(typeof(QuaternionEditor), true, "Quaternion");
    }
    
    private void OnGUI()
    {
        obj.Update();
        DrawBlockGUI("Angle", propAngle);
        DrawBlockGUI("Axis", propAxis);
        if (obj.ApplyModifiedProperties())
        {
            SceneView.RepaintAll();
        }
    }
    public void SceneGUI(SceneView view)
    {
        vertices = new List<Vector3>
        {
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f, 0.5f, 0.5f),
            new Vector3( 0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3( 0.5f, 0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f)
        };
        
        float angle = m_angle * Mathf.PI / 180;
        
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = HQuaternion.Rotate(vertices[i], m_axis, angle);
            Handles.SphereHandleCap(0, vertices[i], Quaternion.identity,
                0.1f, EventType.Repaint);
        }
        
        int[][] index =
        {
            new int[] {0, 1},
            new int[] {1, 2},
            new int[] {2, 3},
            new int[] {3, 0},
            new int[] {4, 5},
            new int[] {5, 6},
            new int[] {6, 7},
            new int[] {7, 4},
            new int[] {4, 0},
            new int[] {5, 1},
            new int[] {6, 2},
            new int[] {7, 3},
        };
        for (int i = 0; i < index.Length; i++)
        {
            Handles.DrawAAPolyLine(vertices[index[i][0]], vertices[index[i][1]]);
        }
    }
}

public struct HQuaternion
{
    private float x;
    private float y;
    private float z;
    private float w;
    public HQuaternion(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
    
    private static HQuaternion Create(float angle, Vector3 axis)
    {
        float sin = Mathf.Sin(angle / 2f);
        float cos = Mathf.Cos(angle / 2f);
        Vector3 v = Vector3.Normalize(axis) * sin;
        return new HQuaternion(v.x, v.y, v.z, cos);
    }
    
    private static HQuaternion Conjugate(HQuaternion q)
    {
        float s = q.w;
        Vector3 v = new Vector3(-q.x, -q.y, -q.z);
        return new HQuaternion(v.x, v.y, v.z, s);
    }
    
    private static HQuaternion Multiplication(HQuaternion q1, HQuaternion q2)
    {
        float s1 = q1.w;
        float s2 = q2.w;
        Vector3 v1 = new Vector3(q1.x, q1.y, q1.z);
        Vector3 v2 = new Vector3(q2.x, q2.y, q2.z);
        float s = s1 * s2 - Vector3.Dot(v1, v2);
        Vector3 v = s1 * v2 + s2 * v1 + Vector3.Cross(v1, v2);
        return new HQuaternion(v.x, v.y, v.z, s);
    }
    
    public static Vector3 Rotate (Vector3 point, Vector3 axis, float angle)
    {
        HQuaternion q = Create(angle, axis);
        HQuaternion _q = Conjugate(q);
        HQuaternion p = new HQuaternion(point.x, point.y, point.z, 0f);
        HQuaternion rotatedPoint = Multiplication(q, p);
        rotatedPoint = Multiplication(rotatedPoint, _q);
        return new Vector3(rotatedPoint.x, rotatedPoint.y, rotatedPoint.z);
    }
}