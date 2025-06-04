using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Drawing;
using Open.Diagramming.Forms;

namespace Open.Diagramming.Testing.Units
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class PaletteTests
    {
        [TestMethod]
        public void PaletteCreate()
        {
            Palette palette = new Palette();
            palette.Size = new Size(200, 200);
            palette.Refresh();

            Assert.IsTrue(palette != null);
        }
    }
}
