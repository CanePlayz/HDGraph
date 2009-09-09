﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HDGraph.Interfaces.ScanEngines;
using HDGraph.Interfaces.DrawEngines;
using System.Globalization;

namespace HDGraph.WpfDrawEngine
{
    /// <summary>
    /// Interaction logic for TreeGraph.xaml
    /// </summary>
    public partial class TreeGraph : UserControl
    {
        public TreeGraph()
        {
            InitializeComponent();
            labelStatus.Content = "Acceleration : " + WpfUtils.GetAccelerationType().ToString();
        }

        public event EventHandler<NodeContextEventArgs> ContextMenuRequired;

        public bool IsRotating
        {
            get { return (bool)GetValue(IsRotatingProperty); }
            set { SetValue(IsRotatingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRotating.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRotatingProperty =
            DependencyProperty.Register("IsRotating", typeof(bool), typeof(TreeGraph), new UIPropertyMetadata(false));



        /// <summary>
        /// Epaisseur d'un niveau sur le graph.
        /// </summary>
        private double singleLevelHeight;

        private DrawOptions currentWorkingOptions;
        private IDirectoryNode rootNode;

        public void SetRoot(IDirectoryNode root, DrawOptions options)
        {
            if (root == null || options == null)
                return;
            this.rootNode = root;


            // Création du bitmap buffer
            currentWorkingOptions = options;

            // init des données du calcul
            //singleLevelHeight = Convert.ToDouble(
            //                Math.Min(this.Width / currentWorkingOptions.ShownLevelsCount / 2,
            //                         this.Height / currentWorkingOptions.ShownLevelsCount / 2));

            // init des données du calcul
            singleLevelHeight = Convert.ToDouble(
                            Math.Min(500 / currentWorkingOptions.ShownLevelsCount / 2,
                                     500 / currentWorkingOptions.ShownLevelsCount / 2));

            labelInfo.Visibility = Visibility.Hidden;
            if (rootNode == null || rootNode.TotalSize == 0)
            {
                PaintSpecialCase();
                return;
            }
            BuildTree(rootNode, 0, 0, 360);
        }


        /// <summary>
        /// Affiche un message spécifique au lieu du graph.
        /// </summary>
        private void PaintSpecialCase()
        {
            labelInfo.Visibility = Visibility.Visible;
            // TODO.

            //string text;
            //if (moteur != null && moteur.WorkCanceled)
            //    text = Resources.ApplicationMessages.UserCanceledAnalysis;
            //else if (rootNode != null && rootNode.TotalSize == 0)
            //    text = Resources.ApplicationMessages.FolderIsEmpty;
            //else
            //    text = Resources.ApplicationMessages.GraphGuideLine;

            //DrawHelper.PrintTextInTheMiddle(frontGraph, currentWorkingOptions.BitmapSize, text, currentWorkingOptions.TextFont, new SolidBrush(Color.Black), false);
        }

        private const float MINIMUM_ANGLE_TO_DRAW = 1;

        /// <summary>
        /// Procédure récursive pour graphiquer les arcs de cercle. Graphique de l'extérieur vers l'intérieur.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rec"></param>
        /// <param name="startAngle"></param>
        /// <param name="endAngle"></param>
        private void BuildTree(IDirectoryNode node, int currentLevel, float startAngle, float endAngle)
        {
            if (node.TotalSize == 0)
                return;
            float nodeAngle = endAngle - startAngle;

            if (node.ExistsUncalcSubDir)
            {
                PaintUnknownPart(node, currentLevel + 1, startAngle, endAngle);
            }
            else
            {
                long cumulSize = 0;
                float currentStartAngle = 0;
                bool multiFolderView = false;
                foreach (IDirectoryNode childNode in node.Children)
                {
                    if (!multiFolderView)
                        currentStartAngle = startAngle + cumulSize * nodeAngle / node.TotalSize;

                    if (childNode.DirectoryType != SpecialDirTypes.FreeSpaceAndHide)
                    {
                        float childAngle = childNode.TotalSize * nodeAngle / node.TotalSize;
                        if (childAngle < MINIMUM_ANGLE_TO_DRAW)
                        {
                            multiFolderView = true;
                        }
                        else
                        {
                            float tempEndAngle = startAngle + cumulSize * nodeAngle / node.TotalSize;
                            if (multiFolderView)
                                PaintMultipleNodesPart(node, currentLevel + 1, currentStartAngle, tempEndAngle);
                            currentStartAngle = tempEndAngle;
                            BuildTree(childNode, currentLevel + 1, currentStartAngle, currentStartAngle + childAngle);
                            multiFolderView = false;
                        }
                        cumulSize += childNode.TotalSize;
                    }
                }
                if (multiFolderView)
                {
                    float tempEndAngle = startAngle + cumulSize * nodeAngle / node.TotalSize;
                    PaintMultipleNodesPart(node, currentLevel + 1, currentStartAngle, tempEndAngle);
                }
                currentStartAngle = startAngle + cumulSize * nodeAngle / node.TotalSize;
                if (node.Children.Count > 0 && node.FilesSize > 0)
                    BuildFilesPart(currentLevel, currentStartAngle, endAngle);
                //if (node.ProfondeurMax <= 1 && endAngle - currentStartAngle > 10)
                //    Console.WriteLine("Processing folder '" + node.Path + "' (Angle:" + startAngle + ";" + endAngle + "; Rec:" + rec + ")...");
            }
            BuildDirPart(node, currentLevel, startAngle, endAngle);
        }

        /// <summary>
        /// Dessine sur l'objet "graph" l'arc de cercle représentant une partie "inconnue" (confettis)
        /// d'un répertoire.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rec"></param>
        /// <param name="startAngle"></param>
        /// <param name="endAngle"></param>
        private void PaintUnknownPart(IDirectoryNode node, int currentLevel, float startAngle, float endAngle)
        {
            Arc2 arc = new Arc2();
            arc.BeginEdit();
            arc.StartAngle = startAngle;
            arc.StopAngle = endAngle - startAngle;
            arc.SmallRadius = Convert.ToSingle(currentLevel * singleLevelHeight);
            arc.LargeRadius = Convert.ToSingle(currentLevel * singleLevelHeight + singleLevelHeight / 6);
            arc.Node = node;
            arc.path1.Style = (Style)FindResource("UncalculatedPart");
            arc.path1.StrokeThickness = 0;
            arc.EndEdit();
            canvas1.Children.Add(arc);
            // TODO : arc.brush1 ==> LargeConfetti

            //frontGraph.FillPie(new System.Drawing.Drawing2D.HatchBrush(
            //                            System.Drawing.Drawing2D.HatchStyle.LargeConfetti,
            //                            Color.Gray,
            //                            Color.White),
            //                    Rectangle.Round(rec), startAngle, nodeAngle);

        }

        private void PaintMultipleNodesPart(IDirectoryNode node, int currentLevel, float startAngle, float endAngle)
        {
            Arc2 arc = new Arc2();
            arc.BeginEdit();
            arc.StartAngle = startAngle;
            arc.StopAngle = endAngle - startAngle;
            arc.SmallRadius = Convert.ToSingle(currentLevel * singleLevelHeight);
            arc.LargeRadius = Convert.ToSingle((currentLevel + 1) * singleLevelHeight);
            arc.Node = node;
            arc.path1.Style = (Style)FindResource("MultipleNodeStyle");
            arc.path1.StrokeThickness = 0;
            arc.EndEdit();
            canvas1.Children.Add(arc);
        }


        /// <summary>
        /// Dessine sur l'objet "graph" l'arc de cercle représentant un répertoire, 
        /// ou dessine le nom de ce répertoire (l'un ou l'autre, pas les 2, en fonction de la valeur de printDirNames).
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rec"></param>
        /// <param name="startAngle"></param>
        /// <param name="nodeAngle"></param>
        private void BuildDirPart(IDirectoryNode node, int currentLevel, float startAngle, float endAngle)
        {
            // on gère les arcs "pleins" (360°) de manière particulière pour avoir un disque "plein", sans trait à l'angle 0
            if ((endAngle - startAngle) == 360)
            {
                Ellipse e = new Ellipse()
                {
                    Width = currentLevel * singleLevelHeight,
                    Height = Width
                };
                canvas1.Children.Add(e);
                // TODO : print text.
            }
            else
            {
                BuildPartialPie(node, currentLevel, startAngle, endAngle);
            }
        }

        /// <summary>
        /// Dessine un semi anneau sur le graph.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rec"></param>
        /// <param name="startAngle"></param>
        /// <param name="nodeAngle"></param>
        private void BuildPartialPie(IDirectoryNode node, int currentLevel, float startAngle, float endAngle)
        {
            Arc2 arc = BuildArc(node, currentLevel, startAngle, endAngle);
            arc.DataContext = node;
            canvas1.Children.Add(arc);
            arc.MouseEnter += new MouseEventHandler(arc_MouseEnter);
            arc.MouseLeave += new MouseEventHandler(arc_MouseLeave);
            ArcToolTip arcTooltip = new ArcToolTip()
            {
                DataContext = node
            };
            arc.ToolTip = arcTooltip;


            Binding b = new Binding()
            {
                Source = rotateTransform,
                Path = new PropertyPath(RotateTransform.AngleProperty),
            };
            BindingOperations.SetBinding(arc, Arc2.TextRotationProperty, b);

            b = new Binding()
            {
                Source = sliderTextSize,
                Path = new PropertyPath(Slider.ValueProperty),
                Mode = BindingMode.OneWay, 
            };
            BindingOperations.SetBinding(arc, Arc2.FontSizeProperty, b);

            



            // TODO : now, apply the correct brush.
            if (node.DirectoryType == SpecialDirTypes.NotSpecial)
            {
                // TODO
                //// standard zone
                //frontGraph.FillPie(
                //    GetBrushForAngles(rec, startAngle, nodeAngle),
                //    Rectangle.Round(rec),
                //    startAngle,
                //    nodeAngle);
                //frontGraph.DrawPie(new Pen(Color.Black, 0.05f), rec, startAngle, nodeAngle);
                //// For tests
                ////float middleAngle = startAngle + (nodeAngle / 2f);
                ////frontGraph.DrawRectangle(new Pen(colorManager.GetNextColor(middleAngle), 0.05f),
                ////                        Rectangle.Round(rec));
                Canvas.SetZIndex(arc, DEFAULT_Z_INDEX_STANDARD_ARC);
            }
            else if (node.DirectoryType == SpecialDirTypes.FreeSpaceAndShow)
            {
                // TODO
                //// free space
                //frontGraph.FillPie(new System.Drawing.Drawing2D.HatchBrush(
                //                            System.Drawing.Drawing2D.HatchStyle.Wave,
                //                            Color.LightGray,
                //                            Color.White),
                //                Rectangle.Round(rec),
                //                startAngle,
                //                nodeAngle);
            }
            else if (node.DirectoryType == SpecialDirTypes.UnknownPart)
            {
                // TODO.
                //// non-calculable files
                //frontGraph.FillPie(new System.Drawing.Drawing2D.HatchBrush(
                //                            System.Drawing.Drawing2D.HatchStyle.Trellis,
                //                            Color.Red,
                //                            Color.White),
                //                Rectangle.Round(rec),
                //                startAngle,
                //                nodeAngle);
            }
        }

        private DivideBy2NumericConverter divideBy2Converter = new DivideBy2NumericConverter(true);

        private const int DEFAULT_Z_INDEX_STANDARD_ARC = 1;
        private const int DEFAULT_Z_INDEX_STANDARD_ARC_OVER = 2;
        private const int DEFAULT_Z_INDEX_ARC_CAPTION = 0;

        void arc_MouseLeave(object sender, MouseEventArgs e)
        {
            Arc2 arc = (Arc2)sender;
            if (arc != null)
            {
                arc.path1.StrokeThickness = 1;
                Canvas.SetZIndex(arc, DEFAULT_Z_INDEX_STANDARD_ARC);
            }
        }

        void arc_MouseEnter(object sender, MouseEventArgs e)
        {
            Arc2 arc = (Arc2)sender;
            if (arc != null)
            {
                arc.path1.StrokeThickness = 3;
                Canvas.SetZIndex(arc, DEFAULT_Z_INDEX_STANDARD_ARC_OVER);
            }
        }


        private Arc2 BuildArc(IDirectoryNode node, int currentLevel, float startAngle, float endAngle)
        {
            Arc2 arc = new Arc2();
            arc.BeginEdit();
            arc.StartAngle = startAngle;
            arc.StopAngle = endAngle - startAngle;
            arc.SmallRadius = Convert.ToSingle(currentLevel * singleLevelHeight);
            arc.LargeRadius = Convert.ToSingle((currentLevel + 1) * singleLevelHeight);
            arc.Node = node;
            arc.EndEdit();
            return arc;
        }

        //private Color myTransparentColor = Color.Black;

        //private Brush GetBrushForAngles(RectangleF rec, float startAngle, float nodeAngle)
        //{
        //    switch (currentWorkingOptions.ColorStyleChoice)
        //    {
        //        case ModeAffichageCouleurs.RandomNeutral:
        //        case ModeAffichageCouleurs.RandomBright:
        //            return new System.Drawing.Drawing2D.LinearGradientBrush(
        //                            rec,
        //                            colorManager.GetNextColor(startAngle),
        //                            Color.SteelBlue,
        //                            LinearGradientMode.ForwardDiagonal
        //                        );
        //        case ModeAffichageCouleurs.Linear2:
        //            return new System.Drawing.Drawing2D.LinearGradientBrush(
        //                            rec,
        //                            colorManager.GetNextColor(startAngle + (nodeAngle / 2f)),
        //                            Color.SteelBlue,
        //                            LinearGradientMode.ForwardDiagonal
        //                        );
        //        case ModeAffichageCouleurs.Linear:
        //            float middleAngle = startAngle + (nodeAngle / 2f);
        //            //return new System.Drawing.Drawing2D.LinearGradientBrush(
        //            //                rec,
        //            //                GetNextColor(middleAngle),
        //            //                Color.SteelBlue,
        //            //                System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal
        //            //            );
        //            if (middleAngle < 90)
        //                return new System.Drawing.Drawing2D.LinearGradientBrush(
        //                                rec,
        //                                Color.SteelBlue,
        //                                colorManager.GetNextColor(middleAngle),
        //                                LinearGradientMode.ForwardDiagonal
        //                            );
        //            else if (middleAngle < 180)
        //                return new System.Drawing.Drawing2D.LinearGradientBrush(
        //                            rec,
        //                            Color.SteelBlue,
        //                            colorManager.GetNextColor(middleAngle),
        //                            LinearGradientMode.BackwardDiagonal
        //                        );
        //            else if (middleAngle < 270)
        //                return new System.Drawing.Drawing2D.LinearGradientBrush(
        //                            rec,
        //                            colorManager.GetNextColor(middleAngle),
        //                            Color.SteelBlue,
        //                            LinearGradientMode.ForwardDiagonal
        //                        );
        //            else
        //                return new System.Drawing.Drawing2D.LinearGradientBrush(
        //                            rec,
        //                            colorManager.GetNextColor(middleAngle),
        //                            Color.SteelBlue,
        //                            LinearGradientMode.BackwardDiagonal
        //                        );
        //        case ModeAffichageCouleurs.ImprovedLinear:
        //        default:
        //            return new SolidBrush(myTransparentColor);
        //        //if (nodeAngle < 1)
        //        //    return new System.Drawing.Drawing2D.LinearGradientBrush(rec,
        //        //                        GetNextColor(startAngle + (nodeAngle / 2f)),
        //        //                        Color.SteelBlue,
        //        //                        System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal);
        //        //PointF p1 = new PointF();
        //        //p1.X = rec.Left + rec.Width / 2f + Convert.ToSingle(Math.Cos(GetRadianFromDegree(startAngle))) * rec.Height / 2f;
        //        //p1.Y = rec.Top + rec.Height / 2f + Convert.ToSingle(Math.Sin(GetRadianFromDegree(startAngle))) * rec.Height / 2f;
        //        //PointF p2 = new PointF();
        //        //p2.X = rec.Left + rec.Width / 2f + Convert.ToSingle(Math.Cos(GetRadianFromDegree(startAngle + nodeAngle))) * rec.Height / 2f;
        //        //p2.Y = rec.Top + rec.Height / 2f + Convert.ToSingle(Math.Sin(GetRadianFromDegree(startAngle + nodeAngle))) * rec.Height / 2f;
        //        //if (nodeAngle == 360)
        //        //    p2.X = -p2.X;
        //        //try
        //        //{
        //        //    return new System.Drawing.Drawing2D.LinearGradientBrush(
        //        //                    p1, p2,
        //        //                    GetNextColor(startAngle),
        //        //                    GetNextColor(startAngle + nodeAngle)
        //        //                );
        //        //}
        //        //catch (Exception ex)
        //        //{
        //        //    throw;
        //        //}
        //    }

        //}


        /// <summary>
        /// A l'image de PaintDirPart, génère l'arc de cercle correspondant aux fichiers d'un répertoire.
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="startAngle"></param>
        /// <param name="endAngle"></param>
        private void BuildFilesPart(int currentLevel, float startAngle, float endAngle)
        {
            //if (!printDirNames && treeGraph.OptionAlsoPaintFiles)
            //{
            //    float nodeAngle = endAngle - startAngle;
            //    rec.Inflate(singleLevelHeight, singleLevelHeight);
            //    //Console.WriteLine("Processing Files (Angle:" + startAngle + ";" + endAngle + "; Rec:" + rec + ")...");
            //    frontGraph.FillPie(new SolidBrush(Color.White), Rectangle.Round(rec), startAngle, nodeAngle); //TODO

            //}
        }


        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog() == true)
            {
                dialog.PrintVisual(canvas1, "HDGraph diagram");
            }

        }

        private Point? initialCursorLocation;
        private double initialRotationAngle;

        private void canvas1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                initialCursorLocation = e.MouseDevice.GetPosition(viewBoxTreegraph);
                e.Handled = true;
                initialRotationAngle = rotateTransform.Angle;
                IsRotating = true;
                if (!canvas1.CaptureMouse())
                    initialCursorLocation = null;
                
            }
        }

