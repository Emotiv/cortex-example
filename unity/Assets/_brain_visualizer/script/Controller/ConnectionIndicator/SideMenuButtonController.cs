using UnityEngine;
using Zenject;

namespace dirox.emotiv.controller
{
    public class SideMenuButtonController : BaseCanvasView
    {

        [Inject]
        public void InjectDependencies ()
        {
        }

        public void ShowSideMenu ()
        {
            this.Deactivate ();
        }

        private void stopCameraMoving ()
        {
        }
    }
}