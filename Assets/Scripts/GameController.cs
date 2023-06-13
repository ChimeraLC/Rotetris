using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
        // Start is called before the first frame update
        private float tickRate = 1f;
        private float lifetime = 0f;
        
        // Temporary tetris pieces for testing
        public GameObject tetroPrefab;
        private List<TetroControllerParent> tetros = new List<TetroControllerParent>();

        // Tetris grid
        int[,] grid = new int[10, 25];

        void Start()
        {
                TetroControllerParent newTetro = Instantiate(tetroPrefab, new Vector2(0.25f, 4.25f), Quaternion.identity).GetComponent<TetroControllerParent>();
                tetros.Add(newTetro);
                newTetro.state = 1;
                newTetro.position = new Vector2(5, 18);
                newTetro.gameController = this;
        }

        // Update is called once per frame
        void Update()
        {
                lifetime += Time.deltaTime;
                if (lifetime >= tickRate) {
                        lifetime -= tickRate;
                        foreach (TetroControllerParent tetro in tetros) {
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
}
