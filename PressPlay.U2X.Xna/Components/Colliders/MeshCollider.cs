﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PressPlay.U2X.Xna.Interfaces;
using Box2D.XNA;

namespace PressPlay.U2X.Xna.Components
{
    public class MeshCollider : Component, IRenderable
    {
        #region ContentProperties
        public string Material { get; set; }
        public bool IsTrigger { get; set; }
        public int[] Triangles { get; set; }
        public Vector3[] Vertices { get; set; }
//        public Vector3[] Normals { get; set; }
        #endregion

        #region Debug drawing
        private BasicEffect effect;
        private VertexPositionColor[] pointList;
        private VertexPositionColor[] collisionLine;
        private VertexPositionColor[] collTriangles;
        #endregion

        public override void Awake()
        {
            // For drawing the original mesh
            pointList = new VertexPositionColor[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                pointList[i] = new VertexPositionColor(Vertices[i], Color.Green);
            }

            List<Vector3> chosenVerts = new List<Vector3>();
            for (int i = 0; i < Triangles.Length; i += 3)
            {
                Vector3 v1 = Vertices[Triangles[i]];
                Vector3 v2 = Vertices[Triangles[i + 1]];
                Vector3 v3 = Vertices[Triangles[i + 2]];

                if (v1.Y < 0 && v2.Y < 0)
                {
                    chosenVerts.Add(v1);
                    chosenVerts.Add(v2);
                }
                if (v2.Y < 0 && v3.Y < 0)
                {
                    chosenVerts.Add(v2);
                    chosenVerts.Add(v3);
                }
                if (v3.Y < 0 && v1.Y < 0)
                {
                    chosenVerts.Add(v3);
                    chosenVerts.Add(v1);
                }
            }
            collisionLine = new VertexPositionColor[chosenVerts.Count];
            for (int i = 0; i < chosenVerts.Count; i++)
            {
                float progress = (float)i / (float)(chosenVerts.Count - 1);
                collisionLine[i] = new VertexPositionColor(new Vector3(chosenVerts[i].X, chosenVerts[i].Y, chosenVerts[i].Z), new Color(progress, 1.0f - progress, 0.0f));
            }
        }

        private void TestVerts(List<VertexPositionColor> colVerts, int vert1, int vert2)
        {
            Vector3 dir = Vertices[vert2] - Vertices[vert1];
            //if (dir.Y > 0.01)
            //{
                colVerts.Add(new VertexPositionColor(Vertices[vert1] + (dir / 2), Color.Red));
            //}
        }

        public override void Start()
        {
        }


        #region IRenderable Members
        public void Draw(SpriteBatch batch)
        {
            if (effect == null)
            {
                effect = new BasicEffect(batch.GraphicsDevice);
                effect.VertexColorEnabled = true;
            }

            effect.World = transform.world;
            effect.View = Camera.main.View();
            effect.Projection = Camera.main.projectionMatrix;

            RasterizerState oldrasterizerState = batch.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.FillMode = FillMode.WireFrame;
            rasterizerState.CullMode = CullMode.None;
            batch.GraphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                batch.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleList,
                    pointList,
                    0,   // vertex buffer offset to add to each element of the index buffer
                    Vertices.Length,   // number of vertices to draw
                    Triangles,
                    0,   // first index element to read
                    Triangles.Length / 3    // number of primitives to draw
                );
                if (collisionLine.Length > 0)
                {
                    batch.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                        PrimitiveType.LineList,
                        collisionLine,
                        0,
                        collisionLine.Length / 2
                    );
                }
            }

            batch.GraphicsDevice.RasterizerState = oldrasterizerState;
        }
        #endregion
    }
}
