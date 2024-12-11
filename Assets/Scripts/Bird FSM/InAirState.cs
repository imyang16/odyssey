using UnityEngine;

namespace Bird_FSM
{
    [System.Serializable]
    public class InAirState<T> : BaseState<T> where T : BirdStateManager<T>
    {
        protected float rotationXSpeed = 50f;
        protected float tiltResetSpeed = 5f;
        protected Vector3 rightCenterOfRotation;
        protected Vector3 leftCenterOfRotation;
        protected float groundLimitedMaxDownTilt = 40f;
        protected float maxUpTilt = 80f;
        protected float maxLeftRightTilt = 30f;

        public override void EnterState(T bird)
        {
            bird.UseGravity(false);
        }

        public override void UpdateState(T bird)
        {
        }

        public override void ExitState()
        {
        }
    
        public override void OnCollisionEnter(T bird, Collision collision)
        {
        }
        
        /// <summary>
        /// Updates shared variable currentRotationX to value based on vertical input. Upwards tilt amount is limited
        /// to maxUpTilt, downwards tilt amount is limited to groundLimitedMaxDownTilt. Does not actually change the
        /// player's rotation. 
        /// </summary>
        /// <param name="bird"></param>
        /// <param name="verticalInput">Float between -1 (down arrow/S input) and 1 (up arrow/W input).</param>
        protected void TiltUpDown(T bird, float verticalInput)
        {
            float currentRotationX = bird.GetCurrentRotationX();
            float goalRotationX = Mathf.Min(groundLimitedMaxDownTilt,
                currentRotationX - verticalInput * rotationXSpeed * Time.deltaTime);
            goalRotationX = Mathf.Clamp(goalRotationX, -maxUpTilt, groundLimitedMaxDownTilt);
            
            // smoothly interpolate + set shared variable value
            currentRotationX = Mathf.Lerp(currentRotationX, goalRotationX, 0.7f);
            bird.SetCurrentRotationX(currentRotationX);
        }

        /// <summary>
        /// Updates shared variable currentRotationX to transition value back to flat (no vertical tilt) position.
        /// Does not actually change the player's rotation.
        /// </summary>
        protected void ResetUpDownTilt(T bird)
        {
            float currentRotationX = bird.GetCurrentRotationX();
            bird.SetCurrentRotationX(Mathf.Lerp(currentRotationX, 0, (tiltResetSpeed / 3f) * Time.deltaTime));
        }

        /// <summary>
        /// Updates shared variable currentTilt based on tiltDirection. Does not actually change the player's rotation.
        /// </summary>
        /// <param name="bird"></param>
        /// <param name="tiltDirection">Float between -1 (left arrow/A input) and 1 (right arrow/D input).</param>
        protected void TiltLeftRight(T bird, float tiltDirection)
        {
            float currentTilt = bird.GetCurrentTilt();
            bird.SetCurrentTilt(Mathf.Lerp(currentTilt, maxLeftRightTilt * -tiltDirection, 3f * Time.deltaTime));
        }

        /// <summary>
        /// Updates shared variable currentTilt to transition value back to upright (no horizontal tilt) position.
        /// Does not actually change the player's rotation.
        /// </summary>
        protected void ResetLeftRightTilt(T bird, float speedMultiplier = 1f)
        {
            float currentTilt = bird.GetCurrentTilt();
            bird.SetCurrentTilt(Mathf.Lerp(currentTilt, 0, tiltResetSpeed * speedMultiplier * Time.deltaTime));
        }
    }
}
