﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum PoiSizeType { BigPoi, SmolPoi };
public enum PoiPersonalityType { ShiPoi, NormiPoi, FabPoi };

public class PoiController : MonoBehaviour {

    private PoiSizeType poiType;
    private PoiPersonalityType poiPersonalityType;
    public int poiNoteOffset;
    private int moveRoomAfter;

    public Shader outlineShader;
    public bool showPath = true;

    private Material poiMaterial;
    private Color poiColor = Color.red;
    private float maxOutlineWidth = 0.25f;
    private float outlineDecayDuration = 0.7f;
    private float timeSinceActivation = 0f;

    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private List<RoomController> roomsInScene;
    private LineRenderer lineRenderer;

    private int roundsInSameRoom = 0;
    private int currentRoomIndex = 0;
    private bool isMoving = false;

    private void Start() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        Debug.Assert(navMeshAgent != null);
        animator = GetComponent<Animator>();
        Debug.Assert(animator != null);
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        moveRoomAfter = (int) Random.Range(5, 20);

        if (poiType == PoiSizeType.SmolPoi) {
            transform.localScale *= 0.6f; // Scale smaller.
        }
        
        // Create unique material for each Poi, and make sure it's not saved in filesystem too.
        poiMaterial = new Material(outlineShader);
        poiMaterial.hideFlags = HideFlags.HideAndDontSave;
        poiMaterial.SetVector("_Color", poiColor);
        poiMaterial.SetVector("_OutlineColor", new Vector4(0f, 0f, 0f, 1f));
        poiMaterial.SetFloat("_Outline", 0f);

        Component[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.material = poiMaterial;
        }

        roomsInScene = new List<RoomController>(FindObjectsOfType<RoomController>());
        MoveToNewRoom();
    }

    public void SetPoiProperties(PoiSizeType newPoiSize, int newPoiNote, Vector4 newPoiColor, PoiPersonalityType newPoiPersonality) {
        poiType = newPoiSize;
        poiNoteOffset = newPoiNote;
        if (newPoiSize == PoiSizeType.BigPoi) poiNoteOffset -= 12; // Lower octave is BigPoi.
        poiPersonalityType = newPoiPersonality;
        poiColor = newPoiColor;
    }

    public void OneBeatFinished() {
        if (isMoving) return;

        if (poiPersonalityType == PoiPersonalityType.FabPoi 
            && roomsInScene[currentRoomIndex].GetNumPois() < 2) {
            MoveToNewRoom(); // If FabPoi, then move if no one else is in room.
        } else if (poiPersonalityType == PoiPersonalityType.ShiPoi 
            && roomsInScene[currentRoomIndex].GetNumPois() > 1) {
            MoveToNewRoom(); // If ShiPoi, move if anyone else is in room.
        }

        if (poiPersonalityType != PoiPersonalityType.ShiPoi 
            && roundsInSameRoom > moveRoomAfter) {
            MoveToNewRoom(); // If not ShiPoi, move around!
        }
        roundsInSameRoom++;
    }

    public void Activate() {
        timeSinceActivation = 0;
        poiMaterial.SetVector("_OutlineColor", roomsInScene[currentRoomIndex].roomColor);
        poiMaterial.SetFloat("_Outline", maxOutlineWidth);
        if(animator.GetBool("IsPartying")) {
            animator.Play(animator.GetCurrentAnimatorStateInfo(-1).fullPathHash);
        }
    }

    public bool IsWalking() {
        return animator.GetBool("IsWalking");
    }

    public bool IsPartying() {
        return animator.GetBool("IsPartying");
    }

    private void FadeActivation() {
        if (poiMaterial.GetFloat("_Outline") > 0) {
            float t = Mathf.Pow(Mathf.Clamp01(outlineDecayDuration - timeSinceActivation), 2f);
            poiMaterial.SetFloat("_Outline", Mathf.Lerp(0, maxOutlineWidth, t));
            timeSinceActivation += Time.deltaTime;
        }
    }

    private Vector2 RandomCoordinateInRoom(RoomController newRoom) {
        Bounds roomBounds = newRoom.GetComponent<BoxCollider>().bounds;
        //Vector3 roomExtents = roomBounds.extents;

        float xPos = Random.Range(roomBounds.min.x, roomBounds.max.x);
        float zPos = Random.Range(roomBounds.min.z, roomBounds.max.z);

        return new Vector2(xPos, zPos);
    }

    private Vector3 ClosestPointOnNavMesh(Vector3 target) {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, transform.localScale.y * 2, NavMesh.AllAreas)) {
            return hit.position;
        }
        return target;
    }

    private void MoveToNewRoom() {
        roundsInSameRoom = 0;

        int previousRoomIndex = currentRoomIndex;
        while (currentRoomIndex == previousRoomIndex) { // Choose new room that is not same is previous.
            currentRoomIndex = (int)Random.Range(0, roomsInScene.Count - 0.5f);
        }
        
        RoomController newRoom = roomsInScene[currentRoomIndex];
        Vector2 targetCoordinate = RandomCoordinateInRoom(newRoom);
        Vector3 targetPosition = new Vector3(targetCoordinate.x, 0.5f, targetCoordinate.y);

        navMeshAgent.SetDestination(ClosestPointOnNavMesh(targetPosition));
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsPartying", false);
        isMoving = true;
        poiMaterial.SetVector("_OutlineColor", new Vector4(0f, 0f, 0f, 1f));
        poiMaterial.SetFloat("_Outline", 0f);
    }

    private void NavigationFinishedCheck() {
        if (isMoving) {
            if (!navMeshAgent.pathPending) {
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
                    if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f) {
                        // Party start!
                        animator.SetBool("IsWalking", false);
                        animator.SetBool("IsPartying", true);
                        isMoving = false;
                        transform.LookAt(roomsInScene[currentRoomIndex].transform.position);
                    }
                }
            }
        }
    }

    private void Update() {
        NavigationFinishedCheck();
        FadeActivation();

        if (navMeshAgent.hasPath && showPath) {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = navMeshAgent.path.corners.Length;
            lineRenderer.SetPositions(navMeshAgent.path.corners);
        } else {
            lineRenderer.enabled = false;
        }
    }
}
