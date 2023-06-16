using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TetroController : TetroControllerParent
{

        public TetroGhostController tetroGhost;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        public override void RemoveSquare(TetroPieceController square)
        {
                pieces.Remove(square);
                // Destroy piece if empty
                if (pieces.Count == 0)
                {
                        gameController.RemoveTetro(this);
                        Destroy(gameObject);
                }
        }
        // Calculate ghost position
        public void CalculateGhost()
        {
                tetroGhost.transform.position = transform.position;
                tetroGhost.position = this.position;
                while (!tetroGhost.TryMoveDown()) { 
                        
                }
        }

        // Overrides on rotation
        public override void RotateRight()
        {
                base.RotateRight();
                if (tetroGhost != null)
                {
                        tetroGhost.RotateRight();
                        CalculateGhost();
                }
        }
        public override void RotateLeft()
        {
                base.RotateLeft();
                if (tetroGhost != null)
                {
                        tetroGhost.RotateLeft();
                        CalculateGhost();
                }
        }
}
