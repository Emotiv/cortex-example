using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dirox.emotiv.controller
{
    public abstract class MutlipleViewManager : BaseManager
    {
        protected BaseMultipleElementView[] subViews;

        protected void InitializeSubViews (params BaseMultipleElementView[] views)
        {
            subViews = new BaseMultipleElementView[views.Length];
            for (int i = 0; i < subViews.Length; i++) {
                subViews [i] = views [i];
                subViews [i].SetPosition (i);
                subViews [i].broadcastActivation = ActivateSpecificView;
                subViews [i].onEnter += VisibleButNotInteractable;
                subViews [i].onDisplayComplete += makeItInteractable;
            }
        }

        private void VisibleButNotInteractable ()
        {
            makeItInteractable (false);
            AnimateActivateOnly ();
        }

        protected void ActivateSpecificView (int idx)
        {
            if (subViews != null) {
                for (int i = 0; i < subViews.Length; i++) {
                    subViews [i].ShowTarget (idx);
                }
            }
        }

        protected override void AnimateActivate (System.Action onComplete)
        {
            base.AnimateActivate (onComplete);
        }

        void OnDestroy ()
        {
            if (subViews != null) {
                for (int i = 0; i < subViews.Length; i++) {
                    subViews [i].onEnter -= Activate;
                    subViews [i].onDisplayComplete -= makeItInteractable;
                }
            }
        }

        public virtual void SaveSubViewOpened(int index){
            if (subViews != null)
            {
                for (int i = 0; i < subViews.Length; i++)
                {
                    if(index == i){
                        subViews[i].LastOpen = true;
                    }else{
                        subViews[i].LastOpen = false;
                    }
                }
            }
        }
    }
}