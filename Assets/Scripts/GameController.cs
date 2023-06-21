using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
public class GameController : MonoBehaviour
{
        // Natural fall tickrate
        private float tickRate = 1f;
        private float lifetime = 0f;

        // TODO: fix the failure condition, state is unsafe
        private bool active = true;
        // Held button tickrate
        //private float tickRateHeld = 0.1f;
        //private float lifetimeHeld = 0f;

        // Game states (0 - playing, 1 - rotate, -1 check for line breaks, -2 loss)
        private float state = 0;

        // Rotation variables
        private float rotationProgress = 0;
        private int rotationDirection = 1;

        // Temporary tetris pieces for testing
        public GameObject tetroPrefab;
        public GameObject tetroGhostPrefab;
        private List<TetroController> tetros = new List<TetroController>();
        private TetroController currentTetro;
        private TetroGhostController currentTetroGhost;

        // Tetris grid
        private float gridSize = 10;
        TetroPieceController[,] grid;
        private int pieceSize = 4;

        // Other UI Elements
        public GameObject lowerSquare;

        void Start()
        {
                grid = new TetroPieceController[(int)gridSize, (int)(2 * gridSize + gridSize/2)];
                CreateTetro(pieceSize);
                lowerSquare = Instantiate(lowerSquare, new Vector2(0, -2.5f), Quaternion.identity);
        }

