using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetroGhostController : TetroControllerParent
{


        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public override bool TryMoveDown()
        {
                if (CheckDirection(Vector2.down))
                {
                        transform.position += (Vector3) Vector2.down * 5 / gridSize;
                        position += Vector2.down;
                        return false;
                }
                // Reached the bottom TODO: add some leeway here
                return true;
        }
        // Toggles the visibility of the ghost
        public void Toggle()
        {
                gameObject.SetActive(!gameObject.activeSelf);
        }
}
