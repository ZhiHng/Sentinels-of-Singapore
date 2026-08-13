/*
* Author: Zhi Hng
* Date: 13 August 2026
* Description: Handles the marking of NPCs
*/

using System.Collections;
using UnityEngine;

public class BeaconScript : MonoBehaviour
{
    GameObject objectToLink;
    public void LinkToGameObject(GameObject gameObject)
    {
        objectToLink = gameObject;
        gameObject.GetComponent<NPCScript>().isTrackedScript = this;
        GameManager.Instance.BroadcastMessage("Suspect has been identified");
    }
    public void Despawn()
    {
        print("escaped");
        GameManager.Instance.BroadcastMessage("Suspect has escaped");
        GameManager.Instance.AddScore(-20);
    }
    void Update()
    {
        if (objectToLink != null)
        {
            transform.position = objectToLink.transform.position;
        }
        else
        {
            StartCoroutine(DelayDelete());
        }
    }
    IEnumerator DelayDelete()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }
}
