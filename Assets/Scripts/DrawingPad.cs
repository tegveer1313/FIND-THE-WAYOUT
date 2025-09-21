using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class DrawingPad : MonoBehaviour
{
    public int textureWidth = 1024;
    public int textureHeight = 1024;
    public int brushSize = 8;
    public Color brushColor = Color.black;
    public Color backgroundColor = Color.clear; // transparent background

    RawImage rawImage;
    Texture2D tex;
    RectTransform rectTransform;
    bool isDirty = false;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();
        CreateTexture();
    }

    void CreateTexture()
    {
        tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        ClearTexture();
        rawImage.texture = tex;
    }

    public void ClearTexture()
    {
        Color[] cols = new Color[textureWidth * textureHeight];
        for (int i = 0; i < cols.Length; i++) cols[i] = backgroundColor;
        tex.SetPixels(cols);
        tex.Apply();
        isDirty = true;
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        // left mouse or touch
        if (Input.GetMouseButton(0))
        {
            Vector2 localPoint;
            // convert screen to local point on the rectTransform
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint))
            {
                // map localPoint (-rectPivot..+rectPivot) to texture coords
                Vector2 rectSize = rectTransform.rect.size;
                float px = (localPoint.x + rectSize.x * 0.5f) / rectSize.x;
                float py = (localPoint.y + rectSize.y * 0.5f) / rectSize.y;

                int x = Mathf.FloorToInt(px * textureWidth);
                int y = Mathf.FloorToInt(py * textureHeight);

                DrawCircle(x, y);
                tex.Apply();
                isDirty = true;
            }
        }
    }

    void DrawCircle(int cx, int cy)
    {
        int r = Mathf.Max(1, brushSize);

        int x0 = Mathf.Clamp(cx - r, 0, textureWidth - 1);
        int x1 = Mathf.Clamp(cx + r, 0, textureWidth - 1);
        int y0 = Mathf.Clamp(cy - r, 0, textureHeight - 1);
        int y1 = Mathf.Clamp(cy + r, 0, textureHeight - 1);

        int rsq = r * r;
        for (int x = x0; x <= x1; x++)
        {
            for (int y = y0; y <= y1; y++)
            {
                int dx = x - cx;
                int dy = y - cy;
                if (dx * dx + dy * dy <= rsq)
                {
                    // simple overpaint - you can add blending if required
                    tex.SetPixel(x, y, brushColor);
                }
            }
        }
    }

    // public method to save texture to PNG and return the path
    public string SaveToPNG(string fileName = "drawing.png")
    {
        if (tex == null) return null;
        // Ensure we have final changes applied
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        string path = Path.Combine(Application.persistentDataPath, fileName);
        try
        {
            File.WriteAllBytes(path, bytes);
            Debug.Log($"Saved drawing to: {path}");
            return path;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to save drawing: " + ex);
            return null;
        }
    }

    // helper: get Texture2D if you want to keep it in memory
    public Texture2D GetTexture()
    {
        return tex;
    }
}