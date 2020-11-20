using System;
using System.Collections.Generic;
using System.Text;

using GameOverlay.Drawing;
using GameOverlay.Windows;

using System.Windows.Forms;

namespace Examples
{
	public class Example : IDisposable
	{
		
		private const int FontSize = 20;
		private const float StrokeOpacity = 0.025f;
		private const float SecondaryStrokeOpacity = 0.02f;
		private const float TextOpacity = 0.25f;
		private const float TextOpacity2 = 0.10f;
		private const float BackgroundOpacity = 0.0f;
		private const int GridSize = 4;
		private const int CellSize = 16;
		private const float cellWidth = CellSize*GridSize;
		private const float cellHeight = CellSize*GridSize;
		private const int StrokeSize = 3;
		private const int SecondaryStrokeSize = 2;
		//private const string FontFamilyName = "bahnschrift";
		//private const string FontFamilyName = "bahnschrift";
		//private const string FontFamilyName = "impact";
		//private const string FontFamilyName = "ink free";
		//private const string FontFamilyName = "sitka";
		
		//private const string FontFamilyName = "cascadia mono";
		//private const string FontFamilyName = "consolas";
		//private const string FontFamilyName = "lucida console";
		private const string FontFamilyName = "cascadia code";
		//private const string FontFamilyName = "corbel";
		private readonly GraphicsWindow _window;

		private readonly Dictionary<string, SolidBrush> _brushes;
		private readonly Dictionary<string, Font> _fonts;
		private readonly Dictionary<string, Image> _images;
		
		private Font gridNumberFont;

		private SolidBrush lineBrush;
		private SolidBrush secondaryLineBrush;        
		private SolidBrush backgroundBrush;
		private SolidBrush textXBrush;
		private SolidBrush textYBrush;
		private SolidBrush textXBrushShadow;
		private SolidBrush textYBrushShadow;
		private SolidBrush textCommaBrush;

		
		private Geometry bigGridGeometry;
		private Geometry smallGridGeometry;

		private SolidBrush[] numberColorsX;
		private SolidBrush[] numberColorsY;
		private Point[] numberPositionsY;
		private Point[] numberPositionsX;
		private String[] labelsY;
		private String[] labelsX;
		private float screenWidth;
		private float screenHeight;
		
		private System.Drawing.Rectangle gridBounds;

		public Example()
		{
			_brushes = new Dictionary<string, SolidBrush>();
			_fonts = new Dictionary<string, Font>();
			_images = new Dictionary<string, Image>();
			this.gridBounds = Screen.PrimaryScreen.Bounds;

			var gfx = new Graphics()
			{
				MeasureFPS = true,
				PerPrimitiveAntiAliasing = true,
				TextAntiAliasing = true
			};

			_window = new GraphicsWindow(0, 0, 800, 600, gfx)
			{
				FPS = 1,
				IsTopmost = true,
				IsVisible = true,
				X = 0,
				Y = 0,
				Width = this.gridBounds.Width,
				Height = this.gridBounds.Height
			};

			_window.DestroyGraphics += _window_DestroyGraphics;
			_window.DrawGraphics += _window_DrawGraphics;
			_window.SetupGraphics += _window_SetupGraphics;
		}

