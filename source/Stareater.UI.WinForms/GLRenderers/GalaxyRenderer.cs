﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using Stareater.AppData;
using Stareater.Controllers;
using Stareater.Utils;

namespace Stareater.GLRenderers
{
	class GalaxyRenderer : IRenderer
	{
		private const string GalaxyTexturePath = "./images/galaxy textures.png";
		
		private const double DefaultViewSize = 15;
		private const double ZoomBase = 1.2f;
		private const int MaxZoom = 10;
		private const int MinZoom = -10;
		
		private const float FarZ = -1;
		private const float WormholeZ = -0.6f;
		private const float StarColorZ = -0.5f;
		private const float StarSaturationZ = -0.4f;
		private const float SelectionIndicatorZ = -0.3f;
		private const float StarNameZ = -0.2f;
		private const float StarNameZRange = 0.1f;

		private const float PanClickTolerance = 0.01f;
		private const float ClickRadius = 0.02f;
		private const float StarMinClickRadius = 0.6f;
		private const float StarNameScale = 0.35f;

		private GameController controller;
		private Control eventDispatcher;
		private Action<StarSystemController> systemOpenedHandler;
		
		private bool resetProjection = true;
		private Matrix4 invProjection;

		private int zoomLevel = 0;
		private Vector4? lastMousePosition = null;
		private float panAbsPath = 0;
		private Vector2 originOffset = Vector2.Zero;
		private float screenLength;
		private Vector2 mapBoundsMin;
		private Vector2 mapBoundsMax;

		private int staticList = -1;

		public GalaxyRenderer(GameController controller, Action<StarSystemController> systemOpenedHandler)
		{
			this.controller = controller;
			this.systemOpenedHandler = systemOpenedHandler;
			
			this.mapBoundsMin = new Vector2(
				(float)controller.Stars.Select(star => star.Position.X).Min() - StarMinClickRadius,
				(float)controller.Stars.Select(star => star.Position.Y).Min() - StarMinClickRadius
			);
			this.mapBoundsMax = new Vector2(
				(float)controller.Stars.Select(star => star.Position.X).Max() + StarMinClickRadius,
				(float)controller.Stars.Select(star => star.Position.Y).Max() + StarMinClickRadius
			);
		}

		public void AttachToCanvas(Control eventDispatcher)
		{
			this.eventDispatcher = eventDispatcher;

			eventDispatcher.MouseMove += mousePan;
			eventDispatcher.MouseWheel += mouseZoom;
			eventDispatcher.MouseClick += mouseClick;
			eventDispatcher.MouseDoubleClick += mouseDoubleClick;
			
			resetProjection = true;
		}

		public void DetachFromCanvas()
		{
			eventDispatcher.MouseMove -= mousePan;
			eventDispatcher.MouseWheel -= mouseZoom;
			eventDispatcher.MouseClick -= mouseClick;
			eventDispatcher.MouseDoubleClick -= mouseDoubleClick;

			this.eventDispatcher = null;
		}

		public void ResetProjection()
		{
			resetProjection = true;
		}
		
		public void Load()
		{
			GalaxyTextures.Get.Load();
			TextRenderUtil.Get.Prepare(controller.Stars.Select(x => x.Name.ToText(SettingsWinforms.Get.Language)));
		}
		
		public void Unload()
		{
			GalaxyTextures.Get.Unload();
			
			if (staticList >= 0){
				GL.DeleteLists(staticList, 1);
				staticList = -1;
			}
		}
		
