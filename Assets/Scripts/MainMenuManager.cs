using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Constants;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager instance;
    [SerializeField] private GameObject mainPanel, stagePanel, levelPanel, imgActiveSound;
    public Button btnPlay, btnQuit, btnSound, btnBackToMain, btnBackToStage;
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private Image stageColorInLevelPanel;
    
    public List<Color> colors;
    private void Awake()
    {
        // MakeSingleton();
        instance = this;
        mainPanel.SetActive(true);
        stagePanel.SetActive(false);
    }

    private void Start()
    {
        AudioManager.instance.AddButtonSound();
        btnPlay.onClick.AddListener(OnClickBtnPlay);
        btnQuit.onClick.AddListener(OnClickBtnQuit);
        btnBackToMain.onClick.AddListener(OnClickBtnBackToMain);
        btnBackToStage.onClick.AddListener(OnClickBtnBackToStage);
        btnSound.onClick.AddListener(ToggleSound);
    }

    private void OnClickBtnPlay()
    {
        mainPanel.SetActive(false);
        stagePanel.SetActive(true);
    }

    private void OnClickBtnQuit()
    {
// #if UNITY_EDITOR
//         UnityEditor.EditorApplication.isPlaying = false;
// #endif
        Application.Quit();
        Debug.Log("QUIT");
    }


    private void OnClickBtnBackToMain()
    {
        mainPanel.SetActive(true);
        stagePanel.SetActive(false);
        levelPanel.SetActive(false);
    }

    private void OnClickBtnBackToStage()
    {
        levelPanel.SetActive(false);
        stagePanel.SetActive(true);
    }

    public void ClickedStage()
    {
        stagePanel.SetActive(false);

        int currentStage = PlayerPrefs.GetInt(DATA.CURRENT_STAGE);
        stageText.text = $"STAGE {currentStage}";
        
        levelPanel.SetActive(true);
        stageColorInLevelPanel.color = colors[currentStage - 1];
    }

    public void ToggleSound()
    {
        bool sound = PlayerPrefs.GetInt(DATA.SETTINGS_SOUND) == 0;
        PlayerPrefs.SetInt(DATA.SETTINGS_SOUND, sound ? 1 : 0);
        imgActiveSound.SetActive(!sound);
        
        AudioManager.instance.ToggleSound();
    }
}