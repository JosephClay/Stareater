﻿using OpenTK;
using Stareater.GraphicsEngine.GuiPositioners;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Stareater.GraphicsEngine.GuiElements
{
	abstract class AGuiElement
	{
		private SceneObject graphicObject = null;
		private HashSet<AGuiElement> dependentElements = new HashSet<AGuiElement>();

		protected AScene scene { get; private set; }
		public float Z0 { get; private set; }
		public float ZRange { get; private set; }

		public AGuiElement Parent { get; private set; }
		public ElementPosition Position { get; private set; }
		public bool MasksMouseClick { get; set; }
		public ITooltip Tooltip { get; set; }

		protected AGuiElement()
		{
			this.Position = new ElementPosition(this.measureContent);
			this.MasksMouseClick = true;
		}

		public virtual void Attach(AScene scene, AGuiElement parent)
		{
			this.scene = scene;
			this.Parent = parent;

			this.Parent.dependentElements.Add(this);
			foreach(var element in this.Position.Dependencies)
				element.dependentElements.Add(this);

			this.SetDepth(parent.Z0 - parent.ZRange, parent.ZRange);
			this.updateScene();
		}

		public void Detach()
		{
			this.Parent.dependentElements.Remove(this);
			foreach (var element in this.Position.Dependencies)
				element.dependentElements.Remove(this);

			this.scene.RemoveFromScene(ref this.graphicObject);
			this.scene = null;
		}

		public void SetDepth(float z0, float zRange)
		{
			this.Z0 = z0;
			this.ZRange = zRange;

			this.updateScene();
		}

		public void RecalculatePosition(bool fullRecalculate)
		{
			var oldCenter = this.Position.Center;
			var oldSize = this.Position.Size;

			this.Position.Recalculate((this.Parent != null) ? this.Parent.Position : null);

			if (fullRecalculate || this.Position.Center != oldCenter || this.Position.Size != oldSize)
			{
				foreach (var element in this.dependentElements)
					element.RecalculatePosition(fullRecalculate);

				this.updateScene();
			}
		}

		public bool IsInside(Vector2 point)
		{
			var innerPoint = point - this.Position.Center;
			return Math.Abs(innerPoint.X) <= this.Position.Size.X / 2 &&
				Math.Abs(innerPoint.Y) <= this.Position.Size.Y / 2;
		}

		public virtual bool OnMouseDown(Vector2 mousePosition)
		{
			return false;
		}

		public virtual void OnMouseUp(Keys modiferKeys)
		{
			//No operation
		}

		public virtual void OnMouseDownCanceled()
		{
			//No operation
		}

		public virtual void OnMouseMove(Vector2 mousePosition, Keys modiferKeys)
		{
			//No operation
		}

		public virtual void OnMouseDrag(Vector2 mousePosition)
		{
			//No operation
		}

		public virtual void OnMouseLeave()
		{ }

		protected void updateScene()
		{
			if (this.scene == null)
				return;

			if (this.graphicObject != null && this.Position.ClipArea.IsEmpty)
			{
				this.scene.RemoveFromScene(this.graphicObject);
				this.graphicObject = null;
				return;
			}

			if (this.Position.ClipArea.IsEmpty)
				return;

			var sceneObject = this.makeSceneObject();

			if (sceneObject != null)
				this.scene.UpdateScene(ref this.graphicObject, sceneObject);
		}

		protected void apply<T>(ref T state, T newValue)
		{
			var oldValue = state;
			state = newValue;

			if (!object.Equals(oldValue, newValue))
				this.updateScene();
		}

		protected void reposition()
		{
			if (this.scene == null)
				return;

			this.RecalculatePosition(false);
		}

		protected abstract SceneObject makeSceneObject();

		protected virtual Vector2 measureContent()
		{
			return new Vector2();
		}
	}
}
