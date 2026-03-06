using UnityEngine;
using UnityEngine.InputSystem;

public class CameraPanVertical : MonoBehaviour
{
    [Header("Estado")]
    public bool isLocked = false; // <--- SE ISSO FOR TRUE, A CÂMERA CONGELA

    [Header("Configurações")]
    public float keyboardSpeed = 150f;
    
    [Header("Limites")]
    public bool useStartingPosAsMinLimit = true;
    private float minYLimit;

    private bool isDragging = false;
    private Vector3 dragOriginWorldPos;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        minYLimit = useStartingPosAsMinLimit ? transform.position.y : -Mathf.Infinity;
    }

    void Update()
    {
        // 1. Se estiver pausado OU TRAVADO, não faz nada
        if (Time.timeScale == 0 || isLocked) return;

        if (Pointer.current == null) 
        {
            HandleKeyboard();
            return;
        }

        bool dragging = HandlePointerDrag();

        if (!dragging)
        {
            HandleKeyboard();
        }

        ApplyLimits();
    }

    private bool HandlePointerDrag()
    {
        if (Pointer.current == null) return false;

        if (Pointer.current.press.wasPressedThisFrame)
        {
            isDragging = true;
            Vector2 screenPos = Pointer.current.position.ReadValue();
            dragOriginWorldPos = cam.ScreenToWorldPoint(screenPos);
        }

        if (!Pointer.current.press.isPressed)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Vector3 currentPosWorld = cam.ScreenToWorldPoint(screenPos);
            float diffY = currentPosWorld.y - dragOriginWorldPos.y;

            transform.position -= new Vector3(0, diffY, 0);
            dragOriginWorldPos = cam.ScreenToWorldPoint(screenPos);
            return true;
        }

        return false;
    }

    private void HandleKeyboard()
    {
        if (Keyboard.current == null) return;

        float moveY = 0f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY = -1f;

        if (moveY != 0)
        {
            transform.position += Vector3.up * moveY * keyboardSpeed * Time.deltaTime;
        }
    }

    private void ApplyLimits()
    {
        if (transform.position.y < minYLimit)
        {
            Vector3 pos = transform.position;
            pos.y = minYLimit;
            transform.position = pos;
        }
    }
}