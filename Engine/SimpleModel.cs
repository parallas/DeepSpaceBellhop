using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine;

public class SimpleModel
{
    public static string RootPath { get; set; } = "Content";

    public Vector3[] Vertices { get; private set; } = [];
    public Vector3[] Normals { get; private set; } = [];
    public Vector2[] TexCoords { get; private set; } = [];
    public int[][][] Faces { get; private set; } = [];
    public Color[] VertexColors { get; private set; } = [];

    private VertexPositionColorNormalTexture[] _buffer = [];

    private SimpleModel() { }

    public void Build()
    {
        List<VertexPositionColorNormalTexture> buff = [];
        for(int f = 0; f < Faces.Length; f++)
        {
            for(int v = 0; v < Faces[f].Length; v++)
            {
                // v1/vt1/vn1
                var i = Faces[f][v];
                buff.Add(new(
                    Vertices[i[0]],
                    VertexColors[i[0]],
                    Normals[i[2]],
                    i[1] != -1 ? TexCoords[i[1]] : Vector2.Zero
                ));
            }
        }
        _buffer = [..buff];
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.DrawUserPrimitives(
            PrimitiveType.TriangleList,
            _buffer,
            0,
            Faces.Length,
            VertexPositionColorNormalTexture.VertexDeclaration
        );
    }

    public static SimpleModel Load(string assetName)
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RootPath, assetName + ".obj");

        using StreamReader reader = new(File.Open(path, FileMode.Open));

        List<Vector3> v = [];
        List<Vector3> vn = [];
        List<Vector2> vt = [];
        List<int[][]> f = [];
        List<Color> vc = [];

        bool seenG = false;

        while(!reader.EndOfStream)
        {
            string line = reader.ReadLine();

            if(line.StartsWith("g "))
            {
                if(seenG) // only load the first mesh
                    break;
                seenG = true;
            }
            else if(line.StartsWith("v "))
            {
                var split = line.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
                v.Add(new Vector3(
                    float.Parse(split[1]),
                    float.Parse(split[2]),
                    float.Parse(split[3])
                ));
            }
            else if(line.StartsWith("vn "))
            {
                var split = line.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
                Console.WriteLine($"{line}, {split[1]}, {split[2]}, {split[3]}");
                vn.Add(new Vector3(
                    float.Parse(split[1]),
                    float.Parse(split[2]),
                    float.Parse(split[3])
                ));
            }
            else if(line.StartsWith("vc ")) // custom vertex color implementation
            {
                var split = line.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
                Console.WriteLine($"{line}, {split[1]}, {split[2]}, {split[3]}");
                vc.Add(new Color(
                    float.Parse(split[1]),
                    float.Parse(split[2]),
                    float.Parse(split[3]),
                    1f
                ));
            }
            else if(line.StartsWith("vt "))
            {
                var split = line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                vt.Add(new Vector2(
                    float.Parse(split[1]),
                    float.Parse(split[2])
                ));
            }
            else if(line.StartsWith("f "))
            {
                var split = line.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
                List<int[]> ind = [];
                for(int i = 1; i < 4; i++)
                {
                    var split2 = split[i].Split('/', 3);
                    ind.Add([
                        int.Parse(split2[0]) - 1,
                        split2[1].Length > 0 ? int.Parse(split2[1]) - 1 : -1,
                        int.Parse(split2[2]) - 1,
                    ]);
                }
                f.Add([..ind]);
            }
        }

        SimpleModel mdl = new SimpleModel {
            Faces = [..f],
            Normals = [..vn],
            TexCoords = [..vt],
            Vertices = [..v],
            VertexColors = [..vc]
        };
        mdl.Build();

        return mdl;
    }
}
