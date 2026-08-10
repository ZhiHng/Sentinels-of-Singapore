/*
* Author: Zhi Hng
* Date: 10 August 2026
* Description: Handles the switching of colliders for traffic lights.
*/

using System.Collections;
using UnityEngine;

public class TrafficLightScript : MonoBehaviour
{
    Animator animator;
    GameObject collider1;
    GameObject collider2;
    float timeForRedLight = 20f;
    void Start()
    {
        animator = GetComponent<Animator>();
        collider1 = transform.GetChild(1).gameObject;
        collider2 = transform.GetChild(0).gameObject;
        collider2.transform.Translate(Vector3.up * 5);
        StartCoroutine(SwitchColliders());
    }
    /// <summary>
    /// Alternates between moving colliders up and down to simulate the switching of traffic lights.
    /// </summary>
    /// <returns></returns>
    IEnumerator SwitchColliders()
    {
        while (true) // Traffic Light animation will be 2 seconds long
        {
            yield return new WaitForSeconds(timeForRedLight);
            animator.SetBool("ChangeLights", true);
            yield return new WaitForSeconds(2f);
            StartCoroutine(MoveColliderUp(collider1));
            StartCoroutine(MoveColliderDown(collider2));

            yield return new WaitForSeconds(timeForRedLight);
            animator.SetBool("ChangeLights", false);
            yield return new WaitForSeconds(2f);
            StartCoroutine(MoveColliderUp(collider2));
            StartCoroutine(MoveColliderDown(collider1));
        }
    }
    IEnumerator MoveColliderUp(GameObject collider)
    {
        for (int i = 0; i < 10; i++)
        {
            collider.transform.Translate(Vector3.up * 0.5f);
            yield return new WaitForSeconds(0.05f); // Wait for 0.5 seconds before moving the collider down
        }
    }
    IEnumerator MoveColliderDown(GameObject collider)
    {
        for (int i = 0; i < 10; i++)
        {
            collider.transform.Translate(Vector3.down * 0.5f);
            yield return new WaitForSeconds(0.05f); // Wait for 0.5 seconds before moving the collider up
        }
    }
}
