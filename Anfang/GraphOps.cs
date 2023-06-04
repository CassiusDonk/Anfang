using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Threading;

namespace Anfang
{
    public class GraphOps
    {
        List<int> cols_x = new List<int>();
        List<int> rows_y = new List<int>();
        bool grid_drawn = false;
        int id = 0;
        List<Point> line_points = new List<Point>();
        List<String> drawn_items = new List<String>();
        List<Label> labels = new List<Label>();
        List<Object> CanvasChildrenStorage = new List<Object>();
        public int gridsize;
        private int rotation_value = 0;
        int uid_counter = 0;
        public string removed_uid = "";
        public int rotation
        {
            get
            {
                return rotation_value;
            }
            set
            {
                if (value == 4)
                {
                    rotation_value = 0;
                }
                else
                {
                    rotation_value = value;
                }
            }
        }

        public GraphOps()
        {
            rotation = 0;
        }

        public class DrawnItemPosition
        {
            public string Uid { get; set; }
            public double X1 { get; set; }
            public double Y1 { get; set; }
            public double X2 { get; set; }
            public double Y2 { get; set; }
        }

        public List<DrawnItemPosition> positions = new List<DrawnItemPosition>();

        public int[] GetGridPosition(Grid grid)
        {
            int[] position = new int[2];
            var point = Mouse.GetPosition(grid);

            int row = 0;
            int col = 0;
            double accumulatedHeight = 0.0;
            double accumulatedWidth = 0.0;

            foreach (var rowDefinition in grid.RowDefinitions)
            {
                accumulatedHeight += rowDefinition.ActualHeight;
                if (accumulatedHeight >= point.Y)
                {
                    break;
                }
                row++;
            }

            foreach (var columnDefinition in grid.ColumnDefinitions)
            {
                accumulatedWidth += columnDefinition.ActualWidth;
                if (accumulatedWidth >= point.X)
                {
                    break;
                }
                col++;
            }

            position[0] = row;
            position[1] = col;

            return position;
        }

