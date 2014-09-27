using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace SlidingPanelSample
{
	public class AdaptingBottomViewController : UIViewController
	{
		public AdaptingBottomViewController ()
		{
		}
		public override void LoadView ()
		{
			View = new BottomView ();
		}
		public float GetOffset()
		{
			return View.Frame.Height - View.Frame.Top - MinimumVisible;

		}
		const float MinimumVisible = 55f;
		public static float GetHeight(UIInterfaceOrientation orientation)
		{
			return MinimumVisible;
		}
		public float GetHeight()
		{
			return GetHeight(InterfaceOrientation);
		}


		public float GetVisibleHeight()
		{
			//Can be used to allow different docking;
			return MinimumVisible *2;
		}

		class BottomView : UIView
		{
			UIView overlay;
			public BottomView()
			{
				BackgroundColor = UIColor.Blue;
				Add(overlay = new UIView{
					BackgroundColor = UIColor.Red,
				});
			}
			public override RectangleF Frame
			{
				get { return base.Frame; }
				set
				{
					if (base.Frame == value)
						return;
					base.Frame = value;
					SetColor ();
				}
			}
			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
				overlay.Frame = Bounds;

			}
			void SetColor()
			{
				var alpha = (Frame.Top + MinimumVisible)/Frame.Height;
				overlay.Alpha = alpha;
			}
		}
	}
}

