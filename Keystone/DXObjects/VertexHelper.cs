using System.Reflection;
using Direct3D = Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;

using System.Runtime.InteropServices;
using MTV3D65;

namespace Keystone.DXObjects
{
    static class VertexHelper
    {
        const string VertexElementsFieldName = "VertexElements";

        public static Direct3D.VertexElement[] GetElements<T>()
        {
            return (Direct3D.VertexElement[]) typeof(T).GetField(VertexElementsFieldName, BindingFlags.Static | BindingFlags.Public).GetValue(null);
        }
    }

    // POSITION ONLY
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionInstance : IVertex, IInstancedVertex
    {
        public static readonly Direct3D.VertexElement[] VertexElements;
        // Position + Texture
        static PositionInstance()
        {
            VertexElements = new[] 
            {
            	//stream, offset, declarationtype, declarationmethod, usage, usageIndex
                new Direct3D.VertexElement(0, 0, Direct3D.DeclarationType.Float3, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.Position, 0),
                new Direct3D.VertexElement(0, 12, Direct3D.DeclarationType.Float1, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.TextureCoordinate, 3), // instance Index
                Direct3D.VertexElement.VertexDeclarationEnd
            };
        }

        public Vector3 Position { get; set; }
        public float InstanceIndex { get; set; }

        public PositionInstance(Vector3 position) : this()
        {
            Position = position;
        }
    }

    // POSITION, NORMAL
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionNormalInstance : IVertex, ILitVertex, IInstancedVertex
    {
        public static readonly Direct3D.VertexElement[] VertexElements;
        // Position, Normal

        static PositionNormalInstance()
        {
            VertexElements = new[] 
            {
                new Direct3D.VertexElement(0, 0, Direct3D.DeclarationType.Float3, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.Position, 0),
                new Direct3D.VertexElement(0, 12, Direct3D.DeclarationType.Float3, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.Normal, 0),
                new Direct3D.VertexElement(0, 24, Direct3D.DeclarationType.Float1, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.TextureCoordinate, 3), // instance Index
                Direct3D.VertexElement.VertexDeclarationEnd
            };
        }

        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public float InstanceIndex { get; set; }

        public PositionNormalInstance(Vector3 position, Vector3 normal) : this()
        {
            Position = position;
            Normal = normal;
        }
    }
    
    // POSITION, NORMAL, UV
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionNormalTextureInstance : IVertex, ILitVertex, IInstancedVertex, ITexturedVertex
    {
        public static readonly Direct3D.VertexElement[] VertexElements;
        // Position, Normal, Texture
        static PositionNormalTextureInstance()
        {
            VertexElements = new[] 
            {
                new Direct3D.VertexElement(0, 0, Direct3D.DeclarationType.Float3, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.Position, 0),
                new Direct3D.VertexElement(0, 12, Direct3D.DeclarationType.Float3, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.Normal, 0),
                new Direct3D.VertexElement(0, 24, Direct3D.DeclarationType.Float2, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.TextureCoordinate, 0),
                new Direct3D.VertexElement(0, 32, Direct3D.DeclarationType.Float1, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.TextureCoordinate, 3), // instance Index
                Direct3D.VertexElement.VertexDeclarationEnd
            };
        }

        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector2 TextureCoordinate { get; set; }
        public float InstanceIndex { get; set; }

        public PositionNormalTextureInstance(Vector3 position, Vector3 normal, Vector2 textureCoordinate) : this()
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
        }
    }
    
    // POSITION, NORMAL, UV, COLOR
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionNormalTextureColorInstance : IVertex, ILitVertex, IInstancedVertex, ITexturedVertex, IColoredVertex
    {
        public static readonly Direct3D.VertexElement[] VertexElements;
        // Position, Normal, Texture, Color
        static PositionNormalTextureColorInstance()
        {
            VertexElements = new[] 
            {
                new Direct3D.VertexElement(0, 0, Direct3D.DeclarationType.Float3, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.Position, 0),
                new Direct3D.VertexElement(0, 12, Direct3D.DeclarationType.Float3, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.Normal, 0),
                new Direct3D.VertexElement(0, 24, Direct3D.DeclarationType.Float2, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.TextureCoordinate, 0),
                new Direct3D.VertexElement(0, 32, Direct3D.DeclarationType.Color, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.Color, 0),
                new Direct3D.VertexElement(0, 36, Direct3D.DeclarationType.Float1, Direct3D.DeclarationMethod.Default, Direct3D.DeclarationUsage.TextureCoordinate, 3), // instance Index
                Direct3D.VertexElement.VertexDeclarationEnd
            };
        }

        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector2 TextureCoordinate { get; set; }
        public int Color { get; set; }
        public float InstanceIndex { get; set; }

        public PositionNormalTextureColorInstance(Vector3 position, Vector3 normal, Vector2 textureCoordinate, TV_COLOR color) : this()
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
            Color = color.GetIntColor();
        }
    }

    interface IVertex
    {
        Vector3 Position { get; set; }
    }
    interface ILitVertex
    {
        Vector3 Normal { get; set; }
    }
    interface IInstancedVertex
    {
        float InstanceIndex { get; set; }
    }
    interface ITexturedVertex
    {
        Vector2 TextureCoordinate { get; set; }
    }
    interface IColoredVertex
    {
        int Color { get; set; }
    }
}
