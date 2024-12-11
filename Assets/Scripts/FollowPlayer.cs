using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class FollowPlayer : MonoBehaviour
{
    private Vector3 offset;
    public float verticalAdjustment = 0.6f;
    public GameObject player;
    private float rotationDamping = 2f;
    private Vector3 updatedOffset;
    public float maxTiltDown = 20f;
    public float maxTiltUp = 80f;
    public float maxPlayerDownTilt = 40f; // TODO best coding practice get from InAirState
    private float xAngle;

    public float minAboveGround = 0.2f;
    
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - player.transform.position;
        updatedOffset = offset;
    }

    void LateUpdate()
    {
        // code adapted from https://discussions.unity.com/t/smooth-follow-c/50348/3
        float targetRotationAngle = player.transform.eulerAngles.y;
        float currentRotationAngle = transform.eulerAngles.y;

        // check if player tilt down is more than maxTiltDown; if so, adjust
        if (player.transform.eulerAngles.x <= (maxPlayerDownTilt + 1f) & player.transform.eulerAngles.x > maxTiltDown)
        {
            xAngle = Mathf.LerpAngle(xAngle, maxTiltDown, rotationDamping * Time.deltaTime);
        }
        else if (player.transform.eulerAngles.x > 180f & player.transform.eulerAngles.x < (360f - maxTiltUp))
        {
            xAngle = Mathf.LerpAngle(xAngle, -maxTiltUp, rotationDamping * Time.deltaTime);
        }
        else
        {
            xAngle = Mathf.LerpAngle(xAngle, player.transform.eulerAngles.x, rotationDamping * Time.deltaTime);
        }
        
        Vector3 up = Quaternion.Euler(xAngle, 0, 0) * Vector3.up;
        Vector3 forward = Quaternion.Euler(xAngle, 0, 0) * Vector3.forward;
        updatedOffset = Vector3.Lerp(updatedOffset, forward * offset.z + up * offset.y, rotationDamping * Time.deltaTime);
        
        // smoothly update currentRotationAngle
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, rotationDamping * Time.deltaTime);
        
        // update camera position (rotate around y-axis to make camera stay behind the player)
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);
        Vector3 pos = player.transform.position + currentRotation * updatedOffset;

        // make sure position is not below the ground surface
        float terrainY = Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.transform.position.y;
        float playerTerrainY = Terrain.activeTerrain.SampleHeight(player.transform.position) + Terrain.activeTerrain.transform.position.y;
        transform.position = new Vector3(pos.x, Mathf.Max(pos.y,
            Mathf.Min(terrainY, playerTerrainY) + minAboveGround), pos.z);
        
        // angle towards player
        transform.LookAt(player.transform.position + Vector3.up * verticalAdjustment);
        
        // [TODO] hide objects between camera & player
        // float dist = 0.6f * Vector3.Distance(transform.position, player.transform.position);
        // if (Physics.Raycast(transform.position, player.transform.position - transform.position, out RaycastHit hit, dist))
        // {
        //     Debug.Log("hit: " + hit.collider.gameObject);
        //     hit.collider.gameObject.SetActive(false);
        // }
    }
}