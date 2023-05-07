
using Xamarin.Forms;

namespace Translation.Utils
{
    public static class ImageUtility
    {
        /// <summary>
        /// Method to return FileImageSource from file
        /// </summary>
        /// <param name="fileName">Takes in fileName</param>
        /// <returns>FileImageSource</returns>
        public static FileImageSource ReturnImageSourceFromFile(string fileName)
        {
            return new FileImageSource { File = new FileImageSource { File = fileName } };
        }

        /// <summary>
        /// Method to FontImageSource
        /// </summary>
        /// <param name="fontFamily">Takes in the FontFamily</param>
        /// <param name="glyph">Takes in the glyph</param>
        /// <param name="hexColor">Takes in a color</param>
        /// <param name="size">Takes in the size of the fontImage</param>
        /// <returns>FontImageSource</returns>
        public static FontImageSource ReturnFontImage(string fontFamily, string glyph, string hexColor = null, double size = 25)
        {
            if (!string.IsNullOrEmpty(hexColor))
            {
                return new FontImageSource { FontFamily = fontFamily, Glyph = glyph, Size = size, Color = Color.FromHex(hexColor) };
            }

            return new FontImageSource { FontFamily = fontFamily, Glyph = glyph, Size = size };
        }
    }
}
