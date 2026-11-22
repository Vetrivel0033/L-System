using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LSystemViewer : MonoBehaviour
{
    public Camera cam;               
    public Material lineMaterial;    

    const string FolderName = "LSystems2D";

    readonly List<LSystemData> systems = new List<LSystemData>();
    LSystemData current;    
    int currentIndex;       
    int iterations;          
    float angle;             
    float step;              
    float rotation;          
    string expanded = "";    
    int segments;            
    string message = "";     
    Mesh mesh;
    GUIStyle hudStyle;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
        }

        mesh = new Mesh { name = "LSystemLines" };
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial =
            lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));

        LoadFiles();
        if (systems.Count > 0) LoadSystem(0);
    }

    void LoadFiles()
    {
        systems.Clear();
        string dir = Path.Combine(Application.streamingAssetsPath, FolderName);
        if (!Directory.Exists(dir))
        {
            message = "Folder not found: " + dir;
            Debug.LogError(message);
            return;
        }

        string[] files = Directory.GetFiles(dir, "*.json");
        Array.Sort(files, StringComparer.Ordinal);   
        foreach (string file in files)
            systems.Add(JsonUtility.FromJson<LSystemData>(File.ReadAllText(file)));

        message = systems.Count + " L-system files loaded";
        if (systems.Count < 8) message += " (the coursework needs 8)";
    }

   
    void LoadSystem(int i)
    {
        if (i < 0 || i >= systems.Count) return;
        currentIndex = i;
        current = systems[i];
        angle = current.angle;
        step = current.stepLength > 0f ? current.stepLength : 1f;
        rotation = 0f;
        iterations = 0;
        expanded = current.axiom;
        SetIterations(current.iterations);
        Rebuild(true);
    }

    bool SetIterations(int n)
    {
        n = Mathf.Max(0, n);
        string s = LSystemEngine.Expand(current, n);
        if (s == null)
        {
            message = "n = " + n + " would be too long to draw";
            return false;
        }
        iterations = n;
        expanded = s;
        message = "";
        return true;
    }

    void Rebuild(bool fitCamera)
    {
        segments = LSystemEngine.BuildLines(expanded, angle, step, rotation, mesh);
        if (fitCamera) FitCamera();
    }

    void FitCamera()
    {
        if (cam == null || mesh.vertexCount == 0) return;
        Bounds b = mesh.bounds;
        Vector3 centre = transform.TransformPoint(b.center);
        cam.transform.position = new Vector3(centre.x, centre.y, -10f);
        cam.transform.rotation = Quaternion.identity;
        float halfHeight = b.extents.y;
        float halfWidth = b.extents.x / cam.aspect;
        cam.orthographicSize = Mathf.Max(0.5f, Mathf.Max(halfHeight, halfWidth) * 1.1f);
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.f5Key.wasPressedThisFrame)
        {
            LoadFiles();
            LoadSystem(Mathf.Min(currentIndex, systems.Count - 1));
        }
        if (current == null) return;

        for (int i = 0; i < systems.Count && i < 9; i++)
            if (kb[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame) LoadSystem(i);

        if (kb.upArrowKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
            if (SetIterations(iterations + 1)) Rebuild(true);
        if (kb.downArrowKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
            if (SetIterations(iterations - 1)) Rebuild(true);

        float dt = Time.deltaTime;
        bool changed = false;
        if (kb.qKey.isPressed) { angle += 20f * dt; changed = true; }
        if (kb.aKey.isPressed) { angle -= 20f * dt; changed = true; }
        if (kb.wKey.isPressed) { step *= 1f + dt; changed = true; }
        if (kb.sKey.isPressed) { step /= 1f + dt; changed = true; }
        if (kb.eKey.isPressed) { rotation += 30f * dt; changed = true; }
        if (kb.dKey.isPressed) { rotation -= 30f * dt; changed = true; }
        if (changed) Rebuild(false);  

        if (kb.rKey.wasPressedThisFrame) LoadSystem(currentIndex);
        if (kb.fKey.wasPressedThisFrame) FitCamera();
    }

    void OnGUI()
    {
        if (hudStyle == null)
        {
            hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            hudStyle.normal.textColor = Color.white;
        }

        if (current == null)
        {
            GUI.Label(new Rect(10, 10, 1100, 60), "No L-systems loaded. " + message, hudStyle);
            return;
        }

        GUI.Label(new Rect(10, 10, 1100, 130),
            "[" + (currentIndex + 1) + "/" + systems.Count + "]  " + current.name + "\n" +
            "iteration n = " + iterations +
            "    angle = " + angle.ToString("F1") +
            "    step = " + step.ToString("F2") +
            "    rotation = " + rotation.ToString("F0") + "\n" +
            "segments = " + segments + "    string length = " + expanded.Length + "\n" +
            message + "\n" +
            "1-8 load | Up/Down iterate | Q/A angle | W/S step | E/D rotate | R reset | F fit | F5 reload",
            hudStyle);
    }
}
