using Quaternion = UnityEngine.Quaternion;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Bird_FSM
{
    [System.Serializable]
    public class SwimmingState : BaseState<PlayerStateManager>
    {
        private static readonly int SwimParameter = Animator.StringToHash("Swim");
        protected LayerMask groundLayer;
        protected Transform transform;
        protected bool move;
        public float swimSpeed = 5f;
        public float speedMultiplier = 2f;
        public float rotationSpeed = 20f;
        public float rotationSpeedMultiplier = 2f;
        protected float currentSwimSpeed;
        protected float currentRotSpeed;
        private float waterY;
        public bool debug;
        public float speed = 2f;
        
        public override void EnterState(PlayerStateManager player)
        {
            player.UseGravity(false);
            player.SetCurrentVelocity(Vector3.zero);
        
            transform = player.transform;

            groundLayer = player.groundLayer;
            waterY = player.water.transform.position.y;

            player.SetCurrentRotationX(transform.rotation.x);
            
            player.SetBoundingBoxColliderSize(new Vector3(0.1f, 0.1f, 0.4f));
            player.SetBoundingBoxColliderCenter(new Vector3(0, 0.1f, 0.2f));
        }

        public override void UpdateState(PlayerStateManager player)
        {
            ResetUpDownTilt(player);
            
            // if (transform.position.y < -0.35f)
            // {
            //     transform.position = new Vector3(transform.position.x, -0.3f, transform.position.z);
            // }

            // swimming
            if (player.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "SitDownIdle")
            {
                // keep position on surface of water
                transform.position = new Vector3(transform.position.x, waterY, transform.position.z);

                UpdateSpeeds();
                Swim(player);
                TryWalk(player);
            }
            else
            {
                // still transitioning -- reset up down tilt gradually + position too
                float transitionSpeed = transform.position.y > waterY ? speed : 12f;
                
                transform.position = Vector3.Lerp(transform.position, 
                    new Vector3(transform.position.x, waterY, transform.position.z), transitionSpeed * Time.deltaTime);

                // still move forward
                currentSwimSpeed = swimSpeed;
                Swim(player);
            }
        }

        /// <summary>
        /// Raycasts forward to check if player is approaching land. If ground close enough, transitions to Walking.
        /// </summary>
        private void TryWalk(PlayerStateManager player)
        {
            // check distance
            bool groundAhead = Physics.Raycast(transform.position + 0.3f * transform.forward, 
                Vector3.down, out RaycastHit groundHit, groundLayer);
            if (groundAhead && groundHit.distance < 0.2f)
            {
                // check if ground immediately below is close enough (otherwise will auto-swim again)
                bool groundBelow = Physics.Raycast(transform.position, Vector3.down,
                    0.25f, groundLayer); // must match dist below water in WalkingState
                if (groundBelow)
                {
                    player.animator.SetBool(SwimParameter, false);
                    player.SwitchState(player.Walking);
                }
            }
        }

        /// <summary>
        /// Increases currentSwimSpeed and currentRotSpeed if shift is held.
        /// </summary>
        private void UpdateSpeeds()
        {
            currentSwimSpeed = swimSpeed;
            currentRotSpeed = rotationSpeed;

            // TODO: increase speed linearly/use acceleration
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentSwimSpeed *= speedMultiplier;
                currentRotSpeed *= rotationSpeedMultiplier;
            }
        }

        /// <summary>
        /// Sets currentRotationX to 0f. Unlike the implementation in InAirState, this one actually executes the
        /// rotation.
        /// </summary>
        /// <param name="player"></param>
        private void ResetUpDownTilt(PlayerStateManager player)
        {
            float currentRotationX = player.GetCurrentRotationX();
            if (currentRotationX != 0f)
            {
                player.SetCurrentRotationX(Mathf.Lerp(currentRotationX, 0f, 4f * Time.deltaTime));
                transform.rotation = Quaternion.Euler(player.GetCurrentRotationX(),
                    transform.eulerAngles.y,
                    transform.eulerAngles.z);
            }
        }

        /// <summary>
        /// Calls MoveWithCollisionDetection to move the player forward and rotates the player based on horizontal
        /// input. 
        /// </summary>
        private void Swim(PlayerStateManager player)
        {
            // move forward
            move = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W); // TODO: add accel/decel
            Vector3 flattenedForwardVector = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
            Vector3 targetVelocity = (move) ? currentSwimSpeed * flattenedForwardVector : Vector3.zero;
            if (move)
            {
                MoveWithCollisionDetection(player, targetVelocity);
            }
            else
            {
                player.SetCurrentVelocity(targetVelocity);
            }
            
            // rotate
            float tiltDirection = player.GetHorizontalInput();
            transform.forward = Vector3.Lerp(transform.forward, Quaternion.AngleAxis(
                currentRotSpeed * tiltDirection * Time.deltaTime,
                Vector3.up) * transform.forward, 0.9f);
        }
        
        public override void ExitState()
        {
        
        }

        public override void OnCollisionEnter(PlayerStateManager player, Collision collision)
        {
            
        }
    }
}
