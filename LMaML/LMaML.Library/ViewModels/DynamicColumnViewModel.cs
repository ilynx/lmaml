﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LMaML.Infrastructure.Domain.Concrete;
using Microsoft.Practices.Prism.Commands;
using iLynx.Common;

namespace LMaML.Library.ViewModels
{
    /// <summary>
    /// GenericColumnViewModel
    /// </summary>
    public class DynamicColumnViewModel : NotificationBase
    {
        private IQueryable<TagReference> items;
        private IQueryable<TagReference> rawItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicColumnViewModel" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DynamicColumnViewModel(ILogger logger)
            : base(logger)
        {
        }


        private ICommand dragLeaveCommand;
        private ICommand doubleClickCommand;
        private ICommand clickCommand;

        public ICommand DragLeaveCommand
        {
            get
            {
                return dragLeaveCommand ?? (dragLeaveCommand = new iLynx.Common.WPF.DelegateCommand<DragEventArgs>(OnDragLeave));
            }
        }

        /// <summary>
        /// Occurs when [add item].
        /// </summary>
        public event Action AddItems;

        private ICommand addSelection;
        /// <summary>
        /// Gets the add selection.
        /// </summary>
        /// <value>
        /// The add selection.
        /// </value>
        public ICommand AddSelection
        {
            get { return addSelection ?? (addSelection = new DelegateCommand(OnAddSelection)); }
        }

        private void OnAddSelection()
        {
            if (null != AddItems)
                AddItems();
        }

        /// <summary>
        /// Occurs when [play items].
        /// </summary>
        public event Action PlayItems;

        private ICommand playSelection;
        
        /// <summary>
        /// Gets the play selection.
        /// </summary>
        /// <value>
        /// The play selection.
        /// </value>
        public ICommand PlaySelection
        {
            get { return playSelection ?? (playSelection = new DelegateCommand(OnPlaySelection)); }
        }

        private void OnPlaySelection()
        {
            if (null != PlayItems)
                PlayItems();
        }

        private ObservableCollection<TagReference> selectedItems = new ObservableCollection<TagReference>();
        /// <summary>
        /// Gets or sets the selected items.
        /// </summary>
        /// <value>
        /// The selected items.
        /// </value>
        public ObservableCollection<TagReference> SelectedItems
        {
            get { return selectedItems; }
            set
            {
                if (value == selectedItems) return;
                selectedItems = value;
                RaisePropertyChanged(() => SelectedItems);
            }
        }

        private void OnDragLeave(DragEventArgs dragEventArgs)
        {
            //DragDrop.DoDragDrop()
            //dragEventArgs.Data.SetData();
        }

        /// <summary>
        /// Gets the double click command.
        /// </summary>
        /// <value>
        /// The double click command.
        /// </value>
        public ICommand DoubleClickCommand
        {
            get { return doubleClickCommand ?? (doubleClickCommand = new DelegateCommand<TagReference>(OnDoubleClicked)); }
        }

        /// <summary>
        /// Gets the click command.
        /// </summary>
        /// <value>
        /// The click command.
        /// </value>
        public ICommand ClickCommand
        {
            get { return clickCommand ?? (clickCommand = new DelegateCommand(OnClicked)); }
        }

        private void OnClicked()
        {
            OnItemClicked();
        }

        /// <summary>
        /// Called when [item clicked].
        /// </summary>
        protected virtual void OnItemClicked()
        {
            if (null != ItemClicked)
                ItemClicked();
        }

        private Regex filter;

        /// <summary>
        /// Sets the filter.
        /// </summary>
        /// <param name="fil">The fil.</param>
        public async void SetFilter(Regex fil)
        {
            filter = fil;
            Items = await ApplyFilter(rawItems);
            SelectFirst();
            //if (null == sourceItems) return;
            //Items = ApplyFilter(sourceItems);
        }

        private void OnDoubleClicked(TagReference o)
        {
            if (null == o) return;
            OnItemDoubleClicked(o);
        }

        /// <summary>
        /// Called when [item double clicked].
        /// </summary>
        /// <param name="item">The item.</param>
        protected virtual void OnItemDoubleClicked(TagReference item)
        {
            if (null == ItemDoubleClicked) return;
            ItemDoubleClicked(item);
        }

        /// <summary>
        /// Called when [item selected].
        /// </summary>
        /// <param name="item">The item.</param>
        protected virtual void OnItemSelected(TagReference item)
        {
            if (null == ItemSelected) return;
            ItemSelected(item);
        }

        /// <summary>
        /// Occurs when [item selected].
        /// </summary>
        public event Action<TagReference> ItemSelected;

        /// <summary>
        /// Occurs when [item clicked].
        /// </summary>
        public event Action ItemClicked;

        /// <summary>
        /// Selects the first.
        /// </summary>
        public void SelectFirst()
        {
            if (null == Items) return;
            SelectedItem = Items.FirstOrDefault();
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public IQueryable<TagReference> Items
        {
            get { return items; }
            private set
            {
                if (ReferenceEquals(value, items)) return;
                items = value;
                RaisePropertyChanged(() => Items);
            }
        }

        private TagReference selectedItem;
        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>
        /// The selected item.
        /// </value>
        public TagReference SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (value == selectedItem) return;
                selectedItem = value;
                RaisePropertyChanged(() => SelectedItem);
                OnItemSelected(value);
            }
        }

        private string displayMember;

        private async Task<IQueryable<TagReference>> ApplyFilter(IQueryable<TagReference> to)
        {
            return await Task.Factory.StartNew(() => null == filter ? to : to.Where(f => f is FilterAll || null == filter || filter.IsMatch(f.Name)));
        }

        /// <summary>
        /// Gets or sets the display member.
        /// </summary>
        /// <value>
        /// The display member.
        /// </value>
        public string DisplayMember
        {
            get { return displayMember; }
            set
            {
                if (value == displayMember) return;
                displayMember = value;
                RaisePropertyChanged(() => DisplayMember);
            }
        }

        /// <summary>
        /// Occurs when [item selected].
        /// </summary>
        public event Action<TagReference> ItemDoubleClicked;

        /// <summary>
        /// Sets the items.
        /// </summary>
        /// <param name="newItems">The new items.</param>
        public async void SetItems(IQueryable<TagReference> newItems)
        {
            if (null == newItems) return;
            rawItems = newItems;
            Items = await ApplyFilter(rawItems);
            SelectedItem = Items.FirstOrDefault();
        }
    }
}
