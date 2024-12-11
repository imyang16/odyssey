using UnityEngine;

public class Fog : MonoBehaviour
{
    private float baseDensity;
    public float denserFogHeight = 80f;
    public Gradient skyGradient;
    
    // Start is called before the first frame update
    void Start()
    {
        baseDensity = RenderSettings.fogDensity;
    }

    // Update is called once per frame
    void Update()
    {
        float cameraHeight = Camera.main.transform.position.y;
        if (cameraHeight > denserFogHeight)
        {
            float density = baseDensity + 0.001f * (cameraHeight - denserFogHeight) / denserFogHeight;
            RenderSettings.fogDensity = density;
        }
        else
        {
            RenderSettings.fogDensity = baseDensity;
        }

        float height = 0f;
        if (cameraHeight > 10f)
        {
            height = (cameraHeight - 10f) / 350f;
        }

        RenderSettings.fogColor = skyGradient.Evaluate(height);
    }
}
