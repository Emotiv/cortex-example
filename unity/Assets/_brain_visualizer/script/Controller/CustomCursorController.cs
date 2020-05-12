using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

namespace dirox.emotiv.controller {
    
    public class CustomCursorController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Texture2D cursorTexture;
        public CursorMode cursorMode = CursorMode.Auto;
        public Vector2 hotSpot = Vector2.zero;
        public bool    disableClicking = false;
        Button button = null;
        Toggle toggle = null;

        void Start() {
            hotSpot = new Vector2(cursorTexture.width / 3f, cursorTexture.width / 4f);
            button = GetComponent<Button>();
            toggle = GetComponent<Toggle>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {			
            Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
            if(button!=null) {
                button.interactable = true;
            }

            if(toggle!=null) {
                toggle.interactable = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {			
            Cursor.SetCursor(null, hotSpot, cursorMode);
            if (button != null) {
                button.OnPointerExit(null);
                button.interactable = false;
            }

            if (toggle != null) {
                toggle.OnPointerExit(null);
                toggle.interactable = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {			
            if (disableClicking)
                return;
            Cursor.SetCursor(null, hotSpot, cursorMode);
        }
    }
}

