using UnityEngine;
using Zenject;
using EmotivUnityPlugin;

namespace dirox.emotiv.controller {
    public class ContactQualityBaseManager : MonoBehaviour
    {
        [Inject]
        public void InjectDependencies(ContactQualityColorSetting colorSettings)
        {
            SetColorSettingForNodes(colorSettings.colors);
        }

        protected virtual void SetColorSettingForNodes(Color[] allColors)
        {
        }

        public virtual void SetContactQualityColor(ContactQualityValue[] contacts)
        {
        }
    }
}

