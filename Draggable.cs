using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Runtime;
using System.Linq;

public class Draggable : MonoBehaviour
{
    GameObject enemyCard;
    bool allowDrop = false;
    bool isHeld;
    Vector2 dif = Vector2.zero;
    Vector2 startPos = Vector2.zero;
    Vector2 contactPoint;
    Vector2 cellOffset;
    bool[,] allow;
    Tilemap tm;
    Grid grid;
    int enemyStrength;
    Vector2Int[] cells;
    Vector2Int[] baseLocations;
    Texture2D[] colours;
    Texture2D[] numbers;
    public int[] PlayerHealth;
    CardControl cardScript;
    public bool isFresh;
    float freshRound;
    public bool isStationary;
    public float cardRound;
    int boardSize;
    public bool temp;
    bool[] cellOccupied;
    private void Awake()
    {
        Physics2D.autoSyncTransforms = true;
        isStationary = false;
        isFresh = true;
        temp = false;
        boardSize = 11;
        isHeld = false;
        cardRound = 0;
        allow = new bool[boardSize, boardSize];
        tm = gameObject.GetComponentInParent<Tilemap>();
        grid = tm.layoutGrid;
        for(int i = 0; i < boardSize; i++)
        {
            for(int ii = 0; ii < boardSize; ii++)
            {
                if ((i == 0 && ii == 2) || (i == 1 && ii == 2) || (i == 1 && ii == 4) || (i == 2 && ii == 0) || (i == 2 && ii == 1) || (i == 2 && ii == 5) ||
                    (i == 2 && ii == 6) || (i == 3 && ii == 6) || (i == 4 && ii == 1) || (i == 4 && ii == 7) || (i == 4 && ii == 8) || (i == 5 && ii == 2) ||
                    (i == 5 && ii == 8) || (i == 6 && ii == 2) || (i == 6 && ii == 3) || (i == 6 && ii == 9) || (i == 7 && ii == 4) || (i == 8 && ii == 4) ||
                    (i == 8 && ii == 5) || (i == 8 && ii == 9) || (i == 8 && ii == 10) || (i == 9 && ii == 6) || (i == 9 && ii == 8) || (i == 10 && ii == 8))
                {
                    allow[i, ii] = true;
                }
                else
                {
                    allow[i, ii] = false;
                }
            }
        }
        cardScript = gameObject.transform.parent.GetComponent<CardControl>();
        freshRound = cardScript.round;
        cells = cardScript.cells;
        baseLocations = cardScript.baseLocations;
        colours = cardScript.colours;
        numbers = cardScript.numbers;
        cellOffset = new Vector2(grid.cellSize.x * transform.parent.parent.localScale.x, grid.cellSize.y * transform.parent.parent.localScale.y);
        cellOccupied = cardScript.cellOccupied;
    }
    private void OnMouseDown()
    {
        startPos = transform.position;
        dif = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position;
    }
    private void OnMouseDrag()
    {
        if (cardRound <= cardScript.round
        && System.Array.IndexOf(colours, transform.Find("Border").GetComponent<SpriteRenderer>().sprite.texture) == cardScript.thisTurn)
        {
            isHeld = true;
            transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - dif;
            isStationary = false;
        }
    }
    private void OnMouseUp()
    {
        if (!allowDrop)
        {
            transform.position = startPos;
            isStationary = true;
            isHeld = false;
        }
        else
        {
            int original = System.Array.IndexOf(cells, tm.WorldToCell(startPos + cellOffset));
            int current = System.Array.IndexOf(cells, grid.WorldToCell(contactPoint + cellOffset));
            int colourIndex = System.Array.IndexOf(colours, transform.Find("Border").GetComponent<SpriteRenderer>().sprite.texture);
            if (allow[original ,current])
            {
                cardRound = cardScript.round + 1;
                float x = GetComponent<BoxCollider2D>().bounds.size.x; //card size on x-axis
                float y = GetComponent<BoxCollider2D>().bounds.size.y; //card size on y-axis
                if (System.Array.IndexOf(baseLocations, cells[current]) == -1 || 
                    System.Array.IndexOf(baseLocations, cells[current]) == colourIndex)
                {
                    Vector2 cellPosition = tm.GetCellCenterWorld(grid.WorldToCell(contactPoint + cellOffset));
                    transform.position = cellPosition - new Vector2(x / 2f, y / 2f);
                }
                else
                {
                    DealDamage(current, colourIndex);
                }
                if (enemyCard != null)
                {
                }
                else
                {
                    isStationary = true;
                }
            }
            else
                transform.position = startPos;
        }
        isHeld = false;
    }
    private void DealDamage(int current, int colourIndex)
    {
        float x = GetComponent<BoxCollider2D>().bounds.size.x; //card size on x-axis
        float y = GetComponent<BoxCollider2D>().bounds.size.y; //card size on y-axis
        float scale = cardScript.healthScale;
        int enemyIndex = System.Array.IndexOf(baseLocations, cells[current]);
        Vector2 location = tm.CellToLocal((Vector3Int)baseLocations[enemyIndex]);
        int strength = System.Array.IndexOf(numbers, gameObject.transform.Find("Number").GetComponent<SpriteRenderer>().sprite.texture);
        PlayerHealth = cardScript.PlayerHealth;
        int health = PlayerHealth[enemyIndex] - strength;
        if (health <= 0)
        {
            health = 0;
            cardScript.game = false;
        }
        cardScript.PlayerHealth[enemyIndex] = health;
        gameObject.transform.parent.transform.Find("Health " + enemyIndex).GetComponent<SpriteRenderer>().sprite
            = Sprite.Create(numbers[health], new Rect(0, 0, numbers[health].width, numbers[health].height), new Vector2(0, 0), 100f);
        gameObject.transform.parent.transform.Find("Health " + enemyIndex).transform.localPosition
            = location - new Vector2(numbers[health].width * scale / 200f, numbers[health].height * scale / 200f);
        Vector2 cellPosition = tm.GetCellCenterWorld(grid.WorldToCell((Vector2)grid.CellToWorld((Vector3Int)baseLocations[colourIndex]) + cellOffset));
        transform.position = cellPosition - new Vector2(x / 2f, y / 2f);
    }
    private void FightOrPush(int strength)
    {
        do
        {
            if (System.Array.IndexOf(colours, transform.Find("Border").GetComponent<SpriteRenderer>().sprite.texture)
            == System.Array.IndexOf(colours, enemyCard.transform.Find("Border").GetComponent<SpriteRenderer>().sprite.texture))
            {
                enemyCard = Push(strength);
                print("Pushed");
            }
            else
            {
                enemyCard = Fight(strength);
                print("Murdered");
            }
            int colourIndex = System.Array.IndexOf(colours, transform.Find("Border").GetComponent<SpriteRenderer>().sprite.texture);
            int current = System.Array.IndexOf(cells, WorldToCellOffset(transform.position));
            if (enemyCard == null && System.Array.IndexOf(baseLocations, cells[current]) != -1 &&
                                        System.Array.IndexOf(baseLocations, cells[current]) != colourIndex)
            {
                DealDamage(current, colourIndex);
            }
        } while (enemyCard != null);
        isStationary = true;
    }
    private GameObject Push(int strength)
    {
        int positionIndex = System.Array.IndexOf(cells, WorldToCellOffset(transform.position));
        List<int> listOfIndexes = new List<int>();
        for (int num = 0; num < Mathf.Sqrt(allow.Length); num++)
        {
            if (allow[positionIndex, num] == true)
            {
                listOfIndexes.Add(num);
            }
        }
        int[] arrOfIndexes = listOfIndexes.ToArray();
        int locationIndex = Random.Range(0, arrOfIndexes.Length);
        if (strength <= enemyStrength)
        {
            transform.position = (Vector2)tm.CellToWorld((Vector3Int)cells[arrOfIndexes[locationIndex]])
                - new Vector2(gameObject.GetComponent<Collider2D>().bounds.size.x / 2f, gameObject.GetComponent<Collider2D>().bounds.size.y / 2f);
            List<Collider2D> newCollider = new List<Collider2D>();
            gameObject.GetComponent<Collider2D>().OverlapCollider(new ContactFilter2D().NoFilter(), newCollider);
            foreach (Collider2D c in newCollider)
            {
                if (c.gameObject.CompareTag("Card") && c.gameObject != gameObject)
                {
                    return c.gameObject;
                }
            }
        }
        else
        {
            enemyCard.transform.position = (Vector2)tm.CellToWorld((Vector3Int)cells[arrOfIndexes[locationIndex]])
                - new Vector2(enemyCard.GetComponent<Collider2D>().bounds.size.x / 2f, enemyCard.GetComponent<Collider2D>().bounds.size.y / 2f);
            if (enemyCard.GetComponent<Collider2D>().OverlapCollider(new ContactFilter2D().NoFilter(), new Collider2D[5]) == 2)
            {
                enemyCard.GetComponent<Draggable>().isStationary = false;
            }
        }
        return null;
    }
    private GameObject Fight(int strength)
    {
        int fight = Random.Range(0, strength + enemyStrength);
        if (fight <= strength - 1)
        {
            Destroy(enemyCard);
            return null;
        }
        else
        {
            Destroy(gameObject);
        }
        return null;
    }
    private Vector2Int WorldToCellOffset(Vector3 position)
    {
        return (Vector2Int)tm.WorldToCell((Vector2)position + cellOffset);
    }
    private Vector2 CellToWorldOffset(Vector2Int cellPosition)
    {
        return (Vector2)tm.CellToWorld((Vector3Int)cellPosition) + cellOffset;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Board")
        {
            allowDrop = true;
        }
        if (collision.tag == "Card")
        {
            enemyStrength = System.Array.IndexOf(numbers, collision.transform.Find("Number").GetComponent<SpriteRenderer>().sprite.texture);
            enemyCard = collision.gameObject;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isFresh && cardScript.round != freshRound)
        {
            isStationary = true;
            isFresh = false;
        }
        if (!isHeld && !isStationary && collision.CompareTag("Card"))
        {
            int strength = System.Array.IndexOf(numbers, transform.Find("Number").GetComponent<SpriteRenderer>().sprite.texture);
            FightOrPush(strength);
        }
        if (allowDrop)
        {
            contactPoint = Physics2D.ClosestPoint(transform.position, collision);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Board")
        {
            allowDrop = false;
        }
        if (collision.tag == "Card")
        {
            enemyCard = null;
        }
    }
}
