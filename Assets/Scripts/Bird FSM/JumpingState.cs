using UnityEngine;
using System.Collections;
using Vector3 = UnityEngine.Vector3;

namespace Bird_FSM
{
    [System.Serializable]
    public class JumpingState : PlayerWalkingState
    {
        private float elapsedTime;
        private Vector3 startPos;
        private Vector3 jumpDir;
        private float jumpTime = 1f;
        
        public override void EnterState(PlayerStateManager player)
        {
            transform = player.transform;
            player.UseGravity(false);

            elapsedTime = 0f;
            startPos = transform.position;
            jumpDir = 1.1f*(transform.forward + Vector3.up);

            if (player.GetJumpType() == PlayerStateManager.JumpType.Short)
            {
                jumpDir *= 0.6f;
            }
            
            player.StartCoroutine(Jump(player));
        }

        public override void UpdateState(PlayerStateManager player)
        {
            // move up and forward
        }

        private IEnumerator Jump(PlayerStateManager player)
        {
            while (elapsedTime < jumpTime)
            {
                elapsedTime += Time.deltaTime;
                
                // first half move up
                if (elapsedTime < 0.5f * jumpTime)
                {
                    player.transform.position = Vector3.Lerp(startPos, startPos + Vector3.up, elapsedTime / (0.5f * jumpTime));
                }
                // second half move transform.forward
                else
                {
                    player.transform.position = Vector3.Lerp(startPos + Vector3.up, startPos + jumpDir,
                        (elapsedTime - 0.5f * jumpTime) / (0.5f * jumpTime));
                }
                yield return null;
            }
            player.animator.SetBool(Jump1, false);
            player.UseGravity(true);
        }
        
        public override void OnCollisionEnter(PlayerStateManager player, Collision collision) {
            player.SwitchState(player.Walking);
        }
    }
}
