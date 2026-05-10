using System;
using System.IO;
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

        // Textures
        private Texture _tilesetTexture;
        private Texture _player1Texture;
        private Texture _player2Texture;

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

        /// <summary>
        /// �������������� ������, OpenGL ������ � ��������� ��������.
        /// </summary>
        public GameRenderer()
        {
            InitializeGL();
            LoadTextures();
        }

        /// <summary>
        /// ������� � ����������� VAO, VBO, EBO � ������.
        /// </summary>
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

            // Включаем прозрачность (Blending)
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _shader = new Shader();
            _shader.Use();
            _shader.SetInt("texture0", 0);
        }

        /// <summary>
        /// ��������� �������� ��� ������ � ����������.
        /// </summary>
        private void LoadTextures()
        {
            try
            {
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Maps");
                
                string tilesetPath = Path.Combine(baseDir, "set.bmp");
                if (File.Exists(tilesetPath))
                    _tilesetTexture = new Texture(tilesetPath);

                string p1Path = Path.Combine(baseDir, "slime1", "Idle", "Slime1_Idle_full.png");
                if (File.Exists(p1Path))
                    _player1Texture = new Texture(p1Path);

                string p2Path = Path.Combine(baseDir, "slime2", "Idle", "Slime3_Idle_full.png");
                if (File.Exists(p2Path))
                    _player2Texture = new Texture(p2Path);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось загрузить текстуры: " + ex.Message);
            }
        }

        /// <summary>
        /// ������������ ������� ��������� ���� (�������, �����, ������).
        /// </summary>
        /// <param name="state">��������� ����.</param>
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
            int uvLocation = GL.GetUniformLocation(_shader.Handle, "uvOffsetScale");
            int useTexLocation = GL.GetUniformLocation(_shader.Handle, "useTexture");

            // Расчеты UV для set.bmp (тайлы 16x16)
            Vector4 grassUV = new Vector4(0, 0, 1, 1);
            Vector4 wallUV = new Vector4(0, 0, 1, 1);

            if (_tilesetTexture != null)
            {
                float tsW = _tilesetTexture.Width;
                float tsH = _tilesetTexture.Height;
                float tileU = 16.0f / tsW;
                float tileV = 16.0f / tsH;
                int totalRows = _tilesetTexture.Height / 16;

                // Трава: 7-й столбик (индекс 6), 14-я строчка (индекс 13)
                float grassU = (6 * 16.0f) / tsW;
                float grassV = (13 * 16.0f) / tsH;
                grassUV = new Vector4(grassU, grassV, tileU, tileV);

                // Стена: 2-й столбик (индекс 1), 3-я строчка снизу
                float wallU = (1 * 16.0f) / tsW;
                float wallV = ((totalRows - 3) * 16.0f) / tsH;
                wallUV = new Vector4(wallU, wallV, tileU, tileV);
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var element = state.Level.Grid[x, y];
                    if (element == null) continue;

                    Matrix4 model = Matrix4.CreateTranslation(x + 0.5f, y + 0.5f, 0.0f);
                    GL.UniformMatrix4(modelLocation, false, ref model);

                    bool drawAsColor = false;

                    if (element.Type == ElementType.Empty)
                    {
                        if (_tilesetTexture != null)
                        {
                            _tilesetTexture.Bind();
                            GL.Uniform1(useTexLocation, 1);
                            GL.Uniform4(uvLocation, grassUV); 
                        }
                        else drawAsColor = true;
                    }
                    else if (element.Type == ElementType.Wall || element is TemporaryWallDecorator)
                    {
                        if (_tilesetTexture != null)
                        {
                            _tilesetTexture.Bind();
                            GL.Uniform1(useTexLocation, 1);
                            GL.Uniform4(uvLocation, wallUV);
                        }
                        else drawAsColor = true;
                    }
                    else
                    {
                        // Призы и сокровища - пока квадраты
                        drawAsColor = true;
                    }

                    if (drawAsColor)
                    {
                        GL.Uniform1(useTexLocation, 0);
                        GL.Uniform4(colorLocation, GetColorForElement(element.Type));
                    }

                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                }
            }
            
            // Расчеты UV для слаймов
            DrawPlayer(state.Player1, _player1Texture, new Vector4(1.0f, 0.0f, 0.0f, 1.0f), modelLocation, colorLocation, useTexLocation, uvLocation);
            DrawPlayer(state.Player2, _player2Texture, new Vector4(0.0f, 0.0f, 1.0f, 1.0f), modelLocation, colorLocation, useTexLocation, uvLocation);
        }

        /// <summary>
        /// ������������ ���������.
        /// </summary>
        private void DrawPlayer(Player p, Texture tex, Vector4 fallbackColor, int modelLoc, int colorLoc, int useTexLoc, int uvLoc)
        {
            if (p == null) return;
            Matrix4 model = Matrix4.CreateTranslation(p.X + 0.5f, p.Y + 0.5f, 0.0f);
            GL.UniformMatrix4(modelLoc, false, ref model);
            
            if (tex != null)
            {
                tex.Bind();
                GL.Uniform1(useTexLoc, 1);
                
                // --- НАСТРОЙКА РАЗМЕРА КАДРА СЛАЙМА ---
                // Задаем жесткий размер одного кадра в пикселях.
                // Обычно спрайты 22x22 рисуются в сетке 32x32 или 48x48.
                float frameWidth = 32.0f;  // Поменяйте на 48.0f или 64.0f, если слайм обрезан
                float frameHeight = 32.0f; // Поменяйте на 48.0f или 64.0f, если слайм обрезан

                float uvScaleX = frameWidth / tex.Width;
                float uvScaleY = frameHeight / tex.Height;

                // Берем самый первый кадр (левый верхний угол картинки)
                GL.Uniform4(uvLoc, new Vector4(0.0f, 0.0f, uvScaleX, uvScaleY)); 
            }
            else
            {
                GL.Uniform1(useTexLoc, 0);
                GL.Uniform4(colorLoc, fallbackColor);
            }

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

        /// <summary>
        /// ���������� ��������� ���� ��� ��������, ���� �������� �� ���������.
        /// </summary>
        private Vector4 GetColorForElement(ElementType type)
        {
            return type switch
            {
                ElementType.Empty => new Vector4(0.1f, 0.3f, 0.1f, 1.0f),
                ElementType.Wall => new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
                ElementType.Treasure => new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                ElementType.Prize => new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                _ => new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            };
        }

        /// <summary>
        /// ����������� ������� OpenGL.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            _shader?.Dispose();
            _tilesetTexture?.Dispose();
            _player1Texture?.Dispose();
            _player2Texture?.Dispose();
        }
    }
}