        // Update is called once per frame
        void Update()
        {
                // Debug message
                if (Input.GetKeyDown(KeyCode.O)) {
                        Debug.Log(state);
                        string aaa = "";
                        for (int j = (int) gridSize - 1; j>=0; j--) {
                                for (int i = 0; i < gridSize; i++) {
                                        if (grid[i, j] == null) aaa += "0";
                                        else aaa += "1";
                                }
                                aaa += "\n";
                        }
                        Debug.Log(aaa);
                }
                // Only tick and allow controls in the playing state
                if (state == 0)
                {
                        // Horizontal movement
                        if (Input.GetKeyDown(KeyCode.D))
                        {
                                if (currentTetro.TryMove(Vector2.right)) {
                                        currentTetro.CalculateGhost();
                                }
                        }
                        if (Input.GetKeyDown(KeyCode.A))
                        {
                                if (currentTetro.TryMove(Vector2.left)) {
                                        currentTetro.CalculateGhost();
                                }
                        }

                        // Downward movememt
                        if (Input.GetKeyDown(KeyCode.S))
                        {
                                if (currentTetro.TryMoveDown()) SignalDropped();
                        }

                        // Rotation
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                                currentTetro.TryRotation(-1);
                        }
                        if (Input.GetKeyDown(KeyCode.Q))
                        {
                                currentTetro.TryRotation(1);
                        }
                        // Hard drop
                        if (Input.GetKeyDown(KeyCode.Space)) {
                                while (!currentTetro.TryMoveDown()) { 
                                        // Skip
                                }
                                lifetime = 0;
                                SignalDropped();
                                state = -1;
                        }

                        // Natural falling
                        lifetime += Time.deltaTime;
                        if (lifetime >= tickRate)
                        {
                                lifetime -= tickRate;
                                if (currentTetro.TryMoveDown())
                                {
                                        SignalDropped();
                                        state = -1;
                                }
                        }


                        // Rotation testing code
                        if (Input.GetKeyDown(KeyCode.R))
                        {
                                if (currentTetro.CheckBox())
                                {
                                        // TODO check that no 'active pieces' in main zone.
                                        // TODO: make prediction invisible

                                        state = 1;
                                        rotationProgress = 0;
                                        rotationDirection = -1;

                                        currentTetroGhost.Toggle();

                                        // Calculate current positions
                                        foreach (TetroController tetro in tetros) {
                                                tetro.CalcPosition();
                                        }
                                }
                        }
                        // Rotation testing code
                        if (Input.GetKeyDown(KeyCode.F))
                        {
                                if (currentTetro.CheckBox())
                                {
                                        // TODO check that no 'active pieces' in main zone.
                                        state = 1;
                                        rotationProgress = 0;
                                        rotationDirection = 1;

                                        currentTetroGhost.Toggle();

                                        // Calculate current positions
                                        foreach (TetroController tetro in tetros)
                                        {
                                                tetro.CalcPosition();
                                        }
                                }
                        }
                }
                // TODO: clean up state values
                if (state == 1)
                {
                        Rotate(rotationDirection);
                }
                // TODO: make this look better
                if (state == 2)
                {
                        foreach (TetroController tetro in tetros)
                        {
                                if (tetro != currentTetro)
                                {

                                        // Calculate new position
                                        tetro.transform.position = new Vector2(0, -2.5f) + tetro.magnitude *
                                            new Vector2(Mathf.Cos((tetro.ang + rotationDirection * 90) * Mathf.Deg2Rad),
                                            Mathf.Sin((tetro.ang + rotationDirection * 90) * Mathf.Deg2Rad));
                                        if (rotationDirection == -1)
                                        {
                                                tetro.position = new Vector2(tetro.position.y, -tetro.position.x + (gridSize - 1));
                                                tetro.rotation = 0;
                                                tetro.transform.eulerAngles = Vector3.forward * 0;
                                                tetro.RotateRight();
                                        }
                                        else
                                        {
                                                tetro.position = new Vector2(-tetro.position.y + (gridSize - 1), tetro.position.x);
                                                tetro.rotation = 0;
                                                tetro.transform.eulerAngles = Vector3.forward * 0;
                                                tetro.RotateLeft();
                                        }
                                }
                        }
                        RotateGrid(rotationDirection);
                        state = 3;
                        lifetime = 0;
                }
                // Calculating falls
                if (state == 3) {
                        lifetime += Time.deltaTime;
                        if (lifetime > 0.1)
                        {
                                lifetime -= 0.1f;

                                if (!DropPieces())
                                {
                                        state = -1;

                                        // Reactivate ghost
                                        currentTetroGhost.Toggle();
                                        currentTetro.CalculateGhost();
                                }
                        }
                }
                // Checking for complete lines
                if (state == -1) {
                        bool full;
                        bool cleared = false;
                        // Sweep through each line
                        for (int j = 0; j < gridSize; j++) {
                                full = true;
                                // Check if that line is entirely full
                                for (int i = 0; i < gridSize; i++) {
                                        if (grid[i, j] == null) {
                                                full = false;
                                        }
                                }
                                if (full) {
                                        ClearLine(j);
                                        cleared = true;
                                }
                        }
                        if (cleared)
                        {
                                state = 3;
                                // Check for breaks
                                // TODO: make this more efficient for when there are many tetros
                                foreach (TetroController tetro in tetros.ToArray()) {
                                        if (tetro.marked)
                                        {
                                                tetro.marked = false;
                                                tetro.CheckBreaks();
                                        }
                                }
                                lifetime = 0;
                        }
                        else state = 0;
                }
                
        }

        // Clears the given line
        private void ClearLine(int line) {
                for (int i = 0; i < gridSize; i++)
                {
                        grid[i, line].ClearSquare();
                        grid[i, line] = null;
                }
        }

        public void RemoveTetro(TetroController tetro) {
                tetros.Remove(tetro);
        }
        // Checks if the given position is open on the grid (0 - open, 1 - taken)
        public int GridCheck(Vector2 position)
        {
                // Checking boundaries
                if (position.x < 0 || position.x > gridSize - 1 || position.y < 0 || position.y > 2 * gridSize - 1 + gridSize / 2) {
                        return 1;
                }
                if (grid[(int) position.x, (int) position.y] == null) return 0;
                return 1;
        }
        // Sets the given position in the grid
        public void GridSet(Vector2 position, TetroPieceController value)
        {
                grid[(int)position.x, (int)position.y] = value;

                //TODO: fix this loss check
                if (position.y > gridSize - 1) {
                        state = -2;
                        active = false;
                }
        }
        // Signals that the previous tetro has been placed
        public void SignalDropped()
        {
                CreateTetro(pieceSize);      
        }
        // Creates a tetro piece TODO: make it randomized
        private void CreateTetro(int size)
        {
                // Destroy old tetro ghost
                if (currentTetroGhost != null) {
                        Destroy(currentTetroGhost.gameObject);
                }
                // Creating tetro
                TetroController newTetro = Instantiate(tetroPrefab, new Vector2(5 / gridSize / 2, gridSize * 5 / gridSize - 10/gridSize + 5/gridSize/2), Quaternion.identity).GetComponent<TetroController>();
                newTetro.gameController = this;
                tetros.Add(newTetro);
                newTetro.position = new Vector2((int) gridSize / 2, gridSize * 2 - 2);
                currentTetro = newTetro;
                
                // Generation of random tetris shapes
                List<Vector2> tempVectors = new List<Vector2>();
                HashSet<Vector2> potential = new HashSet<Vector2>();
                Vector2[] dirs = { Vector2.left, Vector2.right, Vector2.down, Vector2.up };
                potential.Add(Vector2.zero);
                for (int i = 0; i < size; i++) {
                        // Get random element from possible squares
                        Vector2 current = potential.ElementAt(Random.Range(0, potential.Count - 1));
                        tempVectors.Add(current);
                        potential.Remove(current);
                        // Add all adjacent elements
                        foreach (Vector2 dir in dirs) {
                                Vector2 newVec = current + dir;
                                if (!tempVectors.Contains(newVec)) potential.Add(newVec);
                        }
                }

                // Setting tetro color and scale
                currentTetro.SetColor(new Color(Random.Range(0.5f, 1), Random.Range(0.5f, 1), Random.Range(0.5f, 1), 1));
                currentTetro.transform.localScale = new Vector2(10 / gridSize, 10 / gridSize);

                newTetro.Initiate(tempVectors);
                newTetro.gridSize = gridSize;
                newTetro.Center();
                // Creating tetro ghost
                TetroGhostController newTetroGhost = Instantiate(tetroGhostPrefab, new Vector2(0.25f, (gridSize - 1) / 2 - 0.25f), Quaternion.identity).GetComponent<TetroGhostController>();
                newTetroGhost.gameController = this;
                newTetroGhost.position = newTetro.position;
                newTetroGhost.Initiate(tempVectors);
                newTetroGhost.gridSize = gridSize;
                newTetroGhost.Center();
                newTetroGhost.SetColor(new Color(0, 0, 0, 0.2f));
                currentTetro.tetroGhost = newTetroGhost;
                currentTetroGhost = newTetroGhost;
                currentTetroGhost.transform.localScale = new Vector2(10 / gridSize, 10 / gridSize);
                currentTetro.CalculateGhost();
        }
        // Creates a tetro piece without initializing it (used for splitting)
        public TetroController CreateBlankTetro() {
                TetroController newTetro = Instantiate(tetroPrefab).GetComponent<TetroController>();
                newTetro.gameController = this;
                tetros.Add(newTetro);
                return newTetro;
        }
        // Function used to have already placed pieces fall, returns true any pieces fell
        private bool DropPieces()
        {
                // Setting all fall locks
                foreach (TetroController tetro in tetros)
                {
                        tetro.FallLock = false;

                }
                // Checking if actually falling
                bool falling = false;

                // Checking bottom row
                for (int i = 0; i < gridSize; i++)
                {
                        if (grid[i, 0] != null)
                        {
                                grid[i, 0].FallLock();
                        }
                }
                // Checking rows bottom up
                for (int j = 1; j < gridSize; j++)
                {
                        for (int i = 0; i < gridSize; i++)
                        {
                                if (grid[i, j] != null)
                                {
                                        if (grid[i, j - 1] != null && grid[i, j - 1].GetFallLock())
                                        {
                                                grid[i, j].FallLock();
                                        }
                                }
                        }
                }
                // Clearing each tetro
                foreach (TetroController tetro in tetros)
                {
                        if (tetro != currentTetro)
                                tetro.ForceClear();

                }
                // Shifting all tetros down if they are falling
                foreach (TetroController tetro in tetros)
                {
                        if (tetro.GetFallLock() == false && tetro != currentTetro)
                        {
                                tetro.ForceMoveDown();
                                falling = true;
                        }
                }
                // Updating positinos of those tetros
                foreach (TetroController tetro in tetros)
                {
                        if (tetro != currentTetro)
                                tetro.ForceSet();
                }
                return falling;
        }

        // Function used for both rotations
        private void Rotate(int rotateDirection)
        {
                rotationProgress += Time.deltaTime * 180;
                if (rotationProgress > 90)
                {
                        rotationProgress = 90;
                        state = 2;
                }
                // Rotate each tetro
                foreach (TetroController tetro in tetros)
                {
                        if (tetro != currentTetro)
                        {

                                // Calculate new position
                                tetro.transform.position = new Vector2(0, -2.5f) + tetro.magnitude *
                                    new Vector2(Mathf.Cos((tetro.ang + rotateDirection * rotationProgress) * Mathf.Deg2Rad),
                                    Mathf.Sin((tetro.ang + rotateDirection * rotationProgress) * Mathf.Deg2Rad));

                                tetro.transform.eulerAngles = Vector3.forward * (tetro.rotation + rotateDirection * rotationProgress);
                        }
                }

                // Rotate square
                lowerSquare.transform.eulerAngles = Vector3.forward * (rotateDirection * rotationProgress);

        }
        // Rotates the grid in a given direction
        private void RotateGrid(int rotateDirection)
        {
                TetroPieceController[,] tempGrid = new TetroPieceController[(int)gridSize, (int)gridSize];
                // Find the rotated grid
                for (int i = 0; i < gridSize; i++)
                {
                        for (int j = 0; j < gridSize; j++)
                        {
                                float xOffset = i - (gridSize - 1) / 2;
                                float yOffset = j - (gridSize - 1) / 2;
                                tempGrid[(int)(- rotationDirection * yOffset + (gridSize - 1) / 2), (int)(rotationDirection * xOffset + (gridSize - 1) / 2)] = grid[i, j];
                        }
                }
                // Copy these new values over
                for (int i = 0; i < gridSize; i++)
                {
                        for (int j = 0; j < gridSize; j++)
                        {
                                grid[i, j] = tempGrid[i, j];
                        }
                }
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
