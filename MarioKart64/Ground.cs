using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartEngine
{
    public class Ground : GameObject3D
    {
        static Color GROUND_COLOR = Color.SeaGreen;
        public const float TERRAIN_ENHANCE = (1/5f);
        public const int TEXTURE_REPITITIONS = 1;
        public const int WORLD_BOTTOM = 0, WORLD_TOP = 1000;

        public static int Width, Height;
        public static Vector3 HighestElevation;

        public bool UseGROUND_DATA = false;

        Texture2D _heightmap;

        public Ground(GraphicsDevice device) : base(new DrawData())
        {
            UserPrimitiveData.device = device;
            RenderMode = RenderingMode.UserPrimitive;
        }

        public override void Load(ContentManager Content)
        {
            UserPrimitiveData.Texture = Content.Load<Texture2D>("grass");
            _heightmap = Content.Load<Texture2D>("heightmap");
            base.Load(Content);
        }

        public override void Initialize()
        {
            UserPrimitiveData.SetVertices(GenerateGround(_heightmap, WORLD_BOTTOM, WORLD_TOP));
            UserPrimitiveData.GetIndices(Width, Height);
            UserPrimitiveData.GenerateNormals(Width, Height);
            UserPrimitiveData.BlanketTexture(Width, Height);
            //UserPrimitiveData.UploadToGPU();
            base.Initialize();
        }        

        private static VertexPositionNormalTexture[] GenerateGround(Texture2D heightmap, float min, float max)
        {
            int w = heightmap.Width, h = heightmap.Height;
            var map_colors = new Color[w * h];
            var vertices = new VertexPositionNormalTexture[w * h];
            //TODO: Make async and take out only portions of heightmap for big maps.
            heightmap.GetData(map_colors);
            for (int x = 0; x < heightmap.Width; x++)
                for (int y = 0; y < heightmap.Height; y++)
                {
                    var i = x + y * w;
                    float height = map_colors[i].R * TERRAIN_ENHANCE;
                    if (height > HighestElevation.Z)
                        HighestElevation = new Vector3(x, y, height);
                    vertices[x + y * w] = new VertexPositionNormalTexture(new Vector3(x, y, height), Vector3.Zero, new Vector2(0, 1));
                }
            Width = w;
            Height = h;
            return vertices;
        }
    }
}
