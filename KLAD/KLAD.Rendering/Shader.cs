using System;
using OpenTK.Graphics.OpenGL4;

namespace KLAD.Rendering
{
    public class Shader : IDisposable
    {
        public int Handle { get; private set; } //id по которому будет обращение к шейдеруы
        //GLSL
        private const string VertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec2 aPosition; 
            layout (location = 1) in vec2 aTexCoord; 

            out vec2 texCoord;
            uniform mat4 model; 
            uniform mat4 projection;
            uniform vec4 uvOffsetScale; 

            void main()
            {
                gl_Position = projection * model * vec4(aPosition, 0.0, 1.0);
                
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
                    
                    if (texColor.a < 0.1) discard;
                    FragColor = texColor;
                } else {
                    FragColor = spriteColor;
                }
            }";
        public Shader()
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, VertexShaderSource); //вызов компиляции текста программы 
            int fragmentShader = CompileShader(ShaderType.FragmentShader, FragmentShaderSource); //вызов компиляции текста программы

            Handle = GL.CreateProgram();// создаёт пустую оболочку для проги в гпу
            GL.AttachShader(Handle, vertexShader);//пришивка вершинного шейдера к проге
            GL.AttachShader(Handle, fragmentShader);//тоже ток для фрагментного
            GL.LinkProgram(Handle);//соединяет 

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);//удаление промежжуточных
            GL.DeleteShader(fragmentShader);//удаление промежуточных
        }
        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);//просит гпу создать объект шейдера
            GL.ShaderSource(shader, source);// передаёт glsl в gpu
            GL.CompileShader(shader);// gpu переводит в двоичный код
            return shader;
        }
        public void Use()
        {
            GL.UseProgram(Handle);//все след объекты рисуй юзая инструкции handle
        }
        //метод указывающий какую картинку использовать для квадрата
        public void SetInt(string name, int data)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, data);
        }
        public void Dispose()
        {
            GL.DeleteProgram(Handle);//удаляет программу из видеокарты
        }
    }
}
