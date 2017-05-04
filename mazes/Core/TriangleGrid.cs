﻿namespace mazes.Core {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public class TriangleGrid : Grid {
        public TriangleGrid(int rows, int cols) : base(rows, cols) {
            PrepareGrid();
            ConfigureCells();
        }

        private void PrepareGrid() {
            var rows = new List<List<Cell>>();
            for (var row = 0; row < Rows; row++) {
                var newRows = new List<Cell>();
                for (var column = 0; column < Columns; column++) {
                    newRows.Add(new TriangleCell(row, column));
                }
                rows.Add(newRows);
            }
            _grid = rows;
        }

        private void ConfigureCells() {
            foreach (var cell in Cells) {
                var tCell = (TriangleCell)cell;
                var row = cell.Row;
                var column = cell.Column;

                tCell.West = (TriangleCell)this[row, column - 1];
                tCell.East = (TriangleCell)this[row, column + 1];

                if (tCell.Upright) {
                    tCell.South = (TriangleCell)this[row + 1, column];
                } else {
                    tCell.North = (TriangleCell)this[row - 1, column];
                }
            }
        }

        public override Image ToImg(int cellSize = 50) {
            var halfWidth = cellSize / 2.0;
            var height = cellSize * Math.Sqrt(3) / 2;
            var halfHeight = height / 2.0;

            var imgWidth = (int)(cellSize * (Columns + 1) / 2.0)+1;
            var imgHeight = (int)(height * Rows)+1;


            var img = new Bitmap(imgWidth, imgHeight);
            using (var g = Graphics.FromImage(img)) {
                g.Clear(Color.Transparent);
                foreach (var mode in new[] { DrawMode.Background, DrawMode.Walls, DrawMode.Path }) {
                    foreach (var cell in Cells.Cast<TriangleCell>()) {
                        var cx = halfWidth + cell.Column * halfWidth;
                        var cy = halfHeight + cell.Row * height;

                        var westX = (int)(cx - halfWidth);
                        var midX = (int)cx;
                        var eastX = (int)(cx + halfWidth);

                        var apexY = cell.Upright ? (int)(cy - halfHeight) : (int)(cy + halfHeight);
                        var baseY = cell.Upright ? (int)(cy + halfHeight) : (int)(cy - halfHeight);

                        if (mode == DrawMode.Background) {
                            var color = BackgroundColorFor(cell);
                            if (color != null) {
                                g.FillPolygon(new SolidBrush(color.GetValueOrDefault()), new [] {
                                    new Point(westX, baseY), new Point(midX, apexY), new Point(eastX, baseY)   
                                });
                            }
                        } else if (mode == DrawMode.Walls) {
                            if (cell.West == null) {
                                g.DrawLine(Pens.Black, westX, baseY, midX, apexY);
                            }
                            if (!cell.IsLinked(cell.East)) {
                                g.DrawLine(Pens.Black, eastX, baseY, midX, apexY);
                            }

                            var noSouth = cell.Upright && cell.South == null;
                            var notLinked = !cell.Upright && !cell.IsLinked(cell.North);
                            if (noSouth || notLinked) {
                                g.DrawLine(Pens.Black, eastX, baseY, westX, baseY);
                            }

                            if (cell == ActiveCell) {
                                g.FillPolygon(Brushes.GreenYellow, new[] {
                                    new Point(westX, baseY), new Point(midX, apexY), new Point(eastX, baseY)
                                });
                            }

                        } else if (mode == DrawMode.Path) {
                            DrawPath(cell, g, cellSize);
                        }
                    }
                }
            }

            return img;

        }
    }
}