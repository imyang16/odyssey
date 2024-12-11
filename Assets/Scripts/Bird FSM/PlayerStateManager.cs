using UnityEngine;
using System.Collections;
using TMPro;

namespace Bird_FSM
{
    public class PlayerStateManager : BirdStateManager<PlayerStateManager>
    {
        public TextMeshPro instructionText;
        
        public bool training;
        public bool firstFly;
        public bool firstLand;
        public bool firstJump;
        
        public enum JumpType
        {
            Short,
            Tall
        };
        private JumpType jumpType;
        
        public FlyingState Flying = new FlyingState();
        public TakingFlightState TakingFlight = new TakingFlightState();
        public JumpingState Jumping = new JumpingState();
        public SwimmingState Swimming = new SwimmingState();
        
        new void Start()
        {
            base.Start();
            Walking = new PlayerWalkingState();
            
            currentState = Flying;
            cranesCollected = 0f;

            // for training mode
            if (training)
            {
                transform.parent.position = new Vector3(16, 10, -650);
            }
            firstFly = true;
            firstLand = true;
            firstJump = true;

            currentState.EnterState(this);
        }

        public JumpType GetJumpType()
        {
            return jumpType;
        }

        public void SetJumpType(JumpType jt)
        {
            this.jumpType = jt;
        }
        
        private void ShowInstruction(string text)
        {
            instructionText.text = text;
            SetInstructionTextAlpha(0);
            StartCoroutine(FadeInOutInstructionText());
        }

        private IEnumerator FadeInOutInstructionText()
        {
            yield return StartCoroutine(FadeInstructionText(1));
            yield return new WaitForSeconds(3f);
            yield return StartCoroutine(FadeInstructionText(0));
        }

        private IEnumerator FadeInstructionText(int type /*1 in, 0 out*/)
        {
            for (float t = 0; t <= 1f; t += Time.deltaTime)
            {
                SetInstructionTextAlpha(type == 1 ? t : 1 - t);
                yield return null;
            }

            SetInstructionTextAlpha(type == 1 ? 1 : 0);
        }

        private void SetInstructionTextAlpha(float alpha)
        {
            Color color = instructionText.color;
            color.a = alpha;
            instructionText.color = color;
        }

        /// <summary>
        /// Calls ShowFlyInstructionText, which sets instruction text (the public TextMeshPro variable attached to this
        /// GameObject) to the flying instructions and smoothly fades the text in/out. For training mode.
        /// </summary>
        public void StartFlyInstructionSequence()
        {
            StartCoroutine(ShowFlyInstructionText());
        }

        private IEnumerator ShowFlyInstructionText()
        {
            SetInstructionTextAlpha(0);
            
            yield return new WaitForSeconds(2f);
            
            instructionText.text = "Use \u2190\u2191\u2192\u2193 to tilt";
            yield return StartCoroutine(FadeInOutInstructionText());
            
            yield return new WaitForSeconds(1f);
            
            instructionText.text = "Hold shift to speed up";
            yield return StartCoroutine(FadeInOutInstructionText());

            instructionText.text = "";
        }
        
        /// <summary>
        /// Shows landing instruction and smoothly fades the text in/out; for player training mode.
        /// </summary>
        public void StartLandInstructionSequence()
        {
            ShowInstruction("Press space to land/take flight");
        }

        /// <summary>
        /// Shows jump instruction and smoothly fades the text in/out; for player training mode.
        /// </summary>
        public void StartJumpInstructionSequence()
        {
            ShowInstruction("Press J to jump");
        }

        /// <summary>
        /// Freezes movement and animation in walking state; for Player.
        /// </summary>
        public void Freeze()
        {
            StartCoroutine(FreezeForSeconds(4f));
        }

        private IEnumerator FreezeForSeconds(float duration)
        {
            Walking.frozen = true;
            yield return new WaitForSeconds(duration);
            Walking.frozen = false;
        }
    }
}
