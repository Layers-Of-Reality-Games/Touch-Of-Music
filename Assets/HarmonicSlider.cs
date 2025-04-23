using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HarmonicSlider : MonoBehaviour
{
    [SerializeField] private BaseString baseString;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI valueText;

    public void Start()
    {
        slider.minValue = 1;
        slider.maxValue = 10;
        slider.wholeNumbers = true;
        slider.value = baseString.HarmonicsCount;
        valueText.text = "Harmonics: " + Mathf.RoundToInt(slider.value);
    }

    public void OnValueChanged()
    {
        baseString.OnChangeHarmonicsCount(Mathf.RoundToInt(slider.value));
        valueText.text = "Harmonics: " + Mathf.RoundToInt(slider.value);
    }
}