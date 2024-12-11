using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    private static readonly int Tint = Shader.PropertyToID("_Tint");
    public Transform sun;
    public Light sunLight;
    public Material skyboxMaterial;
    
    private float cycle = 1f; // length of day in minutes
    public float timeOfDay; // ranges 0-1

    private float rotationSpeed;
    public float startRotation;
    private float intensity;
    private float sunAngle;
    
    public Gradient sunColor;
    public Gradient skyboxColor;
    
    // Start is called before the first frame update
    void Start()
    {
        rotationSpeed = 360f / (cycle * 60);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAngleAndTimeOfDay();
        
        // say sunrise/set are between 165-180 deg and 320-335 deg
        // rotate sun
        AdjustSunRotation();
        AdjustSunIntensity();

        // change skybox settings
        skyboxMaterial.SetColor(Tint, skyboxColor.Evaluate(intensity));
    }

    private void UpdateAngleAndTimeOfDay()
    {
        sunAngle = Mathf.Atan2(-sun.forward.y, sun.forward.z) * Mathf.Rad2Deg;
        if (sunAngle < 0f)
        {
            sunAngle += 360f;
        }

        timeOfDay = 0.25f + sunAngle / 360f;
        if (sunAngle > 270f)
        {
            timeOfDay -= 1f;
        }
    }
    private void AdjustSunRotation()
    {
        sun.transform.Rotate(rotationSpeed * Time.deltaTime, 0, 0);
    }

    private void AdjustSunIntensity()
    {
        intensity = -Vector3.Dot(-sun.forward, Vector3.down);
        intensity = Mathf.Clamp01(intensity);
        sunLight.intensity = intensity;
    }

    private void AdjustSunColor()
    {
        sunLight.color = sunColor.Evaluate(intensity);
    }
}
