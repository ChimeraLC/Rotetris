using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TetroControllerParent : MonoBehaviour
{
        public GameController gameController;
        public GameObject piecePrefab;
        private List<TetroPieceController> pieces = new List<TetroPieceController>();
        private Color currentColor;
        public int state
        {
                get; set;
        } = 0;
        // Position used to represent center of tetro on the board
        public Vector2 position
        {
                get; set;
        } = Vector2.zero;

        public float rotation
        {
                get; set;
        } = 0;
        // Start is called before the first frame update
        void Start()
        {
                Vector2[] tempVectors = { Vector2.zero, Vector2.left, Vector2.right, Vector2.up };
                foreach (Vector2 piecePos in tempVectors) {
                        TetroPieceController newPiece = Instantiate(piecePrefab, transform.position + (Vector3) piecePos/2,
                            Quaternion.identity).GetComponent<TetroPieceController>();
                        newPiece.transform.SetParent(transform);
                        newPiece.gameController = gameController;
                        newPiece.offset = piecePos;
                        newPiece.SetColor(currentColor);
                        pieces.Add(newPiece);
                }
        }

        // Update is called once per frame
        void Update()
        {
        }
        // Trys to move in the given direction
        public void TryMove(Vector2 direction) {
                if (CheckDirection(direction)) {
                        transform.position += (Vector3) direction / 2;
                        position += direction;
                }
        }
        // Specifically moves down, returns true if at the bottom
        public bool TryMoveDown() {
                if (CheckDirection(Vector2.down))
                {
                        transform.position += new Vector3(0, -0.5f);
                        position += Vector2.down;
                        return false;
                }
                // Reached the bottom TODO: add some leeway here
                else
                {
                        foreach (TetroPieceController piece in pieces)
                        {
                                gameController.GridSet(position + piece.offset, piece);
                        }

                        return true;
                }
        }
        // Trys to rotate the piece in the given direction (1 -> right, -1 -> left)
        public void TryRotation(int direction) {

                if (CheckRotation(new Vector2(-1 * direction, 1 * direction))) {
                        if (direction == 1)
                        {
                                RotateRight();
                        }
                        else {
                                RotateLeft();
                        }
                }
        }
        // Checks that the given piece is not within the 10x10 rotation grid
        public bool CheckBox() {
                bool valid = true;
                foreach (TetroPieceController piece in pieces) {
                        Vector2 loc = position + piece.offset;
                        if (loc.x >= 0 && loc.x <= 9 && loc.y >= 0 && loc.y <= 9) {
                                valid = false;
                        }
                }
                return valid;
        }
        // Checks if the tetris piece can move in the given direction
        bool CheckDirection(Vector2 direction) {
                bool valid = true;
                foreach (TetroPieceController piece in pieces) {
                        if (!piece.CheckDirection(direction, position + piece.offset)) {
                                valid = false;
                        }
                }
                return valid;
        }
        bool CheckRotation(Vector2 direction) {
                bool valid = true;
                foreach (TetroPieceController piece in pieces)
                {
                        if (!piece.CheckRotation(position + 
                            new Vector2(piece.offset.y * direction.x, piece.offset.x * direction.y)))
                        {
                                valid = false;
                        }
                }
                return valid;
        }
        // Calculations called every game tick
        public void Tick() {
                
        }
        // Rotation methods TODO: perform checks
        public void RotateRight() {
                foreach (TetroPieceController piece in pieces)
                {
                        piece.offset = new Vector2(piece.offset.y, -piece.offset.x);
                        piece.transform.position = transform.position + (Vector3) piece.offset / 2;
                }        
        }

        public void RotateLeft()
        {
                foreach (TetroPieceController piece in pieces)
                {
                        piece.offset = new Vector2(-piece.offset.y, piece.offset.x);
                        piece.transform.position = transform.position + (Vector3)piece.offset / 2;
                }
        }

        // Visual methods
        public void SetColor(Color newColor) {
                // Set each individual piece.
                foreach (TetroPieceController piece in pieces)
                {
                        piece.SetColor(newColor);
                }
                // Also set any new pieces
                currentColor = newColor;
        }
}
