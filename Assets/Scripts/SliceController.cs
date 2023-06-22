using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceController : MonoBehaviour
{
        public float gridSize;
        public int position = 0;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        public void Toggle(bool state)
        {
                gameObject.SetActive(state);
        }
        // Place the slice at its initial position
        public void Place()
        {
                transform.position = new Vector2(-2.5f + (int)(gridSize / 2) * 5 / gridSize, -2.5f);
                position = (int)(gridSize / 2);
        }
        // Attempt to move left or right
        public void TryMove(int direction)
        {
                if (direction == 1)
                {
                        // Check not at edges
                        if (position < gridSize - 1)
                        {
                                position++;
                                transform.position += 5 / gridSize * (Vector3) Vector2.right;
                        }
                }
                else
                {
                        if (position > 1) {
                                position--;
                                transform.position += 5 / gridSize * (Vector3) Vector2.left;
                        }
                }
        }
}
