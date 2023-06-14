using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
        // Natural fall tickrate
        private float tickRate = 1f;
        private float lifetime = 0f;

        // Held button tickrate
        //private float tickRateHeld = 0.1f;
        //private float lifetimeHeld = 0f;

        // Temporary tetris pieces for testing
        public GameObject tetroPrefab;
        private List<TetroControllerParent> tetros = new List<TetroControllerParent>();

        // Tetris grid
        int[,] grid = new int[10, 25];

        void Start()
        {
                CreateTetro();
        }

        // Update is called once per frame
        void Update()
        {
                lifetime += Time.deltaTime;
                if (lifetime >= tickRate) {
                        lifetime -= tickRate;
                        foreach (TetroControllerParent tetro in tetros.ToList()) {
                                tetro.Tick();
                        }
                }
        }
        // Checks if the given position is open on the grid (0 - open, 1 - taken)
        public int GridCheck(Vector2 position)
        {
                // Checking boundaries
                if (position.x < 0 || position.x > 9 || position.y < 0 || position.y > 24) {
                        return 1;
                }
                return grid[(int) position.x, (int) position.y];
        }
        // Sets the given position in the grid
        public void GridSet(Vector2 position, int value)
        {
                grid[(int)position.x, (int)position.y] = value;
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
                tetros.Add(newTetro);
                newTetro.state = 1;
                newTetro.position = new Vector2(5, 18);
                newTetro.gameController = this;
        }
}
