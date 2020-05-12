using UnityEngine;
using UnityEngine.UI;
using EmotivUnityPlugin;

namespace dirox.emotiv.controller
{
    public class ContactQualityNodeView
    {
        private Image display;
        private Color[] colors;

        public ContactQualityNodeView SetDisplay (Image display)
        {
            this.display = display;
            return this;
        }

        public ContactQualityNodeView SetColors (Color[] colors)
        {
            this.colors = colors;
            return this;
        }

        /// <summary>
        /// Call this to set the quality color for the corresponding nodes
        /// </summary>
        /// <param name="quality">Quality.</param>
        public void SetQuality (ContactQualityValue quality)
        {
            if (display != null) {
                int ordinal = (int)quality;
                if (ordinal > colors.Length - 1)
                    ordinal = colors.Length - 1;
                display.color = colors [ordinal];
            }
        }
    }
}