        private void canvas1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (initialCursorLocation != null)
            {
                initialCursorLocation = null;
                e.Handled = true;
                canvas1.ReleaseMouseCapture();
                IsRotating = false;
            }
        }

        private void canvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (initialCursorLocation == null || e.LeftButton != MouseButtonState.Pressed)
                return;
            Point newPoint = e.MouseDevice.GetPosition(viewBoxTreegraph);
            Vector centerPoint = new Vector(viewBoxTreegraph.ActualWidth / 2, viewBoxTreegraph.ActualHeight / 2);
            Vector initVector = new Vector(initialCursorLocation.Value.X, initialCursorLocation.Value.Y);
            Vector newVector = new Vector(newPoint.X, newPoint.Y);
            double rotationAngle = Vector.AngleBetween(initVector - centerPoint, newVector - centerPoint);
            rotationAngle = (rotationAngle + initialRotationAngle) % 360;
            if (rotationAngle < 0)
                rotationAngle = rotationAngle + 360;

            rotateTransform.Angle = rotationAngle;
            e.Handled = true;
            labelStatus.Content = "rotationAngle:" + rotationAngle + " initVector:" + initVector + " newVector:" + newVector + " centerPoint:" + centerPoint;
        }

        private void treeGraph1_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (ContextMenuRequired != null)
            {
                IDirectoryNode selectedNode = null; // TODO
                ContextMenuRequired(sender, new NodeContextEventArgs() 
                                            { 
                                                Position = new System.Drawing.PointF(Convert.ToSingle(e.CursorLeft), Convert.ToSingle(e.CursorTop)),
                                                Node = selectedNode,
                                            }
                                   );
                e.Handled = true;
            }
        }
    }
}
