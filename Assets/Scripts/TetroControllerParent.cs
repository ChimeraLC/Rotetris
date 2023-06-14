using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TetroControllerParent : MonoBehaviour
{
        public GameController gameController;
        public GameObject piecePrefab;
        private List<TetroPieceController> pieces = new List<TetroPieceController>();
        public int state
        {
                get; set;
        } = 0;
        // Position used to represent center of tetro on the board
        public Vector2 position
        {
                get; set;
        } = Vector2.zero;
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
                        pieces.Add(newPiece);
                }
        }

        // Update is called once per frame
        void Update()
        {
                if (state == 1)
                {
                        // Left + Right movement
                        if (Input.GetKeyDown(KeyCode.D))
                        {
                                if (CheckDirection(Vector2.right))
                                {
                                        transform.position += new Vector3(0.5f, 0);
                                        position += Vector2.right;
                                }
                        }
                        if (Input.GetKeyDown(KeyCode.A))
                        {
                                if (CheckDirection(Vector2.left))
                                {
                                        transform.position += new Vector3(-0.5f, 0);
                                        position += Vector2.left;
                                }
                        }

                        // Downward movememt
                        if (Input.GetKeyDown(KeyCode.S))
                        {
                                if (CheckDirection(Vector2.down))
                                {
                                        transform.position += new Vector3(0, -0.5f);
                                        position += Vector2.down;
                                }
                        }

                        // Rotation
                        if (CheckRotation(new Vector2(1, -1))) 
                        {
                                if (Input.GetKeyDown(KeyCode.Q)) 
                                {
                                        RotateLeft();
                                }
                        }
                        if (CheckRotation(new Vector2(-1, 1)))
                        {
                                if (Input.GetKeyDown(KeyCode.E))
                                {
                                        RotateRight();
                                }
                        }
                }
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
                if (state == 1)
                {
                        if (CheckDirection(Vector2.down))
                        {
                                transform.position += new Vector3(0, -0.5f);
                                position += Vector2.down;
                        }
                        // Reached the bottom TODO: add some leeway here
                        else
                        {
                                state = 0;
                                gameController.SignalDropped();
                                foreach (TetroPieceController piece in pieces)
                                {
                                        gameController.GridSet(position + piece.offset, 1);
                                }
                        }
                }
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
}
