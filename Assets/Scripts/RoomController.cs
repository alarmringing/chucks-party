using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoomController : MonoBehaviour {

    public static int BEAT_COUNT = 8;

    public string beatFileName;
    public Light partyLight;
    public Color roomColor;

    private ChuckSubInstance chuckSubInstance;
    private float maxIntensity = 15f;
    private float decayDuration = 0.7f;
    private float timeSinceFlickerOnset = 0f;

    private List<PoiController> poisInRoom;
    private bool[] activatedBeats;

    private void Awake() {
        activatedBeats = new bool[BEAT_COUNT]; // Defaults to false.
    }

	private void Start() {
        chuckSubInstance = GetComponent<ChuckSubInstance>();
        Debug.Assert(chuckSubInstance != null);

        poisInRoom = new List<PoiController>();

        partyLight.intensity = 0;
        partyLight.color = roomColor;
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

    public void SetActivationOn(int beat, bool isActivated) {
        if (beat >= activatedBeats.Length) return;
        activatedBeats[beat] = isActivated;
    }

    public bool IsActivatedOn(int beat) {
        return activatedBeats[beat];
    }

    public int GetNumPois() { return poisInRoom.Count; }

    public void TurnOnLight() {
        partyLight.intensity = maxIntensity;
        timeSinceFlickerOnset = 0;
        chuckSubInstance.RunCode(string.Format(@"
                    ""Audio/{0}.wav"" => string fileName;
                    me.sourceDir() + fileName => string filePath;
                    SndBuf buf => PitShift p => dac;
                    filePath => buf.read;

                    1::second => now;
                    ", beatFileName));

        for(int i = 0; i < poisInRoom.Count; i++) {
            if (poisInRoom[i].IsPartying()) {
                chuckSubInstance.RunCode(string.Format(@"
                    ""Audio/bam.wav"" => string fileName;
                    me.sourceDir() + fileName => string filePath;
                    SndBuf buf => PitShift p => dac;
                    filePath => buf.read;

                    float noteToShift;
                    {0} => noteToShift;
                    Math.pow(2, noteToShift / 12) => buf.rate;

                    1::second => now;
                    ", poisInRoom[i].poiNoteOffset));
                poisInRoom[i].Activate();
            }
        }
    }

    public void OneBeatFinished() { // Use this to ask Pois if they will leave.
        for (int i = 0; i < poisInRoom.Count; i++)
        {
            poisInRoom[i].OneBeatFinished();
        }
    }

    void OnTriggerEnter(Collider other) {
        // Add Poi
        if (other.transform.tag == "Poi") {
            poisInRoom.Add(other.transform.GetComponent<PoiController>());
        }
    }

    void OnTriggerExit(Collider other) {
        // Remove Poi
        if (other.transform.tag == "Poi") {
            poisInRoom.Remove(other.transform.GetComponent<PoiController>());
        }
    }
}
