using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{

	float hitVelocity = 0;

	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.collider.CompareTag("Ball"))
		{
			hitVelocity = other.relativeVelocity.sqrMagnitude;

			if (hitVelocity > 1)
			{
				GetComponent<AudioSource>().Play();
			}
		}
	}
}
