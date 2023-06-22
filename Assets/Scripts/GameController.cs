using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
public class GameController : MonoBehaviour
{
        // Natural fall tickrate
        private float tickRate = 1f;
        private float lifetime = 0f;
        private float horizontalLifetime = 0f;
        // TODO: fix the failure condition, state is unsafe
        private bool active = true;
        private float leeway = 0;
        private bool cantMoveDown;
        // Held button tickrate
        //private float tickRateHeld = 0.1f;
        //private float lifetimeHeld = 0f;

        // Game states (0 - playing, 1 - rotate, 2 - rotation finish, 3 - checking falls, 4 - slicing, -1 check for line breaks)
        private float state = 0;

        // Rotation variables
        private float rotationProgress = 0;
        private int rotationDirection = 1;

        // Tetris pieces
        public GameObject tetroPrefab;
        public GameObject tetroGhostPrefab;
        private List<TetroController> tetros = new List<TetroController>();
        private TetroController currentTetro;
        private TetroGhostController currentTetroGhost;
        private TetroController nextTetro;
        private TetroGhostController nextTetroGhost;

        // Tetris grid
        private float gridSize = 8;
        TetroPieceController[,] grid;
        private int pieceSize = 5;

        // Other UI Elements
        public GameObject lowerSquare;
        public TextMeshProUGUI gameOver;
        private Vector2 nextPos = new Vector2(5.5f, 2.5f);

        // Slicer
        public GameObject slicePrefab;
        private SliceController sliceController;
        void Start()
        {
                grid = new TetroPieceController[(int)gridSize, (int)(2 * gridSize + gridSize/2)];
                lowerSquare = Instantiate(lowerSquare, new Vector2(0, -2.5f), Quaternion.identity);
                sliceController = Instantiate(slicePrefab, Vector2.zero, Quaternion.identity).GetComponent<SliceController>();
                sliceController.Toggle(false);
                sliceController.gridSize = gridSize;
                gameOver.enabled = false;

                // Creating first tetro
                CreateTetro(pieceSize);
                currentTetro = nextTetro;
                tetros.Add(currentTetro);
                currentTetroGhost = nextTetroGhost;
                currentTetro.transform.position += new Vector3(5 / gridSize / 2 - nextPos.x, gridSize * 5 / gridSize - 10 / gridSize + 5 / gridSize / 2 - nextPos.y);
                currentTetro.Centralize();
                currentTetro.CalculateGhost();
                CreateTetro(pieceSize);
        }

