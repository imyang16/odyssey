using UnityEngine;

namespace Bird_FSM
{
    public class NPCWalkingState : WalkingState<NpcStateManager>
    {
        Vector3 waypoint;
        private bool arrivedAtWaypoint;
        private float terrainHeightOffset;
        private bool stuck;
        private bool triedRotating;
        private readonly float stoppingDistance = 4f; // within this close of player, stops moving
        private bool closeToPlayer;

        // Start is called before the first frame update
        public override void EnterState(NpcStateManager npc)
        {
            base.EnterState(npc);

            // set speeds (lower than player speed)
            currentWalkSpeed = 0.4f;
            currentRotSpeed = 0.5f;
            npc.animator.SetFloat(WalkSpeed, 1f);
            
            // randomly move to a starting spot on the terrain
            // RandomlyPlaceOnTerrain(npc);

            terrainHeightOffset = transform.position.y - Terrain.activeTerrain.SampleHeight(transform.position);
            waypoint = GetRandomWaypoint();
        }

        // Update is called once per frame
        public override void UpdateState(NpcStateManager npc)
        {
            base.UpdateState(npc);
            
            // initial activation (once finished transitioning from landing animation)
            ActivateMove(npc);
            
            // stop moving if close to player
            float distToPlayer = npc.GetDistanceToPlayer();
            closeToPlayer = distToPlayer < stoppingDistance;
            move = !closeToPlayer;

            // get new waypoint if necessary
            if (move && (
                    /* 1: close to waypoint */ Vector3.Distance(waypoint, transform.position) < 2f ||
                         /* 2: tried and still can't move */ (triedRotating && !canMoveForward) ||
                         /* 3: in the water */ transform.position.y - minHeight <= 0.3f ||
                         /* 4: waypoint in the water */ waypoint.y < minHeight + 0.3f))
            {
                waypoint = GetRandomWaypoint();
                triedRotating = false;
            }
            
            // check if can move forward - don't let NPCs fall off edges
            UpdateCanMoveForward();
            
            // get horizontal input + update animation
            float tiltDirection = 0f;
            Rotate(npc, distToPlayer);
            Walk(npc, tiltDirection);
            
            // keep above water
            var boundedY = Mathf.Max(transform.position.y, minHeight + 0.2f);
            var boundedPosition = new Vector3(transform.position.x, boundedY, transform.position.z);
            transform.position = boundedPosition;
        }

        private void RandomlyPlaceOnTerrain(NpcStateManager npc)
        {
            // not implemented
        }
        
        private void Rotate(NpcStateManager npc, float distToPlayer)
        {
            // save current position to reset to if near edge/steep slope
            Vector3 lockedPos = npc.transform.position;

            if (move)
            {
                // try rotating
                Quaternion targetRotation = Quaternion.LookRotation((waypoint - transform.position).normalized);
                if (Quaternion.Angle(transform.rotation, targetRotation) > 2f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, currentRotSpeed * Time.deltaTime);
                }
                else
                {
                    triedRotating = true;
                }
            }
            else if (closeToPlayer)
            {
                // face player when close
                Quaternion targetRotation = Quaternion.LookRotation(npc.GetDirectionToPlayer());
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, currentRotSpeed * Time.deltaTime);

                if (Quaternion.Angle(transform.rotation, targetRotation) < 5f || distToPlayer < 2f)
                {
                    // stop NPC animation if facing player or super close
                    canMoveForward = false;
                }
            }
            
            // tilt up/down based on terrain and rotate left/right
            TiltUpDownLand(npc);

            if (isNearEdge || isNearSteepSlope)
            {
                // lock position if near edge to prevent jitters
                npc.transform.position = lockedPos;
            }
        }

        /// <summary>
        /// Initial activation once NPC bird has landed - sets move to true.
        /// </summary>
        private void ActivateMove(NpcStateManager npc)
        {
            if (!move && npc.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Idle")
            {
                move = true;
            }
        }
        
        /// <returns>Random Vector3 waypoint on terrain 10f away.</returns>
        private Vector3 GetRandomWaypoint()
        {
            float radius = 10f;
            Vector3 currWaypoint = Random.onUnitSphere * radius;
            currWaypoint += transform.position;
            currWaypoint.y = Terrain.activeTerrain.SampleHeight(currWaypoint) + terrainHeightOffset;
            return currWaypoint;
        }
        
        public override void OnCollisionEnter(NpcStateManager npc, Collision collision)
        {
        }
    }
}