using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Microsoft.Research.Kinect.Nui;

namespace OFWGKTA
{
    public class MenuEventArgs : EventArgs
    {
        public int SelectedIndex;
    }

    public class MenuRecognizer : ViewModelBase
    {
        private double selectionZTolerance = 0.20;

        private bool menuEnabled = false;
        private bool isHorizontal; // whether menu is enabled above head and panned L/R, or to right and U/D
        private int maxIndex;
        private int menuSize; // width (isHorizontal = true) or height (isHorizontal = false) of menu
        private Vector center; // center assigned on menuEnabled
        private int hoverIndex = -1; // index of item hovered over from 0 to numberOfItems - 1
        private int selectedIndex = -1;
        public bool selectionDead = false;

        //public event EventHandler<MenuEventArgs> MenuItemSelected;

        public bool Disabled { get; private set; }

        public MenuRecognizer(int numberOfItems, int menuSize)
            : this(numberOfItems, menuSize, true) { }

        public MenuRecognizer(int numberOfItems, int menuSize, bool isHorizontal)
        {
            this.Disabled = false;
            this.isHorizontal = isHorizontal;
            this.maxIndex = numberOfItems - 1;
            this.menuSize = menuSize;
        }

        private void ShowMenu(Vector center)
        {
            this.MenuEnabled = true;
            this.center = center;
        }

        private void HideMenu()
        {
            this.MenuEnabled = false;
            this.SelectionDead = false;
            this.HoverIndex = -1;
            this.SelectedIndex = -1;
        }

        private void AddHorizontal(Vector handRight, Vector shoulderCenter, Vector shoulderRight)
        {
            if (handRight.Y < shoulderCenter.Y)
            {
                if (!this.MenuEnabled)
                {
                    ShowMenu(handRight);
                }

                float delta = handRight.X - center.X;
                this.HoverIndex = Math.Min(this.maxIndex, Math.Max(0, (int)Math.Ceiling((delta / this.menuSize + 0.5) * this.maxIndex)));

                float deltaZ = center.Z - handRight.Z;
                if (deltaZ > this.selectionZTolerance)
                {
                    if (!this.SelectionDead)
                    {
                        if (this.SelectedIndex < 0)
                        {
                            this.SelectedIndex = this.HoverIndex;
                        }
                        else
                        {
                            if (this.SelectedIndex != this.HoverIndex)
                            {
                                this.SelectedIndex = -1;
                                this.SelectionDead = true;
                                // disable things until they back up and push back down again
                            }
                        }
                    }
                }
                else
                {
                    this.SelectedIndex = -1;
                    this.SelectionDead = false;
                }
            }
            else
            {
                HideMenu();
                this.SelectionDead = false;
            }
        }

        private void AddVertical(Vector handRight, Vector shoulderCenter, Vector shoulderRight)
        {
        }

        public void Add(Vector handRight, Vector shoulderCenter, Vector shoulderRight)
        {
            if (!Disabled)
            {
                if (isHorizontal)
                {
                    AddHorizontal(handRight, shoulderCenter, shoulderRight);
                }
                else
                {
                    AddVertical(handRight, shoulderCenter, shoulderRight);
                }
            }
        }

        public void Disable()
        {
            this.Disabled = true;
            this.menuEnabled = false;
        }

        public void Enable()
        {
            this.Disabled = false;
        }

        public bool MenuEnabled
        {
            get { return this.menuEnabled; }
            private set
            {
                if (this.menuEnabled != value)
                {
                    this.menuEnabled = value;
                    RaisePropertyChanged("MenuEnabled");
                }
            }
        }

        public int HoverIndex
        {
            get { return this.hoverIndex; }
            private set
            {
                if (this.hoverIndex != value)
                {
                    this.hoverIndex = value;
                    RaisePropertyChanged("HoverIndex");
                }
            }
        }

        public bool SelectionDead
        {
            get { return this.selectionDead; }
            private set
            {
                if (this.selectionDead != value)
                {
                    this.selectionDead = value;
                    RaisePropertyChanged("SelectionDead");
                }
            }
        }

        public int SelectedIndex
        {
            get { return this.selectedIndex; }
            private set
            {
                if (this.selectedIndex != value)
                {
                    this.selectedIndex = value;
                    RaisePropertyChanged("SelectedIndex");
                }
            }
        }
    }
}
