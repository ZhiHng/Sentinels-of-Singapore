/*
* Author: Zhi Hng
* Date: 10 August 2026
* Description: Handles the interaction with player and calls NPC for addition of score
*/

using UnityEngine;

public class FightCloud : MonoBehaviour
{
    NPCScript leader;
    void Update()
    {
        if (leader == null)
        {
            Destroy(gameObject);
        }
    }

    public void Initialise(NPCScript fighter)
    {
        leader = fighter;
    }

    public void Interacted()
    {
        if (leader != null)
        {
            leader.ResolveFight(true);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("Electric Truck"))
        {
            leader.ResolveFight(false);
            GameManager.Instance.AddScore(-40);
        }
    }
}