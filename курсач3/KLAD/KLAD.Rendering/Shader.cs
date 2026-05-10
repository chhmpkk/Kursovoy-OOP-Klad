using System;
using OpenTK.Graphics.OpenGL4;

namespace KLAD.Rendering
{
    public class Shader : IDisposable
    {
        public int Handle { get; private set; }

        private const string VertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec2 aPosition;
            layout (location = 1) in vec2 aTexCoord;

            out vec2 texCoord;
            uniform mat4 model;
            uniform mat4 projection;
            uniform vec4 uvOffsetScale; // x,y = offset, z,w = scale

            void main()
            {
                gl_Position = projection * model * vec4(aPosition, 0.0, 1.0);
                // Apply UV offset and scale to get the correct sub-sprite
                texCoord = (aTexCoord * uvOffsetScale.zw) + uvOffsetScale.xy;
            }";

        private const string FragmentShaderSource = @"
            #version 330 core
            in vec2 texCoord;
            out vec4 FragColor;

            uniform sampler2D texture0;
            uniform vec4 spriteColor; 
            uniform bool useTexture;

            void main()
            {
                if (useTexture) {
                    vec4 texColor = texture(texture0, texCoord);
                    // Handle transparency
                    if (texColor.a < 0.1) discard;
                    FragColor = texColor;
                } else {
                    FragColor = spriteColor;
                }
            }";

        /// <summary>
        /// Конструктор. Компилирует вершинный и фрагментный шейдеры и связывает их в шейдерную программу.
        /// </summary>
        public Shader()
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, VertexShaderSource);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, FragmentShaderSource);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        /// <summary>
        /// Компилирует отдельный шейдер из исходного кода.
        /// </summary>
        /// <param name="type">Тип шейдера (вершинный или фрагментный).</param>
        /// <param name="source">Исходный код шейдера на языке GLSL.</param>
        /// <returns>Хэндл скомпилированного шейдера.</returns>
        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            return shader;
        }

        /// <summary>
        /// Устанавливает текущую шейдерную программу как активную для рендеринга.
        /// </summary>
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        /// <summary>
        /// Устанавливает целочисленное значение (uniform) для шейдера (например, текстурный юнит).
        /// </summary>
        /// <param name="name">Имя uniform-переменной в шейдере.</param>
        /// <param name="data">Устанавливаемое значение.</param>
        public void SetInt(string name, int data)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, data);
        }

        /// <summary>
        /// Освобождает ресурсы, связанные с шейдерной программой.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }
}