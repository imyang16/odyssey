using UnityEngine;
using System.Collections;
namespace JSukoAnimals
{
	public class NewRedCrownedCraneCharacterScript : MonoBehaviour
	{
		Animator redCrownedCraneAnimator;
		public float groundCheckDistance = 0.1f;
		public float flyingGroundCheckDistance = .5f;
		public float groundCheckOffset = 0.01f;
		public float maxFlyTurnSpeed = .3f;
		public bool isGrounded;
		public float upDown = 0f;
		public bool soaring = false;
		public bool isFlying = false;
		public float forwardSpeed = 0f;
		public float forward = 0f;
		public float turn = 0f;
		public float maxForwardSpeed = 3f;
		public float meanForwardSpeed = 1.5f;
		public float speedDumpingTime = .1f;
		float timeFromSoar = 0f;
		public float soaringTime = 3f;
		public bool IsLived = true;

		Rigidbody redCrownedCraneRigid;

		void Start()
		{
			redCrownedCraneAnimator = GetComponent<Animator>();
			redCrownedCraneRigid = GetComponent<Rigidbody>();
		}

		void FixedUpdate()
		{
			if (IsLived)
			{
				Move();
				CheckGroundStatus();
				if (soaring)
				{
					timeFromSoar += Time.deltaTime;
					if (timeFromSoar > soaringTime)
					{
						soaring = false;
					}
				}
			}
		}

		public void Attack()
		{
			redCrownedCraneAnimator.SetTrigger("Attack");
		}

		public void Hit()
		{
			redCrownedCraneAnimator.SetTrigger("Hit");
		}

		public void EatStart()
		{
			redCrownedCraneAnimator.SetBool("IsEat", true);
		}

		public void GroomingStart()
		{
			redCrownedCraneAnimator.SetBool("IsGrooming", true);
		}

		public void SleepStart()
		{
			redCrownedCraneAnimator.SetBool("IsSleep", true);
		}

		public void OneLegSleepStart()
		{
			redCrownedCraneAnimator.SetBool("IsOneLegSleep", true);
		}

		public void EatEnd()
		{
			redCrownedCraneAnimator.SetBool("IsEat", false);
		}

		public void GroomingEnd()
		{
			redCrownedCraneAnimator.SetBool("IsGrooming", false);
		}

		public void SleepEnd()
		{
			redCrownedCraneAnimator.SetBool("IsSleep", false);
		}

		public void OneLegSleepEnd()
		{
			redCrownedCraneAnimator.SetBool("IsOneLegSleep", false);
		}

		public void Death()
		{
			IsLived = false;
			redCrownedCraneAnimator.SetBool("IsLived", false);
			redCrownedCraneAnimator.SetTrigger("Death");
		}

		public void Rebirth()
		{
			IsLived = true;
			redCrownedCraneAnimator.SetBool("IsLived", true);
			redCrownedCraneAnimator.SetTrigger("Death");
		}

		public void SitDown()
		{
			redCrownedCraneAnimator.SetBool("IsSitDown", true);
		}

		public void StandUp()
		{
			redCrownedCraneAnimator.SetBool("IsSitDown", false);
		}

		public void Soar()
		{
			if (isGrounded && !soaring && forward > .4f)
			{
				redCrownedCraneAnimator.SetTrigger("Soar");
				redCrownedCraneAnimator.SetBool("Landing", false);
				isFlying = true;
				soaring = true;

				timeFromSoar = 0f;
				redCrownedCraneAnimator.applyRootMotion = false;
				redCrownedCraneRigid.useGravity = false;
				forwardSpeed = forward * 2f;
			}
		}

		void CheckGroundStatus()
		{
			RaycastHit hitInfo;
			if (isFlying)
			{
				isGrounded = Physics.Raycast(transform.position + (Vector3.up * groundCheckOffset), Vector3.down, out hitInfo, flyingGroundCheckDistance);

			}
			else
			{
				isGrounded = Physics.Raycast(transform.position + (Vector3.up * groundCheckOffset), Vector3.down, out hitInfo, groundCheckDistance);
			}


			if (isGrounded)
			{
				if (!soaring)
				{
					redCrownedCraneAnimator.applyRootMotion = true;
					if (isFlying)
					{
						redCrownedCraneAnimator.SetBool("Landing", true);
						isFlying = false;
						redCrownedCraneAnimator.applyRootMotion = true;
						redCrownedCraneRigid.useGravity = true;
					}
				}
			}
			else
			{
				redCrownedCraneAnimator.applyRootMotion = false;
			}
		}

		public void Move()
		{
			redCrownedCraneAnimator.SetFloat("Forward", forward);
			redCrownedCraneAnimator.SetFloat("Turn", turn);
			redCrownedCraneAnimator.SetFloat("UpDown", upDown);
			upDown = Mathf.Lerp(upDown, 0, Time.deltaTime * 3f);

			if (isFlying)
			{
				if (forward < 0f)
				{
					redCrownedCraneRigid.velocity = transform.up * upDown * 2f + transform.forward * forwardSpeed;
				}
				else if (forward > 0f)
				{
					redCrownedCraneRigid.velocity = transform.up * (upDown * 2f + (forwardSpeed - meanForwardSpeed)) + transform.forward * forwardSpeed;
				}
				else
				{
					redCrownedCraneRigid.velocity = transform.up * (upDown * 2f + (forwardSpeed - maxForwardSpeed)) + transform.forward * forwardSpeed;
				}
				transform.RotateAround(transform.position, Vector3.up, Time.deltaTime * turn * 100f);
				forwardSpeed = Mathf.Lerp(forwardSpeed, Mathf.Min(meanForwardSpeed, forwardSpeed), Time.deltaTime * speedDumpingTime);
				forwardSpeed = Mathf.Clamp(forwardSpeed + forward * Time.deltaTime, 0f, maxForwardSpeed);
			}
		}
	}
}