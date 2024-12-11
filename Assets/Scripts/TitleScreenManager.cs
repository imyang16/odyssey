using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    private static readonly int RotationSpeed = Shader.PropertyToID("_RotationSpeed");
    private static readonly int FadeOut = Animator.StringToHash("FadeOut");
    public Animator animator;
    public Material skybox;
    
    void Start()
    {
        // Set the new rotation
        skybox.SetFloat(RotationSpeed, 2f);
    }
    
    public void OnFadeComplete()
    {
        skybox.SetFloat(RotationSpeed, 0.2f);
        SceneManager.LoadScene("Game");
    }
    
    public void PlayGame()
    {
        animator.SetTrigger(FadeOut);
    }

    public void OpenCreditsScene()
    {
        SceneManager.LoadScene("Credits");
    }

    // Settings scene not designed
    // public void OpenSettingScene()
    // {
    //     SceneManager.LoadScene("Settings");
    // }
}
