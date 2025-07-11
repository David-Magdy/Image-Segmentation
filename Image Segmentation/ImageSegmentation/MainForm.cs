using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ImageTemplate;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using Seg = Segmentation.Segmentation;

namespace ImageTemplate
{
    public partial class MainForm : Form
    {
        private Seg segmentation;
        private TextBox txtSegmentSigma, txtK, txtSegmentSizes;
        private Label lblSegmentSigma, lblK;
        public RGBPixel[,] ImageMatrix;

        public MainForm()
        {
            InitializeComponent();
            InitializeSegmentationControls();
        }

        private void InitializeSegmentationControls()
        {
            // Initialize Segment Sigma Label and Text(specifically for this example)
            lblSegmentSigma = new Label
            {
                Text = "Sigma:",
                Location = new Point(16, 480),
                Size = new Size(80, 20),
                Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold)
            };
            this.Controls.Add(lblSegmentSigma);

            txtSegmentSigma = new TextBox
            {
                Location = new Point(96, 480),
                Size = new Size(50, 20),
                Text = "0.8",
                Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold)
            };
            this.Controls.Add(txtSegmentSigma);

            // Initialize K Label and TextBox
            lblK = new Label
            {
                Text = "k:",
                Location = new Point(156, 480),
                Size = new Size(30, 20),
                Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold)
            };
            this.Controls.Add(lblK);

            txtK = new TextBox
            {
                Location = new Point(186, 480),
                Size = new Size(50, 20),
                Text = "300",
                Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold)
            };
            this.Controls.Add(txtK);

            // Add TextBox for segment sizes
            txtSegmentSizes = new TextBox
            {
                Location = new Point(16, 510),
                Size = new Size(220, 100),
                Multiline = true,
                ReadOnly = true,
                Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular)
            };
            this.Controls.Add(txtSegmentSizes);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Empty or add initialization if needed
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string openedFilePath = openFileDialog1.FileName;
                    ImageMatrix = ImageOperations.OpenImage(openedFilePath);
                    ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                    txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
                    txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();

                    segmentation = new Seg { ImageMatrix = ImageMatrix };

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error");
                }
            }
        }

        private void BtnSegment_Click(object sender, EventArgs e)
        {
            if (ImageMatrix == null || segmentation == null)
            {
                MessageBox.Show("Please load an image first!", "Error");
                return;
            }

            try
            {
                if (!double.TryParse(txtSegmentSigma.Text, out double sigma) || sigma <= 0)
                {
                    MessageBox.Show("Invalid sigma value. Please enter a positive number.", "Error");
                    return;
                }
                if (!double.TryParse(txtK.Text, out double k) || k <= 0)
                {
                    MessageBox.Show("Invalid k value. Please enter a positive number.", "Error");
                    return;
                }
                int filterSize = 5;
                RGBPixel[,] smoothedImage = ImageOperations.GaussianFilter1D(ImageMatrix, filterSize, sigma);
                segmentation.Segment(smoothedImage, k);
                MessageBox.Show("Segmentation completed!", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during segmentation: {ex.Message}", "Error");
            }
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            if (ImageMatrix == null || segmentation == null)
            {
                MessageBox.Show("Please load an image first!", "Error");
                return;
            }

            try
            {

                if (segmentation.finalLabels.Count == 0)
                {
                    BtnSegment_Click(sender, e);
                }


                string tempImagePath = Path.Combine(Directory.GetCurrentDirectory(), "tempImage.bmp");
                string tempTextPath = Path.Combine(Directory.GetCurrentDirectory(), "tempText.txt");

                int segmentCount = segmentation.getNumberOfSegments();
                segmentation.SaveOutput(tempTextPath, tempImagePath);

                // Display the segmented image
                RGBPixel[,] segmentedImage = ImageOperations.OpenImage(tempImagePath);
                ImageOperations.DisplayImage(segmentedImage, pictureBox2);

                // Display segment sizes in the TextBox
                var segmentSizes = segmentation.GetSegmentSizes();
                txtSegmentSizes.Text = $"{segmentCount}\r\n{string.Join("\r\n", segmentSizes)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying segmentation: {ex.Message}", "Error");
            }
        }
    }
}
