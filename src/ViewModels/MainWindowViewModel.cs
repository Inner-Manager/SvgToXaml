using Avalonia.Controls.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

namespace SvgToXaml.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Enums

        public enum OutputType
        {
            WpfPath,
            DrawingBrush,
            AvaloniaStreamGeometry
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MainWindowViewModel()
        {
            SelectedOutputType = OutputTypes.First();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Application version
        /// </summary>
        public string Version => App.Version;

        /// <summary>
        /// Input SVG
        /// </summary>
        public string InputSvg
        {
            get => _InputSvg;
            set => SetField(ref _InputSvg, value, nameof(InputSvg));
        }

        /// <summary>
        /// Output SVG as selected output type
        /// </summary>
        public string OutputSvg
        {
            get => _OutputSvg;
            set => SetField(ref _OutputSvg, value, nameof(OutputSvg));
        }

        /// <summary>
        /// Selected output type from dropdown
        /// </summary>
        public KeyValuePair<OutputType, string> SelectedOutputType
        {
            get => _SelectedOutputType;
            set => SetField(ref _SelectedOutputType, value, nameof(SelectedOutputType));
        }

        public Dictionary<OutputType, string> OutputTypes => new()
        {
            { OutputType.WpfPath, "WPF Path (Single Color)" },
            { OutputType.DrawingBrush, "WPF/Avalonia Drawing Brush (Multi Color)" },
            { OutputType.AvaloniaStreamGeometry, "Avalonia Stream Geometry (Single Color)" },
        };

        /// <summary>
        /// Object Key
        /// </summary>
        public string Key
        {
            get => _Key;
            set => SetField(ref _Key, value, nameof(Key));
        }

        /// <summary>
        /// Notification Manager
        /// </summary>
        public WindowNotificationManager? NotificationManager { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds notification
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public void AddNotification(string title, string message, NotificationType type)
        {
            NotificationManager?.Show(new Notification(title, message, type));
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Input SVG
        /// </summary>
        private string _InputSvg;

        /// <summary>
        /// Output SVG as selected output type
        /// </summary>
        private string _OutputSvg;

        /// <summary>
        /// Selected output type from dropdown
        /// </summary>
        private KeyValuePair<OutputType, string> _SelectedOutputType;

        /// <summary>
        /// Object Key
        /// </summary>
        private string _Key;

        #endregion

        #region Private Methods

        /// <summary>
        /// Convert to WPF Path
        /// </summary>
        /// <returns></returns>
        private string SvgToWpfPath()
        {
            try
            {
                // Create an XML document and load the SVG content
                var svgDoc = new XmlDocument();
                svgDoc.LoadXml(InputSvg);

                // Check for invalid elements
                CheckSvg(svgDoc);

                // Create a StringBuilder to construct the XAML string
                var xamlBuilder = new StringBuilder();

                // Start building the XAML structure
                xamlBuilder.AppendLine($"<Path x:Key=\"{Key}\" Fill=\"Black\">");
                xamlBuilder.AppendLine("    <Path.Data>");
                xamlBuilder.AppendLine("        <GeometryGroup>");

                // Select all <path> elements in the SVG
                var pathElements = svgDoc.GetElementsByTagName("path");

                // Iterate over each <path> element and extract the 'd' attribute (path data)
                foreach (XmlNode pathElement in pathElements)
                {
                    if (pathElement.Attributes == null) continue;

                    // Get the 'd' attribute (SVG path data)
                    string pathData = pathElement.Attributes["d"]?.Value;

                    if (!string.IsNullOrEmpty(pathData))
                    {
                        // Append the PathGeometry XAML for the current path data
                        xamlBuilder.AppendLine($"           <PathGeometry Figures=\"{pathData}\" />");
                    }
                }

                // Close the XAML structure
                xamlBuilder.AppendLine("        </GeometryGroup>");
                xamlBuilder.AppendLine("    </Path.Data>");
                xamlBuilder.AppendLine("</Path>");

                // Return the constructed XAML string
                return xamlBuilder.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                AddNotification("Error", $"Error converting SVG\n{e.Message}", NotificationType.Error);
                return string.Empty;
            }
        }

        /// <summary>
        /// Convert to WPF/Avalonia Drawing Brush
        /// </summary>
        /// <returns></returns>
        private string SvgToDrawingBrush()
        {
            try
            {
                var hexPattern =
                    @"#([a-fA-F0-9]{6})\b"; // This regex pattern matches a '#' followed by exactly 6 hex digits

                // Create an XML document and load the SVG content
                var svgDoc = new XmlDocument();
                svgDoc.LoadXml(InputSvg);

                // Check for invalid elements
                CheckSvg(svgDoc);

                // Create a StringBuilder to construct the XAML string
                var xamlBuilder = new StringBuilder();

                // Start building the XAML structure
                xamlBuilder.AppendLine($"<DrawingBrush x:Key=\"{Key}\" Stretch=\"Uniform\">");
                xamlBuilder.AppendLine("    <DrawingBrush.Drawing>");
                xamlBuilder.AppendLine("        <DrawingGroup>");
                xamlBuilder.AppendLine("            <DrawingGroup.Children>");

                // Select all <path> elements in the SVG
                var pathElements = svgDoc.GetElementsByTagName("path");

                // Iterate over each <path> element and extract the 'd' attribute (path data)
                foreach (XmlNode pathElement in pathElements)
                {
                    if (pathElement.Attributes == null) continue;

                    // Get the 'd' attribute (SVG path data)
                    var pathData = pathElement.Attributes["d"]?.Value;
                    var styleData = pathElement.Attributes["style"]?.Value;
                    var fillData = pathElement.Attributes["fill"]?.Value;

                    if (string.IsNullOrEmpty(pathData)) continue;

                    var brush = "#FFFFFF";
                    if (!string.IsNullOrWhiteSpace(styleData))
                    {
                        var match = Regex.Match(styleData, hexPattern);
                        brush = match.Success ? match.Value : "#FFFFFF";
                    }
                    else if (!string.IsNullOrWhiteSpace(fillData))
                    {
                        var match = Regex.Match(fillData, hexPattern);
                        brush = match.Success ? match.Value : "#FFFFFF";
                    }

                    // Append the PathGeometry XAML for the current path data
                    xamlBuilder.AppendLine(
                        $"               <GeometryDrawing Brush=\"{brush}\" Geometry=\"{pathData}\" />");
                }

                // Close the XAML structure
                xamlBuilder.AppendLine("            </DrawingGroup.Children>");
                xamlBuilder.AppendLine("        </DrawingGroup>");
                xamlBuilder.AppendLine("    </DrawingBrush.Drawing>");
                xamlBuilder.AppendLine("</DrawingBrush>");

                // Return the constructed XAML string
                return xamlBuilder.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                AddNotification("Error", $"Error converting SVG\n{e.Message}", NotificationType.Error);
                return string.Empty;
            }
        }

        /// <summary>
        /// Convert to Avalonia Stream Geometry
        /// </summary>
        /// <returns></returns>
        private string SvgToAvaloniaStreamGeometry()
        {
            try
            {
                // Create an XML document and load the SVG content
                var svgDoc = new XmlDocument();
                svgDoc.LoadXml(InputSvg);

                // Check for invalid elements
                CheckSvg(svgDoc);

                // Create a StringBuilder to construct the XAML string
                var xamlBuilder = new StringBuilder();

                // Start building the XAML structure
                xamlBuilder.AppendLine($"<StreamGeometry x:Key=\"{Key}\">");

                // Select all <path> elements in the SVG
                var pathElements = svgDoc.GetElementsByTagName("path");

                // Iterate over each <path> element and extract the 'd' attribute (path data)
                foreach (XmlNode pathElement in pathElements)
                {
                    if (pathElement.Attributes == null) continue;

                    // Get the 'd' attribute (SVG path data)
                    string pathData = pathElement.Attributes["d"]?.Value;

                    if (!string.IsNullOrEmpty(pathData))
                    {
                        // Append the PathGeometry XAML for the current path data
                        xamlBuilder.AppendLine($"   {pathData}");
                    }
                }

                // Close the XAML structure
                xamlBuilder.AppendLine("</StreamGeometry>");

                // Return the constructed XAML string
                return xamlBuilder.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                AddNotification("Error", $"Error converting SVG\n{e.Message}", NotificationType.Error);
                return string.Empty;
            }
        }

        /// <summary>
        /// Checks svg for tags that cannot be converted
        /// </summary>
        private void CheckSvg(XmlDocument svgXml)
        {
            // List of allowed tags
            string[] allowedTags = { "svg", "g", "path", "?xml" };

            // Recursively check each node
            if (CheckInvalidNodes(svgXml.DocumentElement, allowedTags))
                AddNotification("Warning", 
                    "SVG Contains elements that cannot be converted\n\nPlease use the SVG Shape to Path converter", 
                    NotificationType.Warning);
        }

        /// <summary>
        /// Checks children of given xml node for invalid tags. Returns true if invalid
        /// </summary>
        /// <param name="node"></param>
        /// <param name="allowedTags"></param>
        /// <returns></returns>
        private bool CheckInvalidNodes(XmlNode node, string[] allowedTags)
        {
            // Check if the current node is an element node and its name is not in the allowed tags
            if (node.NodeType == XmlNodeType.Element && !Array.Exists(allowedTags, tag => tag == node.Name))
            {
                return true; // Found an invalid element
            }

            // Recursively check child nodes
            return node.ChildNodes.Cast<XmlNode>().Any(childNode => CheckInvalidNodes(childNode, allowedTags));
        }

        #endregion

        #region Convert Command

        /// <summary>
        /// Converts SVG to desired output
        /// </summary>
        public void Convert()
        {
            if (string.IsNullOrWhiteSpace(InputSvg))
            {
                AddNotification("Error", "Input SVG is empty", NotificationType.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(Key))
                AddNotification("Warning", "Element Key is Blank", NotificationType.Warning);

            OutputSvg = SelectedOutputType.Key switch
            {
                OutputType.WpfPath => SvgToWpfPath(),
                OutputType.DrawingBrush => SvgToDrawingBrush(),
                OutputType.AvaloniaStreamGeometry => SvgToAvaloniaStreamGeometry(),
                _ => OutputSvg
            };
        }

        #endregion

        #region Open Shape to Path

        /// <summary>
        /// Opens SVG Shape to Path converter
        /// </summary>
        public void OpenShapeToPath()
        {
            const string url = "https://thednp.github.io/svg-path-commander/convert.html";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
        }

        #endregion
    }
}