        public int GetNextIdValue()
        {
            id++;
            return id;
        }
        public void DrawLineCanvas(Canvas canvas, int x1, int y1, int x2, int y2, int thickness, Brush color, bool is_enabled, string Uid)
        {
            Line line = new Line() { Uid = Uid };
            //Thickness thickness = new Thickness(101, -11, 362, 250);
            //line.Margin = thickness;
            line.IsEnabled = is_enabled;
            line.Visibility = System.Windows.Visibility.Visible;
            line.StrokeThickness = thickness;
            line.Stroke = color;
            line.X1 = x1;
            line.X2 = x2;
            line.Y1 = y1;
            line.Y2 = y2;
            canvas.Children.Add(line);
        }
        public void DrawRows(Canvas canvas, int size)
        {
            id = 0;
            rows_y.Clear();
            rows_y.TrimExcess();
            int y = size;
            while (y < canvas.ActualHeight)
            {
                rows_y.Add(y);
                y += size;
            }
            foreach (var row_y in rows_y)
            {
                string Guid = "Row" + id.ToString();
                DrawLineCanvas(canvas, 0, row_y, (int)canvas.ActualWidth, row_y, 3, Brushes.GhostWhite, false, Guid);
                id++;
            }
        }
        public void DrawCols(Canvas canvas, int size)
        {
            id = 0;
            cols_x.Clear();
            cols_x.TrimExcess();
            int x = size;
            while (x < canvas.ActualWidth)
            {
                cols_x.Add(x);
                x += size;
            }
            foreach (var row_x in cols_x)
            {
                string Guid = "Col" + id.ToString();
                DrawLineCanvas(canvas, row_x, 0, row_x, (int)canvas.ActualHeight, 3, Brushes.GhostWhite, false, Guid);
                id++;
            }
        }
        public void DrawLineGrid(Canvas canvas, int gridsize)
        {
            if (grid_drawn == false)
            {
                DrawRows(canvas, gridsize);
                DrawCols(canvas, gridsize);
                grid_drawn = true;
            }
        }
        public void CanvasDrawOrRemoveElement(Canvas Canvas, object sender, MouseButtonEventArgs e, string Uid, string LongUID, Brush color, bool freedraw)
        { // All-in-one method for canvas interaction.
            UIElement clicked_object = e.OriginalSource as UIElement;

            if (clicked_object == Canvas & (Uid.Contains("TRAN") | Uid.Contains("TRANM")))
            {
                line_points.Add(e.GetPosition(Canvas));
                if (FindSameUid(Uid, Canvas).Count == 0 & line_points.Count == 1)
                {
                    int[] point1 = SnapToGrid((int)line_points[line_points.Count() - 1].X, (int)line_points[line_points.Count() - 1].Y, gridsize);
                    DrawTransformer(Canvas, point1[0] - gridsize, point1[1] - gridsize, 40, 40, 3, Brushes.Black, Brushes.Black, true, Uid, LongUID);
                }
                line_points.Clear();
                line_points.TrimExcess();
            }

            if (clicked_object == Canvas & (Uid.Contains("GEN")))
            {
                line_points.Add(e.GetPosition(Canvas));
                if (FindSameUid(Uid, Canvas).Count == 0 & line_points.Count == 1)
                {
                    int[] point1 = SnapToGrid((int)line_points[line_points.Count() - 1].X, (int)line_points[line_points.Count() - 1].Y, gridsize);
                    DrawGenerator(Canvas, point1[0], point1[1], 40, 40, 3, Brushes.Black, Brushes.Black, true, Uid);
                }
                line_points.Clear();
                line_points.TrimExcess();
            }

            if (clicked_object == Canvas & (Uid.Contains("BRKR")))
            {
                line_points.Add(e.GetPosition(Canvas));
                if (FindSameUid(Uid, Canvas).Count == 0 & line_points.Count == 1)
                {
                    int[] point1 = SnapToGrid((int)line_points[line_points.Count() - 1].X, (int)line_points[line_points.Count() - 1].Y, gridsize);
                    DrawBreaker(Canvas, point1[0], point1[1], 40, 40, 3, Brushes.Black, Brushes.Black, true, Uid, LongUID);
                }
                line_points.Clear();
                line_points.TrimExcess();
            }

            if (clicked_object == Canvas & (Uid.Contains("LINE")))
            {
                line_points.Add(e.GetPosition(Canvas));
                if (FindSameUid(Uid, Canvas).Count == 0 & line_points.Count == 1)
                {
                    int[] point1 = SnapToGrid((int)line_points[line_points.Count() - 1].X, (int)line_points[line_points.Count() - 1].Y, gridsize);
                    DrawTransLine(Canvas, point1[0], point1[1], 40, 40, 3, Brushes.Black, Brushes.Black, true, Uid, LongUID);
                }
                line_points.Clear();
                line_points.TrimExcess();
            }

            int index = Canvas.Children.IndexOf((UIElement)clicked_object);
            if (index != -1)
            { // This is used to remove items from the canvas. Ignores the gridlines (their Uids contain "Row" or "Col" keywords).
                foreach (UIElement item in FindSameUid(Canvas.Children[index].Uid, Canvas))
                {
                    if (item.Uid.Contains("Row") == false & item.Uid.Contains("Col") == false)
                    {
                        drawn_items.Remove(item.Uid);
                        Canvas.Children.Remove(item);
                        removed_uid = item.Uid;
                        positions.Remove(positions.Find(x => x.Uid == item.Uid));
                    }
                }
            }
        }

