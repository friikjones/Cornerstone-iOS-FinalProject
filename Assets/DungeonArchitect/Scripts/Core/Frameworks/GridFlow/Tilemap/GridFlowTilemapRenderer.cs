using DungeonArchitect.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow.Tilemap
{
    public class GridFlowTilemapRenderResources
    {
        public Texture2D iconOneWayDoor = Texture2D.whiteTexture;
        public TexturedMaterialInstances materials;
    }

    /// <summary>
    /// Renders the tilemap on to a render texture
    /// </summary>
    public class GridFlowTilemapRenderer
    {
        public static void Render(RenderTexture tilemapTexture, GridFlowTilemap tilemap, int tileSize, GridFlowTilemapRenderResources resources, System.Func<GridFlowTilemapCell, bool> FuncCellSelected)
        {
            var oldRTT = RenderTexture.active;
            RenderTexture.active = tilemapTexture;

            GL.PushMatrix();
            GL.LoadOrtho();
            float texWidth = tilemapTexture.width;
            float texHeight = tilemapTexture.height;

            var layers = BuildQuadData(tilemap, tileSize, resources, FuncCellSelected);
            foreach (var layer in layers)
            {
                layer.material.SetPass(0);
                GL.Begin(GL.QUADS);
                var quads = layer.quads;
                foreach (var quad in quads)
                {
                    GL.Color(quad.color);

                    for (int i = 0; i < 4; i++)
                    {
                        var vert = quad.verts[(i + quad.rotateUV) % 4];
                        var uv = vert.uv;
                        GL.TexCoord2(uv.x, uv.y);

                        vert = quad.verts[i];
                        var p = vert.position;
                        GL.Vertex3(p.x, p.y, quad.z);
                    }
                }
                GL.End();
            }

            var lineMaterial = resources.materials.GetMaterial(Texture2D.whiteTexture);
            lineMaterial.SetPass(0);
            // Draw the grid lines
            GL.Begin(GL.LINES);
            GL.Color(new Color(0.0f, 0.0f, 0.0f, 0.1f));
            for (int x = 0; x < tilemap.Width; x++)
            {
                float x0 = (x * tileSize) / texWidth;
                GL.Vertex3(x0, 0, 0);
                GL.Vertex3(x0, 1, 0);
            }
            for (int y = 0; y < tilemap.Height; y++)
            {
                float y0 = (y * tileSize) / texHeight;
                GL.Vertex3(0, y0, 0);
                GL.Vertex3(1, y0, 0);
            }
            GL.End();


            GL.PopMatrix();

            RenderTexture.active = oldRTT;
        }

        static TilemapLayerRenderData[] BuildQuadData(GridFlowTilemap tilemap, int tileSize, GridFlowTilemapRenderResources resources, System.Func<GridFlowTilemapCell, bool> FuncCellSelected)
        {
            var textureSize = new IntVector2(tilemap.Width, tilemap.Height) * tileSize;
            float texWidth = textureSize.x;
            float texHeight = textureSize.y;
            float tileSizeU = tileSize / texWidth;
            float tileSizeV = tileSize / texHeight;

            var quadsByMaterial = new Dictionary<Material, List<TilemapRenderQuad>>();
            var materialDefault = resources.materials.GetMaterial(Texture2D.whiteTexture);
            var oneWayTexture = resources.iconOneWayDoor;

            for (int y = 0; y < tilemap.Height; y++)
            {
                for (int x = 0; x < tilemap.Width; x++)
                {
                    var cell = tilemap.Cells[x, y];
                    var selected = FuncCellSelected.Invoke(cell);

                    Color tileColor;
                    bool canUseCustomColor = cell.CellType != GridFlowTilemapCellType.Door
                        && cell.CellType != GridFlowTilemapCellType.Wall
                        //&& cell.CellType != GridFlowTilemapCellType.Empty
                        ;

                    if (canUseCustomColor && cell.UseCustomColor)
                    {
                        tileColor = cell.CustomColor;
                        if (selected)
                        {
                            tileColor = GetSelectedCellColor(tileColor);
                        }
                    }
                    else
                    {
                        tileColor = GetCellColor(cell);
                    }

                    if (cell.CustomCellInfo != null && cell.CellType == GridFlowTilemapCellType.Custom)
                    {
                        tileColor = cell.CustomCellInfo.defaultColor;
                    }

                    tileColor.a = 1;
                    float x0 = (x * tileSize) / texWidth;
                    float y0 = (y * tileSize) / texHeight;
                    float x1 = x0 + tileSizeU;
                    float y1 = y0 + tileSizeV;

                    var v0 = new TilemapRenderVert(new Vector2(x0, y0), new Vector2(0, 1));
                    var v1 = new TilemapRenderVert(new Vector2(x0, y1), new Vector2(0, 0));
                    var v2 = new TilemapRenderVert(new Vector2(x1, y1), new Vector2(1, 0));
                    var v3 = new TilemapRenderVert(new Vector2(x1, y0), new Vector2(1, 1));
                    var quad = new TilemapRenderQuad(v0, v1, v2, v3, tileColor, 0);

                    AddLayerQuad(quadsByMaterial, quad, materialDefault);

                    var overlay = cell.Overlay;
                    if (overlay != null)
                    {
                        float overlayScale = 0.5f;
                        var overlayQuad = quad.Clone();
                        var shrinkY = (overlayQuad.verts[1].position.y - overlayQuad.verts[0].position.y) * Mathf.Clamp01(1 - overlayScale) * 0.5f;
                        var shrinkX = (overlayQuad.verts[2].position.x - overlayQuad.verts[1].position.x) * Mathf.Clamp01(1 - overlayScale) * 0.5f;

                        overlayQuad.verts[0].position.x += shrinkX;
                        overlayQuad.verts[0].position.y += shrinkY;
                        overlayQuad.verts[1].position.x += shrinkX;
                        overlayQuad.verts[1].position.y -= shrinkY;
                        overlayQuad.verts[2].position.x -= shrinkX;
                        overlayQuad.verts[2].position.y -= shrinkY;
                        overlayQuad.verts[3].position.x -= shrinkX;
                        overlayQuad.verts[3].position.y += shrinkY;
                        overlayQuad.color = overlay.color;
                        overlayQuad.z = 1;
                        AddLayerQuad(quadsByMaterial, overlayQuad, materialDefault);
                    }

                    if (cell.CellType == GridFlowTilemapCellType.Door)
                    {
                        var doorMeta = cell.Userdata as GridFlowTilemapCellDoorInfo;
                        if (doorMeta != null)
                        {
                            if (doorMeta.oneWay)
                            {
                                var doorQuad = quad.Clone();
                                doorQuad.color = Color.white;
                                if (doorMeta.nodeA.x < doorMeta.nodeB.x) doorQuad.rotateUV = 1;
                                if (doorMeta.nodeA.x > doorMeta.nodeB.x) doorQuad.rotateUV = 3;
                                if (doorMeta.nodeA.y < doorMeta.nodeB.y) doorQuad.rotateUV = 2;
                                if (doorMeta.nodeA.y > doorMeta.nodeB.y) doorQuad.rotateUV = 0;
                                var materialOneWayDoor = resources.materials.GetMaterial(oneWayTexture);

                                AddLayerQuad(quadsByMaterial, doorQuad, materialOneWayDoor);
                            }
                        }
                    }
                }
            }

            var layers = new List<TilemapLayerRenderData>();
            foreach (var entry in quadsByMaterial)
            {
                var layer = new TilemapLayerRenderData();
                layer.material = entry.Key;
                layer.quads = entry.Value.ToArray();
                layers.Add(layer);
            }

            return layers.ToArray();
        }

        private static void AddLayerQuad(Dictionary<Material, List<TilemapRenderQuad>> quadsByMaterial, TilemapRenderQuad quad, Material material)
        {
            if (!quadsByMaterial.ContainsKey(material))
            {
                quadsByMaterial.Add(material, new List<TilemapRenderQuad>());
            }

            quadsByMaterial[material].Add(quad);
        }

        static Color GetSelectedCellColor(Color color)
        {
            float H, S, V;
            Color.RGBToHSV(color, out H, out S, out V);
            S = Mathf.Clamp01(S * 2);
            return Color.HSVToRGB(H, S, V);
        }

        static Color GetCellColor(GridFlowTilemapCell cell)
        {
            switch (cell.CellType)
            {
                case GridFlowTilemapCellType.Empty:
                    return Color.black;

                case GridFlowTilemapCellType.Floor:
                    return Color.white;

                case GridFlowTilemapCellType.Door:
                    return Color.blue;

                case GridFlowTilemapCellType.Wall:
                    return new Color(0.5f, 0.5f, 0.5f);


                default:
                    return Color.magenta;
            }
        }


        struct TilemapRenderVert
        {
            public TilemapRenderVert(Vector2 position, Vector2 uv)
            {
                this.position = position;
                this.uv = uv;
            }

            public Vector2 position;
            public Vector2 uv;

            public TilemapRenderVert Clone()
            {
                return new TilemapRenderVert(position, uv);
            }
        }

        struct TilemapRenderQuad
        {
            public TilemapRenderQuad(TilemapRenderVert v0, TilemapRenderVert v1, TilemapRenderVert v2, TilemapRenderVert v3, Color color, float z)
            {
                verts = new TilemapRenderVert[4];
                verts[0] = v0;
                verts[1] = v1;
                verts[2] = v2;
                verts[3] = v3;
                this.color = color;
                this.z = z;
                rotateUV = 0;
            }

            public TilemapRenderQuad Clone()
            {
                var newQuad = new TilemapRenderQuad(
                    verts[0].Clone(),
                    verts[1].Clone(),
                    verts[2].Clone(),
                    verts[3].Clone(),
                    color, z);

                newQuad.rotateUV = rotateUV;
                return newQuad;
            }

            public TilemapRenderVert[] verts;
            public Color color;
            public float z;
            public int rotateUV;
        }


        struct TilemapLayerRenderData
        {
            public Material material;
            public TilemapRenderQuad[] quads;
        }

    }
}
