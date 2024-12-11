using UnityEngine;

namespace Bird_FSM
{
    public class TakingFlightState : InAirState<PlayerStateManager>
    {
        private static readonly int TakeFlight = Animator.StringToHash("TakeFlight");
        private static readonly int Landed = Animator.StringToHash("Landed");
        private Transform transform;
        private float liftOffSpeed = 5f;
        private float groundHeight;
        private float transitionToFlyHeight = 4f;

        public override void EnterState(PlayerStateManager player)
        {
            base.EnterState(player);
            transform = player.transform;
            groundHeight = transform.position.y;

            // update shared variables
            if (transform.eulerAngles.z > 180f)
            {
                player.SetCurrentTilt(transform.eulerAngles.z - 360f);
            }
            else
            {
                player.SetCurrentTilt(transform.eulerAngles.z);
            }
            player.SetCurrentRotationX(transform.rotation.x);
            
            // update bounding box size + position
            player.SetBoundingBoxColliderSize(new Vector3(1.4f, 0.2f, 1.4f));
            player.SetBoundingBoxColliderCenter(new Vector3(0, 0.2f, 0.7f));
        }

        public override void UpdateState(PlayerStateManager player)
        {
            // transition from jump to flying state
            if (player.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "NewUpDown")
            {
                player.animator.SetBool(Landed, false);
                player.animator.SetBool(TakeFlight, false);
            }
            
            // only lift into air once jump is finished
            // [TODO] taking flight under a slope
            if(player.animator.GetBool(TakeFlight) == false)
            {
                Vector3 liftOffVelocity = (transform.forward + transform.up).normalized * liftOffSpeed;
                MoveWithCollisionDetection(player, liftOffVelocity, transform.up);

                // adjust left/right tilt for taking off from slope
                ResetLeftRightTilt(player, 0.4f);
                ResetUpDownTilt(player);
                transform.rotation = Quaternion.Euler(player.GetCurrentRotationX(),
                    transform.eulerAngles.y,
                    player.GetCurrentTilt());
            }
        
            // switch states to allow player to control flying after reaching enough height
            if (transform.position.y - groundHeight > transitionToFlyHeight)
            {
                player.SwitchState(player.Flying);
            }
        }
    }
}