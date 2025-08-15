using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MultiTargetCamera : MonoBehaviour
{
    public static MultiTargetCamera Instance { get; private set; }

    [Header("Targets")]
    public List<Transform> targets = new List<Transform>();

    [Header("Follow")]
    public Vector3 offset = new Vector3(0, 0, -10);
    [Range(0.01f, 1f)] public float moveSmoothTime = 0.2f;

    [Header("Zoom")]
    public float baseZoom = 8f;
    public float minZoom = 4f;
    public float maxZoom = 12f;
    [Tooltip("A mayor valor, menos zoom por la misma separación")]
    public float zoomLimiter = 20f;
    public float zoomSpeed = 3f;

    [Header("Pixel Settings")]
    public int pixelsPerUnit = 16;

    [Header("Tether / Elastic")]
    public float tetherDistance = 5f;
    public float tetherStrength = 5f;

    private Camera cam;
    private Vector3 _velocity;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic) cam.orthographic = true;

        cam.orthographicSize = RoundToPixel(baseZoom);
    }

    void LateUpdate()
    {
        if (targets.Count == 0) return;

        Move();
        Zoom();
    }

    public void Register(Transform t)
    {
        if (t != null && !targets.Contains(t)) targets.Add(t);
    }

    public void Unregister(Transform t)
    {
        if (t != null && targets.Contains(t)) targets.Remove(t);
    }

    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();

        float maxDist = 0f;
        foreach (var t in targets)
        {
            float dist = Vector3.Distance(t.position, centerPoint);
            if (dist > maxDist) maxDist = dist;
        }

        if (maxDist > tetherDistance)
        {
            Vector3 push = (centerPoint - transform.position) * tetherStrength * Time.deltaTime;
            centerPoint += push;
        }

        Vector3 desired = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, moveSmoothTime);
    }

    void Zoom()
    {
        float greatestDistance = GetGreatestDistance();
        float targetSize = Mathf.Lerp(minZoom, maxZoom, greatestDistance / zoomLimiter);
        targetSize = Mathf.Clamp(targetSize, minZoom, maxZoom);

        targetSize = RoundToPixel(targetSize);

        float delta = targetSize - cam.orthographicSize;
        float maxDelta = zoomSpeed * Time.deltaTime;
        delta = Mathf.Clamp(delta, -maxDelta, maxDelta);
        cam.orthographicSize += delta;
    }

    float GetGreatestDistance()
    {
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 1; i < targets.Count; i++) bounds.Encapsulate(targets[i].position);
        return Mathf.Max(bounds.size.x, bounds.size.y);
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1) return targets[0].position;
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 1; i < targets.Count; i++) bounds.Encapsulate(targets[i].position);
        return bounds.center;
    }

    public Bounds GetCameraBounds()
    {
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        return new Bounds(transform.position, new Vector3(camWidth, camHeight, 0f));
    }

    float RoundToPixel(float size)
    {
        if (pixelsPerUnit <= 0) return size;
        float unitsPerPixel = 1f / pixelsPerUnit;
        return Mathf.Round(size / unitsPerPixel) * unitsPerPixel;
    }
}
