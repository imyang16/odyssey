using UnityEngine;

namespace Bird_FSM
{
    public class BirdStateManager<T> : MonoBehaviour where T : BirdStateManager<T>
    {
        protected static readonly int Landing1 = Animator.StringToHash("Landing");
        public GameObject boundingBox;
        public LayerMask groundLayer;
        public Animator animator;
        public GameObject water;

        public SpawnManager spawnManager;
        private BirdBounding boundingScript;
        private BoxCollider boundingBoxCollider;
        private Rigidbody rb;

        private Vector3 currentVelocity = Vector3.zero;
        private float currentTilt; // z (left/right)
        private float currentRotationX; // x (up/down)
        protected static float cranesCollected;

        protected BaseState<T> currentState;
        public LandingState<T> Landing = new LandingState<T>();
        public WalkingState<T> Walking;
        
        public void Start()
        {
            animator = GetComponent<Animator>();
            boundingScript = boundingBox.GetComponent<BirdBounding>(); // this is the PlayerBounding script
            boundingBoxCollider = boundingBox.GetComponent<BoxCollider>(); // this contains the size info
            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            if (currentState != null) currentState.UpdateState(this as T);
        }

        void OnCollisionEnter(Collision collision)
        {
            currentState.OnCollisionEnter(this as T, collision);
        }

        public void SwitchState(BaseState<T> state)
        {
            currentState = state;
            if (state != null) state.EnterState(this as T);
        }

        public BaseState<T> GetCurrentState()
        {
            return currentState;
        }

        /// <returns>Value of bounds exceeded from PlayerBounding script</returns>
        public float GetBoundsExceeded()
        {
            return boundingScript.boundsExceeded; 
        }

        public void SetBoundingBoxColliderSize(Vector3 size)
        {
            boundingBoxCollider.size = size;
        }
        
        public void SetBoundingBoxColliderCenter(Vector3 center)
        {
            boundingBoxCollider.center = center;
        }

        /// <summary>
        /// Gets left/right arrow key or D/A key inputs using Input.GetKey.
        /// </summary>
        /// <returns>1f if right arrow/D and -1f if left arrow/A.</returns>
        public float GetHorizontalInput()
        {
            float horizontalInput = 0f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                horizontalInput = 1f;
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                horizontalInput = -1f;
            }
            return horizontalInput;
        }
        
        public Vector3 GetCurrentVelocity()
        {
            return currentVelocity;
        }

        public void SetCurrentVelocity(Vector3 velocity)
        {
            currentVelocity = velocity;
        }
        
        public float GetCurrentTilt()
        {
            return currentTilt;
        }

        public void SetCurrentTilt(float tilt)
        {
            currentTilt = tilt;
        }

        public float GetCurrentRotationX()
        {
            return currentRotationX;
        }

        public void SetCurrentRotationX(float rotationX)
        {
            currentRotationX = rotationX;
        }
        
        /// <summary>
        /// Sets the "Use Gravity" field of GameObject's Rigidbody.
        /// </summary>
        public void UseGravity(bool value)
        {
            rb.useGravity = value;
        }
    }
}
