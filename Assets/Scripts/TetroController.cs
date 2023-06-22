using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TetroController : TetroControllerParent
{

        public TetroGhostController tetroGhost;


        // Values calculated for rotation
        public float ang;
        public float magnitude;

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
        public override bool TryMove(Vector2 direction)
        {
                bool res = base.TryMove(direction);
                CalculateGhost();
                return res;
        }
        public override void Rotate(int direction)
        {
                base.Rotate(direction);
                if (tetroGhost != null)
                {
                        tetroGhost.Rotate(direction);
                        // TODO: might not be needed
                        CalculateGhost();
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

        public void CalcPosition()
        {
                float xOffset = transform.position.x;
                float yOffset = transform.position.y + 2.5f;

                // TODO: make these onetime calculations
                magnitude = new Vector2(xOffset, yOffset).magnitude;
                ang = AccurateAtan(new Vector2(xOffset, yOffset));

        }

        /*
         *  Helper functions
         */
        // Returns the full 0-2pi tan of a vector from 0
        private float AccurateAtan(Vector2 vector)
        {
                float ang = Vector2.Angle(vector, Vector2.right);
                if (vector.y < 0)
                {
                        ang = -ang;
                }
                return ang;
        }
}
