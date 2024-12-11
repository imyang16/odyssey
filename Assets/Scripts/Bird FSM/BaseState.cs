using UnityEngine;

namespace Bird_FSM
{
    [System.Serializable]
    public abstract class BaseState<T> where T : BirdStateManager<T>
    {
        public virtual void EnterState(T player)
        {
            
        }

        public virtual void UpdateState(T player)
        {
            
        }

        public virtual void ExitState()
        {
            
        }

        public virtual void OnCollisionEnter(T player, Collision collision)
        {
            
        }

        /// <summary>
        /// Smoothly transitions the player's velocity from the shared variable currentVelocity to the specified
        /// targetVelocity. The speed of the transition (last argument in Lerp) can be scaled up/down with
        /// optional argument transitionMultiplier. Moves the player's transform by the updated
        /// currentVelocity * Time.deltaTime. </summary>
        /// <param name="player"></param>
        /// <param name="targetVelocity">Vector3 target velocity</param>
        /// <param name="transitionMultiplier">Optional (defaults to 1f) - multiplies the default transition time
        /// of 2f * Time.deltaTime</param>
        /// /// <param name="customTransitionTime">Optional - entirely replaces default transition time
        /// of 2f * Time.deltaTime</param>
        public void Move(BirdStateManager<T> bird, Vector3 targetVelocity, float transitionMultiplier = 1f,
            float customTransitionTime = 0f)
        {
            float transitionTime = (customTransitionTime == 0f) ? 2f * transitionMultiplier * Time.deltaTime
                : customTransitionTime;
            Vector3 moveVelocity = Vector3.Lerp(bird.GetCurrentVelocity(), targetVelocity, transitionTime);
            bird.SetCurrentVelocity(moveVelocity);
            bird.transform.position += moveVelocity * Time.deltaTime;
        }

        /// <summary>
        /// Checks for objects in the area of the player's bounding box and if there is a non-zero number of triggers,
        /// the player's velocity is set to replacementVelocity (which defaults to Vector3.zero) and the
        /// transitionMultiplier set to 4 for a faster stop. Calls Move(player, targetVelocity, transitionMultiplier)
        /// and returns updated value of targetVelocity.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="targetVelocity"></param>
        /// <param name="replacementVelocity">Optional parameter that replaces targetVelocity when there are objects in
        /// the bounding box area. Defaults to Vector3.zero.</param>
        /// <param name="customTransitionTime"></param>
        public Vector3 MoveWithCollisionDetection(BirdStateManager<T> bird, Vector3 targetVelocity,
            Vector3 replacementVelocity = default, float customTransitionTime = 0f)
        {
            if (replacementVelocity == default)
            {
                replacementVelocity = Vector3.zero;
            }
        
            // check for objects in bounding box area
            float transitionMultiplier = 1f;
            if (bird.GetBoundsExceeded() > 0)
            {
                transitionMultiplier = 6f;
                Move(bird, replacementVelocity, transitionMultiplier, customTransitionTime);
            }
            else
            {
                Move(bird, targetVelocity, transitionMultiplier, customTransitionTime);
            }
            
            return targetVelocity;
        }
    }
}