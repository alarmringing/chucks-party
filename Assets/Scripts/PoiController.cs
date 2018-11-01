using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum PoiType { HighPoi, LowPoi };

public class PoiController : MonoBehaviour {

    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private List<RoomController> roomsInScene;

    private bool isMoving = false;

    private void Start() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        Debug.Assert(navMeshAgent != null);
        animator = GetComponent<Animator>();
        Debug.Assert(animator != null);

        roomsInScene = new List<RoomController>(FindObjectsOfType<RoomController>());
        MoveToNewRoom();
    }

    private Vector2 RandomCoordinateInRoom(RoomController newRoom) {
        Bounds roomBounds = newRoom.GetComponent<BoxCollider>().bounds;
        float xOffset = (Random.Range(0, roomBounds.max.x) - roomBounds.max.x / 2) * 0.6f;
        float zOffset = (Random.Range(0, roomBounds.max.z) - roomBounds.max.z / 2) * 0.6f;

        return new Vector2(roomBounds.center.x + xOffset, roomBounds.center.z + zOffset);
    }

    private Vector3 ClosestPointOnNavMesh(Vector3 target) {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, transform.localScale.y * 2, NavMesh.AllAreas)) {
            return hit.position;
        }
        return target;
    }

    private void MoveToNewRoom() {
        RoomController newRoom = roomsInScene[(int) Random.Range(0, roomsInScene.Count - 1)];
        Vector2 targetCoordinate = RandomCoordinateInRoom(newRoom);
        Vector3 targetPosition = new Vector3(targetCoordinate.x, 0.5f, targetCoordinate.y);

        navMeshAgent.SetDestination(ClosestPointOnNavMesh(targetPosition));
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsPartying", false);
    }
	
    private void NavigationFinishedCheck() {
        if (!navMeshAgent.pathPending) {
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
                if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f) {
                    animator.SetBool("IsWalking", false);
                    animator.SetBool("IsPartying", true);
                }
            }
        }
    }

    private void Update() {
        NavigationFinishedCheck();
    }
}
