using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Linq;

public class CardControl : MonoBehaviour
{
    public TileBase[] tiles;
    public Tilemap tilemap;
    public GameObject cardsParent;
    public Texture2D cardTemplate;
    public Texture2D slash;
    public Texture2D[] colours;
    public Texture2D[] numbers;
    public Texture2D[] icons;
    public Texture2D bridge;
    public Vector2Int[] cells;
    public string[] Triggers;
    public string[] Powers;
    public int playerAmount;
    public int[] PlayerHealth;
    public bool[] cellOccupied;
    public Vector2Int[] baseLocations;
    public float healthScale;
    public int playerTurn;
    public bool game;
    public GameObject button;
    bool end;
    int winner;
    public int thisTurn;
    public float round;
    private void Awake()
    {
        end = false;
        playerAmount = 2;
        PlayerHealth = new int[playerAmount];
        for (int amount = 0; amount<PlayerHealth.Length; amount++)
            PlayerHealth[amount] = 10;
        Triggers = new string[11];
        Powers = new string[11];
        game = true;
    }
    void Start()
    {
        startGame();
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.L))
        {
            buttonClick();
        }
        if (!game && !end)
        {
            print(colours[winner].name + " WON!");
            end = true;
            foreach(Transform child in transform)
            {
                if(child != transform && child != transform.Find("Main Camera"))
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
    void startGame()
    {
        makeMap(playerAmount, tiles);
        playerTurn = Random.Range(0,playerAmount);
        //Give 3 triggers and powers to each player
        GameTurn(playerTurn, out playerTurn, out thisTurn);
    }
    public void GameTurn(int turn, out int nextTurn, out int currentTurn)
    {
        winner = turn;
        //Check if special wheel should be used
        if (probabilityStage(out int strength, out string trigger, out string power))
        {
            //print("Trigger: " + trigger);
            //print("Power: " + power);
            makeCard(icons[0], colours[turn], numbers[strength], cardTemplate, cardsParent, tilemap.CellToWorld((Vector3Int)baseLocations[turn]));
            //Add trigger and power to card
        }
        currentTurn = turn;
        if (turn == playerAmount - 1)
            turn = 0;
        else
            turn++;
        nextTurn = turn;
        //Allow Player to move
        //If fight happened, make it work
    }
    void makeCard(Texture2D icons, Texture2D colours, Texture2D numbers, Texture2D template, GameObject firstParent, Vector2 location)
    {
        GameObject card = new GameObject("Card");
        card.tag = "Card";
        makeSprite(card, template, firstParent, 1.5f, 3);
        card.AddComponent<BoxCollider2D>().isTrigger = true;
        card.AddComponent<Draggable>();
        card.transform.position = location
            - new Vector2(card.GetComponent<BoxCollider2D>().bounds.size.x / 2f, card.GetComponent<BoxCollider2D>().bounds.size.y / 2f);
        Rigidbody2D rb = card.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
        GameObject border = new GameObject("Border");
        makeSprite(border, colours, card, 1f, 4);
        GameObject number = new GameObject("Number");
        makeSprite(number, numbers, card, 1f, 5);
        number.transform.localPosition = new Vector2(0.38f, 0.04f);
        GameObject icon = new GameObject("Icon");
        makeSprite(icon, icons, card, 1f, 6);
    }
    void makeSprite(GameObject thing, Texture2D sprite, GameObject parent, float scale, int layer)
    {
        Sprite image = Sprite.Create(sprite, new Rect(0, 0, sprite.width, sprite.height), new Vector2(0, 0), 100f);
        thing.transform.parent = parent.transform;
        thing.transform.localPosition = new Vector3(0, 0, (parent.transform.localPosition.z) -1);
        thing.transform.localScale = new Vector2(scale, scale);
        SpriteRenderer sr = thing.AddComponent<SpriteRenderer>();
        sr.sprite = image;
        sr.sortingOrder = layer;
    }
    void buttonClick()
    {
        round = round + (1f / (float)playerAmount);
        GameTurn(playerTurn, out playerTurn, out thisTurn);
        print("Turn Ended");
    }
    void makeBridgeSprite(Vector2Int startCell, Texture2D sprite, float zMultiple)
    {
        Vector2 start = tilemap.CellToWorld((Vector3Int)startCell);
        float pixelPerUnit = 100f;
        float xGap = tilemap.cellGap.x;
        float yGap = tilemap.cellGap.y;
        float height = sprite.height / pixelPerUnit;
        Sprite image = Sprite.Create(sprite, new Rect(0, 0, sprite.width, sprite.height), Vector2.zero, pixelPerUnit);
        GameObject bridge = new GameObject("Bridge");
        bridge.transform.position = start;
        bridge.transform.parent = transform;
        bridge.transform.localScale = Vector2.one;
        float x;
        float y;
        float f;
        if (zMultiple == 1f)
        {
            f = Mathf.Atan(yGap/xGap) * 180f / Mathf.PI;
            bridge.transform.rotation = Quaternion.Euler(0, 0, f * zMultiple);
            x = Mathf.Pow(Mathf.Sqrt(Mathf.Pow(xGap, 2f) + Mathf.Pow(yGap, 2f)), -1f)* xGap * height/2;
            //equation is cos(of cell gaps) = cos(of x and half the textures height), rearanged to find x
            y = x * -1f * yGap / xGap;
            //equation is tan(of cell gaps) = tan(of x and y), rearanged to find y
        }
        else
        {
            f = 45f;
            x = height * zMultiple /4;
            y = height / 2 * (-1 + zMultiple / 2);
        }
        bridge.transform.rotation = Quaternion.Euler(0, 0, f * zMultiple);
        bridge.transform.localPosition += new Vector3(x, y, 0);
        SpriteRenderer sr = bridge.AddComponent<SpriteRenderer>();
        sr.sprite = image;
        sr.sortingOrder = 1;
    }
    void makeMap(int playerAmount, TileBase[] tiles)
    {
        GameObject newButton = Instantiate(button, new Vector3(0,0, 1), Quaternion.identity, transform);
        newButton.transform.localPosition = new Vector3(-7.2f, -4.1f, 1);
        newButton.GetComponent<Button>().onClick.AddListener(buttonClick);
        if (playerAmount == 2)
        {
            baseLocations = new Vector2Int[2];
            cells = new Vector2Int[11];
            Vector2Int pos;
            float z;
            int count = 0;
            pos = new Vector2Int(-2, 0);
            cells[count] = pos;
            count++;
            tilemap.SetTile((Vector3Int)pos, tiles[6]);
            makeBridgeSprite(pos, bridge, 0);
            for (int i = -1; i < 2; i++)
            {
                for (int ii = -1; ii < 2; ii++)
                {
                    pos = new Vector2Int(i, ii);
                    cells[count] = pos;
                    count++;
                    if (i * ii < 0)
                    {
                        if (i == 1)
                            tilemap.SetTile((Vector3Int)pos, tiles[1]);
                        else
                            tilemap.SetTile((Vector3Int)pos, tiles[2]);
                    }
                    else
                        tilemap.SetTile((Vector3Int)pos, tiles[0]);
                    if (pos.x<1 || pos == new Vector2Int(1,0))
                    {
                        z = 0;
                        makeBridgeSprite(pos, bridge, z);
                    }
                    if (pos.x + pos.y == -1f)
                    {
                        z = 1f;
                        makeBridgeSprite(pos, bridge, z);
                    }
                    if (pos == new Vector2Int(1, 0) || pos == new Vector2Int(-1, -1))
                    {
                        z = 2f;
                        makeBridgeSprite(pos, bridge, z);
                    }
                }
            }
            pos = new Vector2Int(2, 0);
            cells[count] = pos;
            tilemap.SetTile((Vector3Int)pos, tiles[7]);
            baseLocations[0] = cells[0];
            makeHealthBar(tilemap.CellToLocal((Vector3Int)baseLocations[0]), 0);
            baseLocations[1] = cells[cells.Length - 1];
            makeHealthBar(tilemap.CellToLocal((Vector3Int)baseLocations[1]), 1);
        }
        else if (playerAmount == 3)
        {

        }
        else if (playerAmount == 4)
        {

        }
        else if (playerAmount == 5)
        {

        }
        else
        {

        }
        cellOccupied = new bool[cells.Length];
    }
    bool probabilityStage(out int strength, out string triggerIndex, out string powerIndex)
    {
        strength = 0;
        int rand = Random.Range(0, 2);
        triggerIndex = Triggers[Random.Range(0,Triggers.Length)];
        powerIndex = Powers[Random.Range(0, Powers.Length)];
        if (rand == 1)
        {
            strength = Random.Range(1, 6);
            //print("Heads");
            return true;
        }
        //print("Tails");
        return false;
    }
    void makeHealthBar(Vector2 location, int order)
    {
        healthScale = 7f;
        GameObject health = new GameObject("Health " + order);
        makeSprite(health, numbers[10], gameObject, healthScale, 2);
        health.transform.localPosition = location - new Vector2(numbers[10].width * healthScale / 200f, numbers[10].height * healthScale / 200f);
    }
}
