using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Bird_FSM
{
    [System.Serializable]
    public class WalkingState<T> : BaseState<T> where T : BirdStateManager<T>
    {
        protected static int Landed = Animator.StringToHash("Landed");
        protected static int IdleRotation = Animator.StringToHash("IdleRotation");
        protected static int WalkSpeed = Animator.StringToHash("WalkSpeed");
        protected static int Landing = Animator.StringToHash("Landing");
        protected static int Glide = Animator.StringToHash("Glide");
        protected static int Walking = Animator.StringToHash("Walking");
        protected static int Flight = Animator.StringToHash("TakeFlight");
        
        protected LayerMask groundLayer;
        protected Transform transform;

        protected bool move;
        protected float walkSpeed = 2f;
        protected float speedMultiplier = 5f;
        protected float rotationSpeed = 20f;
        protected float rotationSpeedMultiplier = 3f;
        protected float currentWalkSpeed;
        protected float currentRotSpeed;
        protected Vector3 goalForward;

        protected float forwardCheck = 0.3f;
        public float slopeCheck = 1.5f;
        public float steepSlope = 50f;
        protected float maxFall = 5f;
        protected float angle;
        protected bool isNearEdge;
        protected bool isNearSteepSlope;
        protected bool canMoveForward;
        protected Vector3 lastTerrainNormal;
        public bool frozen = false;
        protected bool justLanded;
        protected bool groundHit;
        protected float groundDist;
        protected bool falling;
        protected bool wasFalling;
        protected bool smoothing;
        protected bool onBridge;
        protected float minHeight;

        public override void EnterState(T bird)
        {
            // turn on gravity so it doesn't bounce off the ground
            bird.UseGravity(true);
        
            transform = bird.transform;
            groundLayer = bird.groundLayer;
            bird.SetCurrentVelocity(Vector3.zero);
            minHeight = bird.water.transform.position.y - 0.3f;

            justLanded = true;
            falling = false;
            wasFalling = true;
            
            // case for landing close to ground - animation transition may not have finished
            if (bird.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Flap1Forward")
            {
                bird.animator.SetBool(Landing, true);
            }
            else
            {
                bird.animator.SetBool(Landing, false);
            }

            bird.animator.SetBool(Landed, true);
            bird.animator.SetBool(Glide, false);
            
            bird.SetBoundingBoxColliderSize(new Vector3(0.1f, 0.3f, 0.4f));
            bird.SetBoundingBoxColliderCenter(new Vector3(0, 0.4f, 0.2f));
        }

        public override void UpdateState(T bird)
        {
            // case for landing close to the ground - animation transition may not have finished
            if (bird.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Flap1Forward")
            {
                bird.animator.SetBool(Landing, true);
            }
            if (bird.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Landing")
            {
                bird.animator.SetBool(Landing, false);
            }

            GetPositionAttributes();
        }

        /// <summary>
        /// Updates values of groundDist, falling, smoothing, and onBridge.
        /// </summary>
        protected void GetPositionAttributes()
        {
            groundHit = Physics.Raycast(transform.position + 0.1f * Vector3.up, -Vector3.up,
                out RaycastHit hit, 10f, groundLayer);
            if (groundHit)
            {
                groundDist = hit.distance;
                falling = groundDist > 0.5f;
                smoothing = !(hit.collider.CompareTag("Land") && groundDist < 0.2f); // terrain must be tagged as land
                onBridge = hit.collider.CompareTag("Bridge") && groundDist < 0.4f;
            }
            else
            {
                groundDist = -1f;
                falling = true;
            }
        }
        
        /// <summary>
        /// Sets targetVelocity based on `move` variable and calls MoveWithCollisionDetection. If not moving,
        /// updates the shared variable currentVelocity instead of calling MoveWithCollisionDetection.
        /// Also updates animation.
        /// </summary>
        /// <param name="bird">PlayerStateManager</param>
        /// <param name="tiltDirection"></param>
        protected void Walk(T bird, float tiltDirection)
        {
            Vector3 targetVelocity = (move) ? currentWalkSpeed * bird.transform.forward : Vector3.zero;

            if (move)
            {
                MoveWithCollisionDetection(bird, targetVelocity);
            }
            else
            {
                // update current velocity directly so that when it starts moving again
                // it doesn't transition velocities and fall off the edge
                bird.SetCurrentVelocity(targetVelocity);
            }
            
            // update animation
            UpdateAnimation(bird, tiltDirection);
        }
        
        /// <summary>
        /// Triggers walking animation if 1) canMoveForward 2) not in front of obstacle and 3) not falling.
        /// </summary>
        /// <param name="bird"></param>
        /// <param name="tiltDirection"></param>
        protected void UpdateAnimation(T bird, float tiltDirection)
        {
            bird.animator.SetFloat(IdleRotation, Mathf.Abs(tiltDirection));
            if (canMoveForward && bird.GetBoundsExceeded() == 0 && !falling)
            {
                bird.animator.SetBool(Walking, true);
            }
            else
            {
                bird.animator.SetBool(Walking, false);
            }
        }

        /// <summary>
        /// Given position, raycasts down maxDist and returns the angle between the terrain (groundLayer) normal and
        /// Vector3.up.
        /// </summary>
        /// <param name="position">Vector3 bird position</param>
        /// <param name="direction">Vector3 direction to raycast, defaults to -transform.up</param>
        /// /// <param name="maxDist">float maximum distance to check in raycast; defaults to 5f</param>
        /// <returns></returns>
        private float GetTerrainSlope(Vector3 position, Vector3 direction = default, float maxDist = 5f)
        {
            if (direction == default)
            {
                direction = -transform.up;
            }
            // raycast down and get slope of terrain
            Physics.Raycast(position, direction, out RaycastHit terrainHit, maxDist,groundLayer);
            Vector3 terrainNormal = terrainHit.normal;
            return Vector3.Angle(terrainNormal, Vector3.up);
        }

        /// <summary>
        /// Raycasts forwardCheck (0.3f) in the direction of the flattened forward vector.
        /// </summary>
        /// <returns>Returns true if Raycast hits groundLayer</returns>
        private bool CheckObstacleAhead()
        {
            Vector3 flattenedForwardVector = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
            return Physics.Raycast(transform.position, flattenedForwardVector, out RaycastHit hit,
                forwardCheck, groundLayer);
        }

        /// <summary>
        /// Checks for steep terrain slope ahead. (Not currently being used)
        /// </summary>
        protected bool CheckSteepSlopeAhead()
        {
            Vector3 basePos = transform.position + 0.5f * Vector3.up;
            float terrainSlope = GetTerrainSlope(basePos, maxDist: 2f);
            float terrainSlopeForward = GetTerrainSlope(basePos, transform.forward, 1f);
            if (!onBridge && Mathf.Abs(terrainSlope - terrainSlopeForward) > 20f)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the values of class variables isNearEdge and isNearSteepSlope. Assigns isNearEdge based on
        /// raycasting forward, leftward, and rightward. Assigns isNearSteepSlope based on raycasting forward.
        /// Steep slope refers to steep upward slope/wall ahead. Currently only used for NPC script.
        /// </summary>
        protected void IsNearEdgeOrSteepSlope()
        {
            Vector3 basePos = transform.position + 0.5f * Vector3.up;
            bool hasObstacleAhead = CheckObstacleAhead();
            
            // downward
            bool hasForwardFall = !Physics.Raycast(basePos + 0.3f * transform.forward,
                Vector3.down, out RaycastHit hit4, maxFall, groundLayer);
            bool hasSteepSlopeAhead = false;
            if (!hasForwardFall)
            {
                float terrainSlope = GetTerrainSlope(basePos + 1f * transform.forward);
                if (terrainSlope > 30f) // [TODO] could check current normal vs upcoming normal instead of 30f
                {
                    hasForwardFall = true;
                }
            }

            // leftward
            Vector3 forwardLeft = (-transform.right + transform.forward).normalized;
            bool hasLeftFall = !Physics.Raycast(basePos + forwardLeft,
                Vector3.down, out RaycastHit hit2, maxFall, groundLayer);
            if (!hasLeftFall)
            {
                float terrainSlope = GetTerrainSlope(transform.position + 1f * forwardLeft);
                if (terrainSlope > 30f)
                {
                    hasLeftFall = true;
                }
            }
            
            // rightward
            Vector3 forwardRight = (transform.right + transform.forward).normalized;
            bool hasRightFall = !Physics.Raycast(basePos + forwardRight,
                Vector3.down, out RaycastHit hit3, maxFall, groundLayer);
            if (!hasRightFall)
            {
                float terrainSlope2 = GetTerrainSlope(basePos + 1f * forwardRight);
                if (terrainSlope2 > 30f)
                {
                    hasRightFall = true;
                }
            }

            // the downward, leftward, OR rightward ray should not detect anything
            isNearEdge = !hasObstacleAhead && (hasForwardFall || hasLeftFall || hasRightFall);
            
            // now check if there is a steep slope ahead
            float terrainSlopeMoreForward = GetTerrainSlope(basePos + slopeCheck * Vector3.up + slopeCheck * transform.forward);
            float terrainSlopeForwardLeft = GetTerrainSlope(basePos + slopeCheck * Vector3.up + slopeCheck * forwardLeft);
            float terrainSlopeForwardRight = GetTerrainSlope(basePos + slopeCheck * Vector3.up + slopeCheck * forwardRight);
            if (terrainSlopeMoreForward > steepSlope || terrainSlopeForwardLeft > steepSlope || terrainSlopeForwardRight > steepSlope)
            {
                hasSteepSlopeAhead = true;
            }
            
            isNearSteepSlope = hasObstacleAhead && hasSteepSlopeAhead;
        }

        protected void UpdateCanMoveForward()
        {
            IsNearEdgeOrSteepSlope();
            canMoveForward = !isNearEdge & !isNearSteepSlope;
        }
        
        /// <summary>
        /// Sets the values of angle and goalForward for use in Rotate function. Takes into account whether the bird
        /// was falling, whether it just landed, whether the bird is on a bridge, and whether the angle change is
        /// too extreme to execute.
        /// </summary>
        protected void TiltUpDownLand(T bird)
        {
            bool inWater = transform.position.y < bird.water.transform.position.y;
            if (Physics.Raycast(transform.position, -transform.up, out RaycastHit terrainHit, 2f,
                    groundLayer))
            {
                Vector3 terrainNormal = terrainHit.normal;
                if (wasFalling)
                {
                    lastTerrainNormal = Vector3.up;
                }
                if (justLanded)
                {
                    lastTerrainNormal = terrainNormal;
                    justLanded = false;
                }
                float angleDiff = Vector3.Angle(lastTerrainNormal, terrainNormal);
                angle = Vector3.Angle(transform.up, terrainNormal); // angle between normal and PLAYER
                float globalAngle = Vector3.Angle(Vector3.up, terrainNormal); // angle between normal and GLOBAL UP

                if (onBridge || inWater)
                {
                    // must stay upright on bridge or in water
                    goalForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
                    lastTerrainNormal = Vector3.up;
                }
                else
                {
                    if (smoothing && (globalAngle > 20f || angle > 60f || (angleDiff > 10f && angle > 20f)))
                    {
                        // constrain tilt on non-land e.g. rocks (smoothing = true) to 20f
                        if (globalAngle > 20f)
                        {
                            float lastGlobalAngle = Vector3.Angle(Vector3.up, lastTerrainNormal);
                            if (lastGlobalAngle > 20f)
                            {
                                lastTerrainNormal = Vector3.Slerp(lastTerrainNormal, Vector3.up,
                                    (lastGlobalAngle - 20f) / lastGlobalAngle);
                            }
                        }
                        goalForward = Vector3.ProjectOnPlane(transform.forward, lastTerrainNormal).normalized;
                    }
                    else
                    {
                        goalForward = Vector3.ProjectOnPlane(transform.forward, terrainNormal).normalized;
                        lastTerrainNormal = terrainNormal;
                    }                    
                }
            }
        }

        public override void ExitState()
        {
        
        }

        public override void OnCollisionEnter(T bird, Collision collision)
        {
            
        }
    }
}
