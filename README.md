# Image Segmentation Project

## Overview
This project is part of design and analysis of algorithms course that implements an image segmentation algorithm using a graph-based approach inspired by the Felzenszwalb-Huttenlocher method. The application is built in C# using Windows Forms, allowing users to load an image, apply Gaussian smoothing, segment the image into regions based on color similarity, and visualize the results. The segmentation process leverages a Disjoint Set Union (DSU) data structure for efficient component merging and supports parallel processing for red, green, and blue channels.

## Features
- **Image Loading and Display**: Load images in various formats (24-bit RGB, 32-bit ARGB, 8-bit indexed) and display them on a PictureBox.
- **Gaussian Smoothing**: Apply a 1D Gaussian filter to enhance edge detection, with configurable sigma and filter size.
- **Graph-Based Segmentation**: Segment images by constructing a graph where pixels are nodes and edges represent color differences, using a DSU for efficient merging.
- **Parallel Processing**: Process red, green, and blue channels concurrently to improve performance.
- **Output Visualization**: Display the segmented image with random colors assigned to each segment and save segment sizes to a text file.
- **User Interface**: Windows Forms interface with controls for specifying sigma and k parameters, loading images, and viewing results.

## Algorithm Description
The segmentation algorithm follows these steps:
1. **Image Loading**: The input image is loaded into a 2D array of `RGBPixel` structs.
2. **Gaussian Smoothing**: A 1D Gaussian filter is applied to the image to reduce noise and enhance edge detection, controlled by the `sigma` parameter.
3. **Graph Construction**: A graph is built where each pixel is a node, and edges connect neighboring pixels (down, down-left, down-right, right) with weights based on color differences.
4. **Channel-Wise Segmentation**: Each color channel (red, green, blue) is segmented independently using the DSU:
   - Edges are sorted by weight using counting sort (weights range from 0 to 255).
   - For each edge, components are merged if the edge weight is below a dynamic threshold, controlled by the `k` parameter.
5. **Final Segmentation**: Pixels with identical label triplets across all channels are merged into final segments using another DSU pass.
6. **Output Generation**: Segments are assigned random colors, and the segmented image is saved as a BMP file. Segment sizes are written to a text file and displayed in the UI.

## Usage
1. **Run the Application**: Launch the program to open the Windows Forms interface.
2. **Load an Image**: Click the "Open" button to select an image file.
3. **Set Parameters**:
   - **Sigma**: Controls the Gaussian filter's spread (default: 0.8).
   - **k**: Controls the segmentation granularity (default: 300; higher values produce larger segments).
4. **Segment the Image**: Click the "Segment" button to process the image.
5. **View Results**: Click the "Show" button to display the segmented image and segment sizes. The segmented image is saved as `tempImage.bmp`, and segment sizes are saved as `tempText.txt`.

## Dependencies
- .NET Framework (Windows Forms for UI)
- System.Drawing for image processing
- No external libraries required

## Performance Considerations
- **Parallel Processing**: The use of `Parallel.Invoke` speeds up channel-wise segmentation.
- **Counting Sort**: Optimizes edge sorting for weights in the range [0, 255].
- **Path Compression in DSU**: Improves the efficiency of find operations in the Disjoint Set Union structure.

## Building and Running
1. Clone the repository:
   ```bash
   git clone <repository-url>
   ```
2. Open the solution in Visual Studio.
3. Build and run the project to launch the Windows Forms application.

## Example Output
- **Segmented Image**: A BMP file (`tempImage.bmp`) with each segment colored randomly.
- **Segment Sizes**: A text file (`tempText.txt`) listing the number of segments followed by their sizes in descending order.
- **UI Display**: The segmented image is shown in a PictureBox, and segment sizes are displayed in a multiline TextBox.

## Screenshots
Below are example images showing the input and output of the segmentation process:

- **Example 1**
  ![image]("/Images/Figure_1.png")

- **Example 2**
  ![image_2]("/Images/Figure_2.png")


## License
This project is licensed under the MIT License.
