using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace KLAD.Rendering
{
    //класс для загзруки картинок в vram
    public class Texture : IDisposable
    {
        public int Handle { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        //вызов только при запуске 
        public Texture(string path)
        {
            Handle = GL.GenTexture();//уникальный id для новой картинки(пустая текстура в opengl)
            Bind();

            using (var image = new Bitmap(path))//читает пнг и загужает в оперативу 
            {
                
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);//переворот для корректного отображения

                Width = image.Width;
                Height = image.Height;

                //блок в памяти  
                var data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),//блок картинки целиком
                    ImageLockMode.ReadOnly,//ток чтение
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);//каждый pxl 32 бита
                //перекидываем в vram
                GL.TexImage2D(TextureTarget.Texture2D,//gpu понимает что 2д изб
                    0,
                    PixelInternalFormat.Rgba,//видеокарта хранит у себяы
                    image.Width,
                    image.Height,
                    0,
                    PixelFormat.Bgra,//переворот из bgra в rgba
                    PixelType.UnsignedByte,//цвет-1 байт(от 0 до 255)
                    data.Scan0);//указатель на первый пиксель картинки

                image.UnlockBits(data);//свобода памяти 
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);//nearest делает картинку чёткой при масштаб
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }
        public void Bind(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);//выбирает слой в видеокарте
            GL.BindTexture(TextureTarget.Texture2D, Handle);//настройка текстурки именно с тем id 
        }
        //удаление текстуры в id  из vram
        public void Dispose()
        {
            GL.DeleteTexture(Handle);
        }
    }
}
