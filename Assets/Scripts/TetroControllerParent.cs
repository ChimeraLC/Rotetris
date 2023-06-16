using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TetroControllerParent : MonoBehaviour
{
        public GameController gameController;
        public GameObject tetroPrefab;
        public GameObject piecePrefab;
        private List<TetroPieceController> pieces = new List<TetroPieceController>();
        private Color currentColor;
        public bool FallLock = false;
        public bool marked;
        public int state
        {
                get; set;
        } = 0;
        // Position used to represent center of tetro on the board
        public Vector2 position
        {
                get; set;
        }

        public float rotation
        {
                get; set;
        } = 0;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Creates a set of pieces corresponding to the following pieces
        public void Initiate(Vector2[] pieceVectors)
        {
                foreach (Vector2 piecePos in pieceVectors)
                {
                        TetroPieceController newPiece = Instantiate(piecePrefab, transform.position + (Vector3)piecePos / 2,
                            Quaternion.identity).GetComponent<TetroPieceController>();
                        newPiece.transform.SetParent(transform);
                        newPiece.gameController = gameController;
                        newPiece.offset = piecePos;
                        newPiece.SetColor(currentColor);
                        newPiece.parent = this;
                        pieces.Add(newPiece);
                }
        }

        // Assigns the given pieces to this bject
        public void SoftInitiate(TetroPieceController[] pieceAdds)
        {
                foreach (TetroPieceController piece in pieceAdds)
                {
                        pieces.Add(piece);
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
        // Moves down ignoring collisions (assumed predetermind), returns true if at the bottom
        public void ForceMoveDown()
        {
                transform.position += new Vector3(0, -0.5f);
                position += Vector2.down;

        }
        public void ForceClear()
        {
                foreach (TetroPieceController piece in pieces)
                {
                        gameController.GridSet(position + piece.offset, null);
                }
        }
        public void ForceSet()
        {
                foreach (TetroPieceController piece in pieces)
                {
                        gameController.GridSet(position + piece.offset, piece);
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

        // Falling lock
        public bool GetFallLock() {
                return FallLock;
        }

        // Removing pieces
        public void RemoveSquare(TetroPieceController square) {
                pieces.Remove(square);
                // Destroy piece if empty
                if (pieces.Count == 0) {
                        gameController.RemoveTetro(this);
                        Destroy(gameObject);
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
        // Checks for any potential breaks in the tetris piece, creating new pieces if so
        public void CheckBreaks() {
                // First, create a grid representation (everything is offset by 5, 5)
                TetroPieceController[,] grid = new TetroPieceController[10, 10];
                int counter = 1;
                foreach (TetroPieceController piece in pieces)
                {

                        grid[(int) piece.offset.x + 5, (int) piece.offset.y + 5] = piece;
                        piece.marker = counter;
                        counter++;
                }
                bool active = true;

                TetroPieceController comparePiece;
                // Perform a backwards search from each piece towards the main piece
                // TODO: make this more efficient
                while (active) {
                        active = false;
                        foreach (TetroPieceController piece in pieces)
                        {
                                comparePiece = grid[(int)piece.offset.x + 4, (int)piece.offset.y + 5];
                                if (comparePiece != null && comparePiece.marker < piece.marker) {
                                        piece.marker = comparePiece.marker;
                                        active = true;
                                }
                                comparePiece = grid[(int)piece.offset.x + 6, (int)piece.offset.y + 5];
                                if (comparePiece != null && comparePiece.marker < piece.marker)
                                {
                                        piece.marker = comparePiece.marker;
                                        active = true;
                                }
                                comparePiece = grid[(int)piece.offset.x + 5, (int)piece.offset.y + 4];
                                if (comparePiece != null && comparePiece.marker < piece.marker)
                                {
                                        piece.marker = comparePiece.marker;
                                        active = true;
                                }
                                comparePiece = grid[(int)piece.offset.x + 5, (int)piece.offset.y + 6];
                                if (comparePiece != null && comparePiece.marker < piece.marker)
                                {
                                        piece.marker = comparePiece.marker;
                                        active = true;
                                }
                        }
                }
                int broken = 0;
                // Check if there needs to be a new tetro
                foreach (TetroPieceController piece in pieces) {
                        if (piece.marker != 1) {
                                broken += 1;
                        }
                }
                if (broken != 0) {
                        // Create a new tetro
                        TetroControllerParent newTetro = gameController.CreateBlankTetro();
                        newTetro.transform.position = transform.position;
                        newTetro.position = position;
                        // Add unconnected pieces
                        TetroPieceController[] offPieces = new TetroPieceController[broken];
                        counter = 0;
                        foreach (TetroPieceController piece in pieces.ToArray<TetroPieceController>()) {
                                if (piece.marker != 1)
                                {
                                        RemoveSquare(piece);
                                        piece.transform.parent = newTetro.transform;
                                        piece.parent = newTetro;
                                        offPieces[counter] = piece;
                                        counter++;
                                }
                        }
                        newTetro.SoftInitiate(offPieces);
                        // Recursively check for breaks in the other tetro
                        newTetro.CheckBreaks();
                }
        }
}
