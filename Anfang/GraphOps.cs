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
            Line line = new Line() { Uid = Uid};
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
                DrawLineCanvas(canvas, 0, row_y, (int)canvas.ActualWidth, row_y, 3, Brushes.Gray, false, Guid);
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
                DrawLineCanvas(canvas, row_x, 0, row_x, (int)canvas.ActualHeight, 3, Brushes.Gray, false, Guid);
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
        public void CanvasLeftClick(Canvas Canvas, object sender, MouseButtonEventArgs e, string Uid, Brush color)
        { // All-in-one method for canvas interaction.
            var clicked_object = e.OriginalSource;

            if (clicked_object == Canvas)
            { // Canvas itself was clicked - we are drawing a new line
                line_points.Add(e.GetPosition(Canvas));
                if (line_points.Count == 2 & Uid != "")
                {
                    if (drawn_items.Contains(Uid) == false)
                    { // Check whether the line is already drawn and draw it.
                        int[] point1 = SnapToGrid((int)line_points[0].X, (int)line_points[0].Y, 50);
                        int[] point2 = SnapToGrid((int)line_points[1].X, (int)line_points[1].Y, 50);
                        DrawLineCanvas(Canvas, point1[0], point1[1], point2[0], point2[1], 3, color, true, Uid);
                        drawn_items.Add(Uid);
                        line_points.Clear();
                        line_points.TrimExcess();
                    }
                    else
                    { // The line is already drawn - abort.
                        line_points.Clear();
                        line_points.TrimExcess();
                    }
                }
                if (line_points.Count == 2 & Uid == "")
                { // This is used to draw a generic line not linked with any branch (Uid is empty).
                    int[] point1 = SnapToGrid((int)line_points[0].X, (int)line_points[0].Y, 50);
                    int[] point2 = SnapToGrid((int)line_points[1].X, (int)line_points[1].Y, 50);
                    DrawLineCanvas(Canvas, point1[0], point1[1], point2[0], point2[1], 3, color, true, Uid);
                    line_points.Clear();
                    line_points.TrimExcess();
                }
            }

            int index = Canvas.Children.IndexOf((UIElement)clicked_object);
            if (index != -1)
            { // This is used to remove items from the canvas. Ignores the gridlines (their Uids contain "Row" or "Col" keywords).
                if (Canvas.Children[index].Uid.Contains("Row") == false & Canvas.Children[index].Uid.Contains("Col") == false)
                {
                    drawn_items.Remove(Canvas.Children[index].Uid);
                    Canvas.Children.Remove((UIElement)clicked_object);
                }
            }
        }
        public int[] SnapToGrid (int x, int y, int gridsize)
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
    }
}