        public void CanvasHoverOver(Canvas Canvas, object sender, MouseEventArgs e, string Uid, Brush color, string LongUid)
        {
            if (FindSameUid(Uid, Canvas).Count() == 0)
            {
                Canvas.CaptureMouse();
                foreach (UIElement item in FindSameUid("preview" + Uid[Uid.Count() - 1], Canvas))
                {
                    Canvas.Children.Remove(item);
                }
                Point position = e.GetPosition(Canvas);
                int[] point1 = SnapToGrid((int)position.X, (int)position.Y, gridsize);
                if (Uid.Contains("TRAN") | Uid.Contains("TRANM"))
                {
                    DrawTransformer(Canvas, point1[0] - gridsize, point1[1] - gridsize, 40, 40, 3, Brushes.Gray, Brushes.Gray, true, "preview" + Uid[Uid.Count() - 1], LongUid);
                }
                if (Uid.Contains("GEN"))
                {
                    DrawGenerator(Canvas, point1[0], point1[1], 40, 40, 3, Brushes.Gray, Brushes.Gray, true, "preview" + Uid[Uid.Count() - 1]);
                }
                if (Uid.Contains("BRKR"))
                {
                    DrawBreaker(Canvas, point1[0], point1[1], 40, 40, 3, Brushes.Gray, Brushes.Gray, true, "preview" + Uid[Uid.Count() - 1], LongUid);
                }
                if (Uid.Contains("LINE"))
                {
                    DrawTransLine(Canvas, point1[0], point1[1], 40, 40, 3, Brushes.Gray, Brushes.Gray, true, "preview" + Uid[Uid.Count() - 1], LongUid);
                }
                Canvas.ReleaseMouseCapture();
            }
            else
            {
                foreach (UIElement item in FindSameUid("preview" + Uid[Uid.Count() - 1], Canvas))
                {
                    Canvas.Children.Remove(item);
                }
                Canvas.ReleaseMouseCapture();
            }
            Canvas.ReleaseMouseCapture();
        }

        public void CanvasClearPreview(Canvas Canvas)
        {
            foreach (UIElement item in FindContainUid("preview", Canvas))
            {
                Canvas.Children.Remove(item);
            }
            Canvas.ReleaseMouseCapture();
        }

        public int[] SnapToGrid(int x, int y, int gridsize)
        { // Self - explanatory.
            int[] snapped_to_grid = new int[2];
            foreach (var col_x in cols_x)
            {
                if (col_x > x)
                {
                    if (col_x - x <= (gridsize / 2))
                    {
                        snapped_to_grid[0] = col_x;
                        break;
                    }
                    if (col_x - x > (gridsize / 2))
                    {
                        snapped_to_grid[0] = col_x - gridsize;
                        break;
                    }
                }
            }
            foreach (var row_y in rows_y)
            {
                if (row_y > y)
                {
                    if (row_y - y <= (gridsize / 2))
                    {
                        snapped_to_grid[1] = row_y;
                        break;
                    }
                    if (row_y - y > (gridsize / 2))
                    {
                        snapped_to_grid[1] = row_y - gridsize;
                        break;
                    }
                }
            }
            return snapped_to_grid;
        }
        public void CanvasDisplayResults(Canvas Canvas, CustomObservable branches)
        { // Updates the whole canvas and displays results as labels.
            if (labels.Count > 0)
            {
                foreach (var item in labels)
                {
                    Canvas.Children.Remove(item);
                }
                labels.Clear();
                labels.TrimExcess();
            }
            CanvasReinitialize(Canvas);
            int index_current = 1;
            foreach (var branch in branches)
            {
                for (int i = 0; i <= Canvas.Children.Count - 1; i++)
                {
                    if (Canvas.Children[i].Uid == "Branch" + index_current.ToString() & drawn_items.Contains("Branch" + index_current.ToString()))
                    {
                        Label label = new Label();
                        label.Content = branch.Current.ToString();
                        label.Width = 300;
                        label.Height = 30;
                        double x = ((double)Canvas.Children[i].GetValue(Line.X1Property) + (double)Canvas.Children[i].GetValue(Line.X2Property)) / 2;
                        double y = ((double)Canvas.Children[i].GetValue(Line.Y1Property) + (double)Canvas.Children[i].GetValue(Line.Y2Property)) / 2;
                        label.Margin = new Thickness(x, y, 0, 0);
                        label.Foreground = new SolidColorBrush(Colors.Black);
                        labels.Add(label);
                        break;
                    }
                }
                index_current++;
            }
            foreach (var label in labels)
            {
                Canvas.Children.Add(label);
            }
        }
        public void CanvasReinitialize(Canvas Canvas)
        { // Redraws the entire canvas. Needed to properly update the content within it.
            CanvasChildrenStorage.Clear();
            CanvasChildrenStorage.TrimExcess();
            foreach (var item in Canvas.Children)
            {
                CanvasChildrenStorage.Add(item);
            }
            Canvas.Children.Clear();
            foreach (var item in CanvasChildrenStorage)
            {
                Canvas.Children.Add((UIElement)item);
            }
        }

