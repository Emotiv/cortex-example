using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace dirox.emotiv.controller {
[RequireComponent (typeof(Image))]
    public class BaseIndicator : BaseCanvasView
    {
        // Green >= 60%, Yellow 20 -> 60%. Red <20%
        [SerializeField] protected Sprite indicatorLevel_0; //<20%
        [SerializeField] protected Sprite indicatorLevel_1; //20-40%
        [SerializeField] protected Sprite indicatorLevel_2; //40-60%
        [SerializeField] protected Sprite indicatorLevel_3; //60-80%
        [SerializeField] protected Sprite indicatorLevel_4; //80-100%

        [SerializeField]
        protected float updateTimeInterval = 0.5f;

        public virtual void SetIndicatorDisplay(float indicatorLevel)
        {
            if(indicatorLevel < 20){
                GetComponent<Image>().sprite = indicatorLevel_0;
            }else if(indicatorLevel < 40){
                GetComponent<Image>().sprite = indicatorLevel_1;
            }else if(indicatorLevel < 60){
                GetComponent<Image>().sprite = indicatorLevel_2;
            }else if(indicatorLevel < 80){
                GetComponent<Image>().sprite = indicatorLevel_3;
            }else{
                GetComponent<Image>().sprite = indicatorLevel_4;
            }
        }
    }
}

