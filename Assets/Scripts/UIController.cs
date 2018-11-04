using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour {

    public Camera camera;
    public RoomUIController roomUIController;

	void Start () {
		
	}
	
	void Update () {
        if (camera.enabled) {
            RaycastHit hit;
            var ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100f, 1 << LayerMask.NameToLayer("Room"))) {
                RoomController hitRoomController = hit.collider.GetComponent<RoomController>();
                Vector2 hitRoomCoord = camera.WorldToScreenPoint(hitRoomController.transform.position); // Drops z.
                roomUIController.GetComponent<CanvasGroup>().alpha = 1;
                roomUIController.Activate(hitRoomController, hitRoomCoord);
            } else { // Cursor is not over any room
                roomUIController.GetComponent<CanvasGroup>().alpha = 0;
            }
        }
    }
}