		public void Draw(double deltaTime)
		{
			if (resetProjection) {
				double aspect = eventDispatcher.Width / (double)eventDispatcher.Height;
				double semiRadius = 0.5 * DefaultViewSize / Math.Pow(ZoomBase, zoomLevel);

				var screen = Screen.FromControl(eventDispatcher);
				if (screen.Bounds.Width > screen.Bounds.Height)
					screenLength = (float)(2 * screen.Bounds.Width * semiRadius * aspect / eventDispatcher.Width);
				else
					//TODO test this, perhaps by flipping monitor.
					screenLength = (float)(2 * screen.Bounds.Height * semiRadius * aspect / eventDispatcher.Height);
				
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadIdentity();
				GL.Ortho(
					-aspect * semiRadius + originOffset.X, aspect * semiRadius + originOffset.X,
					-semiRadius + originOffset.Y, semiRadius + originOffset.Y, 
					0, -FarZ);

				GL.GetFloat(GetPName.ProjectionMatrix, out invProjection);
				invProjection.Invert();
				GL.MatrixMode(MatrixMode.Modelview);
				resetProjection = false;
			}

			if (staticList < 0) {
				staticList = GL.GenLists(1);
				GL.NewList(staticList, ListMode.CompileAndExecute);

				GL.Disable(EnableCap.Texture2D);
				GL.Color4(Color.Blue);
				GL.PushMatrix();
				GL.Translate(0, 0, WormholeZ);
				//TODO: Use textured quads instead plain lines
				GL.Begin(BeginMode.Lines);
				foreach (var wormhole in controller.Wormholes) {
					GL.Vertex2(wormhole.Item1.Position.X, wormhole.Item1.Position.Y);
					GL.Vertex2(wormhole.Item2.Position.X, wormhole.Item2.Position.Y);
				}
				GL.End();
				GL.PopMatrix();

				GL.Enable(EnableCap.Texture2D);

				foreach (var star in controller.Stars) {
					GL.Color4(star.Color);
					GL.PushMatrix();
					GL.Translate(star.Position.X, star.Position.Y, StarColorZ);

					TextureUtils.Get.DrawSprite(GalaxyTextures.Get.StarColor);
				
					GL.Color4(Color.White);
					TextureUtils.Get.DrawSprite(GalaxyTextures.Get.StarGlow, StarSaturationZ - StarColorZ);
					
					GL.PopMatrix();
				}
				
				float starNameZ = StarNameZ;
				foreach (var star in controller.Stars) {
					GL.Color4(Color.LightGray);
					GL.PushMatrix();
					GL.Translate(star.Position.X, star.Position.Y - 0.5, starNameZ);
					GL.Scale(StarNameScale, StarNameScale, StarNameScale);

					TextRenderUtil.Get.RenderText(star.Name.ToText(SettingsWinforms.Get.Language), -0.5f);
					GL.PopMatrix();
					starNameZ += StarNameZRange / controller.StarCount;
				}

				GL.EndList();
			}
			else
				GL.CallList(staticList);

			if (controller.SelectedStar != null) {
				GL.Color4(Color.White);
				GL.PushMatrix();
				GL.Translate(controller.SelectedStar.Position.X, controller.SelectedStar.Position.Y, SelectionIndicatorZ);

				TextureUtils.Get.DrawSprite(GalaxyTextures.Get.SelectedStar);
				GL.PopMatrix();
			}
		}
		
		private Vector4 mouseToView(int x, int y)
		{
			return new Vector4(
				2 * x / (float)eventDispatcher.Width - 1,
				1 - 2 * y / (float)eventDispatcher.Height, 
				0, 1
			);
		}
		
		private void limitPan()
		{
			if (originOffset.X < mapBoundsMin.X) 
				originOffset.X = mapBoundsMin.X;
			if (originOffset.X > mapBoundsMax.X) 
				originOffset.X = mapBoundsMax.X;
			
			if (originOffset.Y < mapBoundsMin.Y) 
				originOffset.Y = mapBoundsMin.Y;
			if (originOffset.Y > mapBoundsMax.Y) 
				originOffset.Y = mapBoundsMax.Y;
		}
		
		private void mousePan(object sender, MouseEventArgs e)
		{
			Vector4 currentPosition = mouseToView(e.X, e.Y);

			if (!lastMousePosition.HasValue)
				lastMousePosition = currentPosition;

			if (!e.Button.HasFlag(MouseButtons.Left)) {
				lastMousePosition = currentPosition;
				panAbsPath = 0;
				return;
			}
			
			panAbsPath += (currentPosition - lastMousePosition.Value).Length;

			originOffset -= (Vector4.Transform(currentPosition, invProjection) -
				Vector4.Transform(lastMousePosition.Value, invProjection)
				).Xy;

			limitPan();
			
			lastMousePosition = currentPosition;
			resetProjection = true;
			eventDispatcher.Refresh();
		}

		private void mouseZoom(object sender, MouseEventArgs e)
		{
			float oldZoom = 1 / (float)(0.5 * DefaultViewSize / Math.Pow(ZoomBase, zoomLevel));

			if (e.Delta > 0)
				zoomLevel++;
			else 
				zoomLevel--;

			zoomLevel = Methods.Clamp(zoomLevel, MinZoom, MaxZoom);

			float newZoom = 1 / (float)(0.5 * DefaultViewSize / Math.Pow(ZoomBase, zoomLevel));
			Vector2 mousePoint = Vector4.Transform(mouseToView(e.X, e.Y), invProjection).Xy;

			originOffset = (originOffset * oldZoom + mousePoint * (newZoom - oldZoom)) / newZoom;
			limitPan();
			resetProjection = true;
		}

		private void mouseClick(object sender, MouseEventArgs e)
		{
			if (panAbsPath > PanClickTolerance)
				return;
			
			Vector4 mousePoint = Vector4.Transform(mouseToView(e.X, e.Y), invProjection);
			controller.SelectClosest(
				mousePoint.X, mousePoint.Y, 
				Math.Max(screenLength * ClickRadius, StarMinClickRadius));
		}
		
		private void mouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (panAbsPath > PanClickTolerance)
				return;
			
			Vector4 mousePoint = Vector4.Transform(mouseToView(e.X, e.Y), invProjection);
			StarSystemController system = controller.OpenStarSystem(
				mousePoint.X, mousePoint.Y, 
				Math.Max(screenLength * ClickRadius, StarMinClickRadius));
			
			if (system != null)
				this.systemOpenedHandler(system);
		}

		public void Dispose()
		{
			if (eventDispatcher != null) {
				DetachFromCanvas();
				Unload();
			}
		}
	}
}
