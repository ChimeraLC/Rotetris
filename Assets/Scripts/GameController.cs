using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public class GameController : MonoBehaviour
{
        // Natural fall tickrate
        private float tickRate = 1f;
        private float lifetime = 0f;

        // Held button tickrate
        //private float tickRateHeld = 0.1f;
        //private float lifetimeHeld = 0f;

        // Game states (0 - playing, 1 - rotate, -1 check for line breaks, -2 loss)
        private float state = 0;

        // Rotation variables
        private float rotationProgress = 0;

        // Temporary tetris pieces for testing
        public GameObject tetroPrefab;
        private List<TetroControllerParent> tetros = new List<TetroControllerParent>();
        private TetroControllerParent currentTetro;

        // Tetris grid
        TetroPieceController[,] grid = new TetroPieceController[10, 25];

        // Other UI Elements
        public GameObject lowerSquare;

        void Start()
        {
                CreateTetro();
                lowerSquare = Instantiate(lowerSquare, new Vector2(0, -2.5f), Quaternion.identity);
        }

        // Update is called once per frame
        void Update()
        {
                // Debug message
                if (Input.GetKeyDown(KeyCode.O)) {
                        Debug.Log(state);
                        string aaa = "";
                        for (int j = 9; j>=0; j--) {
                                for (int i = 0; i < 10; i++) {
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
                        }
                        if (Input.GetKeyDown(KeyCode.A))
                        {
                                currentTetro.TryMove(Vector2.left);
                        }

                        // Downward movememt
                        if (Input.GetKeyDown(KeyCode.S))
                        {
                                if (currentTetro.TryMoveDown()) SignalDropped();
                        }

                        // Rotation
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                                currentTetro.TryRotation(1);
                        }
                        if (Input.GetKeyDown(KeyCode.Q))
                        {
                                currentTetro.TryRotation(-1);
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

                                        state = 1;
                                        rotationProgress = 0;
                                }
                        }
                        // Rotation testing code
                        if (Input.GetKeyDown(KeyCode.F))
                        {
                                if (currentTetro.CheckBox())
                                {
                                        // TODO check that no 'active pieces' in main zone.

                                        state = 4;
                                        rotationProgress = 0;
                                }
                        }
                }
                // TODO: clean up state values
                if (state == 1)
                {
                        RotateRight();
                }
                if (state == 4)
                {
                        RotateLeft();
                }
                // TODO: make this look better
                if (state == 2)
                {
                        foreach (TetroControllerParent tetro in tetros)
                        {
                                if (tetro != currentTetro)
                                {
                                        float xOffset = tetro.position.x - 4.5f;
                                        float yOffset = tetro.position.y - 4.5f;

                                        // Calculate new position
                                        tetro.position = new Vector2(yOffset + 4.5f,
                                            -xOffset + 4.5f);

                                        tetro.rotation = 0;
                                        tetro.transform.eulerAngles = Vector3.forward * 0;
                                        tetro.RotateRight();
                                }
                        }
                        RotateRightGrid();
                        state = 3;
                        lifetime = 0;
                }
                if (state == 5)
                {
                        foreach (TetroControllerParent tetro in tetros)
                        {
                                if (tetro != currentTetro)
                                {
                                        float xOffset = tetro.position.x - 4.5f;
                                        float yOffset = tetro.position.y - 4.5f;

                                        // Calculate new position
                                        tetro.position = new Vector2(-yOffset + 4.5f,
                                            xOffset + 4.5f);

                                        tetro.rotation = 0;
                                        tetro.transform.eulerAngles = Vector3.forward * 0;
                                        tetro.RotateLeft();
                                }
                        }
                        RotateLeftGrid();
                        state = 3;
                        lifetime = 0;
                }
                // Calculating falls
                if (state == 3) {
                        lifetime += Time.deltaTime;
                        if (lifetime > 0.1)
                        {
                                lifetime -= 0.1f;

                                if (!DropPieces()) state = -1;
                        }
                }
                // Checking for complete lines
                if (state == -1) {
                        bool full;
                        bool cleared = false;
                        // Sweep through each line
                        for (int j = 0; j < 10; j++) {
                                full = true;
                                // Check if that line is entirely full
                                for (int i = 0; i < 10; i++) {
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
                                foreach (TetroControllerParent tetro in tetros.ToArray()) {
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
                for (int i = 0; i < 10; i++)
                {
                        grid[i, line].ClearSquare();
                        grid[i, line] = null;
                }
        }

        public void RemoveTetro(TetroControllerParent tetro) {
                tetros.Remove(tetro);
        }
        // Checks if the given position is open on the grid (0 - open, 1 - taken)
        public int GridCheck(Vector2 position)
        {
                // Checking boundaries
                if (position.x < 0 || position.x > 9 || position.y < 0 || position.y > 24) {
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
                if (position.y > 9) {
                        state = -2;
                }
        }
        // Signals that the previous tetro has been placed
        public void SignalDropped()
        {
                CreateTetro();      
        }
        // Creates a tetro piece TODO: make it randomized
        private void CreateTetro()
        {
                TetroControllerParent newTetro = Instantiate(tetroPrefab, new Vector2(0.25f, 4.25f), Quaternion.identity).GetComponent<TetroControllerParent>();
                newTetro.gameController = this;
                tetros.Add(newTetro);
                newTetro.position = new Vector2(5, 18);
                currentTetro = newTetro;
                Vector2[] tempVectors = { Vector2.zero, Vector2.left, Vector2.right, Vector2.up };
                newTetro.Initiate(tempVectors);
                // Setting tetro color
                currentTetro.SetColor(new Color(Random.Range(0.5f, 1), Random.Range(0.5f, 1), Random.Range(0.5f, 1), 1));
        }
        public TetroControllerParent CreateBlankTetro() {
                TetroControllerParent newTetro = Instantiate(tetroPrefab).GetComponent<TetroControllerParent>();
                newTetro.gameController = this;
                tetros.Add(newTetro);
                return newTetro;
        }
        // Function used to have already placed pieces fall, returns true any pieces fell
        private bool DropPieces()
        {
                // Setting all fall locks
                foreach (TetroControllerParent tetro in tetros)
                {
                        tetro.FallLock = false;

                }
                // Checking if actually falling
                bool falling = false;

                // Checking bottom row
                for (int i = 0; i < 10; i++)
                {
                        if (grid[i, 0] != null)
                        {
                                grid[i, 0].FallLock();
                        }
                }
                // Checking rows bottom up
                for (int j = 1; j < 10; j++)
                {
                        for (int i = 0; i < 10; i++)
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
                foreach (TetroControllerParent tetro in tetros)
                {
                        if (tetro != currentTetro)
                                tetro.ForceClear();

                }
                // Shifting all tetros down if they are falling
                foreach (TetroControllerParent tetro in tetros)
                {
                        if (tetro.GetFallLock() == false && tetro != currentTetro)
                        {
                                tetro.ForceMoveDown();
                                falling = true;
                        }
                }
                // Updating positinos of those tetros
                foreach (TetroControllerParent tetro in tetros)
                {
                        if (tetro != currentTetro)
                                tetro.ForceSet();
                }
                return falling;
        }


        // Functino used to rotate the grid right
        private void RotateRight() 
        {
                rotationProgress += Time.deltaTime * 180;
                if (rotationProgress > 90) {
                        rotationProgress = 90;
                        state = 2;
                }
                // Rotate each tetro
                foreach (TetroControllerParent tetro in tetros) {
                        if (tetro != currentTetro)
                        {
                                float xOffset = tetro.position.x - 4.5f;
                                float yOffset = tetro.position.y - 4.5f;

                                // TODO: make these onetime calculations
                                float magnitude = new Vector2(xOffset, yOffset).magnitude / 2;
                                float ang = AccurateAtan(new Vector2(xOffset, yOffset));

                                // Calculate new position
                                tetro.transform.position = new Vector2(0, -2.5f) + magnitude *
                                    new Vector2(Mathf.Cos((ang - rotationProgress) * Mathf.Deg2Rad),
                                    Mathf.Sin((ang - rotationProgress) * Mathf.Deg2Rad));

                                tetro.transform.eulerAngles = Vector3.forward * (tetro.rotation - rotationProgress);
                        }
                }

                // Rotate square
                lowerSquare.transform.eulerAngles = Vector3.forward * (-rotationProgress);

        }

        // Functino used to rotate the grid right
        private void RotateLeft()
        {
                rotationProgress += Time.deltaTime * 180;
                if (rotationProgress > 90)
                {
                        rotationProgress = 90;
                        state = 5;
                }
                // Rotate each tetro
                foreach (TetroControllerParent tetro in tetros)
                {
                        if (tetro != currentTetro)
                        {
                                float xOffset = tetro.position.x - 4.5f;
                                float yOffset = tetro.position.y - 4.5f;

                                // TODO: make these onetime calculations
                                float magnitude = new Vector2(xOffset, yOffset).magnitude / 2;
                                float ang = AccurateAtan(new Vector2(xOffset, yOffset));

                                // Calculate new position
                                tetro.transform.position = new Vector2(0, -2.5f) + magnitude *
                                    new Vector2(Mathf.Cos((ang + rotationProgress) * Mathf.Deg2Rad),
                                    Mathf.Sin((ang + rotationProgress) * Mathf.Deg2Rad));

                                tetro.transform.eulerAngles = Vector3.forward * (tetro.rotation + rotationProgress);
                        }
                }

                // Rotate square
                lowerSquare.transform.eulerAngles = Vector3.forward * (rotationProgress);

        }

        // Rotates the bottom 10x10 values in the grid rightward
        private void RotateRightGrid()
        {
                TetroPieceController[,] tempGrid = new TetroPieceController[10, 10];
                // Find the rotated grid
                for (int i = 0; i < 10; i++) {
                        for (int j = 0; j < 10; j++) {
                                float xOffset = i - 4.5f;
                                float yOffset = j - 4.5f;
                                tempGrid[(int)(yOffset + 4.5f), (int)(-xOffset + 4.5f)] = grid[i, j];
                        }
                }
                // Copy these new values over
                for (int i = 0; i < 10; i++) {
                        for (int j = 0; j < 10; j++) {
                                grid[i, j] = tempGrid[i, j];
                        }
                }
        }

        // Rotates the bottom 10x10 values in the grid leftward
        private void RotateLeftGrid()
        {
                TetroPieceController[,] tempGrid = new TetroPieceController[10, 10];
                // Find the rotated grid
                for (int i = 0; i < 10; i++)
                {
                        for (int j = 0; j < 10; j++)
                        {
                                float xOffset = i - 4.5f;
                                float yOffset = j - 4.5f;
                                tempGrid[(int)(-yOffset + 4.5f), (int)(xOffset + 4.5f)] = grid[i, j];
                        }
                }
                // Copy these new values over
                for (int i = 0; i < 10; i++)
                {
                        for (int j = 0; j < 10; j++)
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
