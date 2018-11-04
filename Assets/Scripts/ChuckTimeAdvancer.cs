using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuckTimeAdvancer : MonoBehaviour {

    private float timeStep = 0.2f;

    private ChuckSubInstance chuckSubInstance;
    private ChuckEventListener myAdvancerListener;
    private ChuckFloatSyncer myFloatSyncer;
    private int timeStepCount = 0;

    private List<RoomController> roomsInScene;

    private void Start() {
        chuckSubInstance = gameObject.GetComponent<ChuckSubInstance>();
        myFloatSyncer = gameObject.AddComponent<ChuckFloatSyncer>();
        myAdvancerListener = gameObject.AddComponent<ChuckEventListener>();

        roomsInScene = new List<RoomController>(FindObjectsOfType<RoomController>());
        StartChuckTimer();
    }

    private void StartChuckTimer() {
        chuckSubInstance.RunCode(@"
			1 => global float timeStep;
			global Event notifier;
			
			while( true )
			{
				notifier.broadcast();
				timeStep::second => now;
			}
		");

        myAdvancerListener.ListenForEvent(chuckSubInstance, "notifier", TimeStepDone);
    }

    public void TimeStepDone() {
        TriggerRooms();
        timeStepCount++;
    }

    private void TriggerRooms() {
        foreach (RoomController room in roomsInScene)
        {
            if(room.IsActivatedOn(timeStepCount % RoomController.BEAT_COUNT)) {
                room.TurnOnLight();
            }
            if (timeStepCount % RoomController.BEAT_COUNT == 0) {
                room.OneBeatFinished();
            }
        }
    }

    public void ChangeTimeStep(float newTimeStep) {
        timeStep = newTimeStep;
    }
	
	private void Update() {
        chuckSubInstance.SetFloat("timeStep", timeStep);
    }
}
