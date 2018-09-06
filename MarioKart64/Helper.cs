using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartEngine
{
    public abstract class GPU_BufferHelper
    {
        public static Dictionary<DrawData, (int index, int length)> ObjectsInBuffer = 
            new Dictionary<DrawData, (int index, int length)>();

        static VertexBuffer GPU_VERTEXBUFFER;        
        static IndexBuffer GPU_INDEXBUFFER;
        static int size;

        public static void CreateBuffers(GraphicsDevice device, int sizeV = 1, int sizeI = 1)
        {
            GPU_VERTEXBUFFER = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, sizeV, BufferUsage.None);
            GPU_INDEXBUFFER = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, sizeI, BufferUsage.None);
        }

        public static void AddPrimitiveDataToBuffer(DrawData source)
        {
            var _length = source.Vertices.Length;            
            if (false)
            {
                var v = new VertexPositionNormalTexture[_length + size];
                GPU_VERTEXBUFFER.GetData(v);
                for (int i = size; i < v.Length; i++)
                    v[i] = source.Vertices[i - size];
            }
            CreateBuffers(source.device, source.Vertices.Length, source.Indices.Length);
            GPU_VERTEXBUFFER.SetData(source.Vertices);
            GPU_INDEXBUFFER.SetData(source.Indices);
            Upload(source.device);
            ObjectsInBuffer.Add(source, (size, _length));
            size += _length;
        }

        static void Upload(GraphicsDevice d)
        {
            d.SetVertexBuffer(GPU_VERTEXBUFFER);
            d.Indices = GPU_INDEXBUFFER;
        }

        public static (int index, int length) Lookup(DrawData source)
        {
            return ObjectsInBuffer[source];
        }
    }

    class Helper
    {
    }
}