        public void DrawTransformer(Canvas canvas, int x1, int y1, int width, int height, int thickness, Brush color1, Brush color2, bool is_enabled, string Uid, string LongUid)
        {
            Ellipse ellipse = new Ellipse() { Uid = Uid };
            ellipse.IsEnabled = is_enabled;
            ellipse.Visibility = System.Windows.Visibility.Visible;
            ellipse.StrokeThickness = thickness;
            ellipse.Stroke = color1;
            ellipse.Width = width;
            ellipse.Height = height;
            ellipse.Focusable = false;
            Thickness position = new Thickness(x1, y1, 0, 0);
            ellipse.Margin = position;

            Ellipse ellipse2 = new Ellipse() { Uid = Uid };
            ellipse2.IsEnabled = is_enabled;
            ellipse2.Visibility = System.Windows.Visibility.Visible;
            ellipse2.StrokeThickness = thickness;
            ellipse2.Stroke = color2;
            ellipse2.Width = width;
            ellipse2.Height = height;
            ellipse2.Focusable = false;

            ellipse.Fill = Brushes.Transparent;
            ellipse2.Fill = Brushes.Transparent;

            Thickness position2 = new Thickness(x1 + gridsize, y1, 0, 0);
            Thickness position3 = new Thickness(x1 + 2 * gridsize, y1 + gridsize * 2, 0, 0);
            if (LongUid.Contains("g"))
            {
                if (rotation == 0)
                {
                    DrawGround(canvas, x1 + gridsize, y1 + gridsize * 2, 1, 0, thickness, color1, color2, is_enabled, Uid);
                }
                if (rotation == 1)
                {
                    DrawGround(canvas, x1, y1 + gridsize, 1, 0, thickness, color1, color2, is_enabled, Uid);
                }
                if (rotation == 2)
                {
                    DrawGround(canvas, x1 + gridsize, y1 + gridsize * 2, 1, 0, thickness, color1, color2, is_enabled, Uid);
                }
                if (rotation == 3)
                {
                    DrawGround(canvas, x1 + 2 * gridsize, y1 + gridsize, 1, 0, thickness, color1, color2, is_enabled, Uid);
                }
            }
            if (rotation == 0)
            {
                DrawLineCanvas(canvas, x1, y1 + gridsize, x1 - gridsize, y1 + gridsize, thickness, color1, is_enabled, Uid);
                DrawLineCanvas(canvas, x1 + gridsize * 3, y1 + gridsize, x1 + gridsize * 4, y1 + gridsize, thickness, color2, is_enabled, Uid);
            }
            if (rotation == 1)
            {
                position2 = new Thickness(x1, y1 + gridsize, 0, 0);
                position3 = new Thickness(x1 + 2 * gridsize, y1 + gridsize * 3, 0, 0);
                DrawLineCanvas(canvas, x1 + gridsize, y1, x1 + gridsize, y1 - gridsize, thickness, color1, is_enabled, Uid);
                DrawLineCanvas(canvas, x1 + gridsize, y1 + gridsize * 3, x1 + gridsize, y1 + gridsize * 4, thickness, color2, is_enabled, Uid);
            }
            if (rotation == 2)
            {
                position2 = new Thickness(x1 - gridsize, y1, 0, 0);
                DrawLineCanvas(canvas, x1 + gridsize * 2, y1 + gridsize, x1 + gridsize * 3, y1 + gridsize, thickness, color1, is_enabled, Uid);
                DrawLineCanvas(canvas, x1 - gridsize, y1 + gridsize, x1 - gridsize * 2, y1 + gridsize, thickness, color2, is_enabled, Uid);
            }
            if (rotation == 3)
            {
                position2 = new Thickness(x1, y1 - gridsize, 0, 0);
                DrawLineCanvas(canvas, x1 + gridsize, y1 + gridsize * 2, x1 + gridsize, y1 + gridsize * 3, thickness, color1, is_enabled, Uid);
                DrawLineCanvas(canvas, x1 + gridsize, y1 - gridsize, x1 + gridsize, y1 - gridsize * 2, thickness, color2, is_enabled, Uid);
            }

            TextBlock label = new TextBlock() { Uid = Uid };
            label.Width = 50;
            if (Uid.Contains("TRAN") | Uid.Contains("TRANM"))
            {
                label.Text = "Y-D-11 " + Uid;
                if (Uid.Contains("g"))
                {
                    label.Text = label.Text.Remove(label.Text.Count() - 1);
                }
            }
            else
            {
                label.Text = "Y-D-11";
            }
            label.TextWrapping = TextWrapping.WrapWithOverflow;

            label.Margin = position3;
            label.Background = Brushes.Transparent;

            ellipse2.Margin = position2;

            canvas.Children.Add(ellipse);
            canvas.Children.Add(ellipse2);
            canvas.Children.Add(label);
        }

