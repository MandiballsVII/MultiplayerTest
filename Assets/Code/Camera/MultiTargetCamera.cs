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
    [Range(0.01f, 1f)] public float moveSmoothTime = 0.12f;

    [Header("Zoom")]
    public float baseZoom = 8f;
    public float minZoom = 4f;
    public float maxZoom = 12f;
    [Tooltip("Distancia entre jugadores que produce el zoom máximo")]
    public float zoomLimiter = 10f;
    public float zoomSpeed = 3f;

    private Camera cam;
    private Vector3 _velocity;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = baseZoom;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Debug.Log(cam.orthographicSize);
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
        Vector3 desired = centerPoint + offset;

        Vector3 smoothed = Vector3.SmoothDamp(
            transform.position,
            desired,
            ref _velocity,
            moveSmoothTime);

        transform.position = smoothed;
    }

    void Zoom()
    {
        //Debug.Log(cam.orthographicSize);
        if (targets.Count == 1)
        {
            cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, baseZoom, zoomSpeed * Time.deltaTime);
            return;
        }

        // Creamos bounds que contenga todos los jugadores
        var bounds = new Bounds(GetTargetPosition(targets[0]), Vector3.zero);
        for (int i = 1; i < targets.Count; i++)
            bounds.Encapsulate(GetTargetPosition(targets[i]));

        // Calculamos el tamaño necesario para que todos entren en la cámara
        float verticalSize = bounds.size.y / 2f + 1f;
        float horizontalSize = bounds.size.x / (2f * cam.aspect) + 1f;

        // Tomamos el mayor
        float targetSize = Mathf.Max(verticalSize, horizontalSize);

        // Limitar el zoom máximo
        targetSize = Mathf.Clamp(targetSize, minZoom, maxZoom);

        // Suavizado
        cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);
    }


    public Bounds GetMaxZoomBounds()
    {
        float camHeight = maxZoom * 2f;
        float camWidth = camHeight * cam.aspect;
        return new Bounds(GetCenterPoint() + offset, new Vector3(camWidth, camHeight, 0f));
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
            return GetTargetPosition(targets[0]);

        var bounds = new Bounds(GetTargetPosition(targets[0]), Vector3.zero);
        for (int i = 1; i < targets.Count; i++)
            bounds.Encapsulate(GetTargetPosition(targets[i]));

        return bounds.center;
    }

    Vector3 GetTargetPosition(Transform t)
    {
        var rb = t.GetComponent<Rigidbody2D>();
        return rb ? (Vector3)rb.position : t.position;
    }

    public Bounds GetCameraBounds()
    {
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        return new Bounds(transform.position, new Vector3(camWidth, camHeight, 0f));
    }
}
