using UnityEngine;

namespace Bird_FSM
{
    [System.Serializable]
    public class FlyingState : InAirState<PlayerStateManager>
    {
        protected static readonly int Glide = Animator.StringToHash("Glide");
        private static readonly int Landing = Animator.StringToHash("Landing");
        private LayerMask groundLayer;

        private float flySpeed = 7f;
        private float currentFlySpeed;
        private float speedMultiplier = 2.8f;
        private float arcRadius = 28f;
        
        private Vector3 targetVelocity;
        public float maxHeight = 250f;
        private float minHeight;
        private float canLandHeight = 3f; // must be > skimDistance
        public float skimDistance = 2f; // value should relate to bounding box forward-axis
        public float levelOffHeight = 6f;
        public float levelHeight = 0.6f; // below this height bird is auto-level with ground

        private float slopeBelow;
        private bool hasGroundBelow;
        private float groundHeight;
        private float groundDistance;
        private float groundSlope;
        private bool groundSlopesDown;
        private float groundTilt;
        private RaycastHit hitDown;

        private Transform transform;
        public bool debug;
    
        public override void EnterState(PlayerStateManager player)
        {
            base.EnterState(player);
            groundLayer = player.groundLayer;
            minHeight = player.water.transform.position.y - 0.2f;
            transform = player.transform;
            
            // update bounding box to be larger (b/c higher speed, need more time to avoid collision)
            player.SetBoundingBoxColliderSize(new Vector3(1.4f, 1, 6));
            player.SetBoundingBoxColliderCenter(new Vector3(0, 0.3f, 3));

            UpdateCentersOfRotation();

            if (player.training && player.firstFly)
            {
                player.StartFlyInstructionSequence();
                player.firstFly = false;
            }
        }

        public override void UpdateState(PlayerStateManager player)
        {
            UpdateSpeeds();
            GetGroundAttributes(player);
            TryLand(player);
            Fly(player);
        }

        public void ExitState(PlayerStateManager player)
        {
        }

        /// <summary>
        /// Sets the values of hasGroundBelow, groundHeight, groundDistance, and groundSlope.
        /// </summary>
        /// <param name="player"></param>
        protected void GetGroundAttributes(PlayerStateManager player)
        {
            hasGroundBelow = Physics.Raycast(transform.position, Vector3.down, out hitDown,
                10f, groundLayer);
            groundHeight = hitDown.point.y;
            if (groundHeight < player.water.transform.position.y)
            {
                hasGroundBelow = false;
            }
            groundDistance = hitDown.distance;
            groundSlope = Vector3.Angle(Vector3.up, hitDown.normal);
        }

        /// <summary>
        /// Increases currentFlySpeed if shift is held.
        /// </summary>
        protected void UpdateSpeeds()
        {
            currentFlySpeed = flySpeed;
            if (debug)
            {
                currentFlySpeed = 1f;
            }
            // TODO: increase speed linearly/use acceleration
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentFlySpeed *= speedMultiplier;
            }
        }

        /// <summary>
        /// Raycasts down to see if player within landing distance of groundLayer. If so, checks whether slope of
        /// ground is less than 30f and player is not rotated more than 10f up. If so and spacebar pressed, switches
        /// to LandingState.
        /// <para>If in training and first time landing, shows landing instruction text.</para>
        /// </summary>
        private void TryLand(PlayerStateManager player)
        {
            // raycast to see if able to land
            if (hasGroundBelow)
            {
                if (groundSlope < 30f && player.GetCurrentRotationX() > -10f && groundDistance <= canLandHeight)
                {
                    if (player.training && player.firstLand)
                    {
                        player.StartLandInstructionSequence();
                        player.firstLand = false;
                    }
                    if (Input.GetKey(KeyCode.Space))
                    {
                        player.animator.SetBool(Landing, true);
                        player.SwitchState(player.Landing);
                    }
                }
            }
        }
    
