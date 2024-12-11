using UnityEngine;

namespace Bird_FSM
{
    public class LandingState<T> : InAirState<T> where T : BirdStateManager<T>
    {
        private Transform transform;
        // private LayerMask groundLayer;

        public override void EnterState(T player)
        {
            base.EnterState(player);
            player.UseGravity(false);
            transform = player.transform;
            // groundLayer = player.groundLayer;
        }

        public override void UpdateState(T bird)
        {
            // set rotation back to 0
            ResetLeftRightTilt(bird);
            ResetUpDownTilt(bird);
            float currTilt = bird.GetCurrentTilt();
            float currRotX = bird.GetCurrentRotationX();
            transform.rotation = Quaternion.Euler(currRotX, transform.eulerAngles.y, currTilt);

            // still movement while rotating to flat, then move downwards to land
            Vector3 direction = (Mathf.Abs(currTilt) >= 0.05f & Mathf.Abs(currRotX) >= 0.05f)
                ? Vector3.zero : Vector3.down;
            
            // land faster if closer to ground
            // Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10f, groundLayer);
            // float distanceToGround = hit.distance;
            // float landSpeed = distanceToGround > 3f ? 0.5f * Time.deltaTime : 0.8f;

            Move(bird, direction, customTransitionTime: 0.8f);
        }
        
        public override void OnCollisionEnter(T bird, Collision collision)
        { if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                bird.SwitchState(bird.Walking);
            }
        }
    }
}