        public void DrawGenerator(Canvas canvas, int x1, int y1, int width, int height, int thickness, Brush color1, Brush color2, bool is_enabled, string Uid)
        {
            Path path = new Path();
            path.Stroke = color1;
            path.StrokeThickness = thickness;
            path.Fill = Brushes.Transparent;

            EllipseGeometry ellipse = new EllipseGeometry();
            ellipse.Center = new Point(x1, y1);
            ellipse.RadiusX = width / 2;
            ellipse.RadiusY = height / 2;

            LineGeometry line = new LineGeometry();
            line.StartPoint = new Point(x1 + 20, y1);
            line.EndPoint = new Point(x1 + 40, y1);

            line.StartPoint = RotatedPoint(line.StartPoint, x1, y1);
            line.EndPoint = RotatedPoint(line.EndPoint, x1, y1);

            GeometryGroup generator = new GeometryGroup();
            generator.Children.Add(ellipse);
            generator.Children.Add(line);

            path.Data = generator;
            path.Uid = Uid;

            Point text_position = RotatedPoint(new Point(x1 - gridsize, y1 - gridsize * 2), x1, y1);

            TextBlock label = Text(text_position, Uid);

            canvas.Children.Add(path);
            canvas.Children.Add(label);
        }

        public void DrawBreaker(Canvas canvas, int x1, int y1, int width, int height, int thickness, Brush color1, Brush color2, bool is_enabled, string Uid, string LongUid)
        {
            Path path = new Path();
            path.Stroke = color1;
            path.StrokeThickness = thickness;

            if (LongUid.Contains("e") == false)
            {
                path.Fill = Brushes.Transparent;
            }
            else
            {
                path.Fill = color1;
            }

            RectangleGeometry rectangle = new RectangleGeometry();
            rectangle.Rect = new Rect(x1 - 0.625*gridsize, y1 - 0.625*gridsize, gridsize*1.25, gridsize*1.25);

            GeometryGroup breaker = new GeometryGroup();
            breaker.Children.Add(rectangle);

            path.Data = breaker;
            path.Uid = Uid;

            canvas.Children.Add(path);
        }

