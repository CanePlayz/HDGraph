﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HDGraph.DrawEngine
{
    internal class ImageGraphGenerator
    {
        /// <summary>
        /// Epaisseur d'un niveau sur le graph.
        /// </summary>
        private float pasNiveau;

        public float PasNiveau
        {
            get { return pasNiveau; }
            set { pasNiveau = value; }
        }

        /// <summary>
        /// Graph associé au bitmap buffer
        /// </summary>
        private Graphics frontGraph;

        /// <summary>
        /// Booléen indiquant le type de parcours lors de la création du graph: 
        /// si false, on est dans la phase de dessin des "camemberts". Si true, on est dans la phase 
        /// qui consiste à imprimer les noms des répertoires sur le dessin.
        /// </summary>
        private bool printDirNames = false;

        private HDGraphScanEngine moteur;
        private DrawOptions options;
        private ColorManager colorManager;
        private DirectoryNode rootNode;

        public ImageGraphGenerator(DirectoryNode rootNode, HDGraphScanEngine moteur, DrawOptions options)
        {
            this.moteur = moteur;
            this.options = options;
            this.colorManager = new ColorManager(options);
            this.rootNode = rootNode;
        }

        internal Bitmap Draw()
        {
            // Création du bitmap buffer
            Bitmap backBufferTmp = new Bitmap(options.BitmapSize.Width, options.BitmapSize.Height);
            frontGraph = Graphics.FromImage(backBufferTmp);

            frontGraph.Clear(Color.White);
            frontGraph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            // init des données du calcul
            pasNiveau = Math.Min(options.BitmapSize.Width / (float)options.ShownLevelsCount / 2, options.BitmapSize.Height / (float)options.ShownLevelsCount / 2);
            RectangleF pieRec = new RectangleF(options.BitmapSize.Width / 2f,
                                    options.BitmapSize.Height / 2f,
                                    0,
                                    0);

            PaintTree(pieRec);

            frontGraph.Dispose();
            return backBufferTmp;
        }

        /// <summary>
        /// Effectue le premier lancement de la méthode PaintTree récursive.
        /// </summary>
        private void PaintTree(RectangleF pieRec)
        {
            if (rootNode == null || rootNode.TotalSize == 0)
            {
                PaintSpecialCase();
                return;
            }
            printDirNames = false;
            PaintTree(rootNode, pieRec, options.ImageRotation, 360 - options.ImageRotation);
            printDirNames = true;
            PaintTree(rootNode, pieRec, options.ImageRotation, 360 - options.ImageRotation);
        }

        /// <summary>
        /// Affiche un message spécifique au lieu du graph.
        /// </summary>
        private void PaintSpecialCase()
        {
            string text;
            if (moteur != null && moteur.WorkCanceled)
                text = Resources.ApplicationMessages.UserCanceledAnalysis;
            else if (rootNode != null && rootNode.TotalSize == 0)
                text = Resources.ApplicationMessages.FolderIsEmpty;
            else
                text = Resources.ApplicationMessages.GraphGuideLine;

            AfficherTexteAuCentre(frontGraph, options.BitmapSize, text, options.TextFont, new SolidBrush(Color.Black), false);
        }

        public static void AfficherTexteAuCentre(Graphics graph, Size graphSize, string text, Font font, Brush brush, bool encadrer)
        {
            float x = graphSize.Width / 2f;
            float y = graphSize.Height / 2f;

            SizeF sizeTextName = graph.MeasureString(text, font);
            x -= sizeTextName.Width / 2f;
            y -= sizeTextName.Height / 2f;

            int padding = 5; // 5 pixels
            if (encadrer)
            {
                // Create a new pen.
                Pen pen = new Pen(Brushes.Gray);

                // Set the pen's width.
                pen.Width = 8;

                // Set the LineJoin property.
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                // background Rectangle
                Rectangle rectangle = new Rectangle(Convert.ToInt32(x) - padding,
                                  Convert.ToInt32(y) - padding,
                                  Convert.ToInt32(sizeTextName.Width) + 2 * padding,
                                  Convert.ToInt32(sizeTextName.Height) + 2 * padding);
                // Draw a rectangle.
                graph.FillRectangle(new SolidBrush(Color.FromArgb(150, 255, 255, 255)), rectangle);
                graph.DrawRectangle(pen, rectangle);

                //Dispose of the pen.
                pen.Dispose();

            }
            graph.DrawString(text, font, brush, x, y);
        }

        /// <summary>
        /// Procédure récursive pour graphiquer les arcs de cercle. Graphique de l'extérieur vers l'intérieur.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rec"></param>
        /// <param name="startAngle"></param>
        /// <param name="endAngle"></param>
        private void PaintTree(DirectoryNode node, RectangleF rec, float startAngle, float endAngle)
        {
            if (node.TotalSize == 0)
                return;
            float nodeAngle = endAngle - startAngle;
            rec.Inflate(pasNiveau, pasNiveau);
            if (node.ExistsUncalcSubDir)
            {
                PaintUnknownPart(node, rec, startAngle, endAngle);
            }
            else
            {
                long cumulSize = 0;
                float currentStartAngle;
                foreach (DirectoryNode childNode in node.Children)
                {
                    if (childNode.DirectoryType != SpecialDirTypes.FreeSpaceAndHide)
                    {
                        currentStartAngle = startAngle + cumulSize * nodeAngle / node.TotalSize;
                        float childAngle = childNode.TotalSize * nodeAngle / node.TotalSize;
                        PaintTree(childNode, rec, currentStartAngle, currentStartAngle + childAngle);
                        cumulSize += childNode.TotalSize;
                    }
                }
                currentStartAngle = startAngle + cumulSize * nodeAngle / node.TotalSize;
                if (node.Children.Count > 0 && node.FilesSize > 0)
                    PaintFilesPart(rec, currentStartAngle, endAngle);
                //if (node.ProfondeurMax <= 1 && endAngle - currentStartAngle > 10)
                //    Console.WriteLine("Processing folder '" + node.Path + "' (Angle:" + startAngle + ";" + endAngle + "; Rec:" + rec + ")...");
            }
            PaintDirPart(node, rec, startAngle, nodeAngle);
        }

        /// <summary>
        /// Dessine sur l'objet "graph" l'arc de cercle représentant une partie "inconnue" (confettis)
        /// d'un répertoire.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rec"></param>
        /// <param name="startAngle"></param>
        /// <param name="endAngle"></param>
        private void PaintUnknownPart(DirectoryNode node, RectangleF rec, float startAngle, float endAngle)
        {
            if (!printDirNames)
            {
                float nodeAngle = endAngle - startAngle;
                rec.Inflate(pasNiveau / 6f, pasNiveau / 6f);
                //Console.WriteLine("Processing Files (Angle:" + startAngle + ";" + endAngle + "; Rec:" + rec + ")...");
                frontGraph.FillPie(new System.Drawing.Drawing2D.HatchBrush(
                                            System.Drawing.Drawing2D.HatchStyle.LargeConfetti,
                                            Color.Gray,
                                            Color.White),
                                    Rectangle.Round(rec), startAngle, nodeAngle);
            }
        }


        /// <summary>
        /// Dessine sur l'objet "graph" l'arc de cercle représentant un répertoire, 
        /// ou dessine le nom de ce répertoire (l'un ou l'autre, pas les 2, en fonction de la valeur de printDirNames).
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rec"></param>
        /// <param name="startAngle"></param>
        /// <param name="nodeAngle"></param>
        private void PaintDirPart(DirectoryNode node, RectangleF rec, float startAngle, float nodeAngle)
        {
            // on gère les arcs "pleins" (360°) de manière particulière pour avoir un disque "plein", sans trait à l'angle 0
            if (nodeAngle == 360)
            {
                if (!printDirNames)
                {
                    // on dessine le disque uniquement                  
                    frontGraph.FillEllipse(
                        GetBrushForAngles(rec, startAngle, nodeAngle),
                        Rectangle.Round(rec));
                    frontGraph.DrawEllipse(new Pen(Color.Black), rec);
                }
                else
                {
                    // on écrit les noms de répertoire uniquement
                    WriteDirectoryNameForFullPie(node, rec);
                }
            }
            else
            {
                if (!printDirNames)
                {
                    // on dessine le disque uniquement
                    DrawPartialPie(node, rec, startAngle, nodeAngle);
                }
                else if (nodeAngle > 10)
                {
                    // on dessine les noms de répertoire uniquement (si l'angle est supérieur à 10°)
                    WriteDirectoryName(node, rec, startAngle, nodeAngle);
                }

            }
        }

        /// <summary>
        /// Dessine le nom d'un répertoire sur le graph, lorsque ce répertoire a un angle de 360°.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rec"></param>
        /// <returns></returns>
        private void WriteDirectoryNameForFullPie(DirectoryNode node, RectangleF rec)
        {
            float x = 0, y;
            if (rec.Height == pasNiveau * 2)
            {
                y = 0;
            }
            else
            {
                y = rec.Height / 2f - pasNiveau * 3f / 4f;
            }
            x += options.BitmapSize.Width / 2f;
            y += options.BitmapSize.Height / 2f;
            string nodeText = node.Name;
            if (options.ShowSize)
                nodeText += Environment.NewLine + HDGTools.FormatSize(node.TotalSize);

            SizeF size = frontGraph.MeasureString(nodeText, options.TextFont);
            x -= size.Width / 2f;
            y -= size.Height / 2f;
            // Adoucir le fond du texte :
            Color colTransp = Color.FromArgb(100, Color.White);
            frontGraph.FillRectangle(new SolidBrush(colTransp),
                                x, y, size.Width, size.Height);
            frontGraph.DrawRectangle(new Pen(Color.Black), x, y, size.Width, size.Height);
            frontGraph.DrawString(nodeText, options.TextFont, new SolidBrush(Color.Black), x, y);
        }

        /// <summary>
        /// Dessine le nom d'un répertoire sur le graph.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rec"></param>
        /// <param name="startAngle"></param>
        /// <param name="nodeAngle"></param>
        /// <returns></returns>
        private void WriteDirectoryName(DirectoryNode node, RectangleF rec, float startAngle, float nodeAngle)
        {
            //float textWidthLimit = pasNiveau * 1.5f;
            float textWidthLimit = pasNiveau * 2f;
            float x, y, angleCentre, hyp;
            hyp = (rec.Width - pasNiveau) / 2f;
            angleCentre = startAngle + nodeAngle / 2f;
            x = (float)Math.Cos(MathHelper.GetRadianFromDegree(angleCentre)) * hyp;
            y = (float)Math.Sin(MathHelper.GetRadianFromDegree(angleCentre)) * hyp;
            x += options.BitmapSize.Width / 2f;
            y += options.BitmapSize.Height / 2f;
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            string nodeText = node.Name;
            SizeF sizeTextName = frontGraph.MeasureString(nodeText, options.TextFont);
            if (sizeTextName.Width <= textWidthLimit)
            {
                if (options.ShowSize)
                {
                    float xName = x - sizeTextName.Width / 2f;
                    float yName = y - sizeTextName.Height;
                    frontGraph.DrawString(nodeText, options.TextFont, new SolidBrush(Color.Black), xName, yName); //, format);
                    string nodeSize = HDGTools.FormatSize(node.TotalSize);
                    SizeF sizeTextSize = frontGraph.MeasureString(nodeSize, options.TextFont);
                    float xSize = x - sizeTextSize.Width / 2f;
                    float ySize = y;
                    // Adoucir le fond du texte :
                    //Color colTransp = Color.FromArgb(50, Color.White);
                    //graph.FillRectangle(new SolidBrush(colTransp),
                    //                    xSize, ySize, sizeTextSize.Width, sizeTextSize.Height);
                    frontGraph.DrawString(nodeSize, options.TextFont, new SolidBrush(Color.Black), xSize, ySize); //, format);
                }
                else
                {
                    x -= sizeTextName.Width / 2f;
                    y -= sizeTextName.Height / 2f;
                    frontGraph.DrawString(nodeText, options.TextFont, new SolidBrush(Color.Black), x, y); //, format);
                }
            }
        }

        /// <summary>
        /// Dessine un semi anneau sur le graph.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rec"></param>
        /// <param name="startAngle"></param>
        /// <param name="nodeAngle"></param>
        private void DrawPartialPie(DirectoryNode node, RectangleF rec, float startAngle, float nodeAngle)
        {
            if (node.DirectoryType == SpecialDirTypes.NotSpecial)
            {
                // standard zone
                frontGraph.FillPie(
                    GetBrushForAngles(rec, startAngle, nodeAngle),
                    Rectangle.Round(rec),
                    startAngle,
                    nodeAngle);
                frontGraph.DrawPie(new Pen(Color.Black), rec, startAngle, nodeAngle);
            }
            else if (node.DirectoryType == SpecialDirTypes.FreeSpaceAndShow)
            {
                // free space
                frontGraph.FillPie(new System.Drawing.Drawing2D.HatchBrush(
                                            System.Drawing.Drawing2D.HatchStyle.Wave,
                                            Color.LightGray,
                                            Color.White),
                                Rectangle.Round(rec),
                                startAngle,
                                nodeAngle);
            }
            else if (node.DirectoryType == SpecialDirTypes.UnknownPart)
            {
                // non-calculable files
                frontGraph.FillPie(new System.Drawing.Drawing2D.HatchBrush(
                                            System.Drawing.Drawing2D.HatchStyle.Trellis,
                                            Color.Red,
                                            Color.White),
                                Rectangle.Round(rec),
                                startAngle,
                                nodeAngle);
            }
        }

        private Color myTransparentColor = Color.Black;

        private Brush GetBrushForAngles(RectangleF rec, float startAngle, float nodeAngle)
        {
            switch (options.ColorStyleChoice)
            {
                case ModeAffichageCouleurs.RandomNeutral:
                case ModeAffichageCouleurs.RandomBright:
                    return new System.Drawing.Drawing2D.LinearGradientBrush(
                                    rec,
                                    colorManager.GetNextColor(startAngle),
                                    Color.SteelBlue,
                                    LinearGradientMode.ForwardDiagonal
                                );
                case ModeAffichageCouleurs.Linear2:
                    return new System.Drawing.Drawing2D.LinearGradientBrush(
                                    rec,
                                    colorManager.GetNextColor(startAngle + (nodeAngle / 2f)),
                                    Color.SteelBlue,
                                    LinearGradientMode.ForwardDiagonal
                                );
                case ModeAffichageCouleurs.Linear:
                    float middleAngle = startAngle + (nodeAngle / 2f);
                    //return new System.Drawing.Drawing2D.LinearGradientBrush(
                    //                rec,
                    //                GetNextColor(middleAngle),
                    //                Color.SteelBlue,
                    //                System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal
                    //            );
                    if (middleAngle < 90)
                        return new System.Drawing.Drawing2D.LinearGradientBrush(
                                        rec,
                                        Color.SteelBlue,
                                        colorManager.GetNextColor(middleAngle),
                                        LinearGradientMode.ForwardDiagonal
                                    );
                    else if (middleAngle < 180)
                        return new System.Drawing.Drawing2D.LinearGradientBrush(
                                    rec,
                                    Color.SteelBlue,
                                    colorManager.GetNextColor(middleAngle),
                                    LinearGradientMode.BackwardDiagonal
                                );
                    else if (middleAngle < 270)
                        return new System.Drawing.Drawing2D.LinearGradientBrush(
                                    rec,
                                    colorManager.GetNextColor(middleAngle),
                                    Color.SteelBlue,
                                    LinearGradientMode.ForwardDiagonal
                                );
                    else
                        return new System.Drawing.Drawing2D.LinearGradientBrush(
                                    rec,
                                    colorManager.GetNextColor(middleAngle),
                                    Color.SteelBlue,
                                    LinearGradientMode.BackwardDiagonal
                                );
                case ModeAffichageCouleurs.ImprovedLinear:
                default:
                    return new SolidBrush(myTransparentColor);
                //if (nodeAngle < 1)
                //    return new System.Drawing.Drawing2D.LinearGradientBrush(rec,
                //                        GetNextColor(startAngle + (nodeAngle / 2f)),
                //                        Color.SteelBlue,
                //                        System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal);
                //PointF p1 = new PointF();
                //p1.X = rec.Left + rec.Width / 2f + Convert.ToSingle(Math.Cos(GetRadianFromDegree(startAngle))) * rec.Height / 2f;
                //p1.Y = rec.Top + rec.Height / 2f + Convert.ToSingle(Math.Sin(GetRadianFromDegree(startAngle))) * rec.Height / 2f;
                //PointF p2 = new PointF();
                //p2.X = rec.Left + rec.Width / 2f + Convert.ToSingle(Math.Cos(GetRadianFromDegree(startAngle + nodeAngle))) * rec.Height / 2f;
                //p2.Y = rec.Top + rec.Height / 2f + Convert.ToSingle(Math.Sin(GetRadianFromDegree(startAngle + nodeAngle))) * rec.Height / 2f;
                //if (nodeAngle == 360)
                //    p2.X = -p2.X;
                //try
                //{
                //    return new System.Drawing.Drawing2D.LinearGradientBrush(
                //                    p1, p2,
                //                    GetNextColor(startAngle),
                //                    GetNextColor(startAngle + nodeAngle)
                //                );
                //}
                //catch (Exception ex)
                //{
                //    throw;
                //}
            }

        }


        /// <summary>
        /// A l'image de PaintDirPart, génère l'arc de cercle correspondant aux fichiers d'un répertoire.
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="startAngle"></param>
        /// <param name="endAngle"></param>
        private void PaintFilesPart(RectangleF rec, float startAngle, float endAngle)
        {
            //if (!printDirNames && treeGraph.OptionAlsoPaintFiles)
            //{
            //    float nodeAngle = endAngle - startAngle;
            //    rec.Inflate(pasNiveau, pasNiveau);
            //    //Console.WriteLine("Processing Files (Angle:" + startAngle + ";" + endAngle + "; Rec:" + rec + ")...");
            //    frontGraph.FillPie(new SolidBrush(Color.White), Rectangle.Round(rec), startAngle, nodeAngle); //TODO

            //}
        }

    }
}
