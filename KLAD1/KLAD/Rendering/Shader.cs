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
            void main()
            {
                gl_Position = projection * model * vec4(aPosition, 0.0, 1.0);
                texCoord = aTexCoord;
            }";

        private const string FragmentShaderSource = @"
            #version 330 core
            in vec2 texCoord;
            out vec4 FragColor;

            // uniform sampler2D texture0;
            uniform vec4 spriteColor; // Fallback color if no texture

            void main()
            {
                // FragColor = texture(texture0, texCoord);
                FragColor = spriteColor;
            }";

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

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            return shader;
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }
}