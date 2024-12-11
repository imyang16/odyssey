using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Bird_FSM
{
    [System.Serializable]
    public class PlayerWalkingState : WalkingState<PlayerStateManager>
    {
        protected static readonly int Jump1 = Animator.StringToHash("Jump");
        private static readonly int Swim = Animator.StringToHash("Swim");

        public override void UpdateState(PlayerStateManager player)
        {
            base.UpdateState(player);

            if (!frozen)
            {
                TryJump(player); // press j
                TryTakeFlight(player); // press space to take flight
                TrySwim(player); // auto-swims if deep enough in water
                UpdateSpeeds(player); // press shift to move + rotate faster
                
                // get vertical + horizontal input
                move = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
                canMoveForward = move;
                float tiltDirection = player.GetHorizontalInput();

                Walk(player, tiltDirection);
                Rotate(player, tiltDirection);
                
                wasFalling = falling;
            }
            else
            {
                // freeze rotation animation + player movement
                player.animator.SetFloat(IdleRotation, 0);
                player.animator.SetBool(Walking, false);
            }
        }

        /// <summary>
        /// Raycasts forward/up to check if clear for jump. If so, and boundsExceeded > 0 and J key is pressed,
        /// player switches to JumpingState. Assigns type of jump (short vs. tall) based on distance to ground.
        /// </summary>
        /// <param name="player"></param>
        private void TryJump(PlayerStateManager player)
        {
            // raycast up/ahead to make sure clear for jump (jump anim gets ~0.8f)
            bool canJump = Physics.Raycast(transform.position + 1f * Vector3.up + 1f * transform.forward,
                -transform.up, out RaycastHit gHit, 1f,
                groundLayer);
            if (player.GetBoundsExceeded() > 0 && canJump)
            {
                if (player.training && player.firstJump)
                {
                    player.StartJumpInstructionSequence();
                    player.firstJump = false;
                }
                if (Input.GetKey(KeyCode.J))
                {
                    // assign jump type
                    player.SetJumpType(gHit.distance > 0.6f
                        ? PlayerStateManager.JumpType.Short // [TODO] could change animation speed
                        : PlayerStateManager.JumpType.Tall);

                    // transition to JumpingState
                    player.animator.SetBool(Jump1, true);
                    player.SwitchState(player.Jumping);                    
                }
            }
        }

        /// <summary>
        /// Checks if spacebar pressed - if so, stops movement and transitions to TakingFlightState. 
        /// </summary>
        /// <param name="player"></param>
        private void TryTakeFlight(PlayerStateManager player)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // stop movement to prepare for flight
                player.SetCurrentVelocity(Vector3.zero);
                
                // transition
                player.animator.SetBool(Flight, true);
                player.SwitchState(player.TakingFlight);
            }
        }

        /// <summary>
        /// Checks if falling above water or if height below minHeight - in either case, transitions to SwimmingState. 
        /// </summary>
        private void TrySwim(PlayerStateManager player)
        {
            bool transitionToSwim = false;
            
            if (transform.position.y is > 0.5f and < 2f)
            {
                // case 1: falling above water
                if (!groundHit ||
                    groundDist > transform.position.y - minHeight /*ground hit but its underwater*/)
                {
                    transitionToSwim = true; // [TODO] add splash FX
                }
            } else if (transform.position.y <= minHeight) 
            {
                // case 2: below water level
                transform.position = new Vector3(transform.position.x, minHeight, transform.position.z);
                transitionToSwim = true;
            }

            if (transitionToSwim)
            {
                // update animation and transition
                player.animator.SetBool(Walking, false);
                player.animator.SetBool(Swim, true);
                player.SwitchState(player.Swimming);
            }
        }
        
        /// <summary>
        /// Increases currentWalkSpeed and currentRotSpeed if shift is held. Also updates walking animation speed.
        /// </summary>
        private void UpdateSpeeds(PlayerStateManager player)
        {
            currentWalkSpeed = walkSpeed;
            currentRotSpeed = rotationSpeed;

            // TODO: increase speed linearly/use acceleration
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentWalkSpeed *= speedMultiplier;
                currentRotSpeed *= rotationSpeedMultiplier;

                player.animator.SetFloat(WalkSpeed, 2f);
            }
            else
            {
                player.animator.SetFloat(WalkSpeed, 1f);
            }
        }
        
        /// <summary>
        /// Tilts player up/down based on terrain and rotates left/right based on keyboard input. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="tiltDirection">Float representing keyboard input: -1, 0, or 1.</param>
        protected void Rotate(PlayerStateManager player, float tiltDirection)
        {
            // save current position to reset to if near edge/steep slope
            Vector3 lockedPos = player.transform.position;
            
            // tilt up/down based on terrain + rotate left/right (based on user input) 
            TiltUpDownLand(player); // sets angle and goalForward
            if (angle > 5f)
            {
                Quaternion horizontalRot =
                    Quaternion.AngleAxis(currentRotSpeed * tiltDirection * Time.deltaTime, transform.up);
                
                // slower transition for terrain tilt
                Vector3 terrainTiltedForward = Vector3.Lerp(transform.forward, goalForward, 8f * Time.deltaTime);
                
                // faster transition for keyboard input
                Vector3 newForward = Vector3.Lerp(terrainTiltedForward, horizontalRot * terrainTiltedForward, 0.9f);
                transform.forward = newForward;
            }
            else
            {
                // don't project for small angles b/c projecting yields very small vector which causes stuttering
                transform.forward = Vector3.Lerp(transform.forward, Quaternion.AngleAxis(currentRotSpeed * tiltDirection * Time.deltaTime,
                    Vector3.up) * transform.forward, 0.9f);
            }

            if (isNearEdge || isNearSteepSlope)
            {
                // lock position if near edge to prevent jitters
                player.transform.position = lockedPos;
            }
        }
        
        public void ExitState(PlayerStateManager player)
        {
        
        }
        
        /// <summary>
        /// For collisions with NPC-tagged objects -- spawns FX, fades NPC, and freezes player while this happens.
        /// </summary>
        public override void OnCollisionEnter(PlayerStateManager player, Collision collision)
        {
            if (collision.gameObject.CompareTag("NPC"))
            {
                NpcStateManager npc = collision.gameObject.GetComponent<NpcStateManager>(); 
                npc.SpawnFoundFX(transform.position + new Vector3(0, 0.6f, 0));
                npc.Fade();
                player.Freeze();
            }
        }
    }
}
