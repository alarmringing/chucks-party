using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class RoomUIController : MonoBehaviour {

    private RoomController roomController;
    private List<Toggle> toggles;

	private void Start() {
        toggles = new List<Toggle>(GetComponentsInChildren<Toggle>());

        for (int i = 0; i < toggles.Count; i++) {
            Toggle thisToggle = toggles[i];
            thisToggle.onValueChanged.AddListener(delegate {
                ToggleValueChanged(thisToggle);
            });
        }
    }

    public void Activate(RoomController mouseOverRoomController, Vector2 coord) {
        // Show this UI over the center of the room
        GetComponent<RectTransform>().anchoredPosition = coord;

        if (mouseOverRoomController == roomController) return;
        roomController = mouseOverRoomController;
        for (int i = 0; i < toggles.Count; i++) {
            toggles[i].isOn = roomController.IsActivatedOn(i);
        }
    }

    //Output the new state of the Toggle into Text
    private void ToggleValueChanged(Toggle toggle) {
        int toggleId = toggles.IndexOf(toggle);
        roomController.SetActivationOn(toggleId, toggle.isOn);
    }
}
