using UnityEngine;
using System.Collections;

namespace Bird_FSM
{
    public class NpcStateManager : BirdStateManager<NpcStateManager>
    {
        public Transform playerTransform;

        new void Start()
        {
            base.Start();

            // NPC starts in flying (default animation) + immediately transitions to walking state
            Walking = new NPCWalkingState();
            animator.SetBool(Landing1, true);
            SwitchState(this.Landing);
            currentState = Landing;
            currentState.EnterState(this);
        }

        /// <summary>
        /// For NPC birds (for player, will return 0).
        /// </summary>
        public float GetDistanceToPlayer()
        {
            return Vector3.Distance(playerTransform.position, transform.position);
        }

        /// <returns>Returns a normalized Vector3 pointing from the bird's transform towards the player's
        /// transform.</returns>
        public Vector3 GetDirectionToPlayer()
        {
            return (playerTransform.position - transform.position).normalized;
        }

        /// <summary>
        /// Spawns and plays found particle (attached to SpawnManager script).
        /// </summary>
        /// <param name="pos">Position to spawn particle</param>
        public void SpawnFoundFX(Vector3 pos)
        {
            GameObject foundFx = spawnManager.SpawnFoundFX(pos);
            StartCoroutine(PlayParticleForDuration(foundFx, 2f));
        }

        private IEnumerator PlayParticleForDuration(GameObject fx, float duration)
        {
            yield return new WaitForSeconds(duration);
            ParticleSystem ps = fx.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            ps.Stop();
        }
        
        /// <summary>
        /// Calls FadeBird() coroutine, which makes the bird mesh transparent over 3 seconds, adds crane icon to the UI,
        /// and destroys the GameObject.
        /// </summary>
        public void Fade()
        {
            StartCoroutine(FadeBird());
        }
        
        // requires System.Collections
        private IEnumerator FadeBird()
        {
            float duration = 3f;
            float elapsedTime = 0;
            Material[] materials = GetComponentInChildren<SkinnedMeshRenderer>().materials;
            Color initialColor0 = materials[0].color; // crane body
            Color initialColor1 = materials[1].color; // crane wings
            Color finalColor0 = new Color(initialColor0.r, initialColor0.g, initialColor0.b, 0);
            Color finalColor1 = new Color(initialColor1.r, initialColor1.g, initialColor1.b, 0);

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                materials[0].color = Color.Lerp(initialColor0, finalColor0, t);
                materials[1].color = Color.Lerp(initialColor1, finalColor1, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(1f);

            // add crane icon
            spawnManager.AddCraneIcon(cranesCollected);
            
            // increment global # cranes collected
            cranesCollected++;
            
            // destroy game object
            Destroy(this.gameObject);
        }
    }
}
