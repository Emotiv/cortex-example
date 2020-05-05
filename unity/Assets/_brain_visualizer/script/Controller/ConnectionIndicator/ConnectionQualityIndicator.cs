using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace dirox.emotiv.controller
{
    public class ConnectionQualityIndicator : BaseIndicator
    {
        private float qualityLevel = 0;
        private ConnectedDevice connectedDevice;        
        private Color[] colors;
        private ContactQualityController contactQualityController;
        [SerializeField] private Text qualityLevelText;
        

        public float QualityLevel{ get { return qualityLevel; } }
        [Inject]
        public void SetDependencies(ConnectedDevice device, ContactQualityColorSetting colorsetting, ContactQualityController contactQualityController)
        {
            this.connectedDevice = device;
            this.colors = colorsetting.colors;
            this.contactQualityController = contactQualityController;
        }

        public override void Activate()
        {
            base.Activate();
            StartCoroutine(RunCoroutineDisplayQualityLevel(updateTimeInterval));
        }

        IEnumerator RunCoroutineDisplayQualityLevel(float timeInterval) {
            while(this.IsActive) {
                yield return new WaitForSeconds(timeInterval);
                SetQualityIndicator((int)DataProcessing.Instance.GetCQOverAll());
            }
        }

        public void SetQualityIndicator(int cqOverAll) {
            
            qualityLevel = cqOverAll;

            SetIndicatorDisplay(qualityLevel);
            SetQualityLevelTextDisplay(qualityLevel, qualityLevelText);
            SetQualityLevelTextDisplay(qualityLevel, contactQualityController.displayText, true);
        }

        private void SetQualityLevelTextDisplay(float qualityLevel, Text qualityText, bool withPercentageSuffix = false){
            if (colors == null || colors.Length == 0)				
                return;			

            if (qualityLevel < 20) {
                qualityText.color = colors[1];
            }
            else if (qualityLevel < 40) {
                qualityText.color = colors[2];
            }
            else if (qualityLevel < 60) {
                qualityText.color = colors[3];
            }
            else if (qualityLevel < 80) {
                qualityText.color = colors[4];
            }
            else {
                qualityText.color = colors[5];
            }

            if (!withPercentageSuffix)
                qualityText.fontSize = 16;
            else qualityText.fontSize = 30;

            if (qualityLevel == 100)
                qualityText.fontSize -= 4;

            qualityText.text = withPercentageSuffix ? ((int)qualityLevel).ToString() + "%" : ((int)qualityLevel).ToString();
        }
    }
}