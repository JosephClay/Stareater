﻿using Stareater.Controllers;
using Stareater.Controllers.Views;
using Stareater.GameData;
using Stareater.GLData;
using Stareater.GraphicsEngine;
using Stareater.GraphicsEngine.GuiElements;
using Stareater.GUI;
using Stareater.GuiUtils;
using Stareater.Localization;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Stareater.GameScenes
{
	class ConstructionSiteView : GuiPanel
	{
		private AConstructionSiteController controller;

		private readonly GuiText title;
		private readonly GuiButton projectButton;
		private readonly GuiButton detailsButton;
		private readonly GuiSlider investmentSlider;
		private readonly GuiText estimationLabel;
		private readonly CycleButton<PolicyInfo> policyToggle;
		private readonly GuiText policyName;

		public ConstructionSiteView()
		{
			this.Background = new BackgroundTexture(GalaxyTextures.Get.PanelBackground, 6);
			this.Position.FixedSize(360, 116);

			this.title = new GuiText { TextColor = Color.Black, TextSize = 8 };
			this.title.Position.WrapContent().Then.ParentRelative(-1, 1).WithMargins(8, 4);
			this.AddChild(this.title);

			this.projectButton = new GuiButton
			{
				BackgroundHover = new BackgroundTexture(GalaxyTextures.Get.ButtonHover, 9),
				BackgroundNormal = new BackgroundTexture(GalaxyTextures.Get.ButtonNormal, 9),
				Padding = 10,
				TextColor = Color.Black,
				TextSize = 8,
				ClickCallback = projectButton_Click
			};
			this.projectButton.Position.FixedSize(88, 88).ParentRelative(-1, -1).WithMargins(8, 8);
			this.AddChild(this.projectButton);

			this.detailsButton = new GuiButton
			{
				BackgroundHover = new BackgroundTexture(GalaxyTextures.Get.ButtonHover, 9),
				BackgroundNormal = new BackgroundTexture(GalaxyTextures.Get.ButtonNormal, 9),
				Padding = 10,
				TextColor = Color.Black,
				TextSize = 8,
				Text = context["SiteDetails"].Text(),
				ClickCallback = detailsButton_Click
			};
			this.detailsButton.Position.WrapContent().Then.ParentRelative(1, -1).WithMargins(8, 8);
			this.AddChild(this.detailsButton);

			this.investmentSlider = new GuiSlider
			{
				SlideCallback = investmentSlider_Change 
			};
			this.investmentSlider.Position.FixedSize(150, 15).RelativeTo(this.projectButton, 1, 1, -1, 1).WithMargins(8, 0).StretchRightTo(this, 1, 8);
			this.AddChild(this.investmentSlider);

			this.estimationLabel = new GuiText { TextColor = Color.Black, TextSize = 10 };
			this.estimationLabel.Position.WrapContent().Then.RelativeTo(this.investmentSlider, -1, -1, -1, 1).WithMargins(0, 8);
			this.AddChild(this.estimationLabel);

			this.policyToggle = new CycleButton<PolicyInfo>
			{
				BackgroundHover = new BackgroundTexture(GalaxyTextures.Get.ToggleHover, 8),
				BackgroundNormal = new BackgroundTexture(GalaxyTextures.Get.ToggleNormal, 8),
				Padding = 4,
				CycleCallback = x => 
				{
					this.controller.Policy = x;
					this.investmentSlider.Value = (float)this.controller.DesiredSpendingRatio;
					this.resetView();
					this.scene.ResetTooltipContents();
				},
				ItemImage = x => GalaxyTextures.Get.Sprite(x.Id + "Policy"),
				Tooltip = new DynamicTooltip("FormMain", () => this.controller.Policy.Id + "PolicyTooltip")
			};
			this.policyToggle.Position.FixedSize(32, 32).RelativeTo(this.projectButton, 1, -1, -1, -1).WithMargins(8, 0);
			this.AddChild(this.policyToggle);

			this.policyName = new GuiText { TextColor = Color.Black, TextSize = 10 };
			this.policyName.Position.WrapContent().Then.RelativeTo(this.policyToggle, 1, 0, -1, 0).WithMargins(8, 0);
			this.AddChild(this.policyName);
		}

		public override void Attach(AScene scene, AGuiElement parent)
		{
			base.Attach(scene, parent);

			this.updateSliderVisibility();
		}

		public void SetView(AConstructionSiteController siteController)
		{
			this.controller = siteController;
			if (this.controller.SiteType == SiteType.Colony)
			{
				var colonyController = this.controller as ColonyController;
				this.title.Text = LocalizationMethods.PlanetName(colonyController.PlanetBody);
				this.investmentSlider.ReadOnly = true;
			}
			else
			{
				this.title.Text = this.controller.HostStar.Name.ToText(LocalizationManifest.Get.CurrentLanguage);
				this.investmentSlider.ReadOnly = false;
			}

			this.investmentSlider.Value = (float)siteController.DesiredSpendingRatio;

			this.policyToggle.Items = siteController.Policies;
			this.policyToggle.Selection = siteController.Policy;

			this.resetView();
		}

		private void resetView()
		{
			if (controller.ConstructionQueue.Any())
			{
				this.projectButton.Text = null;
				this.projectButton.ForgroundImage = GalaxyTextures.Get.Sprite(this.controller.ConstructionQueue.First().ImagePath);
			}
			else
			{
				this.projectButton.Text = context["NotBuilding"].Text();
				this.projectButton.ForgroundImage = null;
			}

			this.policyName.Text = this.controller.Policy.Name;

			this.updateSliderVisibility();
			this.resetEstimation();
		}

		private void resetEstimation()
		{
			if (this.controller.ConstructionQueue.Any())
				this.estimationLabel.Text = LocalizationMethods.ConstructionEstimation(
					this.controller.ConstructionQueue.First(),
					context["EtaNever"],
					context["BuildingsPerTurn"],
					context["Eta"]
				);
			else
				this.estimationLabel.Text = "No construction plans";
		}

		private void updateSliderVisibility()
		{
			if (this.scene == null || this.controller == null)
				return;

			if (controller.ConstructionQueue.Any())
			{
				this.scene.ShowElement(this.estimationLabel);
				this.scene.ShowElement(this.investmentSlider);
			}
			else
			{
				this.scene.HideElement(this.investmentSlider);
				this.scene.HideElement(this.estimationLabel);
			}
		}

		private void detailsButton_Click()
		{
			Form form = null;

			switch (this.controller.SiteType)
			{
				case SiteType.Colony:
					form = new FormColonyDetails(this.controller as ColonyController);
					break;
				case SiteType.StarSystem:
					form = new FormStellarisDetails(this.controller as StellarisAdminController);
					break;
			}

			form.ShowDialog();
			form.Dispose();
		}

		private void investmentSlider_Change(float ratio)
		{
			//TODO(v0.8) make slider read only for colonies

			this.controller.DesiredSpendingRatio = ratio;
			this.resetEstimation();
		}

		private void projectButton_Click()
		{
			if (this.controller == null)
				return;

			using (var form = new FormBuildingQueue(this.controller))
				form.ShowDialog();

			this.resetView();
		}

		//TODO(v0.8) use GalaxyScene context
		private static Context context
		{
			get { return LocalizationManifest.Get.CurrentLanguage["FormMain"]; }
		}
	}
}
