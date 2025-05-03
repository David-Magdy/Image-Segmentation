using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ImageTemplate;
using Segmenetation;

namespace ImageTemplate
{
    public partial class MainForm : Form
    {
        private Segmentation segmentation;
        private TextBox txtSegmentSigma, txtK;
        private Label lblSegmentSigma, lblK;
        public RGBPixel[,] ImageMatrix;

        public MainForm()
        {
            InitializeComponent();
            InitializeSegmentationControls();
        }

        private void InitializeSegmentationControls()
        {
            // Initialize Segment Sigma Label and TextBox
            lblSegmentSigma = new Label
            {
                Text = "Segment Sigma:",
                Location = new Point(16, 480), // Below panels (y=471), matching screenshot
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

            // Add Label for segment count
            var lblSegmentCount = new Label
            {
                Text = "Segments: 0",
                Location = new Point(466, 510), // Below text boxes, aligned with buttons
                Size = new Size(100, 20),
                Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold)
            };
            this.Controls.Add(lblSegmentCount);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Empty or add initialization if needed
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

                    segmentation = new Segmentation { ImageMatrix = ImageMatrix };
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

                segmentation.ImageMatrix = ImageMatrix;
                segmentation.Segment(sigma, k);
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

            if (segmentation.finalLabels.Count == 0)
            {
                BtnSegment_Click(sender, e); // Reuse existing segmentation logic
            }

            try
            {
                // Use a temporary file to save the segmented image
                string tempImagePath = System.IO.Path.GetTempFileName() + ".png";
                string tempTextPath = System.IO.Path.GetTempFileName() + ".txt";

                // Save the segmentation results
                int segmentCount = segmentation.getNumberOfSegments();
                segmentation.SaveOutput(tempTextPath, tempImagePath);

                // Display the segmented image
                RGBPixel[,] segmentedImage = ImageOperations.OpenImage(tempImagePath);
                ImageOperations.DisplayImage(segmentedImage, pictureBox2);

                // Update the segment count label
                var lblSegmentCount = this.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Segments:"));
                if (lblSegmentCount != null)
                {
                    lblSegmentCount.Text = $"Segments: {segmentCount}";
                }

                // Clean up temporary files
                System.IO.File.Delete(tempImagePath);
                System.IO.File.Delete(tempTextPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying segmentation: {ex.Message}", "Error");
            }
        }
    }
}