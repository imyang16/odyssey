using UnityEngine;
using System.Collections;
namespace JSukoAnimals
{
	public class NewRedCrownedCraneUserControllerScript : MonoBehaviour
	{
		NewRedCrownedCraneCharacterScript redCrownedCraneCharacter;
		public float upDownInputSpeed = 3f;
		float forwardMultiplier = .25f;
		float forwardSpeed = .25f;

		void Start()
		{
			redCrownedCraneCharacter = GetComponent<NewRedCrownedCraneCharacterScript>();
		}

		void Update()
		{
			if (Input.GetButtonDown("Fire1"))
			{
				redCrownedCraneCharacter.Attack();
			}

			if (Input.GetButtonDown("Jump"))
			{
				redCrownedCraneCharacter.Soar();
			}

			if (Input.GetKeyDown(KeyCode.H))
			{
				redCrownedCraneCharacter.Hit();
			}

			if (Input.GetKeyDown(KeyCode.K))
			{
				redCrownedCraneCharacter.Death();
			}

			if (Input.GetKeyDown(KeyCode.R))
			{
				redCrownedCraneCharacter.Rebirth();
			}

			if (Input.GetKeyUp(KeyCode.LeftShift))
			{
				forwardSpeed = .25f;
			}

			if (Input.GetKeyDown(KeyCode.LeftShift))
			{
				forwardSpeed = 1f;
			}

			if (Input.GetKeyDown(KeyCode.B))
			{
				redCrownedCraneCharacter.SitDown();
			}

			if (Input.GetKeyUp(KeyCode.B))
			{
				redCrownedCraneCharacter.StandUp();
			}

			if (Input.GetKeyDown(KeyCode.E))
			{
				redCrownedCraneCharacter.EatStart();
			}

			if (Input.GetKeyUp(KeyCode.E))
			{
				redCrownedCraneCharacter.EatEnd();
			}

			if (Input.GetKeyDown(KeyCode.G))
			{
				redCrownedCraneCharacter.GroomingStart();
			}

			if (Input.GetKeyUp(KeyCode.G))
			{
				redCrownedCraneCharacter.GroomingEnd();
			}

			if (Input.GetKeyDown(KeyCode.O))
			{
				redCrownedCraneCharacter.OneLegSleepStart();
			}

			if (Input.GetKeyUp(KeyCode.O))
			{
				redCrownedCraneCharacter.OneLegSleepEnd();
			}

			if (Input.GetKeyDown(KeyCode.M))
			{
				redCrownedCraneCharacter.SleepStart();
			}

			if (Input.GetKeyUp(KeyCode.M))
			{
				redCrownedCraneCharacter.SleepEnd();
			}

			if (Input.GetKey(KeyCode.N))
			{
				redCrownedCraneCharacter.upDown = Mathf.Clamp(redCrownedCraneCharacter.upDown - Time.deltaTime * upDownInputSpeed, -1f, 1f);
			}

			if (Input.GetKey(KeyCode.U))
			{
				redCrownedCraneCharacter.upDown = Mathf.Clamp(redCrownedCraneCharacter.upDown + Time.deltaTime * upDownInputSpeed, -1f, 1f);
			}
		}

		private void FixedUpdate()
		{
			forwardMultiplier = Mathf.Lerp(forwardMultiplier, forwardSpeed, Time.deltaTime);

			redCrownedCraneCharacter.turn = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");
			redCrownedCraneCharacter.forward = v * forwardMultiplier;
			if (v < .1f)
			{
				forwardSpeed = .25f;
				forwardMultiplier = .25f;
			}
		}
	}
}
