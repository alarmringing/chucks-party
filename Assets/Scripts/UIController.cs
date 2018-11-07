using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UIController : MonoBehaviour {

    public Camera mainCamera;
    public RoomUIController roomUIController;
    public Button addPoiButton;
    public PiUIManager piUIManager;

    public Transform spawnPoint;
    public GameObject poiPrefab;
    public ParticleSystem poofEffectParticle;

    private PiUI poiSizeMenu;
    private PiUI poiNoteMenu;

    private PoiType nextPoiSize;
    private int nextPoiNote;
    private Color nextPoiColor;

    private bool isPoiMenuOpen = false;

	private void Start () {
        roomUIController.GetComponent<CanvasGroup>().alpha = 0;

        poiSizeMenu = piUIManager.GetPiUIOf("Poi Size Menu");
        poiNoteMenu = piUIManager.GetPiUIOf("Poi Note Menu");
        EditPoiMenus();

        addPoiButton.onClick.AddListener(ShowPoiSizeMenu);
    }

    private void EditPoiMenus() {
        // Poi Size Menu
        poiSizeMenu.openTransition = PiUI.TransitionType.ScaleAndFan;
        poiSizeMenu.closeTransition = PiUI.TransitionType.ScaleAndFan;
        foreach (PiUI.PiData poiSizeData in poiSizeMenu.piData) {
            poiSizeData.onSlicePressed.AddListener(delegate { ShowPoiNoteMenu(poiSizeData); });
        }

        // Poi Note Menu
        poiNoteMenu.openTransition = PiUI.TransitionType.ScaleAndFan;
        poiNoteMenu.closeTransition = PiUI.TransitionType.ScaleAndFan;
        foreach (PiUI.PiData poiNoteData in poiNoteMenu.piData) {
            poiNoteData.onSlicePressed.AddListener(delegate { SpawnNewPoi(poiNoteData); });
        }

        // Edit colors
        poiNoteMenu.syncColors = false;
        float H = 0f;
        float S = 0.8f;
        float V = 0.7f;
        float HIncrement = (float) 1 / poiNoteMenu.piData.Length;

        // Color each pie based on a rainbow scheme
        for (int i = 0; i < poiNoteMenu.piData.Length; i++) {
            PiUI.PiData data = poiNoteMenu.piData[i];
            poiNoteMenu.piData[i].nonHighlightedColor = Color.HSVToRGB(H % 1, S, V);
            poiNoteMenu.piData[i].highlightedColor = Color.HSVToRGB(H % 1, 1f, 1f);
            H += HIncrement;
        }
        poiNoteMenu.UpdatePiUI();
    }
    
    private void ShowPoiSizeMenu() {
        isPoiMenuOpen = true;
        poiSizeMenu.OpenMenu(new Vector2(Screen.width / 2f, Screen.height / 2f));
    }

    private void ShowPoiNoteMenu(PiUI.PiData poiSizeData) {
        switch (poiSizeData.sliceLabel) {
            case ("Big Poi"):
                nextPoiSize = PoiType.BigPoi;
                break;
            case ("Smol Poi"):
                nextPoiSize = PoiType.SmolPoi;
                break;
        }
        poiSizeMenu.CloseMenu();
        poiNoteMenu.OpenMenu(new Vector2(Screen.width / 2f, Screen.height / 2f));
    }

    private void SpawnNewPoi(PiUI.PiData poiNoteData) {
        if (!isPoiMenuOpen) return;

        poiNoteMenu.CloseMenu();
        isPoiMenuOpen = false;
        nextPoiNote = poiNoteData.order;
        nextPoiColor = poiNoteData.highlightedColor;

        poofEffectParticle.Play();
        spawnPoint.GetComponent<AudioSource>().Play();
        GameObject newPoi = Instantiate(poiPrefab, spawnPoint.position, Quaternion.identity);
        newPoi.GetComponent<PoiController>().SetPoiProperties(nextPoiSize, nextPoiNote, nextPoiColor);
    }
	
    private void ShowRoomUI() {
        if (!mainCamera.enabled) return;
        RaycastHit hit;
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100f, 1 << LayerMask.NameToLayer("Room"))) {
            RoomController hitRoomController = hit.collider.GetComponent<RoomController>();
            Vector2 hitRoomCoord = mainCamera.WorldToScreenPoint(hitRoomController.transform.position); // Drops z.
            roomUIController.GetComponent<CanvasGroup>().alpha = 1;
            roomUIController.Activate(hitRoomController, hitRoomCoord);
        }
        else { // Cursor is not over any room
            roomUIController.GetComponent<CanvasGroup>().alpha = 0;
        }
    }

	private void Update () {
        if (!isPoiMenuOpen) {
            ShowRoomUI();
        } else {
            roomUIController.GetComponent<CanvasGroup>().alpha = 0;
            // Close pi menu if outside the pi is clicked.
            if (Input.GetMouseButtonDown(0) && !piUIManager.OverAMenu()) {
                if (poiSizeMenu.openedMenu) poiSizeMenu.CloseMenu();
                if (poiNoteMenu.openedMenu) poiNoteMenu.CloseMenu();
                isPoiMenuOpen = false;
            }
        }
    }
}
