using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapAdjuster : MonoBehaviour
{
    public TileBase wallTile;
    public TileBase pathTile;
    private Tilemap tilemap;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    private void Start()
    {
        GameManager.Instance.onGridSizeChanged += ChangeTileSize;
    }



    private void ChangeTileSize(int width, int height)
    {
        tilemap.size = new Vector3Int(width, height, 1);
        tilemap.origin = new Vector3Int(0, 0, 0);
        TileBase[] tileBases = new TileBase[width * height];
        tilemap.ResizeBounds();
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                tileBases[x + y * bounds.size.x] = pathTile;
            }
        }
        tilemap.SetTilesBlock(tilemap.cellBounds, tileBases);
        DeleteTileMapFlags(width,height);
        CleanMap(width,height);
    }

    private void DeleteTileMapFlags(int width, int height)
    {
        //Renklendirme için
        Vector3Int coord = Vector3Int.zero;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                coord.x = i;
                coord.y = j;
                tilemap.SetTileFlags(coord, TileFlags.None);
            }
        }
    }

    private void CleanMap(int width, int height)
    {
        //Renklendirme silme
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tilemap.SetColor(new Vector3Int(i, j, 0), Color.white);
            }
        }
    }
}
