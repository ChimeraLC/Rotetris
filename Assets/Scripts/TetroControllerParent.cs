using System.Collections;
using System.Collections.Generic;
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
                        if (Input.GetKeyDown(KeyCode.S))
                        {
                                if (CheckDirection(Vector2.down))
                                {
                                        transform.position += new Vector3(0, -0.5f);
                                        position += Vector2.down;
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
        // Calculations called every game tick
        public void Tick() {
                if (CheckDirection(Vector2.down))
                {
                        transform.position += new Vector3(0, -0.5f);
                        position += Vector2.down;
                }
        }
}
