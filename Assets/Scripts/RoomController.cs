using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoomController : MonoBehaviour {

    public static int BEAT_COUNT = 8;

    public Light partyLight;

    private ChuckSubInstance chuckSubInstance;
    private float maxIntensity = 15f;
    private float decayDuration = 0.7f;
    private float timeSinceFlickerOnset = 0f;

    private List<PoiController> poisInRoom;
    private bool[] activatedBeats;

    private void Awake() {
        activatedBeats = new bool[BEAT_COUNT]; // Defaults to false.

        //Debug
        activatedBeats[0] = true;
        activatedBeats[4] = true;
    }

	private void Start() {
        chuckSubInstance = GetComponent<ChuckSubInstance>();
        Debug.Assert(chuckSubInstance != null);

        partyLight.intensity = 0;
	}
	
	private void Update() {
        FadeLight();
    }

    private void FadeLight() {
        if (partyLight.intensity > 0) {
            float t = Mathf.Pow(Mathf.Clamp01(decayDuration - timeSinceFlickerOnset), 2f);
            partyLight.intensity = Mathf.Lerp(0, maxIntensity, t);
            timeSinceFlickerOnset += Time.deltaTime;
        }
    }

    public bool IsActivatedOn(int beat) {
        return activatedBeats[beat];
    }

    public void TurnOnLight() {
        partyLight.intensity = maxIntensity;
        timeSinceFlickerOnset = 0;

        /*
        for(int i = 0; i < poisInRoom.Count; i++) {
            // Signal Each Poi to do something
        }*/
    }

    void OnTriggerEnter(Collider other) {
        // Add Poi
        if (other.transform.tag == "poi") {
            poisInRoom.Add(other.transform.GetComponent<PoiController>());
        }
    }

    void OnTriggerExit(Collider other) {
        // Remove Poi
        if (other.transform.tag == "poi") {
            poisInRoom.Remove(other.transform.GetComponent<PoiController>());
        }
    }
}
