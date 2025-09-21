using StarterAssets;
using UnityEngine;

public class DrawingToggle : MonoBehaviour
{
    public GameObject drawingCanvasObject; // parent that contains DrawingPad (or the RawImage)
    public string saveFileName = "drawing.png";
    public StarterAssetsInputs inputs;
    DrawingPad pad;

    private bool lastValue = false;

    void Start()
    {
        if (drawingCanvasObject != null)
            pad = drawingCanvasObject.GetComponent<DrawingPad>();

        if (drawingCanvasObject != null)
            drawingCanvasObject.SetActive(false);
    }

    void Update()
    {
        if (inputs == null)
            return;

        bool current = inputs.drawPad;

        // Detect rising edge: false -> true
        if (current && !lastValue)
        {
            ToggleDrawing();
        }

        // store current for next frame's edge detection
        lastValue = current;
    }

    public void ToggleDrawing()
    {
        if (drawingCanvasObject == null)
        {
            Debug.LogWarning("DrawingToggle: no drawingCanvasObject assigned.");
            return;
        }

        bool isActive = drawingCanvasObject.activeInHierarchy;

        if (!isActive)
        {
            // show
            drawingCanvasObject.SetActive(true);
            // optional: clear previous drawing
            // pad?.ClearTexture();
        }
        else
        {
            // hide & save
            if (pad != null)
            {
                string saved = pad.SaveToPNG(saveFileName);
                Debug.Log("Saved drawing: " + saved);
            }
            drawingCanvasObject.SetActive(false);
        }
    }
}