using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Constants;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] private int currentLevel;

    private GameObject activeImage, lockedImage;
    private Button button;
    private Image buttonImage;

    private void Awake()
    {
        button = GetComponent<Button>();
        lockedImage = transform.GetChild(0).gameObject;
        activeImage = transform.GetChild(1).gameObject;
        TMP_Text countText = GetComponentInChildren<TMP_Text>();
        countText.text = currentLevel.ToString();
        buttonImage = GetComponent<Image>();
    }

    private void Start()
    {
        button.onClick.AddListener(UpdateLevel);
    }

    private void OnEnable()
    {
        int currentStage = PlayerPrefs.GetInt(DATA.CURRENT_STAGE);
        string currentStageName = DATA.CURRENT_STAGE + "_" + currentStage;
        string currentButtonLevelName = currentStageName + "_" + currentLevel;
        int levelActive = PlayerPrefs.HasKey(currentButtonLevelName)
            ? PlayerPrefs.GetInt(currentButtonLevelName)
            : 0;
        if (currentLevel <= 5 && levelActive == 0)
        {
            levelActive = 1;
            PlayerPrefs.SetInt(currentButtonLevelName, levelActive);
        }
        
        lockedImage.SetActive(levelActive == 0);
        activeImage.SetActive(levelActive == 1);
        buttonImage.color = MainMenuManager.instance.colors[currentStage - 1];
    }

    private void UpdateLevel()
    {
        if(lockedImage.activeInHierarchy) return;
        PlayerPrefs.SetInt(DATA.CURRENT_LEVEL, currentLevel);
        SceneManager.LoadScene(1);
    }
}
