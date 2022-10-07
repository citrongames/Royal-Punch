using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    [SerializeField] private float MASKFILL_TIMER;
    [SerializeField] private float _maskFillSpeed;
    private float _maskbarTimer = 0;
    private bool _fillMaskBar = false;
    private bool _startMaskTimer = false;
    private float _startMaskFill = 0;
    private float _endMaskFill = 0;
    private float _maskFillTimer = 0;

    private TextMeshProUGUI _text;
    private Image _bar;
    private Image _maskBar;

    void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();

        Image[] images = GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            if (image.gameObject.name == "Bar")
                _bar = image;
            else if (image.gameObject.name == "MaskBar")
                _maskBar = image;
        }
    }

    void Update()
    {
        DecreaseMaskBar(Time.deltaTime);
    }

    private void DecreaseMaskBar(float deltaTime)
    {        
        if (_startMaskTimer)
        {
            if (_fillMaskBar)
                _maskbarTimer = MASKFILL_TIMER;

            _maskbarTimer += deltaTime;
            if (_maskbarTimer >= MASKFILL_TIMER)
            {
                _fillMaskBar = true;
                _startMaskTimer = false;
                _maskFillTimer = 0;
                _maskbarTimer = 0;
                if (_maskBar != null)
                    _startMaskFill = _maskBar.fillAmount;
            }
        }
        if (_fillMaskBar)
        {
            _maskFillTimer += deltaTime;
            if (_maskBar != null)
                _maskBar.fillAmount = Mathf.Lerp(_startMaskFill, _endMaskFill, _maskFillTimer * _maskFillSpeed);
            
            if (Mathf.Approximately(_maskBar.fillAmount, _bar.fillAmount))
            {
                _fillMaskBar = false;
                _maskFillTimer = 0;
            }
        }
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        _text.text = currentHealth.ToString();

        float normalizedHealth;
        if (currentHealth > 0)
            normalizedHealth = (float)currentHealth / (float)maxHealth;
        else
            normalizedHealth = 0;
        SetNormalizedHealth(normalizedHealth);

        _maskbarTimer = 0;
        _startMaskTimer = true;
        _endMaskFill = normalizedHealth;
    }

    private void SetNormalizedHealth(float health)
    {
        if (_bar != null)
            _bar.fillAmount = health;
    }

    private void SetMaskNormalizedHealth(float health)
    {
        if (_bar != null && _maskBar != null)
            _maskBar.fillAmount = _bar.fillAmount;
    }
}