        /// <summary>
        /// Gets horizontal/vertical keyboard input and tilts player accordingly. Player automatically moves forward
        /// with speed `currentFlySpeed`. Player automatically levels off vertical tilt if close to the ground. Calls
        /// GetBoundedAndSkimmingPosition() to bound player position between min height (or ground height) and max
        /// height and move around obstacles. 
        /// </summary>
        /// <param name="player"></param>
        protected void Fly(PlayerStateManager player)
        {
            // get keyboard arrow/WASD input
            float tiltDirection = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
       
            // if close to ground, level off vertical tilt
            if ((hasGroundBelow && groundDistance < levelOffHeight) ||
                (!hasGroundBelow && transform.position.y - player.water.transform.position.y < levelOffHeight))
            {
                float distDown = hasGroundBelow ? groundDistance :
                    transform.position.y - player.water.transform.position.y;
                
                groundLimitedMaxDownTilt = distDown < levelHeight ? 0f :
                    Mathf.Round((distDown - levelHeight) * 40f / (levelOffHeight - levelHeight) / 0.2f) * 0.2f;
                
                if (hasGroundBelow && groundDistance < levelHeight && groundSlope < 0f)
                {
                    // sloping down -> can adjust the down slope to be equal to groundSlope
                    groundLimitedMaxDownTilt = -groundSlope;
                }
            }
            else
            {
                groundLimitedMaxDownTilt = 40f;
            }
            
            // update tilts and targetVelocity
            int movementType = (verticalInput == 0 ? 0 : 1) + (tiltDirection == 0 ? 0 : 2);
            switch (movementType)
            {
                // no input
                case 0:
                    // no vertical input - reset tilt
                    ResetUpDownTilt(player);
                    
                    // no horizontal input
                    goto default;
            
                // just vertical input 
                case 1:
                    // tilt according to vertical input
                    TiltUpDown(player, verticalInput);
                    
                    // no horizontal input
                    goto default;
            
                // just horizontal input
                case 2:
                    // no vertical input - reset tilt
                    ResetUpDownTilt(player);
                
                    // tilt according to horizontal input
                    TiltLeftRight(player, tiltDirection);
                    targetVelocity = GetArcVelocity(tiltDirection, currentFlySpeed);

                    break;
            
                // both vertical AND horizontal input
                case 3:
                    // tilt according to vertical AND horizontal input
                    TiltLeftRight(player, tiltDirection);
                    TiltUpDown(player, verticalInput);
                
                    // combine arc and upwards velocities to get helix shape
                    Vector3 arcVelocity = GetArcVelocity(tiltDirection, currentFlySpeed);
                    Vector3 verticalVelocity = Vector3.up * (verticalInput * (currentFlySpeed/3));
                    targetVelocity = (arcVelocity + verticalVelocity).normalized * currentFlySpeed;
                
                    break;
            
                // from cases 0 & 1
                default:
                    // velocity then is just the forward direction
                    targetVelocity = transform.forward * currentFlySpeed;
                
                    // reset horizontal tilt
                    ResetLeftRightTilt(player);
                    
                    break;
            }
            UpdateCentersOfRotation();
            
            // update animation
            if (verticalInput > 0 | movementType == 0)
            {
                // cases where it flaps: going up, going up and left/right, going forward
                player.animator.SetBool(Glide, false);
            }
            else
            {
                player.animator.SetBool(Glide, true);
            }

            // get bounded & skimming position and update player position
            Vector3 boundedPos = GetBoundedAndSkimmingPosition();
            transform.position = Vector3.Lerp(transform.position, boundedPos, 0.8f);

            // get velocity
            targetVelocity = MoveWithCollisionDetection(player, targetVelocity);
        
            // rotate the player
            transform.rotation = Quaternion.Euler(player.GetCurrentRotationX(),
                transform.eulerAngles.y,
                player.GetCurrentTilt());
        }

