using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace KLAD.Rendering
{
    public class Texture : IDisposable
    {
        public int Handle { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// Конструктор. Загружает текстуру из файла и настраивает параметры фильтрации OpenGL.
        /// </summary>
        /// <param name="path">Путь к файлу изображения.</param>
        public Texture(string path)
        {
            Handle = GL.GenTexture();
            Bind();

            using (var image = new Bitmap(path))
            {
                // Флипаем изображение по Y, так как OpenGL ожидает начало координат в левом нижнем углу
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);

                Width = image.Width;
                Height = image.Height;

                // Load the image data
                var data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    image.Width,
                    image.Height,
                    0,
                    PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);

                image.UnlockBits(data);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }

        /// <summary>
        /// Привязывает текстуру к указанному текстурному юниту.
        /// </summary>
        /// <param name="unit">Текстурный юнит (по умолчанию Texture0).</param>
        public void Bind(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        /// <summary>
        /// Освобождает память, выделенную под текстуру в OpenGL.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteTexture(Handle);
        }
    }
}