		private void _window_SetupGraphics(object sender, SetupGraphicsEventArgs e)
		{
			var g = e.Graphics;

			if (e.RecreateResources)
			{
				foreach (var pair in _brushes) pair.Value.Dispose();
				foreach (var pair in _images) pair.Value.Dispose();
			}

			_brushes["black"] = g.CreateSolidBrush(0, 0, 0);
			_brushes["white"] = g.CreateSolidBrush(255, 255, 255);
			_brushes["red"] = g.CreateSolidBrush(255, 0, 0);
			_brushes["green"] = g.CreateSolidBrush(0, 255, 0);
			_brushes["blue"] = g.CreateSolidBrush(0, 0, 255);
			_brushes["background"] = g.CreateSolidBrush(0x33, 0x36, 0x3F);
			_brushes["grid"] = g.CreateSolidBrush(255, 255, 255, 0.2f);
			
			_brushes["transparent"] = g.CreateSolidBrush(0, 0, 0, 0);

			if (e.RecreateResources) return;

			_fonts["arial"] = g.CreateFont("Arial", 12);
			_fonts["consolas"] = g.CreateFont("Consolas", 14);

			//gridBounds = new Rectangle(20, 60, g.Width - 20, g.Height - 20);
            screenWidth = gridBounds.Width;
            screenHeight = gridBounds.Height;
			int numGridColumns = (int)Math.Ceiling(screenWidth / cellWidth);
			int numGridRows = (int)Math.Ceiling(screenHeight / cellHeight);
            numberPositionsX = new Point[numGridColumns * numGridRows];
            numberPositionsY = new Point[numGridColumns * numGridRows];
            numberColorsX = new SolidBrush[numGridColumns * numGridRows];
            numberColorsY = new SolidBrush[numGridColumns * numGridRows];
            labelsX = new String[numGridColumns * numGridRows];
            labelsY = new String[numGridColumns * numGridRows];
            
            
            gridNumberFont = g.CreateFont(FontFamilyName, FontSize);

            int rc = 50;
            int gc = 255;
            int bc = 50;
            int rd = 0;
            int gd = 0;
            int bd = 0;
            lineBrush = g.CreateSolidBrush(rc, gc, bc, StrokeOpacity);
            secondaryLineBrush = g.CreateSolidBrush(rc, gc, bc, SecondaryStrokeOpacity);
            textXBrush = g.CreateSolidBrush(rc, gc, bc, TextOpacity);
            textYBrush = g.CreateSolidBrush(gc, rc, gc, TextOpacity);
            textXBrushShadow = g.CreateSolidBrush(128,128,128,TextOpacity2);
            textYBrushShadow = g.CreateSolidBrush(128,128,128,TextOpacity2);
            textCommaBrush = g.CreateSolidBrush(rc, gc, bc, TextOpacity2);
            backgroundBrush = g.CreateSolidBrush(1, 1, 1, BackgroundOpacity);

            bigGridGeometry = g.CreateGeometry();
            for (int x = 0; x < numGridColumns; x++)
            {
                float startX = x * cellWidth;
                float startY = 0;
                float endX = startX;
                float endY = screenHeight;
                bigGridGeometry.BeginFigure(new Point(startX, startY));
                bigGridGeometry.AddPoint(new Point(endX, endY));
                bigGridGeometry.EndFigure(false);
            }

            for (int y = 0; y < numGridRows; y++)
            {
                float startX = 0;
                float startY = y * cellHeight;
                float endX = screenWidth;
                float endY = startY;
                bigGridGeometry.BeginFigure(new Point(startX, startY));
                bigGridGeometry.AddPoint(new Point(endX, endY));
                bigGridGeometry.EndFigure(false);
            }
            bigGridGeometry.Close();

            smallGridGeometry = g.CreateGeometry();
            float digitWidth = FontSize * 4 / 7f;
            float digitHeight = FontSize;
            float offsetY = -digitHeight / 2f;
            for (int x = 0; x < numGridColumns; x++)
            {
                for (int y = 0; y < numGridRows; y++)
                {
                    int gridNumber = (y * numGridColumns) + x;                    
                    labelsX[gridNumber] = "⮞" + (x.ToString());
                    labelsY[gridNumber] = "⮟" + y.ToString();
                    float numberOfDigitsX = labelsX[gridNumber].Length;                    
                    float numberOfDigitsY = labelsY[gridNumber].Length;                    
                    float offsetX_X = -digitWidth * numberOfDigitsX/2f;
                    float offsetX_Y = -digitWidth * numberOfDigitsY/2f;
                    float positionX_X = x * cellWidth + cellWidth / 2f + offsetX_X;
                    float positionX_Y = x * cellWidth + cellWidth / 2f + offsetX_Y;
                    float positionY_X = y * cellHeight + cellHeight / 2f - digitHeight;
                    float positionY_Y = y * cellHeight + cellHeight / 2f ;

                    numberColorsX[gridNumber] = x % 2 == 0 ? textXBrush : textXBrushShadow;
                    numberColorsY[gridNumber] = y % 2 == 0 ? textYBrush : textYBrushShadow;
                    numberPositionsX[gridNumber] = new Point(positionX_X, positionY_X);
                    numberPositionsY[gridNumber] = new Point(positionX_Y, positionY_Y);

                    for (int i = 1; i < GridSize; i++)
                    {
                        float startX = x * cellWidth + i * cellWidth / GridSize;
                        float startY = y * cellHeight;
                        float endX = startX;
                        float endY = startY + cellHeight;
                        smallGridGeometry.BeginFigure(new Point(startX, startY));
                        smallGridGeometry.AddPoint(new Point(endX, endY));
                        smallGridGeometry.EndFigure();
                    }

                    for (int j = 1; j < GridSize; j++)
                    {
                        float startX = x * cellWidth;
                        float startY = y * cellHeight + j * cellHeight / GridSize;
                        float endX = startX + cellWidth;
                        float endY = startY;
                        smallGridGeometry.BeginFigure(new Point(startX, startY));
                        smallGridGeometry.AddPoint(new Point(endX, endY));
                        smallGridGeometry.EndFigure();
                    }
                }
            }
            smallGridGeometry.Close();
		}

		private void _window_DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
		{
			foreach (var pair in _brushes) pair.Value.Dispose();
			foreach (var pair in _fonts) pair.Value.Dispose();
			foreach (var pair in _images) pair.Value.Dispose();
		}

		private void _window_DrawGraphics(object sender, DrawGraphicsEventArgs e)
		{
			var g = e.Graphics;

			var padding = 16;

			g.ClearScene(_brushes["transparent"]);

			//g.FillRectangle(_brushes["transparent"], 0, 0, screenWidth, screenHeight);            

			g.DrawGeometry(bigGridGeometry, lineBrush, StrokeSize);
			g.DrawGeometry(smallGridGeometry, secondaryLineBrush, SecondaryStrokeSize);

			for (int i = 0; i < numberPositionsX.Length; i++) {
				g.DrawText(gridNumberFont, numberColorsX[i], numberPositionsX[i].X, numberPositionsX[i].Y, labelsX[i]);
				g.DrawText(gridNumberFont, numberColorsY[i], numberPositionsY[i].X, numberPositionsY[i].Y, labelsY[i]);
				//g.DrawText(gridNumberFont, textXBrushShadow, numberPositionsX[i].X+4, numberPositionsX[i].Y, labelsX[i]);
				//g.DrawText(gridNumberFont, textYBrushShadow, numberPositionsY[i].X, numberPositionsY[i].Y+4, labelsY[i]);
			}
		}
		
		public void Run()
		{
			_window.Create();
			_window.Join();
		}

		~Example()
		{
			gridNumberFont.Dispose();
			bigGridGeometry.Dispose();
			smallGridGeometry.Dispose();
			lineBrush.Dispose();
			secondaryLineBrush.Dispose();
			backgroundBrush.Dispose();
			Dispose(false);
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				_window.Dispose();

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