        // Update is called once per frame
        void Update()
        {
                if (active)
                {
                        // Debug message
                        if (Input.GetKeyDown(KeyCode.O))
                        {
                                Debug.Log(state);
                                string aaa = "";
                                for (int j = (int)gridSize - 1; j >= 0; j--)
                                {
                                        for (int i = 0; i < gridSize; i++)
                                        {
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
                                        currentTetro.TryMove(Vector2.right);
                                        cantMoveDown = currentTetro.CantMoveDown();
                                        horizontalLifetime = 0;
                                }
                                if (Input.GetKey(KeyCode.D))
                                {
                                        horizontalLifetime += Time.deltaTime;
                                        if (horizontalLifetime > 0.3)
                                        {
                                                horizontalLifetime -= 0.1f;

                                                currentTetro.TryMove(Vector2.right);
                                                cantMoveDown = currentTetro.CantMoveDown();
                                        }
                                }
                                if (Input.GetKeyDown(KeyCode.A))
                                {
                                        currentTetro.TryMove(Vector2.left);
                                        cantMoveDown = currentTetro.CantMoveDown();
                                        horizontalLifetime = 0;
                                }
                                if (Input.GetKey(KeyCode.A))
                                {
                                        horizontalLifetime += Time.deltaTime;
                                        if (horizontalLifetime > 0.3)
                                        {
                                                horizontalLifetime -= 0.1f;

                                                currentTetro.TryMove(Vector2.left);
                                                cantMoveDown = currentTetro.CantMoveDown();
                                        }
                                }
                                // Downward movememt
                                if (Input.GetKeyDown(KeyCode.S))
                                {
                                        if (currentTetro.TryMoveDown())
                                        {
                                                // TODO; there might be an exploit by constantly reseeting lifetime?
                                                //SignalDropped();
                                                //state = -1;
                                                cantMoveDown = true;
                                        }
                                        else
                                        {
                                                lifetime = 0; // reset natural dropping timer
                                                leeway = 0;
                                                cantMoveDown = currentTetro.CantMoveDown();
                                        }
                                }
                                if (Input.GetKey(KeyCode.S))
                                {
                                        lifetime += Time.deltaTime;
                                        if (lifetime > 0.3)
                                        {
                                                lifetime -= 0.1f;

                                                if (currentTetro.TryMoveDown())
                                                {
                                                        cantMoveDown = true;
                                                        //SignalDropped();
                                                        //state = -1;
                                                }
                                                else
                                                {
                                                        leeway = 0;
                                                        cantMoveDown = currentTetro.CantMoveDown();
                                                }
                                        }
                                }
                                // Rotation
                                if (Input.GetKeyDown(KeyCode.E))
                                {
                                        currentTetro.TryRotation(-1);
                                        cantMoveDown = currentTetro.CantMoveDown();
                                }
                                if (Input.GetKeyDown(KeyCode.Q))
                                {
                                        currentTetro.TryRotation(1);
                                        cantMoveDown = currentTetro.CantMoveDown();
                                }
                                // Hard drop
                                if (Input.GetKeyDown(KeyCode.Space))
                                {
                                        while (!currentTetro.TryMoveDown())
                                        {
                                                // Skip
                                        }
                                        lifetime = 0;
                                        currentTetro.ForceSet();
                                        SignalDropped();
                                        state = -1;
                                }

                                // Natural falling
                                lifetime += Time.deltaTime;
                                if (lifetime >= tickRate)
                                {
                                        // TODO: give more leeway to this
                                        lifetime -= tickRate;
                                        if (currentTetro.TryMoveDown())
                                        {
                                                cantMoveDown = true;
                                        }
                                        else
                                        {
                                                leeway = 0;
                                        }
                                }
                                if (cantMoveDown)
                                {
                                        // Highlight current tetro
                                        leeway += Time.deltaTime;

                                        currentTetro.SetAlpha(leeway / tickRate);
                                        if (leeway > tickRate)
                                        {
                                                currentTetro.ForceSet();
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

                                                currentTetroGhost.ToggleOff();

                                                // Calculate current positions
                                                foreach (TetroController tetro in tetros)
                                                {
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

                                                currentTetroGhost.ToggleOff();

                                                // Calculate current positions
                                                foreach (TetroController tetro in tetros)
                                                {
                                                        tetro.CalcPosition();
                                                }
                                        }
                                }

                                // Vertical slice
                                if (Input.GetKeyDown(KeyCode.G))
                                {
                                        state = 4;
                                        sliceController.Toggle(true);
                                        sliceController.Place();
                                }
                        }
                        // TODO: clean up state values
                        else if (state == 1)
                        {
                                Rotate(rotationDirection);
                        }
                        // TODO: make this look better
                        else if (state == 2)
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
                        else if (state == 3)
                        {
                                lifetime += Time.deltaTime;
                                if (lifetime > 0.1)
                                {
                                        lifetime -= 0.1f;

                                        if (!DropPieces())
                                        {
                                                state = -1;

                                                // Reactivate ghost
                                                currentTetroGhost.ToggleOn();
                                                currentTetro.CalculateGhost();
                                        }
                                }
                        }

                        else if (state == 4)
                        {
                                // Horizontal movement
                                if (Input.GetKeyDown(KeyCode.D))
                                {
                                        sliceController.TryMove(1);
                                        horizontalLifetime = 0;
                                }
                                if (Input.GetKey(KeyCode.D))
                                {
                                        horizontalLifetime += Time.deltaTime;
                                        if (horizontalLifetime > 0.3)
                                        {
                                                horizontalLifetime -= 0.1f;
                                                sliceController.TryMove(1);
                                        }
                                }
                                if (Input.GetKeyDown(KeyCode.A))
                                {
                                        sliceController.TryMove(-1);
                                        horizontalLifetime = 0;
                                }
                                if (Input.GetKey(KeyCode.A))
                                {
                                        horizontalLifetime += Time.deltaTime;
                                        if (horizontalLifetime > 0.3)
                                        {
                                                horizontalLifetime -= 0.1f;
                                                sliceController.TryMove(-1);
                                        }
                                }

                                // Doing slice
                                if (Input.GetKeyDown(KeyCode.Space))
                                {
                                        // Slice all tetros
                                        foreach (TetroController tetro in tetros.ToArray())
                                        {
                                                if (tetro != currentTetro)
                                                        tetro.Slice(sliceController.position);
                                        }
                                        sliceController.Toggle(false);
                                        // Check for falling
                                        state = 3;
                                        lifetime = 0;
                                }
                        }
                        // Checking for complete lines
                        else if (state == -1)
                        {
                                bool full;
                                bool cleared = false;
                                // Sweep through each line
                                for (int j = 0; j < gridSize; j++)
                                {
                                        full = true;
                                        // Check if that line is entirely full
                                        for (int i = 0; i < gridSize; i++)
                                        {
                                                if (grid[i, j] == null)
                                                {
                                                        full = false;
                                                }
                                        }
                                        if (full)
                                        {
                                                ClearLine(j);
                                                cleared = true;
                                        }
                                }
                                if (cleared)
                                {
                                        state = 3;
                                        // Check for breaks
                                        // TODO: make this more efficient for when there are many tetros
                                        foreach (TetroController tetro in tetros.ToArray())
                                        {
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
                else
                {
                        gameOver.enabled = true;
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
                        active = false;
                }
        }
        // Signals that the previous tetro has been placed
        public void SignalDropped()
        {
                cantMoveDown = false;
                leeway = 0;

                // Destroy old tetro ghost
                if (currentTetroGhost != null)
                {
                        Destroy(currentTetroGhost.gameObject);
                }
                currentTetro = nextTetro;
                currentTetroGhost = nextTetroGhost;
                tetros.Add(currentTetro);
                currentTetro.transform.position += new Vector3(5 / gridSize / 2 - nextPos.x, gridSize * 5 / gridSize - 10 / gridSize + 5 / gridSize / 2 - nextPos.y);
                // TODO: in case too large, move left or right
                currentTetro.Centralize();
                currentTetro.CalculateGhost();

                // Create the next set of tetros
                CreateTetro(pieceSize);
        }
        // Creates a tetro piece
        private void CreateTetro(int size)
        {
                // Generation of random tetris shapes
                List<Vector2> tempVectors = VectorGenSharp(size);

                // Creating tetro
                TetroController newTetro = Instantiate(tetroPrefab, nextPos, Quaternion.identity).GetComponent<TetroController>();
                // Setting variables
                newTetro.gameController = this;
                newTetro.position = new Vector2((int)gridSize / 2, gridSize * 2 - 2);
                newTetro.gridSize = gridSize;
                // Setting tetro color and scale
                newTetro.SetColor(new Color(Random.Range(0.5f, 1), Random.Range(0.5f, 1), Random.Range(0.5f, 1), 1));


                newTetro.Initiate(tempVectors);
                newTetro.transform.localScale = new Vector2(10 / gridSize, 10 / gridSize);
                newTetro.Center();
                // Creating tetro ghost
                TetroGhostController newTetroGhost = Instantiate(tetroGhostPrefab, nextPos, Quaternion.identity).GetComponent<TetroGhostController>();
                // Setting variables
                newTetroGhost.gameController = this;
                newTetroGhost.position = newTetro.position;
                newTetroGhost.gridSize = gridSize;
                // Setting tetro color and scale
                newTetroGhost.SetColor(new Color(0, 0, 0, 0.2f));

                newTetroGhost.Initiate(tempVectors);
                newTetroGhost.transform.localScale = new Vector2(10 / gridSize, 10 / gridSize);
                newTetroGhost.Center();
                // ASsigning pointers
                nextTetro = newTetro;
                nextTetro.tetroGhost = newTetroGhost;
                nextTetroGhost = newTetroGhost;

        }
        // Generates a random tetro shape with the given size
        private List<Vector2> VectorGen(int size) {
                List<Vector2> tempVectors = new List<Vector2>();
                HashSet<Vector2> potential = new HashSet<Vector2>();
                Vector2[] dirs = { Vector2.left, Vector2.right, Vector2.down, Vector2.up };
                potential.Add(Vector2.zero);
                for (int i = 0; i < size; i++)
                {
                        // Get random element from possible squares
                        Vector2 current = potential.ElementAt(Random.Range(0, potential.Count - 1));
                        tempVectors.Add(current);
                        potential.Remove(current);
                        // Add all adjacent elements
                        foreach (Vector2 dir in dirs)
                        {
                                Vector2 newVec = current + dir;
                                if (!tempVectors.Contains(newVec)) potential.Add(newVec);
                        }
                }
                return tempVectors;
        }
        // Generates tetro shapes that are less clumped
        private List<Vector2> VectorGenSharp(int size)
        {
                List<Vector2> tempVectors = new List<Vector2>();
                HashSet<Vector2> potential = new HashSet<Vector2>();
                Vector2[] dirs = { Vector2.left, Vector2.right, Vector2.down, Vector2.up };
                potential.Add(Vector2.zero);
                for (int i = 0; i < size; i++)
                {
                        // Get random element from possible squares
                        Vector2 current = potential.ElementAt(Random.Range(0, potential.Count - 1));
                        // Scale probability with adjacent elements
                        int adj = 0;
                        foreach (Vector2 dir in dirs)
                        {
                                if (tempVectors.Contains(current + dir)) adj++;
                        }
                        if (Random.Range(0f, 1 + adj) >= 2f) {
                                i--;
                                continue;
                        }
                        tempVectors.Add(current);
                        potential.Remove(current);
                        // Add all adjacent elements
                        foreach (Vector2 dir in dirs)
                        {
                                Vector2 newVec = current + dir;
                                if (!tempVectors.Contains(newVec)) potential.Add(newVec);
                        }
                }
                return tempVectors;
        }

        // Creates a tetro piece without initializing it (used for splitting)
        public TetroController CreateBlankTetro() {
                TetroController newTetro = Instantiate(tetroPrefab).GetComponent<TetroController>();
                newTetro.gameController = this;
                newTetro.transform.localScale = new Vector2(10 / gridSize, 10 / gridSize);
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
