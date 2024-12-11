using Bird_FSM;
using UnityEngine;

public class PlayerBounding : BirdBounding
{
    private PlayerStateManager player;
    
    void Start()
    {
        player = GetComponentInParent<PlayerStateManager>();
    }

    public void OnTriggerEnter(Collider other)
    {
        string currentState = player.GetCurrentState().GetType().Name;
        IncrementBounds(other, currentState);
    }
}
