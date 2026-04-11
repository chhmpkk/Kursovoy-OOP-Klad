using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using KLAD.Models;

namespace KLAD.Rendering
{
    public class GameRenderer : IDisposable
    {
        private int _vao;
        private int _vbo;
        private int _ebo;
        private Shader _shader;

        private readonly float[] _vertices = {
             0.5f,  0.5f,       1.0f, 1.0f,
             0.5f, -0.5f,       1.0f, 0.0f,
            -0.5f, -0.5f,       0.0f, 0.0f,
            -0.5f,  0.5f,       0.0f, 1.0f
        };

        private readonly uint[] _indices = {
            0, 1, 3,
            1, 2, 3
        };

        public GameRenderer()
        {
            InitializeGL();
        }

        private void InitializeGL()
        {
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            _shader = new Shader();
        }

        public void RenderState(GameState state)
        {
            if (state == null || state.Level == null) return;

            _shader.Use();
            GL.BindVertexArray(_vao);

            int width = state.Level.Width;
            int height = state.Level.Height;
            if (width == 0 || height == 0) return;

            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1.0f, 1.0f);
            
            int projLocation = GL.GetUniformLocation(_shader.Handle, "projection");
            GL.UniformMatrix4(projLocation, false, ref projection);

            int modelLocation = GL.GetUniformLocation(_shader.Handle, "model");
            int colorLocation = GL.GetUniformLocation(_shader.Handle, "spriteColor");

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var element = state.Level.Grid[x, y];
                    if (element == null) continue;

                    Vector4 color = GetColorForElement(element.Type);
                    
                    if (element.Type == ElementType.Empty) continue;

                    Matrix4 model = Matrix4.CreateTranslation(x + 0.5f, y + 0.5f, 0.0f);

                    GL.UniformMatrix4(modelLocation, false, ref model);
                    GL.Uniform4(colorLocation, color);

                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                }
            }
            
            DrawPlayer(state.Player1, new Vector4(1.0f, 0.0f, 0.0f, 1.0f), modelLocation, colorLocation);
       
            DrawPlayer(state.Player2, new Vector4(0.0f, 0.0f, 1.0f, 1.0f), modelLocation, colorLocation);
        }

        private void DrawPlayer(Player p, Vector4 color, int modelLoc, int colorLoc)
        {
            if (p == null) return;
            Matrix4 model = Matrix4.CreateTranslation(p.X + 0.5f, p.Y + 0.5f, 0.0f);
            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.Uniform4(colorLoc, color);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

        private Vector4 GetColorForElement(ElementType type)
        {
            return type switch
            {
                ElementType.Wall => new Vector4(0.5f, 0.5f, 0.5f, 1.0f), 
                ElementType.Treasure => new Vector4(1.0f, 1.0f, 0.0f, 1.0f), 
                ElementType.Prize => new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                _ => new Vector4(0.0f, 0.0f, 0.0f, 1.0f) 
            };
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            _shader?.Dispose();
        }
    }
}