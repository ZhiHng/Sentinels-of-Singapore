/*
* Author: Zhi Hng
* Date: 1 August 2026
* Description: Logic for the NPC cars.
*/

using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NPCCarScript : MonoBehaviour
{
    NavMeshAgent agent;
    NavMeshLink activeLink;   // keep one link
    Coroutine turnCoroutine;
    bool traversing = false;
    public float gapLength = 8f;   // how far forward the link extends
    public float linkWidth = 8f;   // how wide the link is

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;
    }

    void Update()
    {
        if (turnCoroutine != null)
            return;

        Vector3 forwardDest = transform.position + transform.forward * 15f;
        Vector3 furtherDest = transform.position + transform.forward * 20f;

        NavMeshPath path = new NavMeshPath();
        bool hasPath = agent.CalculatePath(forwardDest, path);

        if (hasPath && path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(forwardDest);
        }
        else
        {
            hasPath = agent.CalculatePath(furtherDest, path);

            if (hasPath && path.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(furtherDest);
            }
            else if (hasPath && path.status == NavMeshPathStatus.PathPartial)
            {
                // Gap detected → spawn/update one link
                Vector3 dest = agent.destination;
                Vector3 start = dest - transform.forward * 1f;
                Vector3 end = start + transform.forward * gapLength;
                CreateOrUpdateLink(start, end);
            }
            else if (turnCoroutine == null)
            {
                print("dead end");
                int leftOrRight = Random.Range(0, 2);
                turnCoroutine = StartCoroutine(Turn(leftOrRight));
            }
        }
        if (agent.isOnOffMeshLink && !traversing)
        {
            traversing = true;
            StartCoroutine(TraverseLink());
        }
    }

    void CreateOrUpdateLink(Vector3 start, Vector3 end)
    {
        // Snap both ends to NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(start, out hit, 2f, NavMesh.AllAreas))
            start = hit.position;
        if (NavMesh.SamplePosition(end, out hit, 2f, NavMesh.AllAreas))
            end = hit.position;

        if (activeLink == null)
        {
            GameObject linkObj = new GameObject("TempLink");
            activeLink = linkObj.AddComponent<NavMeshLink>();
        }

        // Place at midpoint
        Vector3 midPoint = (start + end) * 0.5f;
        activeLink.transform.position = midPoint;

        // Local offsets
        activeLink.startPoint = start - midPoint;
        activeLink.endPoint = end - midPoint;

        activeLink.width = linkWidth;
        activeLink.bidirectional = false;
        activeLink.agentTypeID = -1372625422;
        // Cleanup after traversal
        StartCoroutine(RemoveLinkAfterUse());
    }

    IEnumerator RemoveLinkAfterUse()
    {
        // Wait until agent enters and exits the link
        yield return new WaitUntil(() => agent.isOnOffMeshLink);
        yield return new WaitUntil(() => !agent.isOnOffMeshLink);

        if (activeLink != null)
        {
            Destroy(activeLink.gameObject);
            activeLink = null;
        }
    }
    // Code to check if one side is deadend, then turn left or right accordingly
    IEnumerator Turn(int leftOrRight)
    {
        // Decide direction relative to car's current rotation
        Vector3 offset;
        if (leftOrRight == 0)
            offset = -transform.right * 4f;   // left relative to car
        else
            offset = transform.right * 4f;    // right relative to car

        // New destination is current position plus offset
        Vector3 newDest = transform.position + offset;
        agent.SetDestination(newDest);

        // Wait until agent reaches destination
        while (agent.pathPending || agent.remainingDistance > 0.1f)
            yield return null;

        SnapToWorldAxes();  // Snap rotation to nearest 90°
        turnCoroutine = null;
    }
    void SnapToWorldAxes()
    {
        // Get current Y rotation
        float yRot = transform.eulerAngles.y;

        // Round to nearest 90°
        float snappedY = Mathf.Round(yRot / 90f) * 90f;

        // Apply snapped rotation
        transform.rotation = Quaternion.Euler(0f, snappedY, 0f);
    }
    IEnumerator TraverseLink()
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 start = agent.transform.position;
        Vector3 end = data.endPos;

        // 🔹 Preserve car’s current Y height
        float fixedY = transform.position.y;
        start.y = fixedY;
        end.y = fixedY;

        float duration = 3f;  // fixed crossing time
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            agent.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        agent.CompleteOffMeshLink();
        traversing = false;
    }
}
