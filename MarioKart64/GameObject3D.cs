using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartEngine
{
    public struct DrawData
    {
        public PrimitiveType PType;
        public VertexPositionNormalTexture[] Vertices;
        public int[] Indices;
        public int Offset;
        public GraphicsDevice device;
        public Texture2D Texture;

        public bool ShouldUseGPUBuffer;

        internal void Initialize(GraphicsDevice device)
        {
            this.device = device;
            if (Effect is null)
            {
                var effect = new BasicEffect(device);
                effect.TextureEnabled = true;
                effect.Texture = Texture;
                effect.EnableDefaultLighting();
                effect.PreferPerPixelLighting = false;
                Effect = effect;
            }
        }

        /// <summary>
        /// If null, the engine will supply a shader
        /// </summary>
        public BasicEffect Effect;

        public static DrawData RenderTriangles(GraphicsDevice device, Texture2D Texture, VertexPositionNormalTexture[] Vertices)
        {
            var data = new DrawData();
            data.PType = PrimitiveType.TriangleList;
            data.SetVertices(Vertices);
            data.Offset = 0;
            data.device = device;
            data.Texture = Texture;
            return data;
        }

        /// <summary>
        /// Pushes Vertices to the GPU after applying the camera's perspective shader.
        /// </summary>
        public void DirectDraw(GameCamera Camera)
        {
            Effect.View = Camera.GetView;
            Effect.Projection = Camera.GetProjection;
            Effect.World = Matrix.Identity;
            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                switch (ShouldUseGPUBuffer)
                {
                    case false:
                        device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                            Vertices,
                            0,
                            Vertices.Length,
                            Indices,
                            0,
                            Indices.Length / 3,
                            VertexPositionNormalTexture.VertexDeclaration);
                        break;
                    case true:
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                            0, 0, Indices.Length/3);
                        break;
                }
            }
        }

        public void GetIndices(int Width, int Height)
        {
            var indices = new int[(Width - 1) * (Height - 1) * 6];
            int i = 0;
            for (int y = 0; y < Height - 1; y++)
            {
                for (int x = 0; x < Width - 1; x++)
                {
                    int lLeft = x + y * Width; //Lower Left
                    int lRight = (x + 1) + y * Width; //Lower Right
                    int tLeft = x + (y + 1) * Width; //Top Left
                    int tRight = (x + 1) + (y + 1) * Width; //Top Right

                    indices[i++] = tLeft;
                    indices[i++] = lRight;
                    indices[i++] = lLeft;

                    indices[i++] = tLeft;
                    indices[i++] = tRight;
                    indices[i++] = lRight;
                }
            }
            Indices = indices;
        }

        public void GenerateNormals(int Width, int Height)
        {
            for (int i = 0; i < Indices.Length / 3; i++)
            {
                Vector3 firstvec = Vertices[Indices[i * 3 + 1]].Position - Vertices[Indices[i * 3]].Position;
                Vector3 secondvec = Vertices[Indices[i * 3]].Position - Vertices[Indices[i * 3 + 2]].Position;
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();
                Vertices[Indices[i * 3]].Normal += normal;
                Vertices[Indices[i * 3 + 1]].Normal += normal;
                Vertices[Indices[i * 3 + 2]].Normal += normal;
            }
            for (int i = 0; i < Vertices.Length; i++)
                Vertices[i].Normal.Normalize();
        }

        public void BlanketTexture(int Width, int Height)
        {
            for (int y = 0; y < Height - 1; y++)
            {
                for (int x = 0; x < Width - 1; x++)
                {
                    Vertices[x + y * Width].TextureCoordinate = new Vector2(
                        x / (Width - 1f), y / (Height - 1f));
                }
            }
        }        

        public void SetVertices(VertexPositionNormalTexture[] data)
        {
            Vertices = data;
        }
        
        /// <summary>
        /// This will clear any data already stored in the buffers on the GPU with the new data.
        /// </summary>
        public void UploadToGPU()
        {
            GPU_BufferHelper.AddPrimitiveDataToBuffer(this);
            ShouldUseGPUBuffer = true;
        }
    }

    public class GameObject3D : GameComponent
    {
        public enum RenderingMode
        {
            None,
            FromTarget,
            FromModel,
            UserPrimitive
        }
        public RenderingMode RenderMode
        {
            get;
            internal set;
        }

        public RenderTarget3D RenderTarget
        {
            get;
            internal set;
        }

        public Model Model
        {
            get;
            internal set;
        }
        
        public DrawData UserPrimitiveData;

        public Matrix World = Matrix.Identity;

        public GameObject3D(DrawData data)
        {
            UserPrimitiveData = data;
            RenderMode = RenderingMode.UserPrimitive;
        }

        public GameObject3D(Model model)
        {
            Model = model;
            RenderMode = RenderingMode.FromModel;
        }

        public GameObject3D(RenderTarget3D target)
        {
            RenderTarget = target;
            RenderMode = RenderingMode.FromTarget;
        }        

        public virtual void Initialize()
        {
            switch (RenderMode)
            {
                case RenderingMode.FromModel:
                    foreach(var mesh in Model.Meshes)
                    {
                        foreach(BasicEffect effect in mesh.Effects)
                        {
                            effect.EnableDefaultLighting();
                            effect.PreferPerPixelLighting = true;                            
                        }
                    }
                    break;
                case RenderingMode.UserPrimitive:
                    UserPrimitiveData.Initialize(UserPrimitiveData.device);
                    break;
            }
        }

        public override void Draw(GameCamera Camera)
        {
            switch (RenderMode)
            {
                case RenderingMode.FromModel:
                    Model.Draw(World, Camera.GetView, Camera.GetProjection);
                    break;
                case RenderingMode.UserPrimitive:
                    UserPrimitiveData.DirectDraw(Camera);
                    break;
            }
        }        
    }
}
