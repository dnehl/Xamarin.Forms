﻿using CoreAnimation;
using CoreGraphics;
using System;
using UIKit;
using System.ComponentModel;
using System.Collections.Specialized;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellTableViewController : UITableViewController
	{
		readonly IShellContext _context;
		readonly ShellTableViewSource _source;
		bool _isDisposed;
		Action<Element> _onElementSelected;
		IShellController ShellController => _context.Shell;

		public ShellTableViewController(IShellContext context, UIContainerView headerView, Action<Element> onElementSelected) : this(context, onElementSelected)
		{
			ShellFlyoutContentManager = new ShellFlyoutContentManager(context);
			HeaderView = headerView;
		}

		public ShellTableViewController(IShellContext context, Action<Element> onElementSelected)
		{
			ShellFlyoutContentManager = ShellFlyoutContentManager ?? new ShellFlyoutContentManager(context);
			_context = context;
			_onElementSelected = onElementSelected;
			_source = CreateShellTableViewSource();

			ShellController.StructureChanged += OnStructureChanged;
			_source.ScrolledEvent += OnScrolled;
		}

		internal ShellFlyoutContentManager ShellFlyoutContentManager
		{
			get;
			set;
		}

		void OnScrolled(object sender, UIScrollView e)
		{
			ShellFlyoutContentManager.OnScrolled();
		}

		public virtual UIContainerView HeaderView
		{
			get => ShellFlyoutContentManager.HeaderView;
			set => ShellFlyoutContentManager.HeaderView = value;
		}

		public virtual UIView FooterView
		{
			get => ShellFlyoutContentManager.FooterView;
			set => ShellFlyoutContentManager.FooterView = value;
		}

		protected ShellTableViewSource CreateShellTableViewSource()
		{
			return new ShellTableViewSource(_context, _onElementSelected);
		}

		void OnStructureChanged(object sender, EventArgs e)
		{
			_source.ClearCache();
			TableView.ReloadData();
			ShellFlyoutContentManager.UpdateVerticalScrollMode();
		}

		public void LayoutParallax() => 		
			ShellFlyoutContentManager.LayoutParallax();

		public override void ViewDidLoad()
		{			
			base.ViewDidLoad();

			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			if (Forms.IsiOS11OrNewer)
				TableView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;

			TableView.Source = _source;
			ShellFlyoutContentManager.ViewDidLoad();
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if ((_context?.Shell as IShellController) != null)
					((IShellController)_context.Shell).StructureChanged -= OnStructureChanged;

				if (_source != null)
					_source.ScrolledEvent -= OnScrolled;

				ShellFlyoutContentManager.TearDown();
				_onElementSelected = null;
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}
