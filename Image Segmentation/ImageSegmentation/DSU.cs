using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using ImageTemplate;
using GraphConstruction;
using static System.Net.Mime.MediaTypeNames;

namespace Segmenetation
{
    public class Segmenetation : MainForm
    {
        public Graph redGrid, greenGrid, blueGrid;
        public Segmenetation()
        {
            redGrid = new Graph(ImageMatrix, "red");
            greenGrid = new Graph(ImageMatrix, "green");
            blueGrid = new Graph(ImageMatrix, "blue");
        }

    }
    public class DSU 
    {

    }
}
