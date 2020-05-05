using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace dirox.emotiv.controller
{
    [RequireComponent (typeof(CanvasGroup))]
    public abstract class BaseMultipleElementView : BaseCanvasView
    {
        public event Action onEnter;
        public event Action onDisplayComplete;
        public event Action onExit;

        public Action<int> broadcastActivation;

        private int location = -1;

        public void SetPosition (int idx)
        {
            this.location = idx;
        }

        /// <summary>
        /// Use this to activate the specific view
        /// </summary>
        public virtual void Activate ()
        {
            if (broadcastActivation != null) {
                broadcastActivation (this.location);
            }
        }

        /// <summary>
        /// Do not call this by yourself, it is used internally by the manager
        /// </summary>
        /// <param name="targetIdx">Target index.</param>
        public void ShowTarget (int targetIdx)
        {
            if (this.location == targetIdx && !this.isActive) {
                activateThis ();
                LastOpen = true;
            } else if (this.location != targetIdx && this.isActive) {
                deactivateThis ();
                LastOpen = false;
            }
        }

        protected virtual void activateThis ()
        {
            //this.isActive = true;
            AnimateActivate (onDisplayComplete);
            if (onEnter != null) {
                onEnter.Invoke ();
            }
        }

        protected virtual void deactivateThis ()
        {
            //this.isActive = false;
            AnimateDeactivate ();
            if (onExit != null)
                onExit.Invoke ();
        }
    }
}