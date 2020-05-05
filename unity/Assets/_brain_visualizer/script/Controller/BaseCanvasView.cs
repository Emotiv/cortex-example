using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;

namespace dirox.emotiv.controller
{
    [RequireComponent (typeof(CanvasGroup))]
    public abstract class BaseCanvasView : MonoBehaviour
    {
        [SerializeField]
        protected bool ShowInStartUp = false;
        protected bool isActive = false;
        [SerializeField] protected float FADE_IN_TIME  = 0.5f;
        [SerializeField] protected float FADE_OUT_TIME = 0.5f;
        private CanvasGroup canvasGroup;

        public bool IsActive{ get{ return isActive; } }
        public bool LastOpen{ get; set; }
        void Awake ()
        {
            canvasGroup = GetComponent<CanvasGroup> ();
        }

        void Start ()
        {
            canvasGroup.alpha          = ShowInStartUp ? 1 : 0;
            canvasGroup.interactable   = ShowInStartUp;
            canvasGroup.blocksRaycasts = ShowInStartUp;
            isActive = ShowInStartUp;
            OnStart ();
        }

        protected virtual void OnStart ()
        {
        }

        public virtual void Activate (Action onComplete = null)
        {
            AnimateActivate (onComplete);
        }

        public virtual void Activate()
        {
            AnimateActivate();
        }

        public virtual void Deactivate (Action onComplete = null)
        {
            AnimateDeactivate (onComplete);
        }

        public virtual void Deactivate()
        {
            AnimateDeactivate();
        }

        protected virtual void AnimateActivate (Action onComplete = null)
        {
            if (canvasGroup.alpha < 0.1f) {
                canvasGroup.DOFade (1, FADE_IN_TIME).OnComplete (() => {
                    makeItInteractable (true);
                    if (onComplete != null)
                        onComplete.Invoke ();
                });
            } else {
                canvasGroup.alpha = 1;
                makeItInteractable (true);
                if (onComplete != null)
                    onComplete.Invoke ();
            }
            isActive = true;
        }

        protected virtual void AnimateActivateOnly ()
        {
            if (canvasGroup.alpha < 0.1f) {
                canvasGroup.DOFade (1, FADE_IN_TIME);
            } else {
                canvasGroup.alpha = 1;
            }
        }

        protected void makeItInteractable ()
        {
            makeItInteractable (true);
        }

        protected void makeItInteractable (bool enabled)
        {
            canvasGroup.interactable   = enabled;
            canvasGroup.blocksRaycasts = enabled;
        }

        protected virtual void AnimateDeactivate (Action onComplete = null)
        {			
            if (canvasGroup.alpha > 0.9f)
                canvasGroup.DOFade (0, FADE_OUT_TIME).OnComplete(()=> {
                    makeItInteractable(false);
                    if (onComplete != null)
                        onComplete.Invoke();
                });
            else {
                canvasGroup.alpha = 0;
                makeItInteractable(false);
                if (onComplete != null)
                    onComplete.Invoke();
            }
            isActive = false;
        }
    }
}