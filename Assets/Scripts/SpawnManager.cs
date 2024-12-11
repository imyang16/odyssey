using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{
    public GameObject foundFx;
    public GameObject waterSkimFx;
    public GameObject panel;
    public GameObject craneIconPrefab;
    public float craneIconStart = 350f;
    public float craneIconMove = 70f;
    
    /// <summary>
    /// Instantiates particle system given in Inspector.
    /// </summary>
    public GameObject SpawnFoundFX(Vector3 pos, Quaternion rot = default)
    {
        // instantiate particle system
        rot = rot == default ? Quaternion.identity : rot;
        return Instantiate(foundFx, pos, rot);
    }

    /// <summary>
    /// Fades in crane icon to bottom-right corner of UI.
    /// </summary>
    /// <param name="id">Number of cranes found</param>
    public void AddCraneIcon(float id)
    {
        GameObject craneIcon = Instantiate(craneIconPrefab, panel.transform, false);
        craneIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(craneIconStart - id * craneIconMove, 0);
        Image iconImage = craneIcon.GetComponent<Image>();
        StartCoroutine(FadeIn(iconImage));
    }

    private IEnumerator FadeIn(Image image)
    {
        // fade in
        float duration = 3f;
        float elapsedTime = 0f;
        Color finalColor = image.color;
        Color initialColor = new Color(finalColor.r, finalColor.g, finalColor.b, 0f); // transparent 
        image.color = initialColor;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            image.color = Color.Lerp(initialColor, finalColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    // public void SpawnNPCBird()
    // {
    //     // not implemented
    // }
}