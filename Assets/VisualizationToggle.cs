// using UnityEngine;
// using UnityEngine.UI;
//
// public class VisualizationToggle : MonoBehaviour
// {
//     [SerializeField] private Button visualizationButton;
//
//     private void Start()
//     {
//         // If using a Button
//         if (visualizationButton != null)
//         {
//             visualizationButton.onClick.AddListener(OnButtonClicked);
//             UpdateButtonText();
//         }
//     }
//
//     private void OnToggleChanged(bool value)
//     {
//         StringVibrationVisualizer.VisualizationEnabled = value;
//     }
//
//     private void OnButtonClicked()
//     {
//         StringVibrationVisualizer.VisualizationEnabled = !StringVibrationVisualizer.VisualizationEnabled;
//         UpdateButtonText();
//     }
//
//     private void UpdateButtonText()
//     {
//         if (visualizationButton != null)
//         {
//             var text = visualizationButton.GetComponentInChildren<Text>();
//             if (text != null)
//             {
//                 text.text = StringVibrationVisualizer.VisualizationEnabled ?
//                     "ON" : "OFF";
//             }
//         }
//     }
//
//     private void OnDestroy()
//     {
//         if (visualizationButton != null)
//             visualizationButton.onClick.RemoveListener(OnButtonClicked);
//     }
// }