        /// <summary>
        /// Updates rightCenterOfRotation and leftCenterOfRotation based on transform.position. If close to the ground,
        /// adjusts y-values to be groundHeight + levelHeight.
        /// </summary>
        protected void UpdateCentersOfRotation()
        {
            rightCenterOfRotation = transform.position + transform.right * arcRadius;
            leftCenterOfRotation = transform.position + transform.right * -arcRadius;
            
            // adjust y values if close to ground
            if (hasGroundBelow && groundDistance < levelHeight && groundSlope < 0)
            {
                rightCenterOfRotation.y = groundHeight + levelHeight;
                leftCenterOfRotation.y = groundHeight + levelHeight;
            }
        }
    
        /// <summary>
        /// Returns velocity to rotate around left center of rotation (if left/A key pressed)
        /// or center of rotation (if right/D key pressed). Also rotates the object.
        /// </summary>
        /// <param name="tiltDirection">Left/right keyboard input</param>
        /// <param name="speed">Current speed as a float</param>
        /// <returns></returns>
        protected Vector3 GetArcVelocity(float tiltDirection, float speed)
        {
            // store current position for function return
            Vector3 originalPosition = transform.position;

            // convert linear speed to angular speed
            float arcSpeed = speed * 180 / (arcRadius * Mathf.PI);

            Vector3 centerOfRotation = (tiltDirection > 0) ? rightCenterOfRotation : leftCenterOfRotation;
            Vector3 axisOfRotation = (tiltDirection > 0) ? Vector3.up : Vector3.down;
            Quaternion angle = Quaternion.AngleAxis(arcSpeed * Time.deltaTime, axisOfRotation);
            Vector3 targetPosition = angle * (originalPosition - centerOfRotation) + centerOfRotation;

            // rotate object itself
            transform.rotation = angle * transform.rotation;

            // calculate velocity (change in position per frame)
            Vector3 targetArcVelocity = (targetPosition - originalPosition) / Time.deltaTime;
            return targetArcVelocity;
        }

        /// <summary>
        /// Keeps y-position within min & max height bounds and adds skimming behavior if there are objects close to
        /// the left, right, or front. Ex. if there is an object to the left, it will maintain a distance of 2f from
        /// that object instead of allowing the bounding box to collide and stopping movement altogether.
        /// </summary>
        /// <returns>A modified transform.position</returns>
        protected Vector3 GetBoundedAndSkimmingPosition()
        {
            // min height should take into account ground location
            float minH = minHeight;
            if (hasGroundBelow && groundDistance < levelHeight)
            {
                minH = groundHeight + levelHeight;
            }
            
            // keep position within min/max height bounds
            var boundedY = Mathf.Clamp(transform.position.y, minH, maxHeight);
            var boundedPosition = new Vector3(transform.position.x, boundedY, transform.position.z);
            
            // add skimming behavior if there are close objects left/right/forward
            var forwardFlattened = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
            var hasObjectAhead = Physics.Raycast(transform.position, forwardFlattened, out var hitForward,
                skimDistance, groundLayer);
            var hasObjectLeft = Physics.Raycast(transform.position, -transform.right, out var hitLeft,
                skimDistance, groundLayer);
            var hasObjectRight = Physics.Raycast(transform.position, transform.right, out var hitRight,
                skimDistance, groundLayer);
            
            var skimming = false;
            Vector3 newPos = boundedPosition;
            if (hasObjectAhead) // should I check for verticalInput here? and tiltDirection below? [check]
            {
                newPos = boundedPosition + hitDown.normal * (skimDistance - hitForward.distance);
                skimming = true;
            }
            if (groundDistance > skimDistance) // has unwanted shifting behavior if too close to ground
            {
                if (hasObjectLeft)
                {
                    newPos = boundedPosition + hitLeft.normal * (skimDistance - hitLeft.distance);
                    skimming = true;
                } else if (hasObjectRight)
                {
                    newPos = boundedPosition + hitRight.normal * (skimDistance - hitRight.distance);
                    skimming = true;
                }
            }

            if (skimming)
            {
                targetVelocity /= 10f;
                boundedPosition = newPos; // Vector3.Lerp(boundedPosition, newPos, 20f * Time.deltaTime); // smooth is not fast enough
            }

            return boundedPosition;
        }
    }
}
