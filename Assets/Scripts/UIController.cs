using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UIController : MonoBehaviour {

    public Camera mainCamera;
    public RoomUIController roomUIController;
    public Button quitButton;
    public Button addPoiButton;
    public Button randomPoiButton;
    public PiUIManager piUIManager;

    public Transform spawnPoint;
    public GameObject poiPrefab;
    public ParticleSystem poofEffectParticle;

    private RoomController selectedRoomController;

    private PiUI poiSizeMenu;
    private PiUI poiPersonalityMenu;
    private PiUI poiNoteMenu;

    private PoiSizeType nextPoiSize;
    private PoiPersonalityType nextPoiPersonality;
    private int nextPoiNote;
    private Color nextPoiColor;

    private bool isPoiMenuOpen = false;

	private void Start () {
        roomUIController.GetComponent<CanvasGroup>().alpha = 0;

        poiSizeMenu = piUIManager.GetPiUIOf("Poi Size Menu");
        poiPersonalityMenu = piUIManager.GetPiUIOf("Poi Personality Menu");
        poiNoteMenu = piUIManager.GetPiUIOf("Poi Note Menu");
        EditPoiMenus();

        quitButton.onClick.AddListener(Application.Quit);
        addPoiButton.onClick.AddListener(ShowPoiPersonalityMenu);
        randomPoiButton.onClick.AddListener(delegate { SpawnNewPoi(null, true); });
    }

    private void EditPoiMenus() {
        // Poi Personality Menu
        poiPersonalityMenu.openTransition = PiUI.TransitionType.ScaleAndFan;
        poiPersonalityMenu.closeTransition = PiUI.TransitionType.ScaleAndFan;
        foreach (PiUI.PiData poiPersonalityData in poiPersonalityMenu.piData) {
            poiPersonalityData.onSlicePressed.AddListener(delegate { ShowPoiSizeMenu(poiPersonalityData); });
        }

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
            poiNoteData.onSlicePressed.AddListener(delegate { SpawnNewPoi(poiNoteData, false); });
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

    private void ShowPoiPersonalityMenu() {
        isPoiMenuOpen = true;
        poiPersonalityMenu.OpenMenu(new Vector2(Screen.width / 2f, Screen.height / 2f));
    }

    private void ShowPoiSizeMenu(PiUI.PiData poiPersonalityData) {
        switch (poiPersonalityData.sliceLabel)
        {
            case ("Shi Poi"):
                nextPoiPersonality = PoiPersonalityType.ShiPoi;
                break;
            case ("Normi Poi"):
                nextPoiPersonality = PoiPersonalityType.NormiPoi;
                break;
            case ("Fab Poi"):
                nextPoiPersonality = PoiPersonalityType.FabPoi;
                break;
        }
        poiPersonalityMenu.CloseMenu();
        poiSizeMenu.OpenMenu(new Vector2(Screen.width / 2f, Screen.height / 2f));
    }

    private void ShowPoiNoteMenu(PiUI.PiData poiSizeData)
    {
        switch (poiSizeData.sliceLabel)
        {
            case ("Big Poi"):
                nextPoiSize = PoiSizeType.BigPoi;
                break;
            case ("Smol Poi"):
                nextPoiSize = PoiSizeType.SmolPoi;
                break;
        }
        poiSizeMenu.CloseMenu();
        poiNoteMenu.OpenMenu(new Vector2(Screen.width / 2f, Screen.height / 2f));
    }

    private void SpawnNewPoi(PiUI.PiData poiNoteData, bool isRandom) {
        if (!isRandom) {
            if (!isPoiMenuOpen) return;

            poiNoteMenu.CloseMenu();
            isPoiMenuOpen = false;
        } else {
            if (Random.value > 0.5f) nextPoiSize = PoiSizeType.BigPoi;
            else nextPoiSize = PoiSizeType.SmolPoi;

            float personalityChance = Random.value;
            if (Random.value < 0.3f) nextPoiPersonality = PoiPersonalityType.ShiPoi;
            else if (Random.value < 0.6f) nextPoiPersonality = PoiPersonalityType.NormiPoi;
            else nextPoiPersonality = PoiPersonalityType.FabPoi;

            int randomNoteIndex = (int)Random.Range(0, poiNoteMenu.piData.Length - 0.5f);
            poiNoteData = poiNoteMenu.piData[randomNoteIndex];
        }
        nextPoiNote = poiNoteData.order;
        nextPoiColor = poiNoteData.highlightedColor;

        poofEffectParticle.Play();
        spawnPoint.GetComponent<AudioSource>().Play();
        GameObject newPoi = Instantiate(poiPrefab, spawnPoint.position, Quaternion.identity);
        newPoi.GetComponent<PoiController>().SetPoiProperties(nextPoiSize, nextPoiNote, nextPoiColor, nextPoiPersonality);
    }

    private void ShowRoomUI() {
        if (!mainCamera.enabled) return;
        RaycastHit hit;
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100f, 1 << LayerMask.NameToLayer("Room"))) {
            selectedRoomController = hit.collider.GetComponent<RoomController>();
            roomUIController.GetComponent<CanvasGroup>().alpha = 1;
        }
        else { // Cursor is not over any room
            roomUIController.GetComponent<CanvasGroup>().alpha = 0;
        }
        if (selectedRoomController) {
            Vector2 selectedRoomCoord = mainCamera.WorldToScreenPoint(selectedRoomController.transform.position); // Drops z.
            roomUIController.Activate(selectedRoomController, selectedRoomCoord);
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
                if (poiPersonalityMenu.openedMenu) poiPersonalityMenu.CloseMenu();
                isPoiMenuOpen = false;
            }
        }
    }
}
