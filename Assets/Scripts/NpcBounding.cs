using Bird_FSM;
using UnityEngine;

public class NpcBounding : BirdBounding
{
    private NpcStateManager npc;
    
    void Start()
    {
        npc = GetComponentInParent<NpcStateManager>();
    }

    public void OnTriggerEnter(Collider other)
    {
        string currentState = npc.GetCurrentState().GetType().Name;
        IncrementBounds(other, currentState);
    }
}
