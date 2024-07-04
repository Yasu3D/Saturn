using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SaturnGame
{
    public class SquareGraphicRaycaster : GraphicRaycaster
    {
        [SerializeField] private Canvas canvas;
        
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            
        }
    }
}
