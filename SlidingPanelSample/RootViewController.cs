using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.MediaPlayer;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace SlidingPanelSample
{
	public class RootViewController : UIViewController
	{

		RootView view;

		public override bool PrefersStatusBarHidden()
		{
			return false;
		}

		public override void LoadView()
		{
			View = view = new RootView(this);
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			view.Parent = this;
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			view.Parent = null;
		}
		public void ShowPanel(bool animated = true)
		{
			view.ShowDrawer(animated);
		}
		public void HidePanel(bool animated = true, bool completeClose = true)
		{
			view.HideDrawer(animated,completeClose);

		}

	

		public class RootView : UIView
		{
			static float NowPlayingGestureTollerance = 50;
			const float FlickVelocity = 1000.0f;
			bool isHidden = true;
			float startY;

			public AdaptingBottomViewController BottomBarController { get; set; }
			public RootViewController Parent { get; set; }
			public UIViewController MainViewController { get; set; }
			public RootView(RootViewController root)
			{
				Parent = root;

				MainViewController = new UIViewController{
					View = {
						BackgroundColor = UIColor.LightGray
					}
				};
				Add(MainViewController.View);
				Parent.AddChildViewController(MainViewController);


				BottomBarController = new AdaptingBottomViewController();
				BottomBarController.View.AddGestureRecognizer(new UIPanGestureRecognizer(Panned)
					{
						//CancelsTouchesInView = false,
						ShouldReceiveTouch = (sender, touch) =>
						{
							bool isMovingCell =
								touch.View.ToString().IndexOf("UITableViewCellReorderControl", StringComparison.InvariantCultureIgnoreCase) >
								-1;
							if (isMovingCell || touch.View is UISlider || touch.View is MPVolumeView || isMovingCell)
								return false;
							return true;
						}
					});

				Add(BottomBarController.View);
				Parent.AddChildViewController(BottomBarController);

			}

		

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				RectangleF bounds = Bounds;
				MainViewController.View.Frame = bounds;


				if (isHidden)
				{
					HideDrawer(false);
					return;
				}
				else
				{
					ShowDrawer(false);
				}
				RectangleF frame = MainViewController.View.Frame;
				frame.Size = Bounds.Size;
				BottomBarController.View.Frame = frame;

			}

			public virtual void HideDrawer(bool animated = true, bool completeClose = true)
			{
				isHidden = true;
				if (animated)
					BeginAnimations("hideNowPlaying");
				RectangleF frame = Bounds;
				if (frame == RectangleF.Empty)
					return;

				frame.Y = frame.Height - (completeClose ? BottomBarController.GetHeight() : BottomBarController.GetVisibleHeight());
				BottomBarController.View.Frame = frame;

				if (animated)
					CommitAnimations();
			}
				


			public virtual void ShowDrawer(bool animated = true)
			{
				isHidden = false;
				if (animated)
					BeginAnimations("showNowPlaying");
				RectangleF frame = Bounds;
				BottomBarController.View.Frame = frame;

				if (animated)
					CommitAnimations();
			}
			bool isPanning;
			void Panned(UIPanGestureRecognizer panGesture)
			{
				RectangleF frame = BottomBarController.View.Frame;
				float translation = panGesture.TranslationInView(this).Y;
				if (panGesture.State == UIGestureRecognizerState.Began)
				{
					isPanning = true;
					startY = frame.Y;
				}
				else if (panGesture.State == UIGestureRecognizerState.Changed)
				{
					frame.Y = translation + startY;
					frame.Y = Math.Min(frame.Height, Math.Max(frame.Y, 0));
					BottomBarController.View.Frame = frame;
				}
				else if (panGesture.State == UIGestureRecognizerState.Ended)
				{
					isPanning = false;
					float velocity = panGesture.VelocityInView(this).Y;
					//					Console.WriteLine (velocity);
					bool show = (Math.Abs(velocity) > FlickVelocity)
						? (velocity < 0)
						: (translation*-1 > NowPlayingGestureTollerance);
					if (show)
						ShowDrawer(true);
					else
						HideDrawer(true, Math.Abs(velocity) > FlickVelocity || (translation > 5 && BottomBarController.GetOffset() < BottomBarController.GetHeight() * 2));
				}
			}
		}
	}
}