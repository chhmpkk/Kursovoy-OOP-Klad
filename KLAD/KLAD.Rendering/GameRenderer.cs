using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using KLAD.Models;

namespace KLAD.Rendering
{
    public class GameRenderer : IDisposable
    {
        private int _vao;// координаты вершин квадрата 
        private int _vbo;// порядок как нужно соединять вершины
        private int _ebo;// хранит всё выше
        private Shader _shader = null!;

        
        private Texture _wallTexture = null!;
        private Texture _grassTexture = null!;
        private Texture _player1Texture = null!;
        private Texture _player2Texture = null!;
        private Texture? _treasureTexture;
        private Texture? _prizeTexture;

        //массив описывающий квадрат 1 на 1, с коордами текстур чтобы видеократа знала как натянуть картинку на квадрат 
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
            LoadTextures();
        }

        private void InitializeGL()//выделение всех ресурсов
        {
            _vao = GL.GenVertexArray();//создание пустой папки 
            GL.BindVertexArray(_vao);//делает папку активной 

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);//работа с буфером данных
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);//копирование в видюху
            //передаём ebo 
            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);
            //объяснение позиций в массиве 
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            //настройка прозрачности
            GL.Enable(EnableCap.Blend);//смешивание цветов 
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _shader = new Shader();//компиляция glsl
            _shader.Use();//программа активна
            _shader.SetInt("texture0", 0);//связка шейдера с текстурным слотом номер 0
        }

        
        private void LoadTextures()
        {
            try
            {
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Maps");//путь к папке 
                
                string wallPath = Path.Combine(baseDir, "wall.png");
                if (File.Exists(wallPath))
                    _wallTexture = new Texture(wallPath);

                string grassPath = Path.Combine(baseDir, "grass.png");
                if (File.Exists(grassPath))
                    _grassTexture = new Texture(grassPath);

                
                string p1Path = Path.Combine(baseDir, "slime2", "Idle", "Slime3_Idle_full.png");
                if (File.Exists(p1Path))
                    _player1Texture = new Texture(p1Path);

                
                string p2Path = Path.Combine(baseDir, "slime1", "Idle", "Slime1_Idle_full.png");
                if (File.Exists(p2Path))
                    _player2Texture = new Texture(p2Path);

                string treasurePath = Path.Combine(baseDir, "treasure.png");
                if (File.Exists(treasurePath))
                    _treasureTexture = new Texture(treasurePath);

                string prizePath = Path.Combine(baseDir, "prize.png");
                if (File.Exists(prizePath))
                    _prizeTexture = new Texture(prizePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при загрузке текстур: " + ex.Message);
            }
        }

        
        public void RenderState(GameState state)
        {
            if (state == null || state.Level == null) return;

            _shader.Use();//запуск шейдеров
            GL.BindVertexArray(_vao);//достаёт из видеопамять папку с параметрами

            int width = state.Level.Width;
            int height = state.Level.Height;
            if (width == 0 || height == 0) return;

            
            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1.0f, 1.0f);//создание плоской камеры
            
            int projLocation = GL.GetUniformLocation(_shader.Handle, "projection");
            GL.UniformMatrix4(projLocation, false, ref projection);

            int modelLocation = GL.GetUniformLocation(_shader.Handle, "model");
            int colorLocation = GL.GetUniformLocation(_shader.Handle, "spriteColor");
            int uvLocation = GL.GetUniformLocation(_shader.Handle, "uvOffsetScale");
            int useTexLocation = GL.GetUniformLocation(_shader.Handle, "useTexture");

            
            Vector4 fullUV = new Vector4(0, 0, 1, -1); 
            //пробегает по каждой ячейке массива в 60фпс и отрисовывает 
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
                        if (_grassTexture != null)
                        {
                            _grassTexture.Bind();
                            GL.Uniform1(useTexLocation, 1);
                            GL.Uniform4(uvLocation, new Vector4(0, 1, 1, -1)); 
                        }
                        else drawAsColor = true;
                    }
                    else if (element.Type == ElementType.Wall || element is TemporaryWallDecorator)
                    {
                        if (_wallTexture != null)
                        {
                            _wallTexture.Bind();
                            GL.Uniform1(useTexLocation, 1);
                            GL.Uniform4(uvLocation, new Vector4(0, 1, 1, -1));
                        }
                        else drawAsColor = true;
                    }
                    else if (element.Type == ElementType.Treasure)
                    {
                        if (_grassTexture != null)
                        {
                            _grassTexture.Bind();
                            GL.Uniform1(useTexLocation, 1);
                            GL.Uniform4(uvLocation, new Vector4(0, 1, 1, -1));
                            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                        }

                        if (_treasureTexture != null)
                        {
                            _treasureTexture.Bind();
                            GL.Uniform1(useTexLocation, 1);
                            GL.Uniform4(uvLocation, new Vector4(0, 1, 1, -1));
                        }
                        else drawAsColor = true;
                    }
                    else if (element.Type == ElementType.Prize)
                    {
                        if (_grassTexture != null)
                        {
                            _grassTexture.Bind();
                            GL.Uniform1(useTexLocation, 1);
                            GL.Uniform4(uvLocation, new Vector4(0, 1, 1, -1));
                            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                        }

                        if (_prizeTexture != null)
                        {
                            _prizeTexture.Bind();
                            GL.Uniform1(useTexLocation, 1);
                            GL.Uniform4(uvLocation, new Vector4(0, 1, 1, -1));
                        }
                        else drawAsColor = true;
                    }
                    else
                    {
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
            
            
            DrawPlayer(state.Player1, _player1Texture, new Vector4(1.0f, 0.0f, 0.0f, 1.0f), modelLocation, colorLocation, useTexLocation, uvLocation);
            DrawPlayer(state.Player2, _player2Texture, new Vector4(0.0f, 0.0f, 1.0f, 1.0f), modelLocation, colorLocation, useTexLocation, uvLocation);
        }

        
        private void DrawPlayer(Player p, Texture tex, Vector4 fallbackColor, int modelLoc, int colorLoc, int useTexLoc, int uvLoc)
        {
            if (p == null) return;
            Matrix4 model = Matrix4.CreateTranslation(p.X + 0.5f, p.Y + 0.5f, 0.0f);
            GL.UniformMatrix4(modelLoc, false, ref model);
            
            if (tex != null)
            {
                tex.Bind();
                GL.Uniform1(useTexLoc, 1);
                
                
                float frameWidth = 64.0f;  
                float frameHeight = 64.0f; 

                float uvScaleX = frameWidth / tex.Width;
                float uvScaleY = -(frameHeight / tex.Height); 

                
                GL.Uniform4(uvLoc, new Vector4(0.0f, frameHeight / tex.Height, uvScaleX, uvScaleY)); 
            }
            else
            {
                GL.Uniform1(useTexLoc, 0);
                GL.Uniform4(colorLoc, fallbackColor);
            }

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

        
        private Vector4 GetColorForElement(ElementType type)//система цветов 
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

        
        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            _shader?.Dispose();
            _wallTexture?.Dispose();
            _grassTexture?.Dispose();
            _player1Texture?.Dispose();
            _player2Texture?.Dispose();
            _treasureTexture?.Dispose();
            _prizeTexture?.Dispose();
        }
    }
}