        public void DrawShortCircuit(Canvas canvas, int x1, int y1, int width, int height, int thickness, Brush color1, Brush color2, bool is_enabled, string Uid, string LongUid)
        {
            Path path = new Path();
            path.Stroke = color1;
            path.StrokeThickness = thickness;
            path.Fill = color1;

            EllipseGeometry ellipse = new EllipseGeometry();
            ellipse.Center = new Point(x1, y1);
            ellipse.RadiusX = thickness;
            ellipse.RadiusY = thickness;

            GeometryGroup geometry = new GeometryGroup();
            geometry.Children.Add(ellipse);

            path.Data = geometry;
            path.Uid = Uid;

            canvas.Children.Add(path);

            TextBlock label = new TextBlock() { Uid = Uid };
            label.Width = 50;
            label.Text = Uid;
            label.TextWrapping = TextWrapping.WrapWithOverflow;

            label.Margin = new Thickness(x1, y1 - gridsize * 2, 0, 0);
            label.Background = Brushes.Transparent;

            canvas.Children.Add(label);
        }

        public void DrawTransLine(Canvas canvas, int x1, int y1, int width, int height, int thickness, Brush color1, Brush color2, bool is_enabled, string Uid, string LongUid)
        {
            Path path = new Path();
            path.Stroke = color1;
            path.StrokeThickness = thickness;

            LineGeometry terminal1 = new LineGeometry();
            terminal1.StartPoint = RotatedPoint(new Point(x1, y1 - gridsize), x1, y1);
            terminal1.EndPoint = RotatedPoint(new Point(x1, y1 + gridsize), x1, y1);

            LineGeometry terminal2 = new LineGeometry();
            terminal2.StartPoint = RotatedPoint(new Point(x1 + 5*gridsize, y1 - gridsize), x1, y1);
            terminal2.EndPoint = RotatedPoint(new Point(x1 + 5*gridsize, y1 + gridsize), x1, y1);

            GeometryGroup TransLine = new GeometryGroup();
            TransLine.Children.Add(terminal1);
            TransLine.Children.Add(terminal2);

            path.Data = TransLine;
            path.Uid = Uid;

            canvas.Children.Add(path);

            Path path2 = new Path();
            path2.Stroke = color1;
            path2.StrokeThickness = thickness * 2;

            LineGeometry line1 = new LineGeometry();
            line1.StartPoint = new Point(x1, y1);
            line1.EndPoint = RotatedPoint(new Point(x1 + 5 * gridsize, y1), x1, y1);

            LineGeometry line2 = new LineGeometry();
            line2.StartPoint = RotatedPoint(new Point(x1, y1 + gridsize / 2), x1, y1);
            line2.EndPoint = RotatedPoint(new Point(x1 + 5 * gridsize, y1 + gridsize / 2), x1, y1);

            LineGeometry line3 = new LineGeometry();
            line3.StartPoint = RotatedPoint(new Point(x1, y1 - gridsize / 2), x1, y1);
            line3.EndPoint = RotatedPoint(new Point(x1 + 5 * gridsize, y1 - gridsize / 2), x1, y1);

            GeometryGroup TransLine2 = new GeometryGroup();
            TransLine2.Children.Add(line1);
            //TransLine2.Children.Add(line2);
            //TransLine2.Children.Add(line3);

            path2.Data = TransLine2;
            path2.Uid = Uid;

            canvas.Children.Add(path2);

            if (Uid.Contains("preview") == false)
            {
                positions.Add(new DrawnItemPosition() { Uid = Uid, X1 = x1, Y1 = y1, X2 = RotatedPoint(new Point(x1 + 5 * gridsize, y1), x1, y1).X, Y2 = RotatedPoint(new Point(x1 + 5 * gridsize, y1), x1, y1).Y });
            }
        }

        public void TripBreakers(Canvas canvas, Powersystem.PowSysElementBase breaker)
        {
            canvas.Dispatcher.Invoke(() =>
            {
                string Uid = breaker.GetUid().Remove(breaker.GetUid().Length - 1);
                if (FindContainUid(breaker.GetUid(), canvas).Count > 0)
                {
                    Path element = FindContainUid(breaker.GetUid(), canvas)[0] as Path;
                    element.Fill = Brushes.Transparent;
                    CanvasReinitialize(canvas);
                }
            });
        }

