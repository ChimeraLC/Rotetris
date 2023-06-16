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
                        transform.position += new Vector3(0, -0.5f);
                        position += Vector2.down;
                        return false;
                }
                // Reached the bottom TODO: add some leeway here
                return true;
        }
}
