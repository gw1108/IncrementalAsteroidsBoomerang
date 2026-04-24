using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Scripts.Utility
{
    public static class UIHelpers
    {
        /// <summary>
        /// Returns whether the mouse is currently over any clickable UI.
        /// </summary>
        /// <returns></returns>
        public static bool IsMouseOverClickableUI(GraphicRaycaster graphicRaycaster = null)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            List<RaycastResult> results = new List<RaycastResult>();
            if (graphicRaycaster != null)
            {
                graphicRaycaster.Raycast(eventData, results);
            }
            else
            {
                EventSystem.current.RaycastAll(eventData, results);
            }

            foreach (var r in results)
            {
                if (r.gameObject.GetComponent<UnityEngine.UI.Selectable>())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the mouse is over *any* raycast-target UI element.
        /// Use this to block world-space clicks (pickups, collider buttons) when
        /// a UI overlay is obscuring the scene — any panel/image with
        /// raycastTarget=true counts as blocking.
        /// </summary>
        public static bool IsMouseOverBlockingUI(GraphicRaycaster graphicRaycaster = null)
        {
            if (EventSystem.current == null || Mouse.current == null) return false;

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            List<RaycastResult> results = new List<RaycastResult>();
            if (graphicRaycaster != null)
            {
                graphicRaycaster.Raycast(eventData, results);
            }
            else
            {
                EventSystem.current.RaycastAll(eventData, results);
            }

            return results.Count > 0;
        }
        
        /// <summary>
        /// Finds the first item of type T under the mouse pointer using the given raycaster whose gameobject is
        /// currently active in the hierarchy.
        /// </summary>
        public static T GetItemUnderMouse<T>(GraphicRaycaster graphicRaycaster = null) where T : MonoBehaviour
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            List<RaycastResult> results = new List<RaycastResult>();
            if (graphicRaycaster != null)
            {
                graphicRaycaster.Raycast(pointerData, results);
            }
            else
            {
                EventSystem.current.RaycastAll(pointerData, results);
            }
            
            foreach (RaycastResult result in results)
            {
                T item = result.gameObject.GetComponent<T>();
                if (item != null && result.gameObject.activeInHierarchy)
                {
                    return item;
                }
            }

            return null;
        }
    }
    
    public static class PhysicsHelpers
    {
        /// <summary>
        /// Changes the layer mask on the gameObject and all children if requested.
        /// </summary>
        public static void SetLayer(GameObject gameObject, string newLayer, bool applyToChildren = true)
        {
            var layer = LayerMask.NameToLayer(newLayer);
            gameObject.layer = layer;

            if (applyToChildren)
            {
                Transform[] children = gameObject.GetComponentsInChildren<Transform>();
                foreach (var child in children)
                {
                    child.gameObject.layer = layer;
                }
            }
        }
    }
}