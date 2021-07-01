using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MousePickUp : MonoBehaviour
{
    //snap to this value
    private float snapVal = 1f;

    //object transform
    private Transform objTransform;

    //track if item was placed
    private bool itemPlaced;

    //tilemap and grid reference
    private Grid grid;
    private Tilemap tilemap;
    private float tilemapOffsetX;
    private float tilemapOffsetY;

    //object collider
    private Collider2D objCollider;

    //bool for if a structure is under this one
    private bool aboveStructure;

    //gamemanager
    public GameManager gamemanager { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        objTransform = transform;
        objCollider = GetComponent<Collider2D>();
        itemPlaced = false;

        grid = FindObjectOfType<Grid>();
        tilemap = FindObjectOfType<Tilemap>();

        tilemapOffsetX = tilemap.transform.position.x * -1;
        tilemapOffsetY = tilemap.transform.position.y * -1;

        aboveStructure = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!itemPlaced && !gamemanager.IsPaused)
        {
            //put the object on the mouse
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;
            objTransform.position = Camera.main.ScreenToWorldPoint(mousePos);

            //snap the object to the nearest grid point
            objTransform.position = GetSnapPosition(objTransform.position, snapVal);
        }
    }

    //property for itemPlaced
    public bool ItemPlaced
    {
        get { return itemPlaced; }
    }

    //snap transform to grid
    private Vector3 GetSnapPosition(Vector2 originalPos, float snap = .5f)
    {
        return new Vector3(
            GetSnapValue(originalPos.x, snap), 
            GetSnapValue(originalPos.y, snap),
            10
        );
    }

    //change value according to snap
    private float GetSnapValue(float value, float snap = .5f)
    {
        return (!Mathf.Approximately(snap, 0f)) ? Mathf.RoundToInt(value / snap) * snap : value;
    }

    //place item
    private void OnMouseDown()
    {
        if (itemPlaced)
        {
            return;
        }

        //get half of the width and height since the position is in the center of the object
        float halfOfWidth = objCollider.bounds.size.x / 2;
        float halfOfHeight = objCollider.bounds.size.y / 2;

        //calculate top, right, bottom, and left positions of object
        //.1 offset added to guarentee placement works on edge of wall and non wall tile
        //tilemap offset is for if the tilemap transform is moved the structure's pos should be moved the opposite direction by the same offset
        float boxRight = objTransform.position.x + halfOfWidth - .1f + tilemapOffsetX;
        float boxLeft = objTransform.position.x - halfOfWidth + .1f + tilemapOffsetX;
        float boxTop = objTransform.position.y + halfOfHeight - .1f + tilemapOffsetY;
        float boxBottom = objTransform.position.y - halfOfHeight + .1f + tilemapOffsetY;

        //top left grid point of box
        Vector3Int objTopLeft = grid.WorldToCell(new Vector3(
            boxLeft,
            boxTop,
            objTransform.position.z
        ));

        //top right grid point of box
        Vector3Int objTopRight = grid.WorldToCell(new Vector3(
            boxRight,
            boxTop, 
            objTransform.position.z
        ));

        //bottom left grid point of box
        Vector3Int objBotLeft = grid.WorldToCell(new Vector3(
            boxLeft,
            boxBottom, 
            objTransform.position.z
        ));

        //bottom right grid point of box
        Vector3Int objBotRight = grid.WorldToCell(new Vector3(
            boxRight,
            boxBottom, 
            objTransform.position.z
        ));

        //check if there is a structure below where it is trying to be placed        
        aboveStructure = IsStructureUnderHere(new Bounds(transform.position, new Vector3(.9f, .9f, 1)));

        //only place if all corners are within the tilemap and over a wall
        if (
            tilemap.HasTile(objTopLeft) &&
            tilemap.HasTile(objTopRight) &&
            tilemap.HasTile(objBotLeft) &&
            tilemap.HasTile(objBotRight) &&
            tilemap.GetTile(objTopLeft).name == "wall" && 
            tilemap.GetTile(objTopRight).name == "wall" && 
            tilemap.GetTile(objBotLeft).name == "wall" && 
            tilemap.GetTile(objBotRight).name == "wall" &&
            !aboveStructure
        )
        {
            itemPlaced = true;
            transform.tag = "structure";
        }
    }

    //FIGURE OUT HOW TO USE THIS LATER BECAUSE IT IS MORE EFFICIENT AND LESS COSTLY
    //get collider below ray
    //private bool IsStrctureBelowRay(Ray ray)
    //{
    //    //try using physics.raycast instead to see if that goes through the screen because i dont think raycast 2d works the way i need it to
    //    RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
    //    Debug.Log(hit.collider.tag);

    //    if (hit.collider == null)
    //    {
    //        return false;
    //    }
    //    else
    //    {
    //        return hit.collider.tag == "structure";
    //    }
    //}

    //get all colliders in the scene and check if any of them contain any of the points given
    private bool IsStructureUnderHere(Bounds boundCheck)
    {
        GameObject[] structures = GameObject.FindGameObjectsWithTag("structure");

        foreach (GameObject structure in structures)
        {
            if (structure.GetComponent<BoxCollider2D>().bounds.Intersects(boundCheck))
            {
                return true;
            }
        }

        return false;
    }
}