        public void DrawGround(Canvas canvas, int x1, int y1, int length_grid, int height, int thickness, Brush color1, Brush color2, bool is_enabled, string Uid)
        {
            Line line = new Line() { Uid = Uid };
            line.X1 = x1;
            line.Y1 = y1;
            line.StrokeThickness = thickness;
            line.Stroke = color1;
            line.IsEnabled = is_enabled;


            line.X2 = x1;
            line.Y2 = y1 + gridsize * length_grid;
            if (rotation == 0)
            {
                DrawLine((int)line.X2, (int)line.Y2, (int)line.X2 - 10, (int)line.Y2);
                DrawLine((int)line.X2, (int)line.Y2, (int)line.X2 + 10, (int)line.Y2);
            }
            if (rotation == 1)
            {
                line.X2 = x1 - gridsize * length_grid;
                line.Y2 = y1;
                DrawLine((int)line.X2, (int)line.Y2, (int)line.X2, (int)line.Y2 - 10);
                DrawLine((int)line.X2, (int)line.Y2, (int)line.X2, (int)line.Y2 + 10);
            }
            if (rotation == 2)
            {
                line.X2 = x1;
                line.Y2 = y1 + gridsize * length_grid;
                DrawLine((int)line.X2, (int)line.Y2, (int)line.X2 - 10, (int)line.Y2);
                DrawLine((int)line.X2, (int)line.Y2, (int)line.X2 + 10, (int)line.Y2);
            }
            if (rotation == 3)
            {
                line.X2 = x1 + gridsize * length_grid;
                line.Y2 = y1;
                DrawLine((int)line.X2, (int)line.Y2, (int)line.X2, (int)line.Y2 - 10);
                DrawLine((int)line.X2, (int)line.Y2, (int)line.X2, (int)line.Y2 + 10);
            }
            canvas.Children.Add(line);

            void DrawLine(int x1, int y1, int x2, int y2)
            {
                Line line = new Line() { Uid = Uid };
                line.X1 = x1;
                line.Y1 = y1;
                line.X2 = x2;
                line.Y2 = y2;
                line.StrokeThickness = thickness;
                line.Stroke = color1;
                line.IsEnabled = is_enabled;
                canvas.Children.Add(line);
            }
        }

        Point RotatedPoint(Point point, int center_x, int center_y)
        {
            RotateTransform rotator = new RotateTransform();
            if (rotation == 0)
            {
                rotator.Angle = 0;
            }
            if (rotation == 1)
            {
                rotator.Angle = 90;
            }
            if (rotation == 2)
            {
                rotator.Angle = 180;
            }
            if (rotation == 3)
            {
                rotator.Angle = 270;
            }
            rotator.CenterX = center_x;
            rotator.CenterY = center_y;
            Point rotated = rotator.Transform(point);
            return rotated;
        }

        TextBlock Text(Point point, string Uid)
        {
            TextBlock label = new TextBlock() { Uid = Uid };
            label.Width = 50;
            if (Uid.Contains("GEN"))
            {
                label.Text = Uid;
                if (Uid.Contains("g"))
                {
                    label.Text = label.Text.Remove(label.Text.Count() - 1);
                }
            }
            else
            {
                label.Text = "";
            }
            label.TextWrapping = TextWrapping.WrapWithOverflow;

            Thickness position = new Thickness(point.X, point.Y, 0, 0);
            label.Margin = position;
            label.Background = Brushes.Transparent;
            return label;
        }

        public List<UIElement> FindSameUid(string Uid, Canvas canvas)
        {
            List<UIElement> grouped_elements = new List<UIElement>();
            foreach (UIElement item in canvas.Children)
            {
                if (item.Uid == Uid)
                {
                    grouped_elements.Add(item);
                }
            }
            return grouped_elements;
        }

        public List<UIElement> FindContainUid(string Uid, Canvas canvas)
        {
            List<UIElement> grouped_elements = new List<UIElement>();
            foreach (UIElement item in canvas.Children)
            {
                if (item.Uid.Contains(Uid))
                {
                    grouped_elements.Add(item);
                }
            }
            return grouped_elements;
        }

        public string GenerateUid(string type)
        {
            uid_counter++;
            string Uid = type + uid_counter;
            return Uid;
        }
    }
}
