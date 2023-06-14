using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TetroPieceController : MonoBehaviour
{
        public GameController gameController;
        // Used to represent offset of the piece from the center
        public Vector2 offset
        {
                get; set;
        } = Vector2.zero;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        // Checks if the given piece can move in the given direction
        public bool CheckDirection(Vector2 direction, Vector2 currentPosition)
        {
                return gameController.GridCheck(currentPosition + direction) == 0;
        }

        public bool CheckRotation(Vector2 nextPosition)
        {
                return gameController.GridCheck(nextPosition) == 0;
        }
}
