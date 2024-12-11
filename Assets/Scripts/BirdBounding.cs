using System.Collections.Generic;
using UnityEngine;

public class BirdBounding : MonoBehaviour
{
    private Dictionary<GameObject, bool> triggerVals = new Dictionary<GameObject, bool>();
    public float boundsExceeded;

    protected void IncrementBounds(Collider other, string state)
    {
        // only use bounding for Walking and Swimming states
        if (state != "PlayerWalkingState" && state != "SwimmingState")
        {
            boundsExceeded = 0;
        }
        else
        {
            if (other.CompareTag("Climbable") && CanTrigger(other.gameObject))
            {
                triggerVals[other.gameObject] = true;
                boundsExceeded++;
            }
        }
    }

    private bool CanTrigger(GameObject otherObject)
    {
        // prevent repeatedly triggering the same object
        if (triggerVals.TryGetValue(otherObject, out bool value) && value)
        {
            return false;
        }
        return true;
    }

    public void OnTriggerExit(Collider other)
    {
        if (triggerVals.TryGetValue(other.gameObject, out bool value) && value)
        {
            boundsExceeded--;
            triggerVals[other.gameObject] = false;
        }
    }
}
