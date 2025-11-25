using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public DeckManager deck;
    public FieldManager field;

    public Transform GameOverUI;

    public static UIManager Instance;
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        GameOverUI.gameObject.SetActive(false);
    }

    public void ShowGameOverUi()
    {
        GameOverUI.gameObject.SetActive(true);
    }

    public void RePlayGame()
    {
        field.ClearData();
        deck.ClearData();
        GameOverUI.gameObject.SetActive(false);
}